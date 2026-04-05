using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ReNamer.Models;

namespace ReNamer.Services;

public sealed class FileImportService
{
    private const int ParallelImportCreationThreshold = 256;
    private static readonly int ParallelImportCreationDegree = Math.Clamp(Environment.ProcessorCount, 2, 8);

    private readonly FileImportOptions _options;
    private readonly Action<string>? _log;

    public FileImportService(FileImportOptions options, Action<string>? log = null)
    {
        _options = options;
        _log = log;
    }

    public FileImportBatch BuildBatch(IEnumerable<string> paths, IEnumerable<string> existingPaths, long traceId)
    {
        var pathList = paths?.ToList() ?? new List<string>();
        var existing = new HashSet<string>(existingPaths ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        var added = new List<RenFile>();
        int skippedCount = 0;
        int duplicateInputCount = 0;
        int pathNotFoundCount = 0;

        foreach (var path in pathList)
        {
            if (string.IsNullOrWhiteSpace(path))
                continue;

            try
            {
                if (File.Exists(path))
                {
                    if (!existing.Add(path))
                    {
                        duplicateInputCount++;
                        if (duplicateInputCount <= 5)
                            Log($"[Import#{traceId}] skip-duplicate-file '{path}'");
                        continue;
                    }

                    added.Add(CreateRenFile(path));
                    if (added.Count <= 8)
                        Log($"[Import#{traceId}] add-file '{path}'");
                    continue;
                }

                if (Directory.Exists(path))
                {
                    Log($"[Import#{traceId}] scan-dir '{path}' recursive={_options.Recursive}");
                    var directoryBatch = AddDirectoryEntries(path, existing, ref skippedCount, traceId);
                    added.AddRange(directoryBatch.Items);
                    continue;
                }

                pathNotFoundCount++;
                if (pathNotFoundCount <= 5)
                    Log($"[Import#{traceId}] skip-not-found '{path}'");
            }
            catch (UnauthorizedAccessException)
            {
                skippedCount++;
                if (skippedCount <= 5)
                    Log($"[Import#{traceId}] unauthorized '{path}'");
            }
            catch (IOException)
            {
                skippedCount++;
                if (skippedCount <= 5)
                    Log($"[Import#{traceId}] io-failed '{path}'");
            }
            catch (Exception ex)
            {
                skippedCount++;
                Log($"[Import#{traceId}] exception '{path}' message='{ex.Message}'");
            }
        }

        return new FileImportBatch(added, skippedCount, duplicateInputCount, pathNotFoundCount);
    }

    private DirectoryImportBatch AddDirectoryEntries(string rootPath, HashSet<string> existing, ref int skippedCount, long traceId)
    {
        var scanSw = System.Diagnostics.Stopwatch.StartNew();
        // Directory import behavior must follow the folder filter checkboxes exactly.
        var allowSubfolders = _options.IncludeSubfolders;
        var searchOption = allowSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var includeFiles = _options.IncludeAllFiles;
        var includeMasks = ParseMasks(_options.IncludeMask);
        var excludeMasks = ParseMasks(_options.ExcludeMask);

        int dirCandidates = 0;
        int dirAdded = 0;
        int dirFilteredAttr = 0;
        int dirFilteredMask = 0;
        int dirDuplicate = 0;
        int fileCandidates = 0;
        int fileAdded = 0;
        int fileFilteredAttr = 0;
        int fileFilteredMask = 0;
        int fileDuplicate = 0;

        Log($"[Import#{traceId}] dir-scan root='{rootPath}' allowSubfolders={allowSubfolders} includeFiles={includeFiles} includeFolders={_options.IncludeFolderNames} searchOption={searchOption}");

        var childDirs = allowSubfolders
            ? SafeEnumerateDirectories(rootPath, SearchOption.AllDirectories, ref skippedCount).ToList()
            : new List<string>();
        var traversalRoots = new List<string>(childDirs.Count + 1) { rootPath };
        traversalRoots.AddRange(childDirs);

        var items = new List<RenFile>();

        if (_options.IncludeFolderNames)
        {
            List<string> dirs;
            if (allowSubfolders)
                dirs = _options.IgnoreRootFolder ? childDirs : traversalRoots;
            else
                dirs = _options.IgnoreRootFolder ? new List<string>() : new List<string> { rootPath };

            dirCandidates = dirs.Count;
            var acceptedDirs = new List<string>(dirs.Count);
            var acceptedDirSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dir in dirs)
            {
                if (!PassesAttributes(dir))
                {
                    dirFilteredAttr++;
                    if (dirFilteredAttr <= 5)
                        Log($"[Import#{traceId}] dir-filter-attr '{dir}'");
                    continue;
                }

                if (!PassesMasks(dir, isFolder: true, includeMasks, excludeMasks))
                {
                    dirFilteredMask++;
                    if (dirFilteredMask <= 5)
                        Log($"[Import#{traceId}] dir-filter-mask '{dir}'");
                    continue;
                }

                if (existing.Contains(dir) || !acceptedDirSet.Add(dir))
                {
                    dirDuplicate++;
                    continue;
                }

                acceptedDirs.Add(dir);
            }

            var dirItems = CreateRenFilesForPaths(acceptedDirs, traceId, "dir", ref skippedCount);
            for (int i = 0; i < dirItems.Length; i++)
            {
                var rf = dirItems[i];
                if (rf == null)
                    continue;

                items.Add(rf);
                existing.Add(acceptedDirs[i]);
                dirAdded++;
                if (dirAdded <= 5)
                    Log($"[Import#{traceId}] dir-added '{acceptedDirs[i]}'");
            }
        }

        if (includeFiles)
        {
            var files = SafeEnumerateFilesFromRoots(traversalRoots, ref skippedCount).ToList();
            fileCandidates = files.Count;
            var acceptedFiles = new List<string>(files.Count);
            var acceptedFileSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in files)
            {
                if (!PassesAttributes(file))
                {
                    fileFilteredAttr++;
                    if (fileFilteredAttr <= 5)
                        Log($"[Import#{traceId}] file-filter-attr '{file}'");
                    continue;
                }

                if (!PassesMasks(file, isFolder: false, includeMasks, excludeMasks))
                {
                    fileFilteredMask++;
                    if (fileFilteredMask <= 5)
                        Log($"[Import#{traceId}] file-filter-mask '{file}'");
                    continue;
                }

                if (existing.Contains(file) || !acceptedFileSet.Add(file))
                {
                    fileDuplicate++;
                    continue;
                }

                acceptedFiles.Add(file);
            }

            var fileItems = CreateRenFilesForPaths(acceptedFiles, traceId, "file", ref skippedCount);
            for (int i = 0; i < fileItems.Length; i++)
            {
                var rf = fileItems[i];
                if (rf == null)
                    continue;

                items.Add(rf);
                existing.Add(acceptedFiles[i]);
                fileAdded++;
                if (fileAdded <= 8)
                    Log($"[Import#{traceId}] file-added '{acceptedFiles[i]}'");
            }
        }

        scanSw.Stop();
        Log(
            $"[Import#{traceId}] dir-scan-summary root='{rootPath}' " +
            $"dirCandidates={dirCandidates} dirAdded={dirAdded} dirFilteredAttr={dirFilteredAttr} dirFilteredMask={dirFilteredMask} dirDuplicate={dirDuplicate} " +
            $"fileCandidates={fileCandidates} fileAdded={fileAdded} fileFilteredAttr={fileFilteredAttr} fileFilteredMask={fileFilteredMask} fileDuplicate={fileDuplicate} skipped={skippedCount} elapsedMs={scanSw.ElapsedMilliseconds}");

        return new DirectoryImportBatch(items);
    }

    private RenFile?[] CreateRenFilesForPaths(IReadOnlyList<string> paths, long traceId, string scope, ref int skippedCount)
    {
        var result = new RenFile?[paths.Count];
        if (paths.Count == 0)
            return result;

        if (paths.Count < ParallelImportCreationThreshold)
        {
            int sequentialFailures = 0;
            foreach (var (path, index) in paths.Select((value, idx) => (value, idx)))
            {
                try
                {
                    result[index] = CreateRenFile(path);
                }
                catch (Exception ex)
                {
                    sequentialFailures++;
                    if (sequentialFailures <= 5)
                        Log($"[Import#{traceId}] {scope}-create-failed path='{path}' message='{ex.Message}'");
                }
            }

            if (sequentialFailures > 0)
                skippedCount += sequentialFailures;
            return result;
        }

        Log($"[Import#{traceId}] {scope}-materialize mode=parallel count={paths.Count} degree={ParallelImportCreationDegree}");
        int failed = 0;
        var failures = new ConcurrentQueue<(string Path, string Message)>();
        Parallel.For(
            0,
            paths.Count,
            new ParallelOptions { MaxDegreeOfParallelism = ParallelImportCreationDegree },
            i =>
            {
                try
                {
                    result[i] = CreateRenFile(paths[i]);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failed);
                    if (failures.Count < 5)
                        failures.Enqueue((paths[i], ex.Message));
                }
            });

        if (failed > 0)
        {
            skippedCount += failed;
            foreach (var failure in failures)
                Log($"[Import#{traceId}] {scope}-create-failed path='{failure.Path}' message='{failure.Message}'");
            Log($"[Import#{traceId}] {scope}-create-failed count={failed}");
        }

        return result;
    }

    private RenFile CreateRenFile(string path) => new(RenFileImportData.FromPath(path));

    private IEnumerable<string> SafeEnumerateDirectories(string path, SearchOption searchOption, ref int skippedCount)
    {
        var result = new List<string>();
        var pending = new Queue<string>();
        pending.Enqueue(path);

        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            IEnumerable<string> dirs;
            try
            {
                dirs = Directory.EnumerateDirectories(current, "*", SearchOption.TopDirectoryOnly);
            }
            catch (UnauthorizedAccessException)
            {
                skippedCount++;
                Log($"[ImportEnum] dir-enum unauthorized current='{current}'");
                continue;
            }
            catch (IOException)
            {
                skippedCount++;
                Log($"[ImportEnum] dir-enum io-failed current='{current}'");
                continue;
            }

            foreach (var dir in dirs)
            {
                result.Add(dir);
                if (searchOption == SearchOption.AllDirectories)
                    pending.Enqueue(dir);
            }

            if (searchOption == SearchOption.TopDirectoryOnly)
                break;
        }

        return result;
    }

    private IEnumerable<string> SafeEnumerateFilesFromRoots(IEnumerable<string> roots, ref int skippedCount)
    {
        var result = new List<string>();
        foreach (var current in roots)
        {
            try
            {
                result.AddRange(Directory.EnumerateFiles(current, "*", SearchOption.TopDirectoryOnly));
            }
            catch (UnauthorizedAccessException)
            {
                skippedCount++;
                Log($"[ImportEnum] file-enum unauthorized current='{current}'");
            }
            catch (IOException)
            {
                skippedCount++;
                Log($"[ImportEnum] file-enum io-failed current='{current}'");
            }
        }

        return result;
    }

    private bool PassesAttributes(string fullPath)
    {
        if (_options.IncludeHiddenFiles && _options.IncludeSystemFiles)
            return true;

        try
        {
            var attrs = File.GetAttributes(fullPath);
            if (!_options.IncludeHiddenFiles && attrs.HasFlag(FileAttributes.Hidden))
                return false;
            if (!_options.IncludeSystemFiles && attrs.HasFlag(FileAttributes.System))
                return false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool PassesMasks(string fullPath, bool isFolder, string[] includeMasks, string[] excludeMasks)
    {
        var target = _options.MaskFileNameOnly ? Path.GetFileName(fullPath) : fullPath;
        if (string.IsNullOrEmpty(target))
            return false;

        if (includeMasks.Length > 0 && !includeMasks.Any(mask => MatchWildcard(target, mask, caseSensitive: false)))
            return false;

        if (excludeMasks.Any(mask => MatchWildcard(target, mask, caseSensitive: false)))
            return false;

        return true;
    }

    private static string[] ParseMasks(string? rawMasks)
    {
        if (string.IsNullOrWhiteSpace(rawMasks))
            return Array.Empty<string>();

        return rawMasks
            .Split(';')
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToArray();
    }

    private static bool MatchWildcard(string input, string pattern, bool caseSensitive)
    {
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        return Regex.IsMatch(input, regexPattern, options);
    }

    private void Log(string message) => _log?.Invoke(message);
}

public readonly record struct FileImportOptions(
    bool Recursive,
    bool IncludeAllFiles,
    bool IncludeFolderNames,
    bool IncludeSubfolders,
    bool IncludeHiddenFiles,
    bool IncludeSystemFiles,
    bool IgnoreRootFolder,
    bool MaskFileNameOnly,
    string IncludeMask,
    string ExcludeMask);

public sealed record FileImportBatch(
    IReadOnlyList<RenFile> Items,
    int SkippedCount,
    int DuplicateInputCount,
    int PathNotFoundCount);

internal sealed record DirectoryImportBatch(IReadOnlyList<RenFile> Items);

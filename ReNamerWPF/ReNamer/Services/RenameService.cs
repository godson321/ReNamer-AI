using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ReNamer.Models;
using ReNamer.Rules;

namespace ReNamer.Services;

/// <summary>
/// 重命名服务 - 核心业务逻辑
/// 对应原版的 Preview/Rename/UndoRename 流程
/// </summary>
public class RenameService
{
    private const int ParallelPreviewThreshold = 512;
    private static readonly int ParallelPreviewDegree = Math.Clamp(Environment.ProcessorCount, 2, 8);
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> StringPropertyCache = new();
    private readonly record struct PreviewRuleSegment(IRule[] Rules, bool CanParallelize);

    /// <summary>
    /// 预览 - 应用所有规则计算新文件名
    /// </summary>
    public void Preview(IEnumerable<RenFile> files, IEnumerable<IRule> rules, Action<int, int>? progress = null)
    {
        var previewResults = ComputePreview(files, rules, progress);
        foreach (var (file, newName) in previewResults)
            file.ApplyPreviewName(newName);
    }

    public List<(RenFile file, string newName)> ComputePreview(
        IEnumerable<RenFile> files,
        IEnumerable<IRule> rules,
        Action<int, int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var ruleList = rules.Where(r => r.IsEnabled).ToList();
        var fileList = files.ToList();
        var previewResults = new List<(RenFile file, string newName)>(fileList.Count);
        if (fileList.Count == 0)
            return previewResults;
        var engineSw = Stopwatch.StartNew();

        RuleHelpers.ResetMetaTagCounter();

        // 重置所有有状态规则的内部状态
        // 修复 Bug: 多次 Preview 结果不一致的问题
        foreach (var rule in ruleList)
        {
            if (cancellationToken.IsCancellationRequested)
                return previewResults;

            if (rule is IStatefulRule statefulRule)
                statefulRule.Reset();
        }

        int total = fileList.Count;
        int progressStep = Math.Max(1, total / 100);
        if (ruleList.Count == 0)
        {
            for (int i = 0; i < total; i++)
            {
                previewResults.Add((fileList[i], fileList[i].OriginalName));
                int cur = i + 1;
                if (progress != null && (cur == 1 || cur == total || cur % progressStep == 0))
                    progress(cur, total);
            }

            engineSw.Stop();
            LogPreviewDebug($"[PreviewEngine] end mode=serial reason=no-rules files={total} results={previewResults.Count} elapsedMs={engineSw.ElapsedMilliseconds}");
            return previewResults;
        }

        bool allowParallel = total >= ParallelPreviewThreshold;
        var segments = BuildPreviewSegments(ruleList, allowParallel);
        int parallelSegmentCount = segments.Count(s => s.CanParallelize);
        int serialSegmentCount = segments.Count - parallelSegmentCount;
        var mode = parallelSegmentCount == 0
            ? "serial"
            : serialSegmentCount == 0
                ? "parallel"
                : "hybrid";
        var reason = parallelSegmentCount == 0
            ? total < ParallelPreviewThreshold
                ? $"below-threshold({total}<{ParallelPreviewThreshold})"
                : "no-parallel-segment"
            : serialSegmentCount == 0
                ? "all-rules-parallelizable"
                : "segmented";
        LogPreviewDebug($"[PreviewEngine] start mode={mode} reason={reason} files={total} rules={ruleList.Count} segments={segments.Count} parallelSegments={parallelSegmentCount} serialSegments={serialSegmentCount} degree={ParallelPreviewDegree}");

        var currentNames = fileList.Select(static file => file.OriginalName).ToArray();
        int processed = 0;

        try
        {
            for (int segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var segment = segments[segmentIndex];
                bool reportProgress = segmentIndex == segments.Count - 1;
                if (segment.CanParallelize)
                {
                    Parallel.For(
                        0,
                        total,
                        new ParallelOptions
                        {
                            CancellationToken = cancellationToken,
                            MaxDegreeOfParallelism = ParallelPreviewDegree
                        },
                        i =>
                        {
                            var currentName = currentNames[i];
                            var file = fileList[i];
                            foreach (var rule in segment.Rules)
                                currentName = rule.Execute(currentName, file);

                            currentNames[i] = currentName;

                            if (!reportProgress)
                                return;

                            int cur = Interlocked.Increment(ref processed);
                            if (progress != null && (cur == 1 || cur == total || cur % progressStep == 0))
                                progress(cur, total);
                        });
                }
                else
                {
                    for (int i = 0; i < total; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var currentName = currentNames[i];
                        var file = fileList[i];
                        foreach (var rule in segment.Rules)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            currentName = rule.Execute(currentName, file);
                        }

                        currentNames[i] = currentName;

                        if (!reportProgress)
                            continue;

                        processed++;
                        if (progress != null && (processed == 1 || processed == total || processed % progressStep == 0))
                            progress(processed, total);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                engineSw.Stop();
                LogPreviewDebug($"[PreviewEngine] canceled mode={mode} files={total} results={previewResults.Count} elapsedMs={engineSw.ElapsedMilliseconds}");
                return previewResults;
            }
        }

        for (int i = 0; i < total; i++)
            previewResults.Add((fileList[i], currentNames[i]));

        engineSw.Stop();
        LogPreviewDebug($"[PreviewEngine] end mode={mode} files={total} results={previewResults.Count} elapsedMs={engineSw.ElapsedMilliseconds}");
        return previewResults;
    }

    private static bool CanParallelizePreview(IReadOnlyList<IRule> rules)
    {
        foreach (var rule in rules)
        {
            if (!CanRuleParallelizePreview(rule))
                return false;
        }

        return true;
    }

    private static List<PreviewRuleSegment> BuildPreviewSegments(IReadOnlyList<IRule> rules, bool allowParallel)
    {
        var segments = new List<PreviewRuleSegment>();
        if (rules.Count == 0)
            return segments;

        var currentRules = new List<IRule>();
        bool currentParallel = false;
        bool hasCurrent = false;

        foreach (var rule in rules)
        {
            bool ruleParallel = allowParallel && CanRuleParallelizePreview(rule);
            if (!hasCurrent)
            {
                currentParallel = ruleParallel;
                hasCurrent = true;
            }
            else if (currentParallel != ruleParallel)
            {
                segments.Add(new PreviewRuleSegment(currentRules.ToArray(), currentParallel));
                currentRules.Clear();
                currentParallel = ruleParallel;
            }

            currentRules.Add(rule);
        }

        if (currentRules.Count > 0)
            segments.Add(new PreviewRuleSegment(currentRules.ToArray(), currentParallel));

        return segments;
    }

    private static bool CanRuleParallelizePreview(IRule rule)
    {
        if (UsesCounterMetaTag(rule))
            return false;

        return rule switch
        {
            IPreviewParallelRule previewRule => previewRule.CanParallelizePreview,
            IStatefulRule => false,
            _ => true
        };
    }

    private static bool UsesCounterMetaTag(IRule rule)
    {
        if (rule is JavaScriptRule javaScriptRule && !javaScriptRule.EnableMetaTags)
            return false;

        var props = StringPropertyCache.GetOrAdd(
            rule.GetType(),
            t => t
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead
                            && p.GetIndexParameters().Length == 0
                            && p.PropertyType == typeof(string))
                .ToArray());

        foreach (var prop in props)
        {
            string? value;
            try
            {
                value = prop.GetValue(rule) as string;
            }
            catch
            {
                continue;
            }

            if (string.IsNullOrEmpty(value))
                continue;

            if (value.IndexOf("{:Counter:}", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }

    private static void LogPreviewDebug(string message)
    {
        if (!OperatingSystem.IsWindows())
        {
            Debug.WriteLine(message);
            return;
        }

        try
        {
            ReNamer.App.LogInputDebug(message);
        }
        catch
        {
            Debug.WriteLine(message);
        }
    }

    /// <summary>
    /// 执行重命名
    /// </summary>
    public (int success, int failed, bool canceled) Rename(
        IEnumerable<RenFile> files,
        System.Action<int, int>? progress = null,
        int conflictResolution = 0,
        CancellationToken cancellationToken = default)
    {
        int success = 0;
        int failed = 0;
        bool canceled = false;
        var fileList = files.ToList();
        var reservedTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var mode = conflictResolution is 1 or 2 ? conflictResolution : 0;
        int index = 0;
        int total = fileList.Count;

        foreach (var file in fileList)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                canceled = true;
                break;
            }

            if (file.IsMarked && file.HasChanged)
            {
                var targetPath = Path.Combine(file.FolderPath, file.NewName);
                var canRename = true;

                if (mode == 1)
                {
                    var resolvedName = ResolveNameWithSuffix(file, reservedTargets);
                    if (!string.Equals(resolvedName, file.NewName, StringComparison.Ordinal))
                        file.NewName = resolvedName;

                    targetPath = Path.Combine(file.FolderPath, file.NewName);
                }
                else
                {
                    if (reservedTargets.Contains(targetPath))
                    {
                        file.State = "×";
                        file.Error = "Target name conflicts with another renamed file";
                        canRename = false;
                    }
                    else if (HasExistingTargetConflict(file.FullPath, targetPath))
                    {
                        if (mode == 2)
                        {
                            if (!TryDeleteExistingTarget(targetPath, out var deleteError))
                            {
                                file.State = "×";
                                file.Error = deleteError;
                                canRename = false;
                            }
                        }
                        else
                        {
                            file.State = "×";
                            file.Error = "Target file already exists";
                            canRename = false;
                        }
                    }
                }

                if (canRename && file.Rename())
                {
                    success++;
                    reservedTargets.Add(file.FullPath);
                }
                else
                {
                    failed++;
                }
            }
            index++;
            progress?.Invoke(index, total);
        }

        if (canceled)
            return (success, failed, true);

        return (success, failed, false);
    }

    private static string ResolveNameWithSuffix(RenFile file, ISet<string> reservedTargets)
    {
        var baseName = file.IsFolder ? file.NewName : Path.GetFileNameWithoutExtension(file.NewName);
        var extension = file.IsFolder ? string.Empty : Path.GetExtension(file.NewName);
        var candidateName = file.NewName;
        var candidatePath = Path.Combine(file.FolderPath, candidateName);
        var suffix = 1;

        while (HasTargetConflict(file.FullPath, candidatePath, reservedTargets))
        {
            candidateName = $"{baseName} ({suffix++}){extension}";
            candidatePath = Path.Combine(file.FolderPath, candidateName);
        }

        return candidateName;
    }

    private static bool HasTargetConflict(string sourcePath, string targetPath, ISet<string> reservedTargets)
    {
        if (reservedTargets.Contains(targetPath))
            return true;

        return HasExistingTargetConflict(sourcePath, targetPath);
    }

    private static bool HasExistingTargetConflict(string sourcePath, string targetPath)
    {
        if (string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
            return false;

        return File.Exists(targetPath) || Directory.Exists(targetPath);
    }

    private static bool TryDeleteExistingTarget(string targetPath, out string error)
    {
        try
        {
            if (File.Exists(targetPath))
                File.Delete(targetPath);
            else if (Directory.Exists(targetPath))
                Directory.Delete(targetPath, recursive: true);

            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = $"Failed to overwrite existing target: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 撤销重命名
    /// </summary>
    public (int success, int failed) UndoRename(IEnumerable<RenFile> files)
    {
        int success = 0;
        int failed = 0;

        // 反向顺序撤销
        foreach (var file in files.Reverse())
        {
            if (file.IsRenamed)
            {
                if (file.UndoRename())
                    success++;
                else
                    failed++;
            }
        }

        return (success, failed);
    }

    /// <summary>
    /// 验证新文件名（检测冲突/非法字符/长路径）
    /// </summary>
    public List<string> ValidateNewNames(
        IEnumerable<RenFile> files,
        bool warnInvalidChars = true,
        bool warnLongPaths = true,
        int longPathThreshold = 260)
    {
        var errors = new List<string>();
        var fileList = files.ToList();
        var changedFiles = fileList.Where(f => f.HasChanged).ToList();
        var newNames = new Dictionary<string, List<RenFile>>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in changedFiles)
        {
            if (!TryGetNormalizedPath(Path.Combine(file.FolderPath, file.NewName), out var targetPath))
            {
                errors.Add($"Invalid target path: {file.NewName}");
                continue;
            }

            if (!newNames.TryGetValue(targetPath, out var group))
            {
                group = new List<RenFile>();
                newNames[targetPath] = group;
            }

            group.Add(file);
        }

        // 检查重复和仅大小写差异冲突
        foreach (var kvp in newNames.Where(x => x.Value.Count > 1))
        {
            errors.Add($"Duplicate name: {kvp.Value[0].NewName} ({kvp.Value.Count} files)");

            var caseVariants = kvp.Value
                .Select(f => f.NewName)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            if (caseVariants.Count > 1)
            {
                errors.Add($"Case conflict: {string.Join(", ", caseVariants)}");
            }
        }

        foreach (var file in changedFiles)
        {
            if (!TryGetNormalizedPath(file.FullPath, out var sourcePath)
                || !TryGetNormalizedPath(Path.Combine(file.FolderPath, file.NewName), out var targetPath))
            {
                continue;
            }

            var sourceRoot = Path.GetPathRoot(sourcePath) ?? string.Empty;
            var targetRoot = Path.GetPathRoot(targetPath) ?? string.Empty;
            if (!string.Equals(sourceRoot, targetRoot, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Cross-volume rename is not supported: {sourcePath} -> {targetPath}");
            }

            if (string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(sourcePath, targetPath, StringComparison.Ordinal))
            {
                errors.Add($"Case-only rename may fail on some file systems: {file.OriginalName} -> {file.NewName}");
            }
            else if (HasExistingTargetConflict(file.FullPath, targetPath))
            {
                errors.Add($"Target already exists: {targetPath}");
            }

            if (IsReservedFileName(file.NewName, file.IsFolder))
            {
                errors.Add($"Reserved name is not allowed: {file.NewName}");
            }
        }

        if (warnInvalidChars)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var file in changedFiles)
            {
                if (file.NewName.IndexOfAny(invalidChars) >= 0)
                {
                    errors.Add($"Invalid characters in: {file.NewName}");
                }
            }
        }

        if (warnLongPaths)
        {
            foreach (var file in changedFiles)
            {
                if (TryGetNormalizedPath(Path.Combine(file.FolderPath, file.NewName), out var targetPath)
                    && targetPath.Length > longPathThreshold)
                {
                    errors.Add($"Long path warning ({targetPath.Length}): {targetPath}");
                }
            }
        }

        return errors;
    }

    private static bool TryGetNormalizedPath(string path, out string normalizedPath)
    {
        try
        {
            normalizedPath = Path.GetFullPath(path);
            return true;
        }
        catch
        {
            normalizedPath = string.Empty;
            return false;
        }
    }

    private static bool IsReservedFileName(string newName, bool isFolder)
    {
        var trimmed = newName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var leafName = Path.GetFileName(trimmed);
        if (string.IsNullOrWhiteSpace(leafName))
            leafName = newName;

        var candidate = isFolder ? leafName : Path.GetFileNameWithoutExtension(leafName);
        var upper = candidate.Trim().ToUpperInvariant();

        return upper switch
        {
            "CON" or "PRN" or "AUX" or "NUL" => true,
            _ => upper.Length == 4
                && ((upper.StartsWith("COM") || upper.StartsWith("LPT"))
                    && upper[3] >= '1' && upper[3] <= '9')
        };
    }
}

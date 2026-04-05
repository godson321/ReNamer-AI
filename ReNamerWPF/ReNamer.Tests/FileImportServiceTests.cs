using ReNamer.Models;
using ReNamer.Services;

namespace ReNamer.Tests;

public class FileImportServiceTests
{
    [Fact]
    public void BuildBatch_DeduplicatesExistingPathsAndCountsMissingPaths()
    {
        var root = CreateTempDirectory();
        try
        {
            var first = Path.Combine(root, "a.txt");
            var second = Path.Combine(root, "b.txt");
            var missing = Path.Combine(root, "missing.txt");
            File.WriteAllText(first, "a");
            File.WriteAllText(second, "b");

            var service = CreateService(
                includeAllFiles: false,
                includeFolderNames: false,
                includeSubfolders: false);

            var batch = service.BuildBatch(new[] { first, second, first, missing }, new[] { second }, traceId: 1);

            Assert.Single(batch.Items);
            Assert.Equal(first, batch.Items[0].FullPath);
            Assert.Equal(2, batch.DuplicateInputCount);
            Assert.Equal(1, batch.PathNotFoundCount);
            Assert.Equal(0, batch.SkippedCount);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void BuildBatch_DirectoryImport_PreservesFolderThenFileOrder()
    {
        var root = CreateTempDirectory();
        try
        {
            var child = Path.Combine(root, "child");
            Directory.CreateDirectory(child);

            var rootFile = Path.Combine(root, "root.txt");
            var childFile = Path.Combine(child, "nested.txt");
            File.WriteAllText(rootFile, "root");
            File.WriteAllText(childFile, "child");

            var service = CreateService(
                includeAllFiles: true,
                includeFolderNames: true,
                includeSubfolders: true);

            var batch = service.BuildBatch(new[] { root }, Array.Empty<string>(), traceId: 2);

            AssertBatchItems(
                batch.Items,
                (root, true),
                (child, true),
                (rootFile, false),
                (childFile, false));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void BuildBatch_DirectoryImport_RecursiveFlagDoesNotOverrideFolderOptions()
    {
        var root = CreateTempDirectory();
        try
        {
            var child = Path.Combine(root, "child");
            Directory.CreateDirectory(child);

            var rootFile = Path.Combine(root, "root.txt");
            var childFile = Path.Combine(child, "child.txt");
            File.WriteAllText(rootFile, "root");
            File.WriteAllText(childFile, "child");

            var service = CreateService(
                recursive: true,
                includeAllFiles: false,
                includeFolderNames: true,
                includeSubfolders: false);

            var batch = service.BuildBatch(new[] { root }, Array.Empty<string>(), traceId: 3);

            AssertBatchItems(batch.Items, (root, true));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void BuildBatch_DirectoryImport_IncludeSubfoldersWithoutFiles_AddsFoldersRecursivelyOnly()
    {
        var root = CreateTempDirectory();
        try
        {
            var child = Path.Combine(root, "child");
            var grandChild = Path.Combine(child, "grand");
            Directory.CreateDirectory(grandChild);

            File.WriteAllText(Path.Combine(root, "root.txt"), "root");
            File.WriteAllText(Path.Combine(child, "child.txt"), "child");
            File.WriteAllText(Path.Combine(grandChild, "grand.txt"), "grand");

            var service = CreateService(
                includeAllFiles: false,
                includeFolderNames: true,
                includeSubfolders: true);

            var batch = service.BuildBatch(new[] { root }, Array.Empty<string>(), traceId: 4);

            AssertBatchItems(
                batch.Items,
                (root, true),
                (child, true),
                (grandChild, true));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void BuildBatch_DirectoryImport_IgnoreRootFolder_KeepsFilesButSkipsRootFolderEntry()
    {
        var root = CreateTempDirectory();
        try
        {
            var child = Path.Combine(root, "child");
            Directory.CreateDirectory(child);

            var rootFile = Path.Combine(root, "root.txt");
            var childFile = Path.Combine(child, "child.txt");
            File.WriteAllText(rootFile, "root");
            File.WriteAllText(childFile, "child");

            var service = CreateService(
                includeAllFiles: true,
                includeFolderNames: true,
                includeSubfolders: true,
                ignoreRootFolder: true);

            var batch = service.BuildBatch(new[] { root }, Array.Empty<string>(), traceId: 5);

            AssertBatchItems(
                batch.Items,
                (child, true),
                (rootFile, false),
                (childFile, false));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void BuildBatch_DirectoryImport_IncludeFolderNamesDisabled_AddsOnlyFiles()
    {
        var root = CreateTempDirectory();
        try
        {
            var child = Path.Combine(root, "child");
            Directory.CreateDirectory(child);

            var rootFile = Path.Combine(root, "root.txt");
            var childFile = Path.Combine(child, "child.txt");
            File.WriteAllText(rootFile, "root");
            File.WriteAllText(childFile, "child");

            var service = CreateService(
                includeAllFiles: true,
                includeFolderNames: false,
                includeSubfolders: true);

            var batch = service.BuildBatch(new[] { root }, Array.Empty<string>(), traceId: 6);

            AssertBatchItems(
                batch.Items,
                (rootFile, false),
                (childFile, false));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void BuildBatch_DirectoryImport_HiddenAndSystemFlags_FilterEntriesIndependently()
    {
        var root = CreateTempDirectory();
        var hiddenFile = Path.Combine(root, "hidden.txt");
        var systemFile = Path.Combine(root, "system.txt");
        var hiddenSystemFile = Path.Combine(root, "hidden-system.txt");
        try
        {
            var normalFile = Path.Combine(root, "normal.txt");
            File.WriteAllText(normalFile, "normal");
            File.WriteAllText(hiddenFile, "hidden");
            File.WriteAllText(systemFile, "system");
            File.WriteAllText(hiddenSystemFile, "hidden-system");

            File.SetAttributes(hiddenFile, FileAttributes.Hidden);
            File.SetAttributes(systemFile, FileAttributes.System);
            File.SetAttributes(hiddenSystemFile, FileAttributes.Hidden | FileAttributes.System);

            var expectations = new[]
            {
                new { IncludeHidden = false, IncludeSystem = false, Expected = new[] { normalFile } },
                new { IncludeHidden = true, IncludeSystem = false, Expected = new[] { normalFile, hiddenFile } },
                new { IncludeHidden = false, IncludeSystem = true, Expected = new[] { normalFile, systemFile } },
                new { IncludeHidden = true, IncludeSystem = true, Expected = new[] { normalFile, hiddenFile, systemFile, hiddenSystemFile } }
            };

            foreach (var expectation in expectations)
            {
                var service = CreateService(
                    includeAllFiles: true,
                    includeFolderNames: false,
                    includeSubfolders: false,
                    includeHiddenFiles: expectation.IncludeHidden,
                    includeSystemFiles: expectation.IncludeSystem);

                var batch = service.BuildBatch(new[] { root }, Array.Empty<string>(), traceId: 7);
                Assert.Equal(
                    expectation.Expected.OrderBy(path => path, StringComparer.OrdinalIgnoreCase),
                    batch.Items.Select(item => item.FullPath).OrderBy(path => path, StringComparer.OrdinalIgnoreCase));
            }
        }
        finally
        {
            ResetFileAttributes(hiddenFile, systemFile, hiddenSystemFile);
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static FileImportService CreateService(
        bool recursive = false,
        bool includeAllFiles = false,
        bool includeFolderNames = true,
        bool includeSubfolders = false,
        bool includeHiddenFiles = true,
        bool includeSystemFiles = true,
        bool ignoreRootFolder = false,
        bool maskFileNameOnly = true,
        string includeMask = "",
        string excludeMask = "")
        => new(new FileImportOptions(
            Recursive: recursive,
            IncludeAllFiles: includeAllFiles,
            IncludeFolderNames: includeFolderNames,
            IncludeSubfolders: includeSubfolders,
            IncludeHiddenFiles: includeHiddenFiles,
            IncludeSystemFiles: includeSystemFiles,
            IgnoreRootFolder: ignoreRootFolder,
            MaskFileNameOnly: maskFileNameOnly,
            IncludeMask: includeMask,
            ExcludeMask: excludeMask));

    private static void AssertBatchItems(IReadOnlyList<RenFile> items, params (string Path, bool IsFolder)[] expected)
    {
        Assert.Equal(expected.Length, items.Count);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Path, items[i].FullPath);
            Assert.Equal(expected[i].IsFolder, items[i].IsFolder);
        }
    }

    private static void ResetFileAttributes(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (File.Exists(path))
                File.SetAttributes(path, FileAttributes.Normal);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "ReNamerTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}

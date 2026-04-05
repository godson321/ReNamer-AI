using System;
using System.IO;

namespace ReNamer.Models;

public readonly record struct RenFileImportData(
    string FullPath,
    string OriginalName,
    string FolderPath,
    string Extension,
    long Size,
    DateTime Created,
    DateTime Modified,
    bool IsFolder)
{
    public static RenFileImportData FromPathOrPlaceholder(string fullPath)
    {
        if (File.Exists(fullPath) || Directory.Exists(fullPath))
            return FromPath(fullPath);

        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentException("Path must not be empty.", nameof(fullPath));

        var normalizedPath = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (string.IsNullOrWhiteSpace(normalizedPath))
            normalizedPath = fullPath;

        return new RenFileImportData(
            normalizedPath,
            Path.GetFileName(normalizedPath),
            Path.GetDirectoryName(normalizedPath) ?? string.Empty,
            Path.GetExtension(normalizedPath),
            Size: 0,
            Created: default,
            Modified: default,
            IsFolder: fullPath.EndsWith(Path.DirectorySeparatorChar) || fullPath.EndsWith(Path.AltDirectorySeparatorChar));
    }

    public static RenFileImportData FromPath(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentException("Path must not be empty.", nameof(fullPath));

        var originalName = Path.GetFileName(fullPath);
        var folderPath = Path.GetDirectoryName(fullPath) ?? string.Empty;
        var extension = Path.GetExtension(fullPath);

        if (File.Exists(fullPath))
        {
            var info = new FileInfo(fullPath);
            return new RenFileImportData(
                fullPath,
                originalName,
                folderPath,
                extension,
                info.Length,
                info.CreationTime,
                info.LastWriteTime,
                IsFolder: false);
        }

        if (Directory.Exists(fullPath))
        {
            var info = new DirectoryInfo(fullPath);
            return new RenFileImportData(
                fullPath,
                originalName,
                folderPath,
                extension,
                Size: 0,
                Created: info.CreationTime,
                Modified: info.LastWriteTime,
                IsFolder: true);
        }

        throw new FileNotFoundException("Path does not exist.", fullPath);
    }
}

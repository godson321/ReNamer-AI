using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReNamer.Services;

/// <summary>
/// 应用程序设置 - 持久化到 %APPDATA%/ReNamer/settings.json
/// </summary>
public class AppSettings
{
    public static string? LastLoadError { get; private set; }
    public string? LastSaveError { get; private set; }
    [JsonIgnore]
    private FolderImportSnapshot _persistedFolderImport;

    public AppSettings()
    {
        _persistedFolderImport = CaptureFolderImportSnapshot();
    }

    // ─── General ───
    public bool LoadLastPreset { get; set; } = true;
    public bool RememberWindowPosition { get; set; } = true;
    public bool ConfirmOnExit { get; set; } = false;
    public bool ShowInSystemTray { get; set; } = false;
    public string Language { get; set; } = "zh-CN";
    public string Theme { get; set; } = "Light";

    // ─── Preview ───
    public bool AutoPreview { get; set; } = true;
    public bool PreviewOnFileAdd { get; set; } = false;
    public bool HighlightChanges { get; set; } = true;
    public bool AutoValidate { get; set; } = false;
    public bool WarnInvalidChars { get; set; } = true;
    public bool WarnLongPaths { get; set; } = true;

    // ─── Rename ───
    public bool ConfirmRename { get; set; } = true;
    public bool AutoRemoveRenamed { get; set; } = false;
    public bool CreateUndoLog { get; set; } = true;
    public bool ResolveMetaTags { get; set; } = true;
    public int ConflictResolution { get; set; } = 0; // 0=Skip, 1=AddSuffix, 2=Overwrite

    // ─── Filters ───
    public bool FiltersApplied { get; set; } = false;
    public bool FilterNameEnabled { get; set; } = false;
    public string FilterNamePattern { get; set; } = "";
    public bool FilterNameCaseSensitive { get; set; } = false;
    public bool FilterNameExclude { get; set; } = false;
    public bool FilterExtEnabled { get; set; } = false;
    public string FilterExtensions { get; set; } = "";
    public bool FilterSizeEnabled { get; set; } = false;
    public double FilterMinSize { get; set; } = 0;
    public int FilterMinSizeUnit { get; set; } = 0;
    public double FilterMaxSize { get; set; } = 0;
    public int FilterMaxSizeUnit { get; set; } = 0;
    public bool FilterAttrEnabled { get; set; } = false;
    public bool FilterAttrReadOnly { get; set; } = false;
    public bool FilterAttrHidden { get; set; } = false;
    public bool FilterAttrSystem { get; set; } = false;
    public bool FolderIncludeAllFiles { get; set; } = false;
    public bool FolderIncludeFolderNames { get; set; } = true;
    public bool FolderIncludeSubfolders { get; set; } = false;
    public bool FolderIncludeHiddenFiles { get; set; } = false;
    public bool FolderIncludeSystemFiles { get; set; } = false;
    public bool FolderIgnoreRootFolder { get; set; } = false;
    public string FolderIncludeMask { get; set; } = "";
    public string FolderExcludeMask { get; set; } = "";
    public bool FolderMaskFileNameOnly { get; set; } = false;
    public bool FolderSaveAsDefault { get; set; } = false;
    public bool FolderImportDefaultsInitialized { get; set; } = false;

    // ─── Window State ───
    public double WindowLeft { get; set; } = double.NaN;
    public double WindowTop { get; set; } = double.NaN;
    public double WindowWidth { get; set; } = 1200;
    public double WindowHeight { get; set; } = 720;
    public bool IsMaximized { get; set; } = false;
    public double SplitterPosition { get; set; } = 200;
    public Dictionary<string, double> ColumnWidths { get; set; } = new();
    public List<string> VisibleColumns { get; set; } = new();

    // ─── Last Preset ───
    public string LastPresetPath { get; set; } = "";

    // ─── Persistence ───
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReNamer");
    private static readonly string SettingsFile = Path.Combine(SettingsDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public static AppSettings Load()
    {
        LastLoadError = null;
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
                settings.CapturePersistedFolderImport();
                return settings;
            }
        }
        catch (Exception ex)
        {
            LastLoadError = ex.Message;
            Debug.WriteLine($"[AppSettings] Load failed: {ex}");
        }

        return new AppSettings();
    }

    public bool Save()
    {
        LastSaveError = null;
        var runtimeFolderImportSnapshot = CaptureFolderImportSnapshot();
        try
        {
            if (!Directory.Exists(SettingsDir))
                Directory.CreateDirectory(SettingsDir);

            if (!FolderSaveAsDefault)
                ApplyFolderImportSnapshot(_persistedFolderImport);

            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(SettingsFile, json);
            if (FolderSaveAsDefault)
                CapturePersistedFolderImport();
            return true;
        }
        catch (Exception ex)
        {
            LastSaveError = ex.Message;
            Debug.WriteLine($"[AppSettings] Save failed: {ex}");
            return false;
        }
        finally
        {
            if (!FolderSaveAsDefault)
                ApplyFolderImportSnapshot(runtimeFolderImportSnapshot);
        }
    }

    public bool ApplyFirstFolderImportDefaultsIfNeeded()
    {
        if (FolderImportDefaultsInitialized)
            return false;

        FolderImportDefaultsInitialized = true;

        if (!HasCustomizedFolderImportSettings())
            FolderIncludeAllFiles = true;

        return true;
    }

    private bool HasCustomizedFolderImportSettings()
    {
        return FolderIncludeAllFiles
            || !FolderIncludeFolderNames
            || FolderIncludeSubfolders
            || FolderIncludeHiddenFiles
            || FolderIncludeSystemFiles
            || FolderIgnoreRootFolder
            || !string.IsNullOrWhiteSpace(FolderIncludeMask)
            || !string.IsNullOrWhiteSpace(FolderExcludeMask)
            || FolderMaskFileNameOnly
            || FolderSaveAsDefault;
    }

    private void CapturePersistedFolderImport()
    {
        _persistedFolderImport = CaptureFolderImportSnapshot();
    }

    private FolderImportSnapshot CaptureFolderImportSnapshot()
    {
        return new FolderImportSnapshot(
            FolderIncludeAllFiles,
            FolderIncludeFolderNames,
            FolderIncludeSubfolders,
            FolderIncludeHiddenFiles,
            FolderIncludeSystemFiles,
            FolderIgnoreRootFolder,
            FolderIncludeMask,
            FolderExcludeMask,
            FolderMaskFileNameOnly);
    }

    private void ApplyFolderImportSnapshot(FolderImportSnapshot snapshot)
    {
        FolderIncludeAllFiles = snapshot.FolderIncludeAllFiles;
        FolderIncludeFolderNames = snapshot.FolderIncludeFolderNames;
        FolderIncludeSubfolders = snapshot.FolderIncludeSubfolders;
        FolderIncludeHiddenFiles = snapshot.FolderIncludeHiddenFiles;
        FolderIncludeSystemFiles = snapshot.FolderIncludeSystemFiles;
        FolderIgnoreRootFolder = snapshot.FolderIgnoreRootFolder;
        FolderIncludeMask = snapshot.FolderIncludeMask;
        FolderExcludeMask = snapshot.FolderExcludeMask;
        FolderMaskFileNameOnly = snapshot.FolderMaskFileNameOnly;
    }

    private readonly record struct FolderImportSnapshot(
        bool FolderIncludeAllFiles,
        bool FolderIncludeFolderNames,
        bool FolderIncludeSubfolders,
        bool FolderIncludeHiddenFiles,
        bool FolderIncludeSystemFiles,
        bool FolderIgnoreRootFolder,
        string FolderIncludeMask,
        string FolderExcludeMask,
        bool FolderMaskFileNameOnly);
}

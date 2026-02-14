using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReNamer.Services;

/// <summary>
/// 应用程序设置 - 持久化到 %APPDATA%/ReNamer/settings.json
/// </summary>
public class AppSettings
{
    // ─── General ───
    public bool LoadLastPreset { get; set; } = false;
    public bool RememberWindowPosition { get; set; } = true;
    public bool CheckForUpdates { get; set; } = false;
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
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch { /* Return defaults on error */ }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            if (!Directory.Exists(SettingsDir))
                Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(SettingsFile, json);
        }
        catch { /* Silently fail */ }
    }
}

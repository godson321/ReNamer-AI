using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using ReNamer.Models;
using ReNamer.Rules;
using ReNamer.Services;

namespace ReNamer.Views;

/// <summary>
/// 规则序号转换器 - 将 ListViewItem 转为 1-based 序号
/// </summary>
public class IndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int alternationIndex)
            return (alternationIndex + 1).ToString(CultureInfo.InvariantCulture);

        if (value is ListViewItem item)
        {
            var lv = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            if (lv != null)
                return (lv.ItemContainerGenerator.IndexFromContainer(item) + 1).ToString(CultureInfo.InvariantCulture);
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public partial class MainWindow : Window
{
    private readonly RenameService _renameService;
    private readonly AppSettings _appSettings;
    private Point _rulesDragStartPoint;
    private IRule? _draggedRule;
    private Point _filesDragStartPoint;
    private RenFile? _draggedFile;
    private readonly List<UiAction> _actions = new();
    private readonly object _previewSync = new();
    private CancellationTokenSource? _previewCts;
    private int _previewRequestVersion;
    private readonly object _renameSync = new();
    private CancellationTokenSource? _renameCts;
    private int _renameRequestVersion;
    private const int AutoPreviewDebounceMilliseconds = 200;
    private readonly Dictionary<string, double> _defaultColumnWidths = new(StringComparer.OrdinalIgnoreCase);
    private static readonly string UndoLogsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ReNamer",
        "UndoLogs");

    // Actions (commands)
    public UiAction CmdNewProject { get; private set; } = null!;
    public UiAction CmdNewInstance { get; private set; } = null!;
    public UiAction CmdUndoRename { get; private set; } = null!;
    public UiAction CmdAddFiles { get; private set; } = null!;
    public UiAction CmdAddFolders { get; private set; } = null!;
    public UiAction CmdAddPaths { get; private set; } = null!;
    public UiAction CmdPasteFiles { get; private set; } = null!;
    public UiAction CmdPreview { get; private set; } = null!;
    public UiAction CmdRename { get; private set; } = null!;
    public UiAction CmdExit { get; private set; } = null!;
    public UiAction CmdLoadPreset { get; private set; } = null!;
    public UiAction CmdSavePreset { get; private set; } = null!;
    public UiAction CmdSavePresetAs { get; private set; } = null!;
    public UiAction CmdManagePresets { get; private set; } = null!;
    public UiAction CmdBrowsePresets { get; private set; } = null!;
    public UiAction CmdImportPreset { get; private set; } = null!;
    public UiAction CmdRescanPresets { get; private set; } = null!;
    public UiAction CmdCreatePresetLinks { get; private set; } = null!;
    public UiAction CmdSettings { get; private set; } = null!;
    public UiAction CmdSettingsGeneral { get; private set; } = null!;
    public UiAction CmdSettingsPreview { get; private set; } = null!;
    public UiAction CmdSettingsRename { get; private set; } = null!;
    public UiAction CmdSettingsMetaTags { get; private set; } = null!;
    public UiAction CmdSettingsMisc { get; private set; } = null!;
    public UiAction CmdSettingsFilters { get; private set; } = null!;
    public UiAction CmdHelpOnline { get; private set; } = null!;
    public UiAction CmdQuickGuide { get; private set; } = null!;
    public UiAction CmdUserManual { get; private set; } = null!;
    public UiAction CmdForum { get; private set; } = null!;
    public UiAction CmdLiteVsPro { get; private set; } = null!;
    public UiAction CmdPurchase { get; private set; } = null!;
    public UiAction CmdRegister { get; private set; } = null!;
    public UiAction CmdUnregister { get; private set; } = null!;
    public UiAction CmdVersionHistory { get; private set; } = null!;
    public UiAction CmdCopyrights { get; private set; } = null!;
    public UiAction CmdAbout { get; private set; } = null!;
    public UiAction CmdAddRule { get; private set; } = null!;
    public UiAction CmdRemoveRule { get; private set; } = null!;
    public UiAction CmdMoveRuleUp { get; private set; } = null!;
    public UiAction CmdMoveRuleDown { get; private set; } = null!;
    public UiAction CmdRuleAddAbove { get; private set; } = null!;
    public UiAction CmdRuleAddBelow { get; private set; } = null!;
    public UiAction CmdRuleEdit { get; private set; } = null!;
    public UiAction CmdRuleDuplicate { get; private set; } = null!;
    public UiAction CmdRuleRemoveAll { get; private set; } = null!;
    public UiAction CmdRuleExport { get; private set; } = null!;
    public UiAction CmdRuleComment { get; private set; } = null!;
    public UiAction CmdRulesSelectAll { get; private set; } = null!;
    public UiAction CmdRulesMarkAll { get; private set; } = null!;
    public UiAction CmdRulesUnmarkAll { get; private set; } = null!;
    public UiAction CmdFilters { get; private set; } = null!;
    public UiAction CmdOptions { get; private set; } = null!;
    public UiAction CmdAnalyze { get; private set; } = null!;
    public UiAction CmdValidate { get; private set; } = null!;
    public UiAction CmdAutosizeColumns { get; private set; } = null!;
    public UiAction CmdFixConflicts { get; private set; } = null!;
    public UiAction CmdAnalyzeSampleText { get; private set; } = null!;
    public UiAction CmdApplyRulesToClipboard { get; private set; } = null!;
    public UiAction CmdCountFiles { get; private set; } = null!;
    public UiAction CmdSortForFolders { get; private set; } = null!;
    public UiAction CmdEditNewName { get; private set; } = null!;
    public UiAction CmdOpenFile { get; private set; } = null!;
    public UiAction CmdOpenFolder { get; private set; } = null!;
    public UiAction CmdOpenWithNotepad { get; private set; } = null!;
    public UiAction CmdFileProperties { get; private set; } = null!;
    public UiAction CmdCutFiles { get; private set; } = null!;
    public UiAction CmdCopyFiles { get; private set; } = null!;
    public UiAction CmdDeleteRecycle { get; private set; } = null!;
    public UiAction CmdMarkSelected { get; private set; } = null!;
    public UiAction CmdUnmarkSelected { get; private set; } = null!;
    public UiAction CmdInvertMarking { get; private set; } = null!;
    public UiAction CmdMarkChangedIncCase { get; private set; } = null!;
    public UiAction CmdMarkChangedExcCase { get; private set; } = null!;
    public UiAction CmdMarkByMask { get; private set; } = null!;
    public UiAction CmdMarkOnlySelected { get; private set; } = null!;
    public UiAction CmdClearAll { get; private set; } = null!;
    public UiAction CmdClearRenamed { get; private set; } = null!;
    public UiAction CmdClearFailed { get; private set; } = null!;
    public UiAction CmdClearValid { get; private set; } = null!;
    public UiAction CmdClearInvalid { get; private set; } = null!;
    public UiAction CmdClearNotChanged { get; private set; } = null!;
    public UiAction CmdClearMarked { get; private set; } = null!;
    public UiAction CmdClearNotMarked { get; private set; } = null!;
    public UiAction CmdFilesSelectAll { get; private set; } = null!;
    public UiAction CmdInvertSelection { get; private set; } = null!;
    public UiAction CmdSelectByNameLength { get; private set; } = null!;
    public UiAction CmdSelectByExtension { get; private set; } = null!;
    public UiAction CmdSelectByMask { get; private set; } = null!;
    public UiAction CmdMoveFileUp { get; private set; } = null!;
    public UiAction CmdMoveFileDown { get; private set; } = null!;
    public UiAction CmdRemoveSelectedFiles { get; private set; } = null!;

    public ObservableCollection<RenFile> Files { get; } = new();
    public ObservableCollection<IRule> Rules { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        _renameService = new RenameService();
        _appSettings = AppSettings.Load();
        RuleHelpers.ResolveMetaTagsEnabled = _appSettings.ResolveMetaTags;
        ThemeService.ApplyTheme(_appSettings.Theme);

        lvFiles.ItemsSource = Files;
        lvRules.ItemsSource = Rules;

        Files.CollectionChanged += (_, _) => UpdateStatusBar();
        Rules.CollectionChanged += Rules_CollectionChanged;

        foreach (var rule in Rules)
            rule.PropertyChanged += Rule_PropertyChanged;

        LanguageService.LanguageChanged += OnLanguageChanged;
        UpdateLanguageMenuChecks();
        InitActions();

        // Hide extra columns by default (indices 8+: Folder, NewPath, SizeKB, SizeMB, etc.)
        Loaded += (_, _) =>
        {
            HideExtraColumns();
            RestoreWindowState();
            // Use the "as-opened" widths as reset baseline (includes restored user layout).
            CaptureDefaultColumnWidths();
            EnsureWindowVisible();
            PopulatePresetMenu();
            PopulateLanguageMenu();
        };
    }

    private void Rules_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (var rule in e.OldItems.OfType<IRule>())
                rule.PropertyChanged -= Rule_PropertyChanged;
        }

        if (e.NewItems != null)
        {
            foreach (var rule in e.NewItems.OfType<IRule>())
                rule.PropertyChanged += Rule_PropertyChanged;
        }

        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
        {
            foreach (var rule in Rules)
            {
                rule.PropertyChanged -= Rule_PropertyChanged;
                rule.PropertyChanged += Rule_PropertyChanged;
            }
        }

        RefreshActions();
    }

    private void Rule_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => Rule_PropertyChanged(sender, e));
            return;
        }

        if (sender is not JavaScriptRule rule || !rule.IsEnabled)
            return;

        if (e.PropertyName == nameof(JavaScriptRule.IsScriptLoading) && rule.IsScriptLoading)
        {
            tbStatusInfo.Text = $"脚本“{rule.ScriptDisplayName}”后台加载中，完成后将自动刷新预览。";
            return;
        }

        if (e.PropertyName == nameof(JavaScriptRule.IsScriptReady) && rule.IsScriptReady)
        {
            _ = ExecutePreviewAsync(isAutoTrigger: true, debounceMilliseconds: 0);
            return;
        }

        if (e.PropertyName == nameof(JavaScriptRule.ScriptLoadError) && !string.IsNullOrWhiteSpace(rule.ScriptLoadError))
            tbStatusInfo.Text = $"脚本“{rule.ScriptDisplayName}”加载失败：{rule.ScriptLoadError}";
    }

    private void InitActions()
    {
        CmdNewProject = CreateAction(_ => NewProject_Click(this, new RoutedEventArgs()));
        CmdNewInstance = CreateAction(_ => NewInstance_Click());
        CmdUndoRename = CreateAction(_ => UndoRename_Click(this, new RoutedEventArgs()), _ => HasRenamedFiles);
        CmdAddFiles = CreateAction(_ => AddFiles_Click(this, new RoutedEventArgs()));
        CmdAddFolders = CreateAction(_ => AddFolders_Click(this, new RoutedEventArgs()));
        CmdAddPaths = CreateAction(_ => AddPaths_Click(this, new RoutedEventArgs()));
        CmdPasteFiles = CreateAction(_ => PasteFiles_Click());
        CmdPreview = CreateAction(_ => Preview_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdRename = CreateAction(_ => Rename_Click(this, new RoutedEventArgs()), _ => CanRename);
        CmdExit = CreateAction(_ => Exit_Click(this, new RoutedEventArgs()));

        CmdLoadPreset = CreateAction(_ => LoadPreset_Click(this, new RoutedEventArgs()));
        CmdSavePreset = CreateAction(_ => SavePreset_Click(this, new RoutedEventArgs()), _ => HasRules);
        CmdSavePresetAs = CreateAction(_ => SavePresetAs_Click(this, new RoutedEventArgs()), _ => HasRules);
        CmdManagePresets = CreateAction(_ => ManagePresets_Click(this, new RoutedEventArgs()));
        CmdBrowsePresets = CreateAction(_ => BrowsePresets_Click(this, new RoutedEventArgs()));
        CmdImportPreset = CreateAction(_ => ImportPreset_Click(this, new RoutedEventArgs()));
        CmdRescanPresets = CreateAction(_ => RescanPresets_Click(this, new RoutedEventArgs()));
        CmdCreatePresetLinks = CreateAction(_ => CreatePresetLinks_Click(this, new RoutedEventArgs()));

        CmdSettings = CreateAction(_ => OpenSettingsTab(SettingsTab.General));
        CmdSettingsGeneral = CreateAction(_ => OpenSettingsTab(SettingsTab.General));
        CmdSettingsPreview = CreateAction(_ => OpenSettingsTab(SettingsTab.Preview));
        CmdSettingsRename = CreateAction(_ => OpenSettingsTab(SettingsTab.Rename));
        CmdSettingsMetaTags = CreateAction(_ => OpenSettingsTab(SettingsTab.Rename));
        CmdSettingsMisc = CreateAction(_ => OpenSettingsTab(SettingsTab.Misc));
        CmdSettingsFilters = CreateAction(_ => Filters_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdHelpOnline = CreateAction(_ => HelpOnline_Click(this, new RoutedEventArgs()));
        CmdQuickGuide = CreateAction(_ => QuickGuide_Click(this, new RoutedEventArgs()));
        CmdUserManual = CreateAction(_ => UserManual_Click(this, new RoutedEventArgs()));
        CmdForum = CreateAction(_ => Forum_Click(this, new RoutedEventArgs()));
        CmdLiteVsPro = CreateAction(_ => LiteVsPro_Click(this, new RoutedEventArgs()));
        CmdPurchase = CreateAction(_ => Purchase_Click(this, new RoutedEventArgs()));
        CmdRegister = CreateAction(_ => Register_Click(this, new RoutedEventArgs()));
        CmdUnregister = CreateAction(_ => Unregister_Click(this, new RoutedEventArgs()));
        CmdVersionHistory = CreateAction(_ => VersionHistory_Click(this, new RoutedEventArgs()));
        CmdCopyrights = CreateAction(_ => Copyrights_Click(this, new RoutedEventArgs()));
        CmdAbout = CreateAction(_ => About_Click(this, new RoutedEventArgs()));

        CmdAddRule = CreateAction(_ => AddRule_Click(this, new RoutedEventArgs()));
        CmdRemoveRule = CreateAction(_ => RemoveRule_Click(this, new RoutedEventArgs()), _ => HasSelectedRule);
        CmdMoveRuleUp = CreateAction(_ => MoveRuleUp_Click(this, new RoutedEventArgs()), _ => CanMoveRuleUp);
        CmdMoveRuleDown = CreateAction(_ => MoveRuleDown_Click(this, new RoutedEventArgs()), _ => CanMoveRuleDown);
        CmdRuleAddAbove = CreateAction(_ => AddRuleAbove_Click(this, new RoutedEventArgs()), _ => HasSelectedRule);
        CmdRuleAddBelow = CreateAction(_ => AddRuleBelow_Click(this, new RoutedEventArgs()), _ => HasSelectedRule);
        CmdRuleEdit = CreateAction(_ => EditRule_Click(this, new RoutedEventArgs()), _ => HasSelectedRule);
        CmdRuleDuplicate = CreateAction(_ => DuplicateRule_Click(this, new RoutedEventArgs()), _ => HasSelectedRule);
        CmdRuleRemoveAll = CreateAction(_ => RemoveAllRules_Click(this, new RoutedEventArgs()), _ => HasRules);
        CmdRuleExport = CreateAction(_ => ExportRuleToClipboard_Click(this, new RoutedEventArgs()), _ => HasSelectedRule);
        CmdRuleComment = CreateAction(_ => RuleComment_Click(this, new RoutedEventArgs()), _ => HasSelectedRule);
        CmdRulesSelectAll = CreateAction(_ => RulesSelectAll_Click(this, new RoutedEventArgs()), _ => HasRules);
        CmdRulesMarkAll = CreateAction(_ => RulesMarkAll_Click(this, new RoutedEventArgs()), _ => HasRules);
        CmdRulesUnmarkAll = CreateAction(_ => RulesUnmarkAll_Click(this, new RoutedEventArgs()), _ => HasRules);

        CmdFilters = CreateAction(_ => Filters_Click(this, new RoutedEventArgs()));
        CmdOptions = CreateAction(_ => Options_Click(this, new RoutedEventArgs()));
        CmdAnalyze = CreateAction(_ => Analyze_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
        CmdValidate = CreateAction(_ => Validate_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdAutosizeColumns = CreateAction(_ => AutosizeColumns());
        CmdFixConflicts = CreateAction(_ => FixConflictingNames(), _ => HasFiles);
        CmdAnalyzeSampleText = CreateAction(_ => AnalyzeSampleText(), _ => HasRules);
        CmdApplyRulesToClipboard = CreateAction(_ => ApplyRulesToClipboard(), _ => HasRules);
        CmdCountFiles = CreateAction(_ => CountFiles(), _ => HasFiles);
        CmdSortForFolders = CreateAction(_ => SortForFolders(), _ => HasFiles);

        CmdEditNewName = CreateAction(_ => EditNewName_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
        CmdOpenFile = CreateAction(_ => OpenFile_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
        CmdOpenFolder = CreateAction(_ => OpenFolder_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
        CmdOpenWithNotepad = CreateAction(_ => OpenWithNotepad_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
        CmdFileProperties = CreateAction(_ => FileProperties_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
        CmdCutFiles = CreateAction(_ => CutFilesToClipboard_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
        CmdCopyFiles = CreateAction(_ => CopyFilesToClipboard_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
        CmdDeleteRecycle = CreateAction(_ => DeleteToRecycleBin_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);

        CmdMarkSelected = CreateAction(_ => MarkSelected_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
        CmdUnmarkSelected = CreateAction(_ => UnmarkSelected_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
        CmdInvertMarking = CreateAction(_ => InvertMarking_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdMarkChangedIncCase = CreateAction(_ => MarkChangedIncCase_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdMarkChangedExcCase = CreateAction(_ => MarkChangedExcCase_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdMarkByMask = CreateAction(_ => MarkByMask_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdMarkOnlySelected = CreateAction(_ => MarkOnlySelected_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);

        CmdClearAll = CreateAction(_ => ClearAll_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdClearRenamed = CreateAction(_ => ClearRenamed_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdClearFailed = CreateAction(_ => ClearFailed_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdClearValid = CreateAction(_ => ClearValid_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdClearInvalid = CreateAction(_ => ClearInvalid_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdClearNotChanged = CreateAction(_ => ClearNotChanged_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdClearMarked = CreateAction(_ => ClearMarked_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdClearNotMarked = CreateAction(_ => ClearNotMarked_Click(this, new RoutedEventArgs()), _ => HasFiles);

        CmdFilesSelectAll = CreateAction(_ => FilesSelectAll_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdInvertSelection = CreateAction(_ => InvertSelection_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdSelectByNameLength = CreateAction(_ => SelectByNameLength_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdSelectByExtension = CreateAction(_ => SelectByExtension_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdSelectByMask = CreateAction(_ => SelectByMask_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdMoveFileUp = CreateAction(_ => MoveFileUp_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
        CmdMoveFileDown = CreateAction(_ => MoveFileDown_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
        CmdRemoveSelectedFiles = CreateAction(_ => RemoveSelectedFiles_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles);
    }

    private UiAction CreateAction(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        var action = new UiAction(execute, canExecute);
        _actions.Add(action);
        return action;
    }

    private void RefreshActions()
    {
        foreach (var action in _actions)
            action.RaiseCanExecuteChanged();
    }

    private bool HasFiles => Files.Count > 0;
    private bool HasRules => Rules.Count > 0;
    private bool HasSelectedRule => lvRules.SelectedItem != null;
    private bool HasSelectedFiles => lvFiles.SelectedItems.Count > 0;
    private bool HasRenamedFiles => Files.Any(f => f.IsRenamed);
    private bool CanRename => Files.Any(f => f.IsMarked && f.HasChanged && !f.IsRenamed);
    private bool CanMoveRuleUp => lvRules.SelectedIndex > 0;
    private bool CanMoveRuleDown => lvRules.SelectedIndex >= 0 && lvRules.SelectedIndex < Rules.Count - 1;

    private static readonly HashSet<string> DefaultVisibleColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "IsMarked", "State", "OriginalName", "NewName", "Error"
    };

    private void HideExtraColumns()
    {
        foreach (var col in lvFiles.Columns)
        {
            var key = GetColumnKey(col);
            if (!DefaultVisibleColumns.Contains(key))
                col.Visibility = Visibility.Collapsed;
        }
    }

    private static string GetColumnKey(DataGridColumn col)
    {
        if (!string.IsNullOrWhiteSpace(col.SortMemberPath))
            return col.SortMemberPath;

        if (col.Header is string s)
            return s;

        if (col.Header is TextBlock tb && !string.IsNullOrWhiteSpace(tb.Text))
            return tb.Text;

        return col.Header?.ToString() ?? string.Empty;
    }

    private void CaptureDefaultColumnWidths()
    {
        _defaultColumnWidths.Clear();
        foreach (var col in lvFiles.Columns)
        {
            var key = GetColumnKey(col);
            if (string.IsNullOrEmpty(key))
                continue;

            var width = col.Width.DisplayValue;
            if (double.IsNaN(width) || width <= 0)
                width = col.ActualWidth;

            if (width > 0)
                _defaultColumnWidths[key] = width;
        }
    }

    private void OnLanguageChanged() => UpdateLanguageMenuChecks();

    private void UpdateLanguageMenuChecks()
    {
        foreach (MenuItem item in menuLanguage.Items.OfType<MenuItem>())
        {
            item.IsChecked = (item.Tag as string) == LanguageService.CurrentCulture;
        }
    }

    private void PopulateLanguageMenu()
    {
        menuLanguage.Items.Clear();
        foreach (var culture in LanguageService.GetAvailableCultures())
        {
            var info = new CultureInfo(culture);
            var flag = GetFlagEmoji(culture);
            var mi = new MenuItem
            {
                Header = $"{flag} {info.NativeName}",
                Tag = culture,
                IsCheckable = true
            };
            mi.Click += LanguageMenuItem_Click;
            menuLanguage.Items.Add(mi);
        }
        UpdateLanguageMenuChecks();
    }

    private void LanguageMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item && item.Tag is string culture)
        {
            LanguageService.SetLanguage(culture);
        }
    }

    private static string GetFlagEmoji(string culture)
    {
        try
        {
            var parts = culture.Split('-');
            var region = parts.Length > 1 ? parts[1] : parts[0];
            region = region.ToUpperInvariant();
            if (region.Length == 2)
            {
                int offset = 0x1F1E6;
                return string.Concat(
                    char.ConvertFromUtf32(offset + region[0] - 'A'),
                    char.ConvertFromUtf32(offset + region[1] - 'A'));
            }
        }
        catch { /* ignore */ }
        return "🏳️";
    }

    private void SwitchToChinese_Click(object sender, RoutedEventArgs e) => LanguageService.SwitchToChinese();
    private void SwitchToEnglish_Click(object sender, RoutedEventArgs e) => LanguageService.SwitchToEnglish();

    #region Status Bar

    private void UpdateStatusBar()
    {
        var total = Files.Count;
        var marked = Files.Count(f => f.IsMarked);
        var selected = lvFiles.SelectedItems.Count;
        tbStatusFiles.Text = LanguageService.GetString("Status_FilesSummary", total, marked, selected);
        RefreshActions();
    }

    private void StatusBar_MouseMove(object sender, MouseEventArgs e)
    {
        if (string.IsNullOrEmpty(tbStatusInfo.Text))
            tbStatusInfo.Text = LanguageService.GetString("Status_Ready");
    }

    private void StatusBar_Click(object sender, MouseButtonEventArgs e)
    {
        tbStatusInfo.Text = $"Updated: {DateTime.Now:HH:mm:ss}";
    }

    #endregion

    #region Global Key Handling
    private bool TryExecute(UiAction action)
    {
        if (action.CanExecute(null))
        {
            action.Execute(null);
            return true;
        }
        return false;
    }

    private void BeginProgress(string label)
    {
        tbStatusProgress.Text = label;
        pbProgress.Value = 0;
        pbProgress.Visibility = Visibility.Visible;
    }

    private void UpdateProgress(string label, int current, int total)
    {
        if (total <= 0) return;
        int percent = (int)(current * 100.0 / total);
        pbProgress.Value = percent;
        tbStatusProgress.Text = $"{label}: {percent}%";
    }

    private static bool ShouldReportProgress(int current, int total, int lastReported, int reportStep)
    {
        if (total <= 0 || current <= lastReported)
            return false;

        return current == total || current - lastReported >= reportStep;
    }

    private void EndProgress()
    {
        pbProgress.Value = 0;
        pbProgress.Visibility = Visibility.Collapsed;
        tbStatusProgress.Text = LanguageService.GetString("Status_Ready");
    }

    #endregion

    #region File Operations

    private void AddFiles_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Multiselect = true, Title = "Select Files to Rename" };
        if (dialog.ShowDialog() == true)
        {
            AddFilePaths(dialog.FileNames, recursive: false);
        }
    }

    private void AddFolders_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog { Description = "Select Folder" };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            AddFilePaths(new[] { dialog.SelectedPath }, recursive: true);
        }
    }

    private void AddPaths_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new TextInputDialog(
            LanguageService.GetString("Dialog_AddPathsTitle"),
            LanguageService.GetString("Dialog_AddPathsPrompt"),
            isMultiline: true)
        {
            Owner = this
        };

        if (dlg.ShowDialog() == true)
        {
            var lines = dlg.InputText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l));
            AddFilePaths(lines, recursive: true);
        }
    }

    internal void AddFilePaths(IEnumerable<string> paths, bool recursive)
    {
        var existing = new HashSet<string>(Files.Select(f => f.FullPath), StringComparer.OrdinalIgnoreCase);
        var added = new List<RenFile>();
        int skippedCount = 0;

        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path))
                continue;

            try
            {
                if (File.Exists(path) && !existing.Contains(path))
                {
                    var rf = new RenFile(path);
                    Files.Add(rf);
                    added.Add(rf);
                    existing.Add(path);
                }
                else if (Directory.Exists(path))
                {
                    EnsureFolderImportDefaultsForFirstUse();
                    AddDirectoryEntries(path, recursive, existing, added, ref skippedCount);
                }
            }
            catch (UnauthorizedAccessException)
            {
                skippedCount++;
            }
            catch (IOException)
            {
                skippedCount++;
            }
            catch (Exception ex)
            {
                skippedCount++;
                Debug.WriteLine($"AddFilePaths skipped '{path}': {ex.Message}");
            }
        }

        if (_appSettings.FiltersApplied && added.Count > 0)
        {
            ApplyFiltersToFiles(added);
        }

        if (added.Count > 0 && _appSettings.PreviewOnFileAdd)
        {
            TryExecute(CmdPreview);
        }

        if (skippedCount > 0)
        {
            tbStatusInfo.Text = $"Skipped {skippedCount} path(s) due to access restrictions.";
        }
    }

    private void EnsureFolderImportDefaultsForFirstUse()
    {
        if (!_appSettings.ApplyFirstFolderImportDefaultsIfNeeded())
            return;

        _appSettings.Save();
    }

    private void AddDirectoryEntries(string rootPath, bool recursive, HashSet<string> existing, List<RenFile> added, ref int skippedCount)
    {
        var allowSubfolders = recursive && _appSettings.FolderIncludeSubfolders;
        var searchOption = allowSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        if (_appSettings.FolderIncludeFolderNames)
        {
            List<string> dirs;
            if (allowSubfolders)
            {
                dirs = SafeEnumerateDirectories(rootPath, searchOption, ref skippedCount).ToList();
                if (!_appSettings.FolderIgnoreRootFolder)
                    dirs.Insert(0, rootPath);
            }
            else
            {
                dirs = _appSettings.FolderIgnoreRootFolder ? [] : [rootPath];
            }

            foreach (var dir in dirs)
            {
                if (!PassesFolderImportSettings(dir, isFolder: true))
                    continue;
                if (existing.Contains(dir))
                    continue;

                var rf = new RenFile(dir);
                Files.Add(rf);
                added.Add(rf);
                existing.Add(dir);
            }
        }

        if (_appSettings.FolderIncludeAllFiles)
        {
            foreach (var file in SafeEnumerateFiles(rootPath, searchOption, ref skippedCount))
            {
                if (!PassesFolderImportSettings(file, isFolder: false))
                    continue;
                if (existing.Contains(file))
                    continue;

                var rf = new RenFile(file);
                Files.Add(rf);
                added.Add(rf);
                existing.Add(file);
            }
        }
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string path, SearchOption searchOption, ref int skippedCount)
    {
        try
        {
            return Directory.EnumerateDirectories(path, "*", searchOption).ToList();
        }
        catch (UnauthorizedAccessException)
        {
            skippedCount++;
            return Enumerable.Empty<string>();
        }
        catch (IOException)
        {
            skippedCount++;
            return Enumerable.Empty<string>();
        }
    }

    private static IEnumerable<string> SafeEnumerateFiles(string path, SearchOption searchOption, ref int skippedCount)
    {
        try
        {
            return Directory.EnumerateFiles(path, "*", searchOption).ToList();
        }
        catch (UnauthorizedAccessException)
        {
            skippedCount++;
            return Enumerable.Empty<string>();
        }
        catch (IOException)
        {
            skippedCount++;
            return Enumerable.Empty<string>();
        }
    }

    private bool PassesFolderImportSettings(string fullPath, bool isFolder)
    {
        if (!PassesFolderImportAttributes(fullPath))
            return false;
        return PassesFolderImportMasks(fullPath, isFolder);
    }

    private bool PassesFolderImportAttributes(string fullPath)
    {
        try
        {
            var attrs = File.GetAttributes(fullPath);
            if (!_appSettings.FolderIncludeHiddenFiles && attrs.HasFlag(FileAttributes.Hidden))
                return false;
            if (!_appSettings.FolderIncludeSystemFiles && attrs.HasFlag(FileAttributes.System))
                return false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool PassesFolderImportMasks(string fullPath, bool isFolder)
    {
        var target = _appSettings.FolderMaskFileNameOnly ? Path.GetFileName(fullPath) : fullPath;
        if (string.IsNullOrEmpty(target))
            return false;

        var includeMasks = (_appSettings.FolderIncludeMask ?? string.Empty)
            .Split(';')
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToArray();
        var excludeMasks = (_appSettings.FolderExcludeMask ?? string.Empty)
            .Split(';')
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToArray();

        if (includeMasks.Length > 0 && !includeMasks.Any(mask => MatchWildcard(target, mask, false)))
            return false;

        if (excludeMasks.Any(mask => MatchWildcard(target, mask, false)))
            return false;

        return true;
    }

    private void ApplyFiltersToFiles(IEnumerable<RenFile> files)
    {
        foreach (var file in files)
        {
            file.IsMarked = PassesFilters(file);
        }
        lvFiles.Items.Refresh();
        UpdateStatusBar();
    }

    private bool PassesFilters(RenFile file)
    {
        bool pass = true;

        if (_appSettings.FilterNameEnabled && !string.IsNullOrEmpty(_appSettings.FilterNamePattern))
        {
            bool matches = MatchWildcard(file.OriginalName, _appSettings.FilterNamePattern, _appSettings.FilterNameCaseSensitive);
            pass = _appSettings.FilterNameExclude ? !matches : matches;
        }

        if (pass && _appSettings.FilterExtEnabled && !string.IsNullOrEmpty(_appSettings.FilterExtensions))
        {
            var exts = _appSettings.FilterExtensions.Split(';', ',')
                .Select(x => x.Trim().ToLowerInvariant())
                .Where(x => x.Length > 0)
                .ToHashSet();
            pass = exts.Contains(file.Extension.ToLowerInvariant());
        }

        if (pass && _appSettings.FilterSizeEnabled)
        {
            long minBytes = ParseSize(_appSettings.FilterMinSize, _appSettings.FilterMinSizeUnit);
            long maxBytes = ParseSize(_appSettings.FilterMaxSize, _appSettings.FilterMaxSizeUnit);
            if (minBytes > 0) pass = file.Size >= minBytes;
            if (pass && maxBytes > 0) pass = file.Size <= maxBytes;
        }

        if (pass && _appSettings.FilterAttrEnabled)
        {
            try
            {
                var attrs = File.GetAttributes(file.FullPath);
                if (_appSettings.FilterAttrReadOnly && !attrs.HasFlag(FileAttributes.ReadOnly)) pass = false;
                if (_appSettings.FilterAttrHidden && !attrs.HasFlag(FileAttributes.Hidden)) pass = false;
                if (_appSettings.FilterAttrSystem && !attrs.HasFlag(FileAttributes.System)) pass = false;
            }
            catch { pass = false; }
        }

        return pass;
    }

    private static bool MatchWildcard(string input, string pattern, bool caseSensitive)
    {
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        return Regex.IsMatch(input, regexPattern, options);
    }

    private static long ParseSize(double value, int unitIndex)
    {
        if (value <= 0) return 0;
        return unitIndex switch
        {
            0 => (long)value,             // B
            1 => (long)(value * 1024),    // KB
            2 => (long)(value * 1048576), // MB
            3 => (long)(value * 1073741824), // GB
            _ => 0
        };
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var paths = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            AddFilePaths(paths, recursive: true);
        }
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    #endregion

    #region Rule Operations

    private void AddRule_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddRuleDialog();
        if (dialog.ShowDialog() == true && dialog.SelectedRule != null)
        {
            Rules.Add(dialog.SelectedRule);
            AutoPreviewIfEnabled(sender, e);
        }
    }

    private void EditRule_Click(object sender, RoutedEventArgs e)
    {
        if (lvRules.SelectedItem is IRule rule)
        {
            var oldIndex = Rules.IndexOf(rule);
            var dialog = new AddRuleDialog(rule);
            if (dialog.ShowDialog() == true)
            {
                if (dialog.SelectedRule != null && !ReferenceEquals(dialog.SelectedRule, rule) && oldIndex >= 0)
                {
                    Rules[oldIndex] = dialog.SelectedRule;
                    lvRules.SelectedIndex = oldIndex;
                }
                lvRules.Items.Refresh();
                AutoPreviewIfEnabled(sender, e);
            }
        }
    }

    private void DuplicateRule_Click(object sender, RoutedEventArgs e)
    {
        if (lvRules.SelectedItem is IRule rule)
        {
            // Clone rule via JSON serialization
            var clonedRule = CloneRule(rule);
            if (clonedRule == null) return;

            var dialog = new AddRuleDialog(clonedRule);
            if (dialog.ShowDialog() == true)
            {
                var idx = Rules.IndexOf(rule);
                Rules.Insert(idx + 1, dialog.SelectedRule ?? clonedRule);
                AutoPreviewIfEnabled(sender, e);
            }
        }
    }

    private static IRule? CloneRule(IRule rule)
    {
        try
        {
            var json = PresetService.SaveToJson(new[] { rule });
            var rules = PresetService.LoadFromJson(json);
            return rules.FirstOrDefault();
        }
        catch { return null; }
    }

    private void RemoveRule_Click(object sender, RoutedEventArgs e)
    {
        var selected = lvRules.SelectedItems.Cast<IRule>().ToList();
        foreach (var rule in selected) Rules.Remove(rule);
        if (selected.Count > 0) AutoPreviewIfEnabled(sender, e);
    }

    private void RemoveAllRules_Click(object sender, RoutedEventArgs e)
    {
        Rules.Clear();
        AutoPreviewIfEnabled(sender, e);
    }

    private void MoveRuleUp_Click(object sender, RoutedEventArgs e)
    {
        var index = lvRules.SelectedIndex;
        if (index > 0)
        {
            var item = Rules[index];
            Rules.RemoveAt(index);
            Rules.Insert(index - 1, item);
            lvRules.SelectedIndex = index - 1;
            AutoPreviewIfEnabled(sender, e);
        }
    }

    private void MoveRuleDown_Click(object sender, RoutedEventArgs e)
    {
        var index = lvRules.SelectedIndex;
        if (index >= 0 && index < Rules.Count - 1)
        {
            var item = Rules[index];
            Rules.RemoveAt(index);
            Rules.Insert(index + 1, item);
            lvRules.SelectedIndex = index + 1;
            AutoPreviewIfEnabled(sender, e);
        }
    }

    private void Rules_DoubleClick(object sender, MouseButtonEventArgs e) => EditRule_Click(sender, e);

    private void Rules_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshActions();

    private void Rules_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _rulesDragStartPoint = e.GetPosition(null);
        _draggedRule = GetRuleAtPoint(e.GetPosition(lvRules));
    }

    private void Rules_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _draggedRule == null) return;
        var position = e.GetPosition(null);
        if (Math.Abs(position.X - _rulesDragStartPoint.X) >= SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(position.Y - _rulesDragStartPoint.Y) >= SystemParameters.MinimumVerticalDragDistance)
        {
            DragDrop.DoDragDrop(lvRules, _draggedRule, DragDropEffects.Move);
        }
    }

    private void Rules_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(IRule)) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private void Rules_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(IRule))) return;
        var dragged = e.Data.GetData(typeof(IRule)) as IRule;
        if (dragged == null) return;

        var target = GetRuleAtPoint(e.GetPosition(lvRules));
        int oldIndex = Rules.IndexOf(dragged);
        if (oldIndex < 0) return;

        int newIndex = target != null ? Rules.IndexOf(target) : Rules.Count - 1;
        if (newIndex < 0) newIndex = Rules.Count - 1;
        if (newIndex == oldIndex) return;
        if (newIndex > oldIndex) newIndex--; // account for removal

        Rules.Move(oldIndex, newIndex);
        lvRules.SelectedItem = dragged;
        AutoPreviewIfEnabled(sender, new RoutedEventArgs());
    }

    private IRule? GetRuleAtPoint(Point point)
    {
        var element = lvRules.InputHitTest(point) as DependencyObject;
        while (element != null && element is not ListViewItem)
            element = VisualTreeHelper.GetParent(element);
        return (element as ListViewItem)?.DataContext as IRule;
    }

    private void RuleCheckBox_Click(object sender, RoutedEventArgs e) => AutoPreviewIfEnabled(sender, e);
    private void RulesSelectAll_Click(object sender, RoutedEventArgs e) => lvRules.SelectAll();

    private void RulesMarkAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var rule in Rules) rule.IsEnabled = true;
        lvRules.Items.Refresh();
        AutoPreviewIfEnabled(sender, e);
    }

    private void RulesUnmarkAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var rule in Rules) rule.IsEnabled = false;
        lvRules.Items.Refresh();
        AutoPreviewIfEnabled(sender, e);
    }

    private void AutoPreviewIfEnabled(object sender, RoutedEventArgs e)
    {
        if (_appSettings.AutoPreview)
            _ = ExecutePreviewAsync(isAutoTrigger: true, debounceMilliseconds: AutoPreviewDebounceMilliseconds);
    }

    private (CancellationToken token, int version) BeginNewPreviewRequest()
    {
        lock (_previewSync)
        {
            _previewCts?.Cancel();
            _previewCts?.Dispose();
            _previewCts = new CancellationTokenSource();
            _previewRequestVersion++;
            return (_previewCts.Token, _previewRequestVersion);
        }
    }

    private bool IsLatestPreviewRequest(int version)
    {
        lock (_previewSync)
            return version == _previewRequestVersion;
    }

    private void CancelPendingPreview()
    {
        lock (_previewSync)
        {
            _previewRequestVersion++;
            _previewCts?.Cancel();
            _previewCts?.Dispose();
            _previewCts = null;
        }
    }

    private (CancellationToken token, int version) BeginNewRenameRequest()
    {
        lock (_renameSync)
        {
            _renameCts?.Cancel();
            _renameCts?.Dispose();
            _renameCts = new CancellationTokenSource();
            _renameRequestVersion++;
            return (_renameCts.Token, _renameRequestVersion);
        }
    }

    private bool IsLatestRenameRequest(int version)
    {
        lock (_renameSync)
            return version == _renameRequestVersion;
    }

    private void CancelPendingRename()
    {
        lock (_renameSync)
        {
            _renameRequestVersion++;
            _renameCts?.Cancel();
            _renameCts?.Dispose();
            _renameCts = null;
        }
    }

    private async Task<bool> ExecutePreviewAsync(bool isAutoTrigger, int debounceMilliseconds)
    {
        if (!EnsureRulesReadyForExecution())
            return false;

        var previewLabel = LanguageService.GetString("Status_Previewing");
        var markedFiles = Files.Where(f => f.IsMarked).ToList();
        var unmarkedFiles = Files.Where(f => !f.IsMarked).ToList();
        var ruleSnapshot = Rules.ToList();
        var (token, version) = BeginNewPreviewRequest();

        try
        {
            if (debounceMilliseconds > 0)
            {
                await Task.Delay(debounceMilliseconds);
                if (token.IsCancellationRequested || !IsLatestPreviewRequest(version))
                    return false;
            }

            BeginProgress(previewLabel);
            IProgress<(int current, int total)> progress = new Progress<(int current, int total)>(
                p => UpdateProgress(previewLabel, p.current, p.total));
            int previewProgressStep = Math.Max(1, markedFiles.Count / 100);
            int lastPreviewProgress = 0;

            var previewResults = await Task.Run(
                () => _renameService.ComputePreview(
                    markedFiles,
                    ruleSnapshot,
                    (cur, total) =>
                    {
                        if (!ShouldReportProgress(cur, total, lastPreviewProgress, previewProgressStep))
                            return;

                        lastPreviewProgress = cur;
                        progress.Report((cur, total));
                    },
                    token));

            if (token.IsCancellationRequested || !IsLatestPreviewRequest(version))
                return false;

            foreach (var result in previewResults)
                result.file.NewName = result.newName;

            foreach (var file in unmarkedFiles)
                file.NewName = file.OriginalName;

            ApplyHighlightChangesState(Files.Where(f => !f.IsRenamed));
            lvFiles.Items.Refresh();
            UpdateStatusBar();

            if (_appSettings.AutoValidate)
            {
                var errors = ValidateMarkedFiles();
                tbStatusInfo.Text = errors.Count == 0
                    ? "Auto validation passed."
                    : $"Auto validation found {errors.Count} issue(s). Run Validate to view details.";
            }

            return true;
        }
        catch (Exception ex)
        {
            tbStatusInfo.Text = $"Preview failed: {ex.Message}";
            if (!isAutoTrigger)
            {
                MessageBox.Show(
                    tbStatusInfo.Text,
                    LanguageService.GetString("Menu_Preview"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return false;
        }
        finally
        {
            if (IsLatestPreviewRequest(version))
                EndProgress();
        }
    }

    private void ApplyHighlightChangesState(RenFile file)
    {
        if (file.IsRenamed || file.State == "×")
            return;

        file.State = file.HasChanged && _appSettings.HighlightChanges ? "→" : "";
    }

    private void ApplyHighlightChangesState(IEnumerable<RenFile> files)
    {
        foreach (var file in files)
            ApplyHighlightChangesState(file);
    }

    #endregion

    #region Core Operations

    private bool EnsureRulesReadyForExecution()
    {
        var blocking = Rules
            .OfType<JavaScriptRule>()
            .FirstOrDefault(r => r.IsEnabled && (r.IsScriptLoading || !r.IsScriptReady));

        if (blocking == null)
            return true;

        if (blocking.IsScriptLoading)
        {
            tbStatusInfo.Text = $"脚本“{blocking.ScriptDisplayName}”后台加载中，加载完成后将自动刷新预览。";
            return false;
        }

        var reason = string.IsNullOrWhiteSpace(blocking.ScriptLoadError)
            ? "未选择可用脚本。"
            : blocking.ScriptLoadError;

        tbStatusInfo.Text = $"脚本“{blocking.ScriptDisplayName}”不可执行：{reason}";
        return false;
    }

    private async void Preview_Click(object sender, RoutedEventArgs e)
    {
        await ExecutePreviewAsync(isAutoTrigger: false, debounceMilliseconds: 0);
    }

    private async void Rename_Click(object sender, RoutedEventArgs e)
    {
        if (!await ExecutePreviewAsync(isAutoTrigger: false, debounceMilliseconds: 0))
            return;

        var toRename = Files.Count(f => f.IsMarked && f.HasChanged && !f.IsRenamed);
        if (toRename == 0) return;

        if (_appSettings.AutoValidate)
        {
            var validationErrors = ValidateMarkedFiles();
            if (validationErrors.Count > 0)
            {
                var warningMessage = $"{BuildValidationIssueMessage(validationErrors)}\n\nContinue renaming anyway?";
                var continueRename = MessageBox.Show(warningMessage, "Validation Issues",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
                if (!continueRename)
                {
                    tbStatusInfo.Text = "Rename cancelled due to validation issues.";
                    return;
                }
            }
        }

        var shouldProceed = true;
        if (_appSettings.ConfirmRename)
        {
            var confirmMsg = LanguageService.GetString("Msg_ConfirmRename", toRename);
            shouldProceed = MessageBox.Show(confirmMsg, LanguageService.GetString("Menu_Rename"),
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        if (!shouldProceed)
            return;

        var renameTargets = Files.Where(f => f.IsMarked && f.HasChanged && !f.IsRenamed).ToList();
        var renameLabel = LanguageService.GetString("Status_Renaming");
        var (token, version) = BeginNewRenameRequest();

        try
        {
            BeginProgress(renameLabel);
            IProgress<(int current, int total)> progress = new Progress<(int current, int total)>(
                p => UpdateProgress(renameLabel, p.current, p.total));
            int renameProgressStep = Math.Max(1, renameTargets.Count / 100);
            int lastRenameProgress = 0;

            var (success, failed) = await Task.Run(
                () => _renameService.Rename(
                    renameTargets,
                    (cur, total) =>
                    {
                        if (!ShouldReportProgress(cur, total, lastRenameProgress, renameProgressStep))
                            return;

                        lastRenameProgress = cur;
                        progress.Report((cur, total));
                    },
                    _appSettings.ConflictResolution),
                token);

            if (token.IsCancellationRequested || !IsLatestRenameRequest(version))
                return;

            var renamedFiles = renameTargets.Where(f => f.IsRenamed).ToList();
            string? undoLogPath = null;
            if (_appSettings.CreateUndoLog && renamedFiles.Count > 0)
            {
                undoLogPath = CreateUndoLogFile(renamedFiles);
            }

            if (_appSettings.AutoRemoveRenamed && renamedFiles.Count > 0)
            {
                foreach (var file in renamedFiles)
                    Files.Remove(file);
            }

            lvFiles.Items.Refresh();
            UpdateStatusBar();

            var resultMessage = LanguageService.GetString("Msg_RenameResult", success, failed);
            if (_appSettings.CreateUndoLog && renamedFiles.Count > 0)
            {
                resultMessage += string.IsNullOrWhiteSpace(undoLogPath)
                    ? "\nUndo log was not created."
                    : $"\nUndo log: {undoLogPath}";
            }
            if (_appSettings.AutoRemoveRenamed && renamedFiles.Count > 0)
            {
                resultMessage += $"\nAuto removed renamed items: {renamedFiles.Count}";
            }

            MessageBox.Show(resultMessage,
                LanguageService.GetString("Msg_RenameComplete"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (OperationCanceledException)
        {
            tbStatusInfo.Text = "Rename cancelled.";
        }
        catch (Exception ex)
        {
            tbStatusInfo.Text = $"Rename failed: {ex.Message}";
            MessageBox.Show(
                tbStatusInfo.Text,
                LanguageService.GetString("Menu_Rename"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            if (IsLatestRenameRequest(version))
                EndProgress();
        }
    }

    private void UndoRename_Click(object sender, RoutedEventArgs e)
    {
        _renameService.UndoRename(Files);
        lvFiles.Items.Refresh();
        UpdateStatusBar();
    }

    private void Validate_Click(object sender, RoutedEventArgs e)
    {
        var errors = ValidateMarkedFiles();
        MessageBox.Show(BuildValidationIssueMessage(errors),
            errors.Count == 0 ? "Validation" : "Validation Issues",
            MessageBoxButton.OK, errors.Count == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }

    private List<string> ValidateMarkedFiles()
    {
        return _renameService.ValidateNewNames(
            Files.Where(f => f.IsMarked),
            _appSettings.WarnInvalidChars,
            _appSettings.WarnLongPaths);
    }

    private static string BuildValidationIssueMessage(IReadOnlyList<string> errors, int maxLines = 10)
    {
        if (errors.Count == 0)
            return "All new names are valid.";

        var lines = errors.Take(maxLines).ToList();
        var message = string.Join("\n", lines);
        if (errors.Count > maxLines)
            message += $"\n... and {errors.Count - maxLines} more.";

        return message;
    }

    private static string? CreateUndoLogFile(IReadOnlyCollection<RenFile> renamedFiles)
    {
        try
        {
            if (!Directory.Exists(UndoLogsDir))
                Directory.CreateDirectory(UndoLogsDir);

            var fileName = $"undo_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var logPath = Path.Combine(UndoLogsDir, fileName);
            var lines = renamedFiles.Select(file =>
            {
                var undoPath = string.IsNullOrEmpty(file.OldPath) ? file.FullPath : file.OldPath;
                return $"{file.FullPath}\t{undoPath}";
            });

            File.WriteAllLines(logPath, lines);
            return logPath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CreateUndoLog failed: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region File List Features

    private void Files_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateStatusBar();
    private void FileCheckBox_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar();
    }

    private void FileHeaderCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox headerCheckBox)
            return;

        var isMarked = headerCheckBox.IsChecked == true;
        foreach (var file in Files)
            file.IsMarked = isMarked;

        UpdateStatusBar();
    }

    private void Files_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _filesDragStartPoint = e.GetPosition(null);
        _draggedFile = GetFileAtPoint(e.GetPosition(lvFiles));
    }

    private void Files_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _draggedFile == null) return;
        var position = e.GetPosition(null);
        if (Math.Abs(position.X - _filesDragStartPoint.X) >= SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(position.Y - _filesDragStartPoint.Y) >= SystemParameters.MinimumVerticalDragDistance)
        {
            DragDrop.DoDragDrop(lvFiles, _draggedFile, DragDropEffects.Move);
        }
    }

    private void Files_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else if (e.Data.GetDataPresent(typeof(RenFile)))
        {
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void Files_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var paths = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            AddFilePaths(paths, recursive: true);
            return;
        }

        if (!e.Data.GetDataPresent(typeof(RenFile))) return;
        var dragged = e.Data.GetData(typeof(RenFile)) as RenFile;
        if (dragged == null) return;

        var target = GetFileAtPoint(e.GetPosition(lvFiles));
        int oldIndex = Files.IndexOf(dragged);
        if (oldIndex < 0) return;

        int newIndex = target != null ? Files.IndexOf(target) : Files.Count - 1;
        if (newIndex < 0) newIndex = Files.Count - 1;
        if (newIndex == oldIndex) return;
        if (newIndex > oldIndex) newIndex--;

        Files.Move(oldIndex, newIndex);
        lvFiles.SelectedItem = dragged;
    }

    private RenFile? GetFileAtPoint(Point point)
    {
        var element = lvFiles.InputHitTest(point) as DependencyObject;
        while (element != null && element is not DataGridRow)
            element = VisualTreeHelper.GetParent(element);

        return (element as DataGridRow)?.DataContext as RenFile;
    }

    private void FilesColumnHeader_Click(object sender, DataGridSortingEventArgs e)
    {
        var sortBy = e.Column.SortMemberPath;
        if (string.IsNullOrWhiteSpace(sortBy))
            return;

        var direction = e.Column.SortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;

        var view = CollectionViewSource.GetDefaultView(lvFiles.ItemsSource);
        using (view.DeferRefresh())
        {
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(sortBy, direction));
        }

        foreach (var col in lvFiles.Columns)
        {
            if (!ReferenceEquals(col, e.Column))
                col.SortDirection = null;
        }

        e.Column.SortDirection = direction;
        e.Handled = true;
    }

    private void MarkSelected_Click(object sender, RoutedEventArgs e)
    {
        foreach (RenFile f in lvFiles.SelectedItems) f.IsMarked = true;
        lvFiles.Items.Refresh(); UpdateStatusBar();
    }

    private void UnmarkSelected_Click(object sender, RoutedEventArgs e)
    {
        foreach (RenFile f in lvFiles.SelectedItems) f.IsMarked = false;
        lvFiles.Items.Refresh(); UpdateStatusBar();
    }

    private void InvertMarking_Click(object sender, RoutedEventArgs e)
    {
        foreach (var f in Files) f.IsMarked = !f.IsMarked;
        lvFiles.Items.Refresh(); UpdateStatusBar();
    }

    private void MarkOnlyChanged_Click(object sender, RoutedEventArgs e)
    {
        foreach (var f in Files) f.IsMarked = f.HasChanged;
        lvFiles.Items.Refresh(); UpdateStatusBar();
    }

    private void MarkOnlySelected_Click(object sender, RoutedEventArgs e)
    {
        var selected = lvFiles.SelectedItems.Cast<RenFile>().ToHashSet();
        foreach (var f in Files) f.IsMarked = selected.Contains(f);
        lvFiles.Items.Refresh(); UpdateStatusBar();
    }

    private void ClearAll_Click(object sender, RoutedEventArgs e) => Files.Clear();
    private void ClearRenamed_Click(object sender, RoutedEventArgs e)
    { foreach (var f in Files.Where(f => f.IsRenamed).ToList()) Files.Remove(f); }
    private void ClearFailed_Click(object sender, RoutedEventArgs e)
    { foreach (var f in Files.Where(f => f.State == "×").ToList()) Files.Remove(f); }
    private void ClearNotChanged_Click(object sender, RoutedEventArgs e)
    { foreach (var f in Files.Where(f => !f.HasChanged).ToList()) Files.Remove(f); }
    private void ClearMarked_Click(object sender, RoutedEventArgs e)
    { foreach (var f in Files.Where(f => f.IsMarked).ToList()) Files.Remove(f); }
    private void ClearNotMarked_Click(object sender, RoutedEventArgs e)
    { foreach (var f in Files.Where(f => !f.IsMarked).ToList()) Files.Remove(f); }

    private void FilesSelectAll_Click(object sender, RoutedEventArgs e) => lvFiles.SelectAll();

    private void InvertSelection_Click(object sender, RoutedEventArgs e)
    {
        var selected = lvFiles.SelectedItems.Cast<RenFile>().ToHashSet();
        lvFiles.SelectedItems.Clear();
        foreach (var f in Files.Where(f => !selected.Contains(f))) lvFiles.SelectedItems.Add(f);
    }

    private void MoveFileUp_Click(object sender, RoutedEventArgs e)
    {
        var index = lvFiles.SelectedIndex;
        if (index > 0)
        {
            Files.Move(index, index - 1);
            lvFiles.SelectedIndex = index - 1;
        }
    }

    private void MoveFileDown_Click(object sender, RoutedEventArgs e)
    {
        var index = lvFiles.SelectedIndex;
        if (index >= 0 && index < Files.Count - 1)
        {
            Files.Move(index, index + 1);
            lvFiles.SelectedIndex = index + 1;
        }
    }

    private void RemoveSelectedFiles_Click(object sender, RoutedEventArgs e)
    {
        foreach (var f in lvFiles.SelectedItems.Cast<RenFile>().ToList()) Files.Remove(f);
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        if (lvFiles.SelectedItem is RenFile file && File.Exists(file.FullPath))
            Process.Start(new ProcessStartInfo(file.FullPath) { UseShellExecute = true });
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (lvFiles.SelectedItem is RenFile file)
            Process.Start("explorer.exe", $"/select,\"{file.FullPath}\"");
    }

    private void EditNewName_Click(object sender, RoutedEventArgs e)
    {
        if (lvFiles.SelectedItem is not RenFile file)
            return;

        var dlg = new TextInputDialog(
            LanguageService.GetString("Dialog_EditNewNameTitle"),
            LanguageService.GetString("Column_NewName"),
            file.NewName,
            validator: text => string.IsNullOrWhiteSpace(text) ? LanguageService.GetString("Dialog_InputRequired") : null)
        {
            Owner = this
        };

        if (dlg.ShowDialog() == true)
        {
            file.NewName = dlg.InputText;
            ApplyHighlightChangesState(file);
            lvFiles.Items.Refresh();
        }
    }

    #endregion

    #region Menu/Toolbar Actions

    private void NewProject_Click(object sender, RoutedEventArgs e) { Rules.Clear(); Files.Clear(); }

    private void NewInstance_Click()
    {
        try
        {
            var exe = Process.GetCurrentProcess().MainModule?.FileName;
            if (exe != null) Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
        }
        catch { /* ignore */ }
    }

    private void NewInstance_MenuClick(object sender, RoutedEventArgs e) => NewInstance_Click();
    private void PasteFiles_MenuClick(object sender, RoutedEventArgs e) => PasteFiles_Click();

    private void PasteFiles_Click()
    {
        if (Clipboard.ContainsFileDropList())
        {
            var files = Clipboard.GetFileDropList();
            var paths = new string[files.Count];
            files.CopyTo(paths, 0);
            AddFilePaths(paths, recursive: true);
        }
        else if (Clipboard.ContainsText())
        {
            var lines = Clipboard.GetText().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            AddFilePaths(lines.Where(l => File.Exists(l.Trim()) || Directory.Exists(l.Trim())).Select(l => l.Trim()), recursive: true);
        }
    }

    private void LoadPreset_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON Presets (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Load Preset"
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var json = File.ReadAllText(dialog.FileName);
                var loadedRules = PresetService.LoadFromJson(json);
                Rules.Clear();
                foreach (var rule in loadedRules) Rules.Add(rule);
                _appSettings.LastPresetPath = dialog.FileName;
                _appSettings.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load preset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void SavePreset_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_appSettings.LastPresetPath))
        {
            SavePresetAs_Click(sender, e);
            return;
        }
        try
        {
            var json = PresetService.SaveToJson(Rules);
            File.WriteAllText(_appSettings.LastPresetPath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save preset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SavePresetAs_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "JSON Presets (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Save Preset As"
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var json = PresetService.SaveToJson(Rules);
                File.WriteAllText(dialog.FileName, json);
                _appSettings.LastPresetPath = dialog.FileName;
                _appSettings.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save preset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void ManageFiles_Click(object sender, RoutedEventArgs e) => AddFiles_Click(sender, e);

    private void Filters_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new FiltersDialog(Files, () => { lvFiles.Items.Refresh(); UpdateStatusBar(); }, _appSettings)
        {
            Owner = this
        };
        dlg.ShowDialog();
    }

    private void OpenSettingsTab(SettingsTab tab)
    {
        var dlg = new SettingsDialog(_appSettings, tab) { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            _appSettings.Save();
            RuleHelpers.ResolveMetaTagsEnabled = _appSettings.ResolveMetaTags;
            // Apply language if changed
            if (_appSettings.Language == "en-US")
                LanguageService.SwitchToEnglish();
            else
                LanguageService.SwitchToChinese();

            ApplyHighlightChangesState(Files);
            lvFiles.Items.Refresh();
        }
    }

    private void Options_Click(object sender, RoutedEventArgs e)
    {
        var menu = new ContextMenu { DataContext = this };
        var items = new (string key, UiAction action)[]
        {
            ("Options_AutosizeColumns", CmdAutosizeColumns),
            ("Options_ValidateNewNames", CmdValidate),
            ("Options_FixConflictingNewNames", CmdFixConflicts),
            ("Options_AnalyzeSampleText", CmdAnalyzeSampleText),
            ("Options_ApplyRulesToClipboard", CmdApplyRulesToClipboard),
            ("Options_CountFiles", CmdCountFiles),
            ("Options_SortForFolders", CmdSortForFolders),
        };
        foreach (var (key, action) in items)
        {
            var header = LanguageService.GetString(key);
            var mi = new MenuItem { Header = header, Command = action };
            menu.Items.Add(mi);
        }
        menu.PlacementTarget = sender as UIElement;
        menu.IsOpen = true;
    }

    private void AutosizeColumns()
    {
        foreach (var col in lvFiles.Columns)
        {
            if (col.Visibility == Visibility.Visible)
                AutoSizeColumn(col);
        }
    }

    private void FixConflictingNames()
    {
        var groups = Files.Where(f => f.HasChanged && !f.IsRenamed)
            .GroupBy(f => Path.Combine(f.FolderPath, f.NewName).ToLowerInvariant())
            .Where(g => g.Count() > 1);
        int fixed_ = 0;
        foreach (var group in groups)
        {
            int i = 1;
            foreach (var file in group.Skip(1))
            {
                var ext = Path.GetExtension(file.NewName);
                var name = Path.GetFileNameWithoutExtension(file.NewName);
                file.NewName = $"{name} ({++i}){ext}";
                ApplyHighlightChangesState(file);
                fixed_++;
            }
        }
        lvFiles.Items.Refresh();
        MessageBox.Show($"Fixed {fixed_} conflicting names.", "Fix Conflicts", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void AnalyzeSampleText()
    {
        var dlg = new Window
        {
            Title = "Analyze Sample Text", Width = 450, Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this, ResizeMode = ResizeMode.NoResize
        };
        var sp = new StackPanel { Margin = new Thickness(16) };
        sp.Children.Add(new TextBlock { Text = "Enter sample text to test rules:", Margin = new Thickness(0, 0, 0, 8) });
        var tb = new TextBox { Text = "Sample File Name.txt", Margin = new Thickness(0, 0, 0, 8) };
        var resultTb = new TextBox { IsReadOnly = true, Background = System.Windows.Media.Brushes.WhiteSmoke };
        var btn = new Button { Content = "Apply Rules", Width = 100, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 8) };
        btn.Click += (_, _) =>
        {
            var name = tb.Text;
            var tempFile = new RenFile(Path.Combine(Path.GetTempPath(), name));
            foreach (var rule in Rules.Where(r => r.IsEnabled))
                name = rule.Execute(name, tempFile);
            resultTb.Text = name;
        };
        sp.Children.Add(tb);
        sp.Children.Add(btn);
        sp.Children.Add(resultTb);
        dlg.Content = sp;
        dlg.ShowDialog();
    }

    private void ApplyRulesToClipboard()
    {
        RuleHelpers.ResetMetaTagCounter();
        var text = Clipboard.GetText();
        if (string.IsNullOrEmpty(text)) { MessageBox.Show("Clipboard is empty."); return; }
        var tempFile = new RenFile(Path.Combine(Path.GetTempPath(), text));
        var result = text;
        foreach (var rule in Rules.Where(r => r.IsEnabled))
            result = rule.Execute(result, tempFile);
        Clipboard.SetText(result);
        MessageBox.Show($"Result copied to clipboard:\n{result}", "Apply Rules to Clipboard");
    }

    private void CountFiles()
    {
        var total = Files.Count;
        var marked = Files.Count(f => f.IsMarked);
        var changed = Files.Count(f => f.HasChanged);
        var renamed = Files.Count(f => f.IsRenamed);
        var folders = Files.Select(f => f.FolderPath).Distinct().Count();
        MessageBox.Show(
            $"Total files: {total}\nMarked: {marked}\nChanged: {changed}\nRenamed: {renamed}\nFolders: {folders}",
            "Count Files", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SortForFolders()
    {
        var sorted = Files.OrderBy(f => f.FolderPath.Count(c => c == '\\' || c == '/'))
                          .ThenBy(f => f.FolderPath)
                          .ThenBy(f => f.OriginalName)
                          .ToList();
        Files.Clear();
        foreach (var f in sorted) Files.Add(f);
    }

    private void Analyze_Click(object sender, RoutedEventArgs e)
    {
        if (lvFiles.SelectedItem is RenFile file)
        {
            MessageBox.Show(
                $"Original: {file.OriginalName}\nNew Name: {file.NewName}\nExtension: {file.Extension}\n" +
                $"Path: {file.FolderPath}\nSize: {file.SizeDisplay}\nName Length: {file.NameLength}\nDigits: {file.NameDigits}",
                "Analyze Name", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void FilesColumnHeader_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGridColumnHeader header && header.Column != null)
        {
            ShowColumnsMenu(header);
            e.Handled = true;
        }
    }

    private void FilesColumnHeader_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGridColumnHeader header || header.Column == null)
            return;

        // Match Excel behavior: auto-size only when double-clicking near the right divider.
        var hit = e.GetPosition(header);
        if (hit.X < header.ActualWidth - 12)
            return;

        AutoSizeColumn(header.Column);
        e.Handled = true;
    }

    private void ShowColumnsMenu(UIElement placementTarget)
    {
        var menu = new ContextMenu();
        for (int i = 2; i < lvFiles.Columns.Count; i++)
        {
            var col = lvFiles.Columns[i];
            var header = col.Header?.ToString() ?? $"Column {i}";
            var isVisible = col.Visibility == Visibility.Visible;
            var mi = new MenuItem { Header = header, IsCheckable = true, IsChecked = isVisible, Tag = col };
            mi.Click += (s, _) =>
            {
                var menuItem = (MenuItem)s!;
                var targetCol = (DataGridColumn)menuItem.Tag;
                targetCol.Visibility = menuItem.IsChecked ? Visibility.Visible : Visibility.Collapsed;
                if (targetCol.Visibility == Visibility.Visible && targetCol.Width.DisplayValue <= 0)
                    targetCol.Width = new DataGridLength(120);
            };
            menu.Items.Add(mi);
        }

        menu.Items.Add(new Separator());
        var resetColumnWidths = new MenuItem { Header = LanguageService.GetString("ColumnsMenu_ResetColumnWidths") };
        resetColumnWidths.Click += (_, _) => ResetColumnWidths();
        menu.Items.Add(resetColumnWidths);

        var cancelSort = new MenuItem { Header = LanguageService.GetString("ColumnsMenu_CancelSorting") };
        cancelSort.Click += (_, _) =>
        {
            var view = CollectionViewSource.GetDefaultView(lvFiles.ItemsSource);
            view.SortDescriptions.Clear();
            view.Refresh();
            foreach (var col in lvFiles.Columns)
                col.SortDirection = null;
        };
        menu.Items.Add(cancelSort);

        menu.PlacementTarget = placementTarget;
        menu.IsOpen = true;
    }

    private void ResetColumnWidths()
    {
        foreach (var col in lvFiles.Columns)
        {
            var key = GetColumnKey(col);
            if (!string.IsNullOrEmpty(key) && _defaultColumnWidths.TryGetValue(key, out var width) && width > 0)
            {
                col.Width = new DataGridLength(width);
            }
            else if (col.Visibility == Visibility.Visible)
            {
                col.Width = new DataGridLength(120);
            }
        }
    }

    private void AutoSizeColumn(DataGridColumn col)
    {
        if (col.Visibility != Visibility.Visible)
            return;

        var fallback = col.ActualWidth > 0 ? col.ActualWidth : 80;

        col.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
        lvFiles.UpdateLayout();
        var cellsWidth = col.ActualWidth;

        col.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToHeader);
        lvFiles.UpdateLayout();
        var headerWidth = col.ActualWidth;

        var targetWidth = Math.Max(cellsWidth, headerWidth);
        if (double.IsNaN(targetWidth) || targetWidth <= 0)
            targetWidth = fallback;

        col.Width = new DataGridLength(Math.Ceiling(targetWidth) + 2);
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(LanguageService.GetString("Msg_About"),
            LanguageService.GetString("Menu_About"), MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // Help menu handlers
    private void HelpOnline_Click(object sender, RoutedEventArgs e)
        => OpenUrl("https://www.den4b.com/wiki/ReNamer");
    private void QuickGuide_Click(object sender, RoutedEventArgs e)
        => OpenUrl("https://www.den4b.com/wiki/ReNamer:Quick_Guide");
    private void UserManual_Click(object sender, RoutedEventArgs e)
        => OpenUrl("https://www.den4b.com/wiki/ReNamer:Manual");
    private void Forum_Click(object sender, RoutedEventArgs e)
        => OpenUrl("https://www.den4b.com/forum/");
    private void LiteVsPro_Click(object sender, RoutedEventArgs e)
        => OpenUrl("https://www.den4b.com/renamer");
    private void Purchase_Click(object sender, RoutedEventArgs e)
        => OpenUrl("https://www.den4b.com/purchase");
    private void Register_Click(object sender, RoutedEventArgs e)
        => MessageBox.Show("Registration is not supported in this recreation.", "Register",
            MessageBoxButton.OK, MessageBoxImage.Information);
    private void Unregister_Click(object sender, RoutedEventArgs e)
        => MessageBox.Show("Unregister is not supported in this recreation.", "Unregister",
            MessageBoxButton.OK, MessageBoxImage.Information);
    private void VersionHistory_Click(object sender, RoutedEventArgs e)
        => MessageBox.Show("ReNamer RE v1.0 (WPF Recreation)\n\nBased on ReNamer 7.8 by den4b.",
            "Version History", MessageBoxButton.OK, MessageBoxImage.Information);
    private void Copyrights_Click(object sender, RoutedEventArgs e)
        => MessageBox.Show("ReNamer RE - WPF Recreation\n\nOriginal ReNamer © den4b\nRecreation © 2024",
            "Copyrights", MessageBoxButton.OK, MessageBoxImage.Information);

    private static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { /* ignore */ }
    }

    #endregion

    #region Rule Context Menu Extensions

    private void AddRuleAbove_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddRuleDialog();
        if (dialog.ShowDialog() == true && dialog.SelectedRule != null)
        {
            var idx = Math.Max(0, lvRules.SelectedIndex);
            Rules.Insert(idx, dialog.SelectedRule);
        }
    }

    private void AddRuleBelow_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddRuleDialog();
        if (dialog.ShowDialog() == true && dialog.SelectedRule != null)
        {
            var idx = lvRules.SelectedIndex < 0 ? Rules.Count : lvRules.SelectedIndex + 1;
            Rules.Insert(idx, dialog.SelectedRule);
        }
    }

    private void ExportRuleToClipboard_Click(object sender, RoutedEventArgs e)
    {
        var selected = lvRules.SelectedItems.Cast<IRule>().ToList();
        if (selected.Count == 0) return;
        var json = PresetService.SaveToJson(selected);
        Clipboard.SetText(json);
        MessageBox.Show($"Exported {selected.Count} rule(s) to clipboard.", "Export");
    }

    private void RuleComment_Click(object sender, RoutedEventArgs e)
    {
        if (lvRules.SelectedItem is not IRule rule || rule is not RuleBase rb)
            return;

        var dlg = new TextInputDialog(
            LanguageService.GetString("Dialog_RuleCommentTitle"),
            LanguageService.GetString("Dialog_RuleCommentPrompt", rule.RuleName),
            rb.Comment)
        {
            Owner = this
        };

        if (dlg.ShowDialog() == true)
        {
            rb.Comment = dlg.InputText.Trim();
            lvRules.Items.Refresh();
        }
    }

    #endregion

    #region File Context Menu Extensions

    private void OpenWithNotepad_Click(object sender, RoutedEventArgs e)
    {
        if (lvFiles.SelectedItem is RenFile file && File.Exists(file.FullPath))
            Process.Start("notepad.exe", $"\"{file.FullPath}\"");
    }

    private void FileProperties_Click(object sender, RoutedEventArgs e)
    {
        if (lvFiles.SelectedItem is RenFile file && File.Exists(file.FullPath))
            ShellExecuteProperties(file.FullPath);
    }

    [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private struct SHELLEXECUTEINFO
    {
        public int cbSize;
        public uint fMask;
        public IntPtr hwnd;
        public string lpVerb;
        public string lpFile;
        public string lpParameters;
        public string lpDirectory;
        public int nShow;
        public IntPtr hInstApp;
        public IntPtr lpIDList;
        public string lpClass;
        public IntPtr hkeyClass;
        public uint dwHotKey;
        public IntPtr hIcon;
        public IntPtr hProcess;
    }

    private static void ShellExecuteProperties(string path)
    {
        var info = new SHELLEXECUTEINFO();
        info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
        info.lpVerb = "properties";
        info.lpFile = path;
        info.fMask = 0x0000000C; // SEE_MASK_INVOKEIDLIST
        info.nShow = 5; // SW_SHOW
        ShellExecuteEx(ref info);
    }

    private void CutFilesToClipboard_Click(object sender, RoutedEventArgs e)
    {
        var selected = lvFiles.SelectedItems.Cast<RenFile>().Select(f => f.FullPath).ToArray();
        if (selected.Length == 0) return;
        var col = new System.Collections.Specialized.StringCollection();
        col.AddRange(selected);
        Clipboard.SetFileDropList(col);
        // Remove from list after cut
        foreach (var f in lvFiles.SelectedItems.Cast<RenFile>().ToList()) Files.Remove(f);
    }

    private void CopyFilesToClipboard_Click(object sender, RoutedEventArgs e)
    {
        var selected = lvFiles.SelectedItems.Cast<RenFile>().Select(f => f.FullPath).ToArray();
        if (selected.Length == 0) return;
        var col = new System.Collections.Specialized.StringCollection();
        col.AddRange(selected);
        Clipboard.SetFileDropList(col);
    }

    private void DeleteToRecycleBin_Click(object sender, RoutedEventArgs e)
    {
        var selected = lvFiles.SelectedItems.Cast<RenFile>().ToList();
        if (selected.Count == 0) return;
        if (MessageBox.Show($"Move {selected.Count} file(s) to Recycle Bin?", "Confirm",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        var paths = selected.Select(f => f.FullPath).ToArray();
        var deleted = SendToRecycleBin(paths);
        if (deleted)
        {
            foreach (var f in selected)
                Files.Remove(f);
        }
        else
        {
            MessageBox.Show("Failed to move one or more files to Recycle Bin.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // IFileOperation (modern Shell API) for Recycle Bin delete
    [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, PreserveSig = true)]
    private static extern int SHCreateItemFromParsingName(
        string pszPath, IntPtr pbc, ref Guid riid,
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Interface)] out IShellItem ppv);

    [System.Runtime.InteropServices.ComImport]
    [System.Runtime.InteropServices.Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
    [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem
    {
        void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
        void GetParent(out IShellItem ppsi);
        void GetDisplayName(uint sigdnName, out IntPtr ppszName);
        void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
        void Compare(IShellItem psi, uint hint, out int piOrder);
    }

    [System.Runtime.InteropServices.ComImport]
    [System.Runtime.InteropServices.Guid("947AAB5F-0A5C-4C13-B4D6-4BF7836FC9F8")]
    [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileOperation
    {
        uint Advise(IntPtr pfops);
        void Unadvise(uint dwCookie);
        void SetOperationFlags(uint dwOperationFlags);
        void SetProgressMessage([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pszMessage);
        void SetProgressDialog(IntPtr popd);
        void SetProperties(IntPtr pproparray);
        void SetOwnerWindow(uint hwndOwner);
        void ApplyPropertiesToItem(IShellItem psiItem);
        void ApplyPropertiesToItems(IntPtr punkItems);
        void RenameItem(IShellItem psiItem, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pszNewName, IntPtr pfopsItem);
        void RenameItems(IntPtr pUnkItems, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pszNewName);
        void MoveItem(IShellItem psiItem, IShellItem psiDestinationFolder,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pszNewName, IntPtr pfopsItem);
        void MoveItems(IntPtr punkItems, IShellItem psiDestinationFolder);
        void CopyItem(IShellItem psiItem, IShellItem psiDestinationFolder,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pszCopyName, IntPtr pfopsItem);
        void CopyItems(IntPtr punkItems, IShellItem psiDestinationFolder);
        void DeleteItem(IShellItem psiItem, IntPtr pfopsItem);
        void DeleteItems(IntPtr punkItems);
        void NewItem(IShellItem psiDestinationFolder, uint dwFileAttributes,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pszName,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pszTemplateName, IntPtr pfopsItem);
        int PerformOperations();
        void GetAnyOperationsAborted([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)] out bool pfAnyOperationsAborted);
    }

    [System.Runtime.InteropServices.ComImport]
    [System.Runtime.InteropServices.Guid("3AD05575-8857-4850-9277-11B85BDB8E09")]
    private class FileOperation { }

    private const uint FOS_ALLOWUNDO = 0x00000040;
    private const uint FOS_NOCONFIRMATION = 0x00000010;
    private const uint FOS_NOERRORUI = 0x00000400;
    private const uint FOS_SILENT = 0x00000004;

    private static bool SendToRecycleBin(IEnumerable<string> paths)
    {
        IFileOperation? fileOp = null;
        try
        {
            fileOp = (IFileOperation)new FileOperation();
            fileOp.SetOperationFlags(FOS_ALLOWUNDO | FOS_NOCONFIRMATION | FOS_NOERRORUI | FOS_SILENT);

            foreach (var path in paths)
            {
                var iid = typeof(IShellItem).GUID;
                int hr = SHCreateItemFromParsingName(path, IntPtr.Zero, ref iid, out var item);
                if (hr != 0 || item == null) return false;
                fileOp.DeleteItem(item, IntPtr.Zero);
            }

            int res = fileOp.PerformOperations();
            if (res != 0) return false;
            fileOp.GetAnyOperationsAborted(out bool aborted);
            return !aborted;
        }
        catch { return false; }
        finally
        {
            if (fileOp != null)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fileOp);
        }
    }

    // Mark extensions
    private void MarkChangedIncCase_Click(object sender, RoutedEventArgs e)
    {
        foreach (var f in Files)
            f.IsMarked = !string.Equals(f.OriginalName, f.NewName, StringComparison.Ordinal);
        lvFiles.Items.Refresh(); UpdateStatusBar();
    }

    private void MarkChangedExcCase_Click(object sender, RoutedEventArgs e)
    {
        foreach (var f in Files)
            f.IsMarked = !string.Equals(f.OriginalName, f.NewName, StringComparison.OrdinalIgnoreCase);
        lvFiles.Items.Refresh(); UpdateStatusBar();
    }

    private void MarkByMask_Click(object sender, RoutedEventArgs e)
    {
        var mask = PromptInput(
            LanguageService.GetString("Dialog_MarkByMaskTitle"),
            LanguageService.GetString("Dialog_MarkByMaskPrompt"),
            "*.*");
        if (mask == null) return;
        var regex = WildcardToRegex(mask);
        foreach (var f in Files)
            f.IsMarked = regex.IsMatch(f.OriginalName);
        lvFiles.Items.Refresh(); UpdateStatusBar();
    }

    // Clear extensions
    private void ClearValid_Click(object sender, RoutedEventArgs e)
    { foreach (var f in Files.Where(f => f.State == "✓" || f.HasChanged).ToList()) Files.Remove(f); }

    private void ClearInvalid_Click(object sender, RoutedEventArgs e)
    { foreach (var f in Files.Where(f => f.State == "×" || !string.IsNullOrEmpty(f.Error)).ToList()) Files.Remove(f); }

    // Select extensions
    private void SelectByNameLength_Click(object sender, RoutedEventArgs e)
    {
        var input = PromptInput(
            LanguageService.GetString("Dialog_SelectByNameLengthTitle"),
            LanguageService.GetString("Dialog_SelectByNameLengthPrompt"),
            "20");
        if (input == null || !int.TryParse(input, out int maxLen)) return;
        lvFiles.SelectedItems.Clear();
        foreach (var f in Files.Where(f => f.OriginalName.Length <= maxLen))
            lvFiles.SelectedItems.Add(f);
    }

    private void SelectByExtension_Click(object sender, RoutedEventArgs e)
    {
        var ext = PromptInput(
            LanguageService.GetString("Dialog_SelectByExtensionTitle"),
            LanguageService.GetString("Dialog_SelectByExtensionPrompt"),
            ".txt");
        if (ext == null) return;
        lvFiles.SelectedItems.Clear();
        foreach (var f in Files.Where(f => f.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase)))
            lvFiles.SelectedItems.Add(f);
    }

    private void SelectByMask_Click(object sender, RoutedEventArgs e)
    {
        var mask = PromptInput(
            LanguageService.GetString("Dialog_SelectByMaskTitle"),
            LanguageService.GetString("Dialog_SelectByMaskPrompt"),
            "*.*");
        if (mask == null) return;
        var regex = WildcardToRegex(mask);
        lvFiles.SelectedItems.Clear();
        foreach (var f in Files.Where(f => regex.IsMatch(f.OriginalName)))
            lvFiles.SelectedItems.Add(f);
    }

    // Helpers
    private string? PromptInput(string title, string prompt, string defaultValue)
    {
        var dlg = new TextInputDialog(title, prompt, defaultValue)
        {
            Owner = this
        };

        return dlg.ShowDialog() == true ? dlg.InputText : null;
    }

    private static System.Text.RegularExpressions.Regex WildcardToRegex(string pattern)
    {
        var p = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return new System.Text.RegularExpressions.Regex(p, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    #endregion

    #region Presets Advanced

    private static readonly string PresetsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReNamer", "Presets");

    private void PopulatePresetMenu()
    {
        try
        {
            // Remove dynamic items (everything after the static Load item)
            while (menuLoadPreset.Items.Count > 0)
                menuLoadPreset.Items.RemoveAt(0);

            if (!Directory.Exists(PresetsDir))
            {
                Directory.CreateDirectory(PresetsDir);
                return;
            }

            var files = Directory.GetFiles(PresetsDir, "*.json");
            if (files.Length == 0)
            {
                menuLoadPreset.Items.Add(new MenuItem { Header = "(No presets found)", IsEnabled = false });
                return;
            }

            foreach (var file in files.OrderBy(f => f))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var mi = new MenuItem { Header = name, Tag = file };
                mi.Click += (s, _) =>
                {
                    try
                    {
                        var path = (string)((MenuItem)s!).Tag;
                        var json = File.ReadAllText(path);
                        var loadedRules = PresetService.LoadFromJson(json);
                        Rules.Clear();
                        foreach (var rule in loadedRules) Rules.Add(rule);
                        _appSettings.LastPresetPath = path;
                        _appSettings.Save();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load preset: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
                menuLoadPreset.Items.Add(mi);
            }
        }
        catch { /* ignore */ }
    }

    private void ManagePresets_Click(object sender, RoutedEventArgs e)
    {
        // Simple preset manager: list presets with delete/rename options
        if (!Directory.Exists(PresetsDir)) { Directory.CreateDirectory(PresetsDir); }
        var files = Directory.GetFiles(PresetsDir, "*.json");
        if (files.Length == 0)
        {
            MessageBox.Show("No presets found.", "Manage Presets");
            return;
        }

        var dlg = new Window
        {
            Title = "Manage Presets", Width = 400, Height = 350,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this, ResizeMode = ResizeMode.CanResize
        };
        var grid = new Grid { Margin = new Thickness(12) };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var lb = new ListBox { Margin = new Thickness(0, 0, 0, 8) };
        foreach (var f in files.OrderBy(f => f))
            lb.Items.Add(new ListBoxItem { Content = Path.GetFileNameWithoutExtension(f), Tag = f });
        Grid.SetRow(lb, 0);
        grid.Children.Add(lb);

        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var btnDelete = new Button { Content = "Delete", Width = 80, Margin = new Thickness(0, 0, 8, 0) };
        btnDelete.Click += (_, _) =>
        {
            if (lb.SelectedItem is ListBoxItem item)
            {
                var path = (string)item.Tag;
                if (MessageBox.Show($"Delete preset '{item.Content}'?", "Confirm",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    File.Delete(path);
                    lb.Items.Remove(item);
                }
            }
        };
        var btnClose = new Button { Content = "Close", Width = 80 };
        btnClose.Click += (_, _) => dlg.Close();
        btnPanel.Children.Add(btnDelete);
        btnPanel.Children.Add(btnClose);
        Grid.SetRow(btnPanel, 1);
        grid.Children.Add(btnPanel);

        dlg.Content = grid;
        dlg.ShowDialog();
        PopulatePresetMenu();
    }

    private void BrowsePresets_Click(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(PresetsDir)) Directory.CreateDirectory(PresetsDir);
        Process.Start(new ProcessStartInfo(PresetsDir) { UseShellExecute = true });
    }

    private void ImportPreset_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON Presets (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Import Preset"
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var dest = Path.Combine(PresetsDir, Path.GetFileName(dialog.FileName));
                if (!Directory.Exists(PresetsDir)) Directory.CreateDirectory(PresetsDir);
                File.Copy(dialog.FileName, dest, overwrite: true);
                PopulatePresetMenu();
                MessageBox.Show($"Imported preset to: {dest}", "Import");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void RescanPresets_Click(object sender, RoutedEventArgs e)
    {
        PopulatePresetMenu();
    }

    private void CreatePresetLinks_Click(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(PresetsDir)) Directory.CreateDirectory(PresetsDir);
        var files = Directory.GetFiles(PresetsDir, "*.json");
        if (files.Length == 0)
        {
            MessageBox.Show("No presets found.", "Create Links");
            return;
        }

        var linksDir = Path.Combine(PresetsDir, "Links");
        Directory.CreateDirectory(linksDir);
        foreach (var preset in files)
        {
            var name = Path.GetFileNameWithoutExtension(preset);
            var linkPath = Path.Combine(linksDir, name + ".url");
            var url = $"[InternetShortcut]\r\nURL=file:///{preset.Replace("\\", "/")}\r\n";
            File.WriteAllText(linkPath, url);
        }
        Process.Start(new ProcessStartInfo(linksDir) { UseShellExecute = true });
    }

    #endregion

    #region Window State Persistence

    private void RestoreWindowState()
    {
        if (_appSettings.RememberWindowPosition)
        {
            if (!double.IsNaN(_appSettings.WindowWidth) && _appSettings.WindowWidth > 0)
                Width = _appSettings.WindowWidth;
            if (!double.IsNaN(_appSettings.WindowHeight) && _appSettings.WindowHeight > 0)
                Height = _appSettings.WindowHeight;
        }

        if (_appSettings.RememberWindowPosition && _appSettings.IsMaximized)
        {
            WindowState = WindowState.Maximized;
        }
        else
        {
            var currentWidth = Width;
            var currentHeight = Height;
            if (double.IsNaN(currentWidth) || currentWidth <= 0) currentWidth = 900;
            if (double.IsNaN(currentHeight) || currentHeight <= 0) currentHeight = 600;

            var restoredSavedPosition = false;
            if (_appSettings.RememberWindowPosition
                && !double.IsNaN(_appSettings.WindowLeft)
                && !double.IsNaN(_appSettings.WindowTop))
            {
                var savedBounds = new Rect(_appSettings.WindowLeft, _appSettings.WindowTop, currentWidth, currentHeight);
                if (IsWindowRectVisible(savedBounds))
                {
                    Left = _appSettings.WindowLeft;
                    Top = _appSettings.WindowTop;
                    restoredSavedPosition = true;
                }
            }

            if (!restoredSavedPosition)
            {
                var screenLeft = SystemParameters.VirtualScreenLeft;
                var screenTop = SystemParameters.VirtualScreenTop;
                Left = screenLeft + (SystemParameters.VirtualScreenWidth - currentWidth) / 2;
                Top = screenTop + (SystemParameters.VirtualScreenHeight - currentHeight) / 2;
            }
        }

        // Restore splitter position
        var mainGrid = Content is Border border ? FindMainContentGrid(border) : null;
        if (mainGrid != null && mainGrid.RowDefinitions.Count > 0 && _appSettings.SplitterPosition > 0)
            mainGrid.RowDefinitions[0].Height = new GridLength(_appSettings.SplitterPosition);

        RestoreColumnState();
    }

    private static bool IsWindowRectVisible(Rect rect)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
            return false;

        var virtualBounds = new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);

        if (!virtualBounds.IntersectsWith(rect))
            return false;

        var intersection = Rect.Intersect(virtualBounds, rect);
        return intersection.Width >= 100 && intersection.Height >= 80;
    }

    private void EnsureWindowVisible()
    {
        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;

        Show();
        Activate();
    }

    private void SaveWindowState()
    {
        if (!_appSettings.RememberWindowPosition) return;

        _appSettings.IsMaximized = WindowState == WindowState.Maximized;
        if (WindowState == WindowState.Normal)
        {
            _appSettings.WindowLeft = Left;
            _appSettings.WindowTop = Top;
            _appSettings.WindowWidth = Width;
            _appSettings.WindowHeight = Height;
        }

        // Save splitter position
        var mainGrid = Content is Border border ? FindMainContentGrid(border) : null;
        if (mainGrid != null && mainGrid.RowDefinitions.Count > 0)
            _appSettings.SplitterPosition = mainGrid.RowDefinitions[0].Height.Value;

        SaveColumnState();
        _appSettings.Save();
    }

    private void SaveColumnState()
    {
        _appSettings.ColumnWidths.Clear();
        _appSettings.VisibleColumns.Clear();

        foreach (var col in lvFiles.Columns)
        {
            var key = GetColumnKey(col);
            if (string.IsNullOrEmpty(key))
                continue;

            var width = col.ActualWidth;
            if (width <= 0)
                width = col.Width.DisplayValue;

            _appSettings.ColumnWidths[key] = width;
            if (col.Visibility == Visibility.Visible)
                _appSettings.VisibleColumns.Add(key);
        }
    }

    private void RestoreColumnState()
    {
        if (_appSettings.ColumnWidths.Count == 0 && _appSettings.VisibleColumns.Count == 0)
            return;

        foreach (var col in lvFiles.Columns)
        {
            var key = GetColumnKey(col);
            if (string.IsNullOrEmpty(key))
                continue;

            if (_appSettings.ColumnWidths.TryGetValue(key, out var width) && width > 0)
                col.Width = new DataGridLength(width);

            if (_appSettings.VisibleColumns.Count > 0)
            {
                var isVisible = _appSettings.VisibleColumns.Contains(key);
                col.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                if (isVisible && (!_appSettings.ColumnWidths.TryGetValue(key, out var persistedWidth) || persistedWidth <= 0))
                    col.Width = new DataGridLength(120);
            }
        }
    }

    private static Grid? FindMainContentGrid(Border border)
    {
        // Navigate: Border > Grid (root) > Grid[Row=3] (main content)
        if (border.Child is Grid rootGrid && rootGrid.Children.Count > 3)
        {
            if (rootGrid.Children[3] is Grid mainContent)
                return mainContent;
        }
        return null;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        Rules.CollectionChanged -= Rules_CollectionChanged;
        foreach (var rule in Rules)
            rule.PropertyChanged -= Rule_PropertyChanged;

        CancelPendingPreview();
        CancelPendingRename();
        SaveWindowState();
    }

    #endregion

    #region Modern Window Controls

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            MaximizeButton_Click(sender, e);
        }
        else
        {
            this.DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = this.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    #endregion
}

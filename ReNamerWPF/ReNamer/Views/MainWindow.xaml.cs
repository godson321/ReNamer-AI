using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using ReNamer.Models;
using ReNamer.Rules;
using ReNamer.Services;
using ReNamer.Views.FileList;
using WinForms = System.Windows.Forms;

namespace ReNamer.Views;

/// <summary>
/// 规则序号转换器 - 将 ListViewItem 转为 1-based 序号
/// </summary>
public class IndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DataGridRow dataGridRow)
            return (dataGridRow.GetIndex() + 1).ToString(CultureInfo.InvariantCulture);

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

public class CollectionIndexConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] == DependencyProperty.UnsetValue ||
            values[1] == DependencyProperty.UnsetValue)
        {
            return string.Empty;
        }

        var item = values[0];
        if (values[1] is IList list)
        {
            int index = list.IndexOf(item);
            return index >= 0
                ? (index + 1).ToString(CultureInfo.InvariantCulture)
                : string.Empty;
        }

        if (values[1] is IEnumerable enumerable)
        {
            int index = 0;
            foreach (var current in enumerable)
            {
                if (ReferenceEquals(current, item) || Equals(current, item))
                    return (index + 1).ToString(CultureInfo.InvariantCulture);

                index++;
            }
        }

        return string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class RuleDropPreviewVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] == DependencyProperty.UnsetValue ||
            values[1] == DependencyProperty.UnsetValue)
        {
            return Visibility.Collapsed;
        }

        var currentItem = values[0];
        var previewItem = values[1];
        return previewItem != null && ReferenceEquals(currentItem, previewItem)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class RuleDropPreviewVerticalAlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool insertAfter && insertAfter
            ? VerticalAlignment.Bottom
            : VerticalAlignment.Top;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

[SupportedOSPlatform("windows")]
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly RenameService _renameService;
    private readonly AppSettings _appSettings;
    private Point _rulesDragStartPoint;
    private IRule? _draggedRule;
    private int _draggedRuleIndex = -1;
    private IRule? _ruleDropPreviewTarget;
    private bool _ruleDropPreviewInsertAfter;
    private int _ruleDropPreviewEventVersion;
    private string? _lastRuleDragPreviewSignature;
    private Point _filesDragStartPoint;
    private RenFile? _draggedFile;
    private int _draggedFileIndex = -1;
    private RenFile? _fileDropPreviewTarget;
    private bool _fileDropPreviewInsertAfter;
    private readonly List<UiAction> _actions = new();
    private readonly object _previewSync = new();
    private CancellationTokenSource? _previewCts;
    private int _previewRequestVersion;
    private readonly object _renameSync = new();
    private CancellationTokenSource? _renameCts;
    private int _renameRequestVersion;
    private const int AutoPreviewDebounceMilliseconds = 200;
    private const int PreviewProgressUpdateTarget = 40;
    private const int LargeSelectionThreshold = 1200;
    private const int ParallelImportCreationThreshold = 256;
    private const string RuleDragIndexDataFormat = "ReNamer.RuleDragIndex";
    private const string FileDragIndexDataFormat = "ReNamer.FileDragIndex";
    private const string FileReorderColumnKey = "FileReorder";
    private static readonly int ParallelImportCreationDegree = Math.Clamp(Environment.ProcessorCount, 2, 8);
    private bool _isHighContrastMode;
    private int _ruleOrderVersion;
    private readonly Dictionary<string, double> _defaultColumnWidths = new(StringComparer.OrdinalIgnoreCase);
    private GridLength _ruleDragColumnWidth = new(34);
    private GridLength _ruleCheckColumnWidth = new(46);
    private GridLength _ruleIndexColumnWidth = new(50);
    private GridLength _ruleNameColumnWidth = new(170);
    private GridLength _ruleDescriptionColumnWidth = new(1, GridUnitType.Star);
    private bool _statusBarUpdatePending;
    private bool _actionsRefreshPending;
    private int _fileSelectionBatchDepth;
    private bool _fileSelectionStatusDirty;
    private int _wpfFilesBatchDepth;
    private int _win32FilesBatchDepth;
    private bool _win32FilesChangedWhileBatch;
    private int _win32FilesDeferredChangeCount;
    private NotifyCollectionChangedAction? _win32LastDeferredAction;
    private bool _fileStateCacheDirty;
    private int _fileStateUiFlushPending;
    private bool _autoPreviewQueuedDuringBatch;
    private bool _largeSelectionInProgress;
    private int _totalFileCount;
    private int _markedFileCount;
    private int _renamedFileCount;
    private int _renameableFileCount;
    private int _selectedFileCount;
    private readonly HashSet<RenFile> _trackedFileSubscriptions = new();
    private IFileListView _fileListView = null!;
    private WpfFileListViewAdapter _wpfFileListView = null!;
    private Win32FileListViewHost? _win32FileListView = null;
    private readonly List<FileListColumn> _fileListColumns = new();
    private Win32FileListPalette? _win32Palette;
    private readonly List<RenFile> _viewItems = new();
    private string? _viewSortKey;
    private ListSortDirection? _viewSortDirection;
    private long _importTraceSequence;
    private long _previewTraceSequence;
    private static readonly string AppRootDir = AppContext.BaseDirectory;
    private static readonly object ImportLogSync = new();
    private static readonly string ImportLogFile = Path.Combine(
        AppRootDir,
        "logs",
        "file-import-debug.log");
    private static readonly object RuleDragLogSync = new();
    private static readonly string RuleDragLogFile = Path.Combine(
        AppRootDir,
        "logs",
        "rule-drag-debug.log");
    private static readonly string UndoLogsDir = Path.Combine(
        AppRootDir,
        "logs",
        "UndoLogs");
    private static readonly string SessionRulesFile = Path.Combine(
        AppRootDir,
        "config",
        "last-session-rules.json");
    private static readonly HashSet<string> MutableSortProperties = new(StringComparer.Ordinal)
    {
        nameof(RenFile.IsMarked),
        nameof(RenFile.State),
        nameof(RenFile.OriginalName),
        nameof(RenFile.NewName),
        nameof(RenFile.Error),
        nameof(RenFile.OldPath),
        nameof(RenFile.ExifDate),
        nameof(RenFile.NameDigits),
        nameof(RenFile.NameLength),
        nameof(RenFile.PathDigits),
        nameof(RenFile.PathLength),
        nameof(RenFile.NewPath),
        nameof(RenFile.NewNameLength),
        nameof(RenFile.NewPathLength)
    };
    private static readonly HashSet<string> FileStateImpactingProperties = new(StringComparer.Ordinal)
    {
        nameof(RenFile.IsMarked),
        nameof(RenFile.IsRenamed),
        nameof(RenFile.NewName),
        nameof(RenFile.OriginalName),
        nameof(RenFile.Error),
        nameof(RenFile.State),
        nameof(RenFile.OldPath)
    };
    private WinForms.NotifyIcon? _trayIcon;
    private bool _exitRequestedFromTray;

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
    public UiAction CmdRescanPresets { get; private set; } = null!;
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

    public RangeObservableCollection<RenFile> Files { get; } = new();
    public ObservableCollection<IRule> Rules { get; } = new();
    public event PropertyChangedEventHandler? PropertyChanged;

    public GridLength RuleDragColumnWidth
    {
        get => _ruleDragColumnWidth;
        set => SetRuleColumnWidth(ref _ruleDragColumnWidth, value, nameof(RuleDragColumnWidth));
    }

    public GridLength RuleCheckColumnWidth
    {
        get => _ruleCheckColumnWidth;
        set => SetRuleColumnWidth(ref _ruleCheckColumnWidth, value, nameof(RuleCheckColumnWidth));
    }

    public GridLength RuleIndexColumnWidth
    {
        get => _ruleIndexColumnWidth;
        set => SetRuleColumnWidth(ref _ruleIndexColumnWidth, value, nameof(RuleIndexColumnWidth));
    }

    public GridLength RuleNameColumnWidth
    {
        get => _ruleNameColumnWidth;
        set => SetRuleColumnWidth(ref _ruleNameColumnWidth, value, nameof(RuleNameColumnWidth));
    }

    public GridLength RuleDescriptionColumnWidth
    {
        get => _ruleDescriptionColumnWidth;
        set => SetRuleColumnWidth(ref _ruleDescriptionColumnWidth, value, nameof(RuleDescriptionColumnWidth));
    }

    public int RuleOrderVersion => _ruleOrderVersion;

    public IRule? RuleDropPreviewTarget
    {
        get => _ruleDropPreviewTarget;
    }

    public bool RuleDropPreviewInsertAfter
    {
        get => _ruleDropPreviewInsertAfter;
    }

    public RenFile? FileDropPreviewTarget
    {
        get => _fileDropPreviewTarget;
    }

    public bool FileDropPreviewInsertAfter
    {
        get => _fileDropPreviewInsertAfter;
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        _renameService = new RenameService();
        _appSettings = AppSettings.Load();
        if (!string.IsNullOrWhiteSpace(AppSettings.LastLoadError))
        {
            var loadErrorMessage = $"Settings could not be loaded. Defaults are being used. ({AppSettings.LastLoadError})";
            tbStatusInfo.Text = loadErrorMessage;
            MessageBox.Show(loadErrorMessage, "Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        RuleHelpers.ResolveMetaTagsEnabled = _appSettings.ResolveMetaTags;
        App.SetInputDebugLoggingEnabled(_appSettings.EnableInputDebugLogging);
        ThemeService.ApplyTheme(_appSettings.Theme);

        _wpfFileListView = new WpfFileListViewAdapter(lvFiles, LargeSelectionThreshold);
        _wpfFileListView.SelectionChanged += (_, _) => HandleFileSelectionChanged();
        BuildFileListColumns();
        InitializeFileListView();

        lvRules.ItemsSource = Rules;

        Files.CollectionChanged += Files_CollectionChanged;
        Rules.CollectionChanged += Rules_CollectionChanged;

        foreach (var file in Files)
            TrackFilePropertyChanged(file);

        foreach (var rule in Rules)
            rule.PropertyChanged += Rule_PropertyChanged;

        LanguageService.LanguageChanged += OnLanguageChanged;
        UpdateLanguageMenuChecks();
        InitActions();
        SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        ApplyHighContrastMode(SystemParameters.HighContrast);
        RecalculateFileStateCache();
        _selectedFileCount = _fileListView.SelectedCount;

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
            RestoreLastSessionRulesIfNeeded();
            UpdateTrayIconState();
        };
    }

    private static void LogException(string context, Exception ex)
    {
        Debug.WriteLine($"[MainWindow] {context}: {ex}");
    }

    private void SetRuleColumnWidth(ref GridLength field, GridLength value, string propertyName)
    {
        if (field.Equals(value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void AdvanceRuleOrderVersion()
    {
        unchecked
        {
            _ruleOrderVersion++;
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RuleOrderVersion)));
    }

    private void SetRuleDropPreview(IRule? target, bool insertAfter)
    {
        bool targetChanged = !ReferenceEquals(_ruleDropPreviewTarget, target);
        bool insertAfterChanged = _ruleDropPreviewInsertAfter != insertAfter;
        if (!targetChanged && !insertAfterChanged)
            return;

        _ruleDropPreviewTarget = target;
        _ruleDropPreviewInsertAfter = insertAfter;

        if (targetChanged)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RuleDropPreviewTarget)));

        if (insertAfterChanged)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RuleDropPreviewInsertAfter)));
    }

    private void ClearRuleDropPreview(string reason = "clear")
    {
        if (_ruleDropPreviewTarget != null)
        {
            LogRuleDragDiag(
                $"[RuleDrag] preview-clear reason={reason} targetIndex={Rules.IndexOf(_ruleDropPreviewTarget)} " +
                $"target='{DescribeRule(_ruleDropPreviewTarget)}' insertAfter={_ruleDropPreviewInsertAfter}");
        }

        _lastRuleDragPreviewSignature = null;
        SetRuleDropPreview(null, false);
    }

    private static string DescribeRule(IRule? rule)
        => rule == null
            ? "<null>"
            : $"{rule.RuleName}/{rule.GetType().Name}";

    private static string DescribeFile(RenFile? file)
        => file == null
            ? "<null>"
            : $"{file.OriginalName}";

    private static void LogRuleDragDiag(string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [T{Environment.CurrentManagedThreadId}] {message}";
        Debug.WriteLine(line);

        lock (RuleDragLogSync)
        {
            try
            {
                var dir = Path.GetDirectoryName(RuleDragLogFile);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);

                File.AppendAllText(RuleDragLogFile, line + Environment.NewLine);
            }
            catch
            {
            }
        }
    }

    private void LogRuleDragPreviewState(string stage, Point point, int? hitRowIndex, int rawInsertionIndex, int previewTargetIndex, bool insertAfter)
    {
        string signature = $"{stage}|hit={hitRowIndex?.ToString(CultureInfo.InvariantCulture) ?? "none"}|raw={rawInsertionIndex}|target={previewTargetIndex}|after={insertAfter}";
        if (string.Equals(_lastRuleDragPreviewSignature, signature, StringComparison.Ordinal))
            return;

        _lastRuleDragPreviewSignature = signature;
        var previewTarget = previewTargetIndex >= 0 && previewTargetIndex < Rules.Count
            ? Rules[previewTargetIndex]
            : null;
        LogRuleDragDiag(
            $"[RuleDrag] {stage} pt=({point.X:F1},{point.Y:F1}) hitRow={hitRowIndex?.ToString(CultureInfo.InvariantCulture) ?? "none"} " +
            $"rawInsert={rawInsertionIndex} previewTargetIndex={previewTargetIndex} " +
            $"previewTarget='{DescribeRule(previewTarget)}' insertAfter={insertAfter}");
    }

    private void SetFileDropPreview(RenFile? target, bool insertAfter)
    {
        bool targetChanged = !ReferenceEquals(_fileDropPreviewTarget, target);
        bool insertAfterChanged = _fileDropPreviewInsertAfter != insertAfter;
        if (!targetChanged && !insertAfterChanged)
            return;

        _fileDropPreviewTarget = target;
        _fileDropPreviewInsertAfter = insertAfter;

        if (targetChanged)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileDropPreviewTarget)));

        if (insertAfterChanged)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileDropPreviewInsertAfter)));
    }

    private void ClearFileDropPreview()
        => SetFileDropPreview(null, false);

    private static void LogImportDiag(string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [T{Environment.CurrentManagedThreadId}] {message}";
        Debug.WriteLine(line);
        App.LogInputDebug(message);

        lock (ImportLogSync)
        {
            try
            {
                var dir = Path.GetDirectoryName(ImportLogFile);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);
                File.AppendAllText(ImportLogFile, line + Environment.NewLine);
            }
            catch
            {
            }
        }
    }

    private void ShowBackgroundOperationError(string userMessage, bool showDialog)
    {
        tbStatusInfo.Text = userMessage;
        if (showDialog)
            MessageBox.Show(userMessage, "ReNamer", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private bool SaveSettingsWithFeedback(string context, bool showDialog)
    {
        if (_appSettings.Save())
            return true;

        var detail = string.IsNullOrWhiteSpace(_appSettings.LastSaveError)
            ? "Unknown error."
            : _appSettings.LastSaveError;
        var userMessage = $"Settings could not be saved ({context}): {detail}";
        ShowBackgroundOperationError(userMessage, showDialog);
        return false;
    }

    private void SystemParameters_StaticPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemParameters.HighContrast))
            ApplyHighContrastMode(SystemParameters.HighContrast);
    }

    private void ApplyHighContrastMode(bool enabled)
    {
        if (_isHighContrastMode == enabled)
            return;

        _isHighContrastMode = enabled;
        if (enabled)
        {
            WindowRootBorder.BorderBrush = SystemColors.WindowTextBrush;
            TitleBarBorder.Background = SystemColors.WindowBrush;
            TitleBarText.Foreground = SystemColors.WindowTextBrush;
            TitleBarIcon.Fill = SystemColors.WindowTextBrush;
            return;
        }

        WindowRootBorder.SetResourceReference(Border.BorderBrushProperty, "PrimaryBrush");
        TitleBarBorder.SetResourceReference(Border.BackgroundProperty, "PrimaryBrush");
        TitleBarText.Foreground = Brushes.White;
        TitleBarIcon.Fill = Brushes.White;
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

        AdvanceRuleOrderVersion();
        RefreshActions();
    }

    private void Files_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (var file in e.OldItems.OfType<RenFile>())
                UntrackFilePropertyChanged(file);
        }

        if (e.NewItems != null)
        {
            foreach (var file in e.NewItems.OfType<RenFile>())
                TrackFilePropertyChanged(file);
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
            ResyncFilePropertySubscriptions();

        MarkFileStateCacheDirty();

        if (IsWin32FileList)
        {
            if (_win32FilesBatchDepth > 0)
            {
                _win32FilesChangedWhileBatch = true;
                _win32FilesDeferredChangeCount++;
                _win32LastDeferredAction = e.Action;
                if (_win32FilesDeferredChangeCount == 1 || _win32FilesDeferredChangeCount % 500 == 0)
                {
                    LogImportDiag(
                        $"[FileSync] deferred action={e.Action} files={Files.Count} view={_viewItems.Count} " +
                        $"batchDepth={_win32FilesBatchDepth} deferredCount={_win32FilesDeferredChangeCount}");
                }
                return;
            }

            LogImportDiag($"[FileSync] action={e.Action} files={Files.Count} view={_viewItems.Count} batchDepth={_win32FilesBatchDepth} changedWhileBatch={_win32FilesChangedWhileBatch}");
            RebuildViewItems(preserveSelection: true);
            LogImportDiag($"[FileSync] rebuilt action={e.Action} files={Files.Count} view={_viewItems.Count}");
            FlushFileStateUi();
            return;
        }

        if (_wpfFilesBatchDepth > 0)
            return;

        FlushFileStateUi();
    }

    private void TrackFilePropertyChanged(RenFile file)
    {
        if (_trackedFileSubscriptions.Add(file))
            file.PropertyChanged += File_PropertyChanged;
    }

    private void UntrackFilePropertyChanged(RenFile file)
    {
        if (_trackedFileSubscriptions.Remove(file))
            file.PropertyChanged -= File_PropertyChanged;
    }

    private void ResyncFilePropertySubscriptions()
    {
        var currentFiles = Files.ToHashSet();
        foreach (var file in _trackedFileSubscriptions.Except(currentFiles).ToList())
            UntrackFilePropertyChanged(file);

        foreach (var file in currentFiles)
            TrackFilePropertyChanged(file);
    }

    private void File_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null || !FileStateImpactingProperties.Contains(e.PropertyName))
            return;

        if (!Dispatcher.CheckAccess())
        {
            RequestFileStateUiFlush();
            return;
        }

        MarkFileStateCacheDirty();
        if (IsFileMutationBatchActive)
            return;

        FlushFileStateUi();
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
        CmdRescanPresets = CreateAction(_ => RescanPresets_Click(this, new RoutedEventArgs()));

        CmdSettings = CreateAction(_ => OpenSettingsTab(SettingsTab.General));
        CmdSettingsGeneral = CreateAction(_ => OpenSettingsTab(SettingsTab.General));
        CmdSettingsPreview = CreateAction(_ => OpenSettingsTab(SettingsTab.Preview));
        CmdSettingsRename = CreateAction(_ => OpenSettingsTab(SettingsTab.Rename));
        CmdSettingsMetaTags = CreateAction(_ => OpenSettingsTab(SettingsTab.Rename));
        CmdSettingsMisc = CreateAction(_ => OpenSettingsTab(SettingsTab.Misc));
        CmdSettingsFilters = CreateAction(_ => Filters_Click(this, new RoutedEventArgs()));
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
        CmdAnalyze = CreateAction(_ => Analyze_Click(this, new RoutedEventArgs()), _ => HasSingleSelectedFile);
        CmdValidate = CreateAction(_ => Validate_Click(this, new RoutedEventArgs()), _ => HasFiles);
        CmdAutosizeColumns = CreateAction(_ => AutosizeColumns());
        CmdFixConflicts = CreateAction(_ => FixConflictingNames(), _ => HasFiles);
        CmdAnalyzeSampleText = CreateAction(_ => AnalyzeSampleText(), _ => HasRules);
        CmdApplyRulesToClipboard = CreateAction(_ => ApplyRulesToClipboard(), _ => HasRules);
        CmdCountFiles = CreateAction(_ => CountFiles(), _ => HasFiles);
        CmdSortForFolders = CreateAction(_ => SortForFolders(), _ => HasFiles);

        CmdEditNewName = CreateAction(_ => EditNewName_Click(this, new RoutedEventArgs()), _ => HasSingleSelectedFile);
        CmdOpenFile = CreateAction(_ => OpenFile_Click(this, new RoutedEventArgs()), _ => HasSingleSelectedFile);
        CmdOpenFolder = CreateAction(_ => OpenFolder_Click(this, new RoutedEventArgs()), _ => HasSingleSelectedFile);
        CmdOpenWithNotepad = CreateAction(_ => OpenWithNotepad_Click(this, new RoutedEventArgs()), _ => HasSingleSelectedFile);
        CmdFileProperties = CreateAction(_ => FileProperties_Click(this, new RoutedEventArgs()), _ => HasSingleSelectedFile);
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
        CmdMoveFileUp = CreateAction(_ => MoveFileUp_Click(this, new RoutedEventArgs()), _ => HasSingleSelectedFile);
        CmdMoveFileDown = CreateAction(_ => MoveFileDown_Click(this, new RoutedEventArgs()), _ => HasSingleSelectedFile);
        CmdRemoveSelectedFiles = CreateAction(_ => RemoveSelectedFiles_Click(this, new RoutedEventArgs()), _ => HasSelectedFiles || HasMarkedFiles);
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

    private void RequestActionsRefresh()
    {
        if (_actionsRefreshPending)
            return;

        _actionsRefreshPending = true;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _actionsRefreshPending = false;
            RefreshActions();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private bool HasFiles => _totalFileCount > 0;
    private bool HasRules => Rules.Count > 0;
    private bool HasSelectedRule => lvRules.SelectedItem != null;
    private bool HasSelectedFiles => _selectedFileCount > 0;
    private bool HasSingleSelectedFile => _selectedFileCount == 1;
    private bool HasMarkedFiles => _markedFileCount > 0;
    private bool HasRenamedFiles => _renamedFileCount > 0;
    private bool CanRename => _renameableFileCount > 0;
    private bool CanMoveRuleUp => lvRules.SelectedIndex > 0;
    private bool CanMoveRuleDown => lvRules.SelectedIndex >= 0 && lvRules.SelectedIndex < Rules.Count - 1;
    private bool IsWin32FileList => false;

    private static readonly HashSet<string> DefaultVisibleColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        FileReorderColumnKey,
        "IsMarked", "State", "OriginalName", "NewName", "Error"
    };

    private static bool IsForcedVisibleFileColumn(string key)
        => string.Equals(key, FileReorderColumnKey, StringComparison.OrdinalIgnoreCase);

    private void HideExtraColumns()
    {
        foreach (var col in lvFiles.Columns)
        {
            var key = GetColumnKey(col);
            if (!DefaultVisibleColumns.Contains(key) && !IsForcedVisibleFileColumn(key))
                col.Visibility = Visibility.Collapsed;
        }

        if (IsWin32FileList)
        {
            foreach (var col in _fileListColumns)
                col.Visible = DefaultVisibleColumns.Contains(col.Key) || IsForcedVisibleFileColumn(col.Key);
            _win32FileListView?.ApplyColumnVisibility();
        }
    }

    private void BuildFileListColumns()
    {
        _fileListColumns.Clear();
        foreach (var col in lvFiles.Columns)
        {
            var key = GetColumnKey(col);
            if (string.IsNullOrWhiteSpace(key))
                continue;

            var headerText = GetColumnHeaderText(key, col);
            var width = col.Width.DisplayValue;
            if (double.IsNaN(width) || width <= 0)
                width = col.Width.Value > 0 ? col.Width.Value : 80;

            var listCol = new FileListColumn(key, headerText)
            {
                Width = (int)Math.Round(width),
                Visible = col.Visibility == Visibility.Visible,
                IsCheckBox = string.Equals(key, "IsMarked", StringComparison.OrdinalIgnoreCase),
                Sortable = !string.IsNullOrWhiteSpace(col.SortMemberPath)
            };
            ApplyFileListColumnMappings(listCol);
            _fileListColumns.Add(listCol);
        }
    }

    private void InitializeFileListView()
    {
        _fileListView = _wpfFileListView;
        _fileListView.ItemsSource = Files;
        ConfigureWpfCollectionView();
        win32FileListHost.Visibility = Visibility.Collapsed;
        lvFiles.Visibility = Visibility.Visible;
    }

    private void InitializeWin32FileListView(Win32FileListViewHost host)
    {
        host.CellTextProvider = (row, col) => GetWin32CellText(row, col);
        host.CellStyleProvider = (row, col, selected) => GetWin32CellStyle(row, col, selected);
        host.ItemCheckedProvider = row => GetWin32ItemChecked(row);
        host.ItemCheckedChanged = (row, isChecked) => OnWin32ItemCheckedChanged(row, isChecked);
        host.ItemActivated = (row, col) => OnWin32ItemActivated(row, col);
        host.ColumnHeaderClick = col => OnWin32ColumnHeaderClick(col);
        host.ColumnHeaderRightClick = _ => ShowWin32ColumnsMenuAtCursor();
        host.ColumnHeaderAutoSize = col => OnWin32ColumnHeaderAutoSize(col);
        host.ContextMenuRequested = pt => ShowWin32ContextMenu(pt);
        host.ReorderRequest = (from, to) => OnWin32ReorderRequest(from, to);
        host.ExternalFilesDropped = files => AddFilePaths(files, recursive: true, forcePreview: _appSettings.AutoPreview);
        host.KeyGesture = (key, modifiers) => OnWin32KeyGesture(key, modifiers);
        host.HeaderCheckToggle = _ => ToggleAllMarks();
        host.SelectionChanged += (_, _) => HandleFileSelectionChanged();
        ApplyWin32Palette(host);
        UpdateWin32HeaderCheckState();
    }

    private void ConfigureWpfCollectionView()
    {
        if (CollectionViewSource.GetDefaultView(Files) is ICollectionViewLiveShaping liveShaping && liveShaping.CanChangeLiveSorting)
        {
            liveShaping.LiveSortingProperties.Clear();
            foreach (var property in MutableSortProperties)
                liveShaping.LiveSortingProperties.Add(property);
        }
    }

    private void UpdateWpfLiveSorting(string? sortProperty)
    {
        if (CollectionViewSource.GetDefaultView(Files) is not ICollectionViewLiveShaping liveShaping || !liveShaping.CanChangeLiveSorting)
            return;

        liveShaping.IsLiveSorting = !string.IsNullOrWhiteSpace(sortProperty)
            && MutableSortProperties.Contains(sortProperty);
    }

    private void ClearWpfSort()
    {
        if (CollectionViewSource.GetDefaultView(lvFiles.ItemsSource) is not ICollectionView view)
            return;

        using (view.DeferRefresh())
            view.SortDescriptions.Clear();

        UpdateWpfLiveSorting(null);
        foreach (var col in lvFiles.Columns)
            col.SortDirection = null;
    }

    private void ClearActiveFileSort()
    {
        if (IsWin32FileList)
        {
            _viewSortKey = null;
            _viewSortDirection = null;
            _win32FileListView?.SetSortIndicator(-1, null);
            return;
        }

        ClearWpfSort();
    }

    private void RebuildViewItems(bool preserveSelection)
    {
        if (!IsWin32FileList)
            return;

        List<RenFile>? selected = null;
        if (preserveSelection)
            selected = _fileListView.SelectedItems.Cast<RenFile>().ToList();

        _viewItems.Clear();
        _viewItems.AddRange(Files);

        if (!string.IsNullOrWhiteSpace(_viewSortKey) && _viewSortDirection.HasValue)
            ApplyViewSort();

        _fileListView.ItemsSource = _viewItems;

        if (!string.IsNullOrWhiteSpace(_viewSortKey) && _viewSortDirection.HasValue && _win32FileListView != null)
        {
            var idx = _fileListColumns.FindIndex(c => string.Equals(c.Key, _viewSortKey, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
                _win32FileListView.SetSortIndicator(idx, _viewSortDirection);
        }

        if (selected != null && selected.Count > 0)
            _fileListView.SetSelectedItems(selected);

        LogImportDiag($"[ViewRebuild] preserveSelection={preserveSelection} files={Files.Count} view={_viewItems.Count} selectedRestored={(selected?.Count ?? 0)} sortKey={_viewSortKey ?? "<none>"} sortDir={_viewSortDirection?.ToString() ?? "<none>"}");
    }

    private void ApplyViewSort()
    {
        if (string.IsNullOrWhiteSpace(_viewSortKey) || !_viewSortDirection.HasValue)
            return;

        var column = _fileListColumns.FirstOrDefault(c => string.Equals(c.Key, _viewSortKey, StringComparison.OrdinalIgnoreCase));
        if (column?.SortValueGetter == null)
            return;

        _viewItems.Sort((a, b) =>
        {
            var av = column.SortValueGetter(a);
            var bv = column.SortValueGetter(b);
            var result = Comparer<IComparable>.Default.Compare(av, bv);
            return _viewSortDirection == ListSortDirection.Ascending ? result : -result;
        });
    }

    private void ClearWin32Sort()
    {
        var selected = GetSelectedFiles().ToList();
        _viewSortKey = null;
        _viewSortDirection = null;
        _viewItems.Clear();
        _viewItems.AddRange(Files);
        _win32FileListView?.SetSortIndicator(-1, null);
        _fileListView.Refresh();
        if (selected.Count > 0)
            _fileListView.SetSelectedItems(selected);
    }

    private void ApplyFileListColumnMappings(FileListColumn col)
    {
        col.TextGetter = col.Key switch
        {
            FileReorderColumnKey => _ => string.Empty,
            "IsMarked" => _ => string.Empty,
            "State" => f => ((RenFile)f).State,
            "OriginalName" => f => ((RenFile)f).OriginalName,
            "NewName" => f => ((RenFile)f).NewName,
            "FolderPath" => f => ((RenFile)f).FolderPath,
            "Extension" => f => ((RenFile)f).Extension,
            "SizeDisplay" => f => ((RenFile)f).SizeDisplay,
            "Error" => f => ((RenFile)f).Error ?? string.Empty,
            "FolderName" => f => ((RenFile)f).FolderName,
            "NewPath" => f => ((RenFile)f).NewPath,
            "SizeKB" => f => ((RenFile)f).SizeKB,
            "SizeMB" => f => ((RenFile)f).SizeMB,
            "CreatedDisplay" => f => ((RenFile)f).CreatedDisplay,
            "ModifiedDisplay" => f => ((RenFile)f).ModifiedDisplay,
            "ExifDateDisplay" => f => ((RenFile)f).ExifDateDisplay,
            "NameDigits" => f => ((RenFile)f).NameDigits,
            "PathDigits" => f => ((RenFile)f).PathDigits,
            "NameLength" => f => ((RenFile)f).NameLength.ToString(),
            "NewNameLength" => f => ((RenFile)f).NewNameLength.ToString(),
            "PathLength" => f => ((RenFile)f).PathLength.ToString(),
            "NewPathLength" => f => ((RenFile)f).NewPathLength.ToString(),
            "OldPath" => f => ((RenFile)f).OldPath,
            _ => _ => string.Empty
        };

        col.SortValueGetter = col.Key switch
        {
            FileReorderColumnKey => _ => string.Empty,
            "IsMarked" => f => ((RenFile)f).IsMarked,
            "State" => f => ((RenFile)f).State,
            "OriginalName" => f => ((RenFile)f).OriginalName,
            "NewName" => f => ((RenFile)f).NewName,
            "FolderPath" => f => ((RenFile)f).FolderPath,
            "Extension" => f => ((RenFile)f).Extension,
            "SizeDisplay" => f => ((RenFile)f).Size,
            "Error" => f => ((RenFile)f).Error ?? string.Empty,
            "FolderName" => f => ((RenFile)f).FolderName,
            "NewPath" => f => ((RenFile)f).NewPath,
            "SizeKB" => f => ((RenFile)f).Size,
            "SizeMB" => f => ((RenFile)f).Size,
            "CreatedDisplay" => f => ((RenFile)f).Created,
            "ModifiedDisplay" => f => ((RenFile)f).Modified,
            "ExifDateDisplay" => f => ((RenFile)f).ExifDate ?? DateTime.MinValue,
            "NameDigits" => f => ((RenFile)f).NameDigits,
            "PathDigits" => f => ((RenFile)f).PathDigits,
            "NameLength" => f => ((RenFile)f).NameLength,
            "NewNameLength" => f => ((RenFile)f).NewNameLength,
            "PathLength" => f => ((RenFile)f).PathLength,
            "NewPathLength" => f => ((RenFile)f).NewPathLength,
            "OldPath" => f => ((RenFile)f).OldPath,
            _ => _ => string.Empty
        };

        if (string.Equals(col.Key, "IsMarked", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(col.Key, FileReorderColumnKey, StringComparison.OrdinalIgnoreCase))
            col.Sortable = false;
    }

    private string GetColumnHeaderText(string key, DataGridColumn col)
    {
        if (col.Header is string header && !string.IsNullOrWhiteSpace(header))
            return header;
        if (col.Header is TextBlock tb && !string.IsNullOrWhiteSpace(tb.Text))
            return tb.Text;

        return key switch
        {
            FileReorderColumnKey => string.Empty,
            "IsMarked" => string.Empty,
            "State" => LanguageService.GetString("Column_State"),
            "OriginalName" => LanguageService.GetString("Column_Name"),
            "NewName" => LanguageService.GetString("Column_NewName"),
            "FolderPath" => LanguageService.GetString("Column_Path"),
            "Extension" => LanguageService.GetString("Column_Extension"),
            "SizeDisplay" => LanguageService.GetString("Column_Size"),
            "Error" => LanguageService.GetString("Column_Error"),
            "FolderName" => LanguageService.GetString("Column_Folder"),
            "NewPath" => LanguageService.GetString("Column_NewPath"),
            "SizeKB" => LanguageService.GetString("Column_SizeKB"),
            "SizeMB" => LanguageService.GetString("Column_SizeMB"),
            "CreatedDisplay" => LanguageService.GetString("Column_Created"),
            "ModifiedDisplay" => LanguageService.GetString("Column_Modified"),
            "ExifDateDisplay" => LanguageService.GetString("Column_ExifDate"),
            "NameDigits" => LanguageService.GetString("Column_NameDigits"),
            "PathDigits" => LanguageService.GetString("Column_PathDigits"),
            "NameLength" => LanguageService.GetString("Column_NameLength"),
            "NewNameLength" => LanguageService.GetString("Column_NewNameLength"),
            "PathLength" => LanguageService.GetString("Column_PathLength"),
            "NewPathLength" => LanguageService.GetString("Column_NewPathLength"),
            "OldPath" => LanguageService.GetString("Column_OldPath"),
            _ => key
        };
    }

    private static string GetColumnKey(DataGridColumn col)
    {
        if (col is DataGridBoundColumn boundColumn
            && boundColumn.Binding is Binding binding
            && !string.IsNullOrWhiteSpace(binding.Path?.Path))
        {
            return binding.Path.Path;
        }

        if (!string.IsNullOrWhiteSpace(col.SortMemberPath))
            return col.SortMemberPath;

        if (col.Header is string s)
            return s;

        if (col.Header is TextBlock tb && !string.IsNullOrWhiteSpace(tb.Text))
            return tb.Text;

        return col.Header?.ToString() ?? string.Empty;
    }

    private static double GetPersistedWidth(DataGridColumn col)
    {
        var width = col.Width.DisplayValue;
        if (double.IsNaN(width) || width <= 0)
            width = col.ActualWidth;

        return (double.IsNaN(width) || width <= 0) ? 0 : width;
    }

    private void UpdateColumnWidthCache(DataGridColumn col)
    {
        var key = GetColumnKey(col);
        if (string.IsNullOrWhiteSpace(key))
            return;

        var width = GetPersistedWidth(col);
        if (width > 0)
            _appSettings.ColumnWidths[key] = width;
    }

    private void UpdateColumnWidthCache(FileListColumn col, int width)
    {
        if (string.IsNullOrWhiteSpace(col.Key) || width <= 0)
            return;

        _appSettings.ColumnWidths[col.Key] = width;
    }

    private void CaptureDefaultColumnWidths()
    {
        _defaultColumnWidths.Clear();
        if (IsWin32FileList)
        {
            foreach (var col in _fileListColumns)
            {
                if (string.IsNullOrEmpty(col.Key))
                    continue;
                _defaultColumnWidths[col.Key] = col.Width;
            }
            return;
        }

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

    private void OnLanguageChanged()
    {
        UpdateLanguageMenuChecks();
        UpdateTrayIconState();
        foreach (var rule in Rules.OfType<RuleBase>())
            rule.NotifyLocalizationChanged();
        UpdateWin32ColumnHeaders();
    }

    private void UpdateWin32ColumnHeaders()
    {
        if (!IsWin32FileList || _win32FileListView == null)
            return;

        var count = Math.Min(lvFiles.Columns.Count, _fileListColumns.Count);
        for (int i = 0; i < count; i++)
        {
            var key = GetColumnKey(lvFiles.Columns[i]);
            var header = GetColumnHeaderText(key, lvFiles.Columns[i]);
            _fileListColumns[i].Header = header;
            _win32FileListView.UpdateColumnHeader(i, header);
        }
    }

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
        catch (Exception ex)
        {
            LogException("GetFlagEmoji", ex);
        }
        return "🏳️";
    }

    private void SwitchToChinese_Click(object sender, RoutedEventArgs e) => LanguageService.SwitchToChinese();
    private void SwitchToEnglish_Click(object sender, RoutedEventArgs e) => LanguageService.SwitchToEnglish();

    #region Status Bar

    private void UpdateStatusBar()
    {
        tbStatusFiles.Text = LanguageService.GetString("Status_FilesSummary", _totalFileCount, _markedFileCount, _selectedFileCount);
    }

    private void RequestStatusBarUpdate()
    {
        if (_fileSelectionBatchDepth > 0)
        {
            _fileSelectionStatusDirty = true;
            return;
        }

        if (_statusBarUpdatePending)
            return;

        _statusBarUpdatePending = true;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _statusBarUpdatePending = false;
            if (_fileSelectionBatchDepth > 0)
            {
                _fileSelectionStatusDirty = true;
                return;
            }

            UpdateStatusBar();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void BeginFileSelectionBatch()
    {
        _fileSelectionBatchDepth++;
    }

    private void EndFileSelectionBatch()
    {
        _fileSelectionBatchDepth--;
        if (_fileSelectionBatchDepth == 0 && _fileSelectionStatusDirty)
        {
            _fileSelectionStatusDirty = false;
            RequestStatusBarUpdate();
        }

        if (_fileSelectionBatchDepth == 0)
            RequestActionsRefresh();
    }

    private void RunFileSelectionBatch(Action action)
    {
        BeginFileSelectionBatch();
        try
        {
            action();
        }
        finally
        {
            EndFileSelectionBatch();
        }
    }

    private void RecalculateFileStateCache()
    {
        _totalFileCount = Files.Count;
        _markedFileCount = 0;
        _renamedFileCount = 0;
        _renameableFileCount = 0;

        foreach (var file in Files)
        {
            if (file.IsMarked)
                _markedFileCount++;
            if (file.IsRenamed)
                _renamedFileCount++;
            if (file.IsMarked && file.HasChanged)
                _renameableFileCount++;
        }

        _fileStateCacheDirty = false;
    }

    private bool IsFileMutationBatchActive => _wpfFilesBatchDepth > 0 || _win32FilesBatchDepth > 0;

    private void MarkFileStateCacheDirty()
    {
        _fileStateCacheDirty = true;
    }

    private void RequestFileStateUiFlush()
    {
        if (Interlocked.Exchange(ref _fileStateUiFlushPending, 1) == 1)
            return;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            Interlocked.Exchange(ref _fileStateUiFlushPending, 0);
            MarkFileStateCacheDirty();
            if (IsFileMutationBatchActive)
                return;

            FlushFileStateUi();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void FlushFileStateUi()
    {
        if (_fileStateCacheDirty)
            RecalculateFileStateCache();

        _selectedFileCount = _fileListView.SelectedCount;
        UpdateStatusBar();
        RefreshActions();
        UpdateWin32HeaderCheckState();
    }

    private void BeginWpfFilesBatch()
    {
        if (IsWin32FileList)
            return;

        _wpfFilesBatchDepth++;
    }

    private void EndWpfFilesBatch(IReadOnlyCollection<RenFile>? selectionToRestore = null, bool clearSelection = false)
    {
        if (IsWin32FileList || _wpfFilesBatchDepth <= 0)
            return;

        _wpfFilesBatchDepth--;
        if (_wpfFilesBatchDepth != 0)
            return;

        if (clearSelection)
        {
            RunFileSelectionBatch(() => _fileListView.ClearSelection());
        }
        else if (selectionToRestore != null)
        {
            var survivors = selectionToRestore.Where(Files.Contains).ToList();
            RunFileSelectionBatch(() => SetSelectedFiles(survivors));
        }

        FlushFileStateUi();
        if (_autoPreviewQueuedDuringBatch)
        {
            _autoPreviewQueuedDuringBatch = false;
            if (_appSettings.AutoPreview && Rules.Count > 0)
                _ = ExecutePreviewAsync(isAutoTrigger: true, debounceMilliseconds: AutoPreviewDebounceMilliseconds);
        }
    }

    private void RunWpfFilesBatch(Action action, bool preserveSelection = false, bool clearSelection = false, bool queueAutoPreview = false)
    {
        if (IsWin32FileList)
        {
            action();
            return;
        }

        var selectionToRestore = preserveSelection ? GetSelectedFiles().ToList() : null;
        BeginWpfFilesBatch();
        try
        {
            action();
            if (queueAutoPreview)
                _autoPreviewQueuedDuringBatch = true;
        }
        finally
        {
            EndWpfFilesBatch(selectionToRestore, clearSelection);
        }
    }

    private void BeginWin32FilesBatch()
    {
        if (!IsWin32FileList)
            return;
        if (_win32FilesBatchDepth == 0)
        {
            _win32FilesDeferredChangeCount = 0;
            _win32LastDeferredAction = null;
        }
        _win32FilesBatchDepth++;
    }

    private void EndWin32FilesBatch()
    {
        if (!IsWin32FileList || _win32FilesBatchDepth <= 0)
            return;

        _win32FilesBatchDepth--;
        if (_win32FilesBatchDepth != 0)
            return;

        var changed = _win32FilesChangedWhileBatch;
        var deferredCount = _win32FilesDeferredChangeCount;
        var lastAction = _win32LastDeferredAction;
        _win32FilesChangedWhileBatch = false;
        _win32FilesDeferredChangeCount = 0;
        _win32LastDeferredAction = null;
        if (!changed)
            return;

        LogImportDiag(
            $"[BatchEnd] rebuild-start files={Files.Count} view={_viewItems.Count} " +
            $"deferredCount={deferredCount} lastAction={lastAction?.ToString() ?? "<none>"}");
        RebuildViewItems(preserveSelection: false);
        LogImportDiag($"[BatchEnd] rebuild-end files={Files.Count} view={_viewItems.Count}");
        FlushFileStateUi();
    }

    private IReadOnlyList<RenFile> GetSelectedFiles()
        => _fileListView.SelectedItems.Cast<RenFile>().ToList();

    private RenFile? GetSelectedFile()
        => _fileListView.SelectedCount == 1 ? _fileListView.SelectedItem as RenFile : null;

    private void SetSelectedFiles(IEnumerable<RenFile> files)
        => _fileListView.SetSelectedItems(files is IList list ? list : files.ToList());

    private void RefreshFileList()
        => _fileListView.Refresh();

    private Task SelectAllFilesInChunksAsync()
    {
        if (_largeSelectionInProgress || Files.Count == 0)
            return Task.CompletedTask;

        if (IsWin32FileList)
        {
            var sw = Stopwatch.StartNew();
            RunFileSelectionBatch(() => _fileListView.SelectAll());
            sw.Stop();
            App.LogInputDebug($"[SelectAll] win32 count={Files.Count} totalMs={sw.ElapsedMilliseconds}");
            return Task.CompletedTask;
        }

        _largeSelectionInProgress = true;
        try
        {
            var sw = Stopwatch.StartNew();
            RunFileSelectionBatch(() => _fileListView.SelectAll());
            sw.Stop();
            var mode = _wpfFileListView.IsLogicalSelectAllActive ? "logical-large" : "native-large";
            App.LogInputDebug($"[SelectAll] {mode} count={Files.Count} totalMs={sw.ElapsedMilliseconds}");
        }
        finally
        {
            _largeSelectionInProgress = false;
        }

        return Task.CompletedTask;
    }

    private void SetMarkedInBulk(IEnumerable<RenFile> files, Func<RenFile, bool> isMarkedSelector, bool autoPreview = true)
    {
        var targets = files?.Distinct().ToList() ?? new List<RenFile>();
        if (targets.Count == 0)
            return;

        var changed = false;
        if (IsWin32FileList)
        {
            foreach (var file in targets)
                changed |= file.SetMarkedSilently(isMarkedSelector(file));
        }
        else
        {
            RunWpfFilesBatch(() =>
            {
                foreach (var file in targets)
                {
                    var newValue = isMarkedSelector(file);
                    if (file.IsMarked == newValue)
                        continue;

                    file.IsMarked = newValue;
                    changed = true;
                }
                if (changed && autoPreview)
                    _autoPreviewQueuedDuringBatch = true;
            }, preserveSelection: true);
        }

        if (changed && IsWin32FileList)
        {
            RefreshFileList();
            if (autoPreview && Rules.Count > 0 && _appSettings.AutoPreview)
                _ = ExecutePreviewAsync(isAutoTrigger: true, debounceMilliseconds: AutoPreviewDebounceMilliseconds);
        }

        if (IsWin32FileList)
            FlushFileStateUi();
    }

    private void NotifyPreviewRefreshNeededForFileOrder()
    {
        if (Files.Count == 0 || Rules.Count == 0)
            return;

        tbStatusInfo.Text = LanguageService.GetString("Status_PreviewRefreshNeeded_Order");
    }

    private void NotifyPreviewRefreshNeededForMarking()
    {
        if (Files.Count == 0 || Rules.Count == 0)
            return;

        tbStatusInfo.Text = LanguageService.GetString("Status_PreviewRefreshNeeded_Marking");
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

    internal void AddFilePaths(IEnumerable<string> paths, bool recursive, bool forcePreview = false)
    {
        var traceId = Interlocked.Increment(ref _importTraceSequence);
        var importSw = Stopwatch.StartNew();
        var pathList = paths?.ToList() ?? new List<string>();
        var useWin32Batch = IsWin32FileList;
        var useWpfBatch = !useWin32Batch;
        List<RenFile>? selectionToRestore = null;
        if (useWin32Batch)
        {
            BeginWin32FilesBatch();
        }
        else
        {
            selectionToRestore = GetSelectedFiles().ToList();
            BeginWpfFilesBatch();
        }

        LogImportDiag(
            $"[Import#{traceId}] start paths={pathList.Count} recursive={recursive} forcePreview={forcePreview} " +
            $"engine={(IsWin32FileList ? "Win32" : "WPF")} filesBefore={Files.Count} viewBefore={_viewItems.Count} " +
            $"includeAllFiles={_appSettings.FolderIncludeAllFiles} includeFolders={_appSettings.FolderIncludeFolderNames} includeSubfolders={_appSettings.FolderIncludeSubfolders} " +
            $"includeMask='{_appSettings.FolderIncludeMask}' excludeMask='{_appSettings.FolderExcludeMask}'");

        var added = new List<RenFile>();
        int skippedCount = 0;
        int duplicateInputCount = 0;
        int pathNotFoundCount = 0;

        try
        {
            if (pathList.Any(Directory.Exists))
                EnsureFolderImportDefaultsForFirstUse();
            var importService = new FileImportService(
                CreateFileImportOptions(recursive),
                LogImportDiag);
            var batch = importService.BuildBatch(pathList, Files.Select(f => f.FullPath), traceId);
            added = batch.Items.ToList();
            skippedCount = batch.SkippedCount;
            duplicateInputCount = batch.DuplicateInputCount;
            pathNotFoundCount = batch.PathNotFoundCount;

            if (added.Count > 0)
                Files.AddRange(added);
        }
        finally
        {
            if (useWin32Batch)
                EndWin32FilesBatch();
            else
                EndWpfFilesBatch(selectionToRestore);
        }

        if (_appSettings.FiltersApplied && added.Count > 0)
        {
            ApplyFiltersToFiles(added, autoPreview: false);
        }

        if (added.Count > 0
            && Rules.Count > 0
            && (forcePreview || _appSettings.PreviewOnFileAdd))
        {
            _ = ExecutePreviewAsync(isAutoTrigger: true, debounceMilliseconds: 0);
        }

        if (skippedCount > 0)
        {
            tbStatusInfo.Text = $"Skipped {skippedCount} path(s) due to access restrictions.";
        }

        importSw.Stop();
        LogImportDiag(
            $"[Import#{traceId}] end added={added.Count} skipped={skippedCount} duplicates={duplicateInputCount} notFound={pathNotFoundCount} " +
            $"filesAfter={Files.Count} viewAfter={_viewItems.Count} filtersApplied={_appSettings.FiltersApplied} elapsedMs={importSw.ElapsedMilliseconds}");
    }

    private void EnsureFolderImportDefaultsForFirstUse()
    {
        if (!_appSettings.ApplyFirstFolderImportDefaultsIfNeeded())
            return;

        SaveSettingsWithFeedback("folder import defaults", showDialog: false);
    }

    private FileImportOptions CreateFileImportOptions(bool recursive)
        => new(
            Recursive: recursive,
            IncludeAllFiles: _appSettings.FolderIncludeAllFiles,
            IncludeFolderNames: _appSettings.FolderIncludeFolderNames,
            IncludeSubfolders: _appSettings.FolderIncludeSubfolders,
            IncludeHiddenFiles: _appSettings.FolderIncludeHiddenFiles,
            IncludeSystemFiles: _appSettings.FolderIncludeSystemFiles,
            IgnoreRootFolder: _appSettings.FolderIgnoreRootFolder,
            MaskFileNameOnly: _appSettings.FolderMaskFileNameOnly,
            IncludeMask: _appSettings.FolderIncludeMask,
            ExcludeMask: _appSettings.FolderExcludeMask);

    private void AddDirectoryEntries(string rootPath, bool recursive, HashSet<string> existing, List<RenFile> added, ref int skippedCount, long traceId)
    {
        var scanSw = Stopwatch.StartNew();
        var allowSubfolders = _appSettings.FolderIncludeSubfolders;
        var searchOption = allowSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var includeFiles = _appSettings.FolderIncludeAllFiles;
        var includeMasks = ParseFolderMasks(_appSettings.FolderIncludeMask);
        var excludeMasks = ParseFolderMasks(_appSettings.FolderExcludeMask);

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

        LogImportDiag(
            $"[Import#{traceId}] dir-scan root='{rootPath}' allowSubfolders={allowSubfolders} includeFiles={includeFiles} includeFolders={_appSettings.FolderIncludeFolderNames} searchOption={searchOption}");

        var childDirs = allowSubfolders
            ? SafeEnumerateDirectories(rootPath, SearchOption.AllDirectories, ref skippedCount).ToList()
            : [];
        var traversalRoots = new List<string>(childDirs.Count + 1) { rootPath };
        traversalRoots.AddRange(childDirs);

        if (_appSettings.FolderIncludeFolderNames)
        {
            List<string> dirs;
            if (allowSubfolders)
            {
                dirs = _appSettings.FolderIgnoreRootFolder ? childDirs : traversalRoots;
            }
            else
            {
                dirs = _appSettings.FolderIgnoreRootFolder ? [] : [rootPath];
            }

            dirCandidates = dirs.Count;
            var acceptedDirs = new List<string>(dirs.Count);
            var acceptedDirSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dir in dirs)
            {
                if (!PassesFolderImportAttributes(dir))
                {
                    dirFilteredAttr++;
                    if (dirFilteredAttr <= 5)
                        LogImportDiag($"[Import#{traceId}] dir-filter-attr '{dir}'");
                    continue;
                }

                if (!PassesFolderImportMasks(dir, isFolder: true, includeMasks, excludeMasks))
                {
                    dirFilteredMask++;
                    if (dirFilteredMask <= 5)
                        LogImportDiag($"[Import#{traceId}] dir-filter-mask '{dir}'");
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

                Files.Add(rf);
                added.Add(rf);
                existing.Add(acceptedDirs[i]);
                dirAdded++;
                if (dirAdded <= 5)
                    LogImportDiag($"[Import#{traceId}] dir-added '{acceptedDirs[i]}'");
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
                if (!PassesFolderImportAttributes(file))
                {
                    fileFilteredAttr++;
                    if (fileFilteredAttr <= 5)
                        LogImportDiag($"[Import#{traceId}] file-filter-attr '{file}'");
                    continue;
                }

                if (!PassesFolderImportMasks(file, isFolder: false, includeMasks, excludeMasks))
                {
                    fileFilteredMask++;
                    if (fileFilteredMask <= 5)
                        LogImportDiag($"[Import#{traceId}] file-filter-mask '{file}'");
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

                Files.Add(rf);
                added.Add(rf);
                existing.Add(acceptedFiles[i]);
                fileAdded++;
                if (fileAdded <= 8)
                    LogImportDiag($"[Import#{traceId}] file-added '{acceptedFiles[i]}'");
            }
        }

        scanSw.Stop();
        LogImportDiag(
            $"[Import#{traceId}] dir-scan-summary root='{rootPath}' " +
            $"dirCandidates={dirCandidates} dirAdded={dirAdded} dirFilteredAttr={dirFilteredAttr} dirFilteredMask={dirFilteredMask} dirDuplicate={dirDuplicate} " +
            $"fileCandidates={fileCandidates} fileAdded={fileAdded} fileFilteredAttr={fileFilteredAttr} fileFilteredMask={fileFilteredMask} fileDuplicate={fileDuplicate} skipped={skippedCount} elapsedMs={scanSw.ElapsedMilliseconds}");
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
                    result[index] = new RenFile(path);
                }
                catch (Exception ex)
                {
                    sequentialFailures++;
                    if (sequentialFailures <= 5)
                        LogImportDiag($"[Import#{traceId}] {scope}-create-failed path='{path}' message='{ex.Message}'");
                }
            }

            if (sequentialFailures > 0)
                skippedCount += sequentialFailures;
            return result;
        }

        LogImportDiag($"[Import#{traceId}] {scope}-materialize mode=parallel count={paths.Count} degree={ParallelImportCreationDegree}");
        int failed = 0;
        Parallel.For(
            0,
            paths.Count,
            new ParallelOptions { MaxDegreeOfParallelism = ParallelImportCreationDegree },
            i =>
            {
                try
                {
                    result[i] = new RenFile(paths[i]);
                }
                catch
                {
                    Interlocked.Increment(ref failed);
                }
            });

        if (failed > 0)
        {
            skippedCount += failed;
            LogImportDiag($"[Import#{traceId}] {scope}-create-failed count={failed}");
        }

        return result;
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string path, SearchOption searchOption, ref int skippedCount)
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
                LogImportDiag($"[ImportEnum] dir-enum unauthorized current='{current}'");
                continue;
            }
            catch (IOException)
            {
                skippedCount++;
                LogImportDiag($"[ImportEnum] dir-enum io-failed current='{current}'");
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

    private static IEnumerable<string> SafeEnumerateFilesFromRoots(IEnumerable<string> roots, ref int skippedCount)
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
                LogImportDiag($"[ImportEnum] file-enum unauthorized current='{current}'");
            }
            catch (IOException)
            {
                skippedCount++;
                LogImportDiag($"[ImportEnum] file-enum io-failed current='{current}'");
            }
        }

        return result;
    }

    private bool PassesFolderImportSettings(string fullPath, bool isFolder)
    {
        if (!PassesFolderImportAttributes(fullPath))
            return false;
        return PassesFolderImportMasks(fullPath, isFolder);
    }

    private bool PassesFolderImportAttributes(string fullPath)
    {
        if (_appSettings.FolderIncludeHiddenFiles && _appSettings.FolderIncludeSystemFiles)
            return true;

        try
        {
            var attrs = File.GetAttributes(fullPath);
            if (!_appSettings.FolderIncludeHiddenFiles && attrs.HasFlag(FileAttributes.Hidden))
                return false;
            if (!_appSettings.FolderIncludeSystemFiles && attrs.HasFlag(FileAttributes.System))
                return false;
            return true;
        }
        catch (Exception ex)
        {
            LogException($"PassesFolderImportAttributes({fullPath})", ex);
            return false;
        }
    }

    private bool PassesFolderImportMasks(string fullPath, bool isFolder)
    {
        var includeMasks = ParseFolderMasks(_appSettings.FolderIncludeMask);
        var excludeMasks = ParseFolderMasks(_appSettings.FolderExcludeMask);
        return PassesFolderImportMasks(fullPath, isFolder, includeMasks, excludeMasks);
    }

    private bool PassesFolderImportMasks(string fullPath, bool isFolder, string[] includeMasks, string[] excludeMasks)
    {
        var target = _appSettings.FolderMaskFileNameOnly ? Path.GetFileName(fullPath) : fullPath;
        if (string.IsNullOrEmpty(target))
            return false;

        if (includeMasks.Length > 0 && !includeMasks.Any(mask => MatchWildcard(target, mask, false)))
            return false;

        if (excludeMasks.Any(mask => MatchWildcard(target, mask, false)))
            return false;

        return true;
    }

    private static string[] ParseFolderMasks(string? rawMasks)
    {
        if (string.IsNullOrWhiteSpace(rawMasks))
            return [];

        return rawMasks
            .Split(';')
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToArray();
    }

    private void ApplyFiltersToFiles(IEnumerable<RenFile> files, bool autoPreview = true)
    {
        SetMarkedInBulk(files, PassesFilters, autoPreview);
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
            catch (Exception ex)
            {
                LogException($"PassesFilters({file.FullPath})", ex);
                pass = false;
            }
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
            AddFilePaths(paths, recursive: true, forcePreview: _appSettings.AutoPreview);
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
        catch (Exception ex)
        {
            LogException("CloneRule", ex);
            return null;
        }
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
            Rules.Move(index, index - 1);
            lvRules.SelectedIndex = index - 1;
            AutoPreviewIfEnabled(sender, e);
        }
    }

    private void MoveRuleDown_Click(object sender, RoutedEventArgs e)
    {
        var index = lvRules.SelectedIndex;
        if (index >= 0 && index < Rules.Count - 1)
        {
            Rules.Move(index, index + 1);
            lvRules.SelectedIndex = index + 1;
            AutoPreviewIfEnabled(sender, e);
        }
    }

    private void Rules_DoubleClick(object sender, MouseButtonEventArgs e) => EditRule_Click(sender, e);

    private void Rules_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshActions();

    private void Rules_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement handle || handle.DataContext is not IRule rule)
        {
            _draggedRule = null;
            _draggedRuleIndex = -1;
            _lastRuleDragPreviewSignature = null;
            return;
        }

        _rulesDragStartPoint = e.GetPosition(null);
        _draggedRule = rule;
        _draggedRuleIndex = Rules.IndexOf(rule);
        _lastRuleDragPreviewSignature = null;
        LogRuleDragDiag($"[RuleDrag] mouse-down dragIndex={_draggedRuleIndex} rule='{DescribeRule(rule)}' start=({_rulesDragStartPoint.X:F1},{_rulesDragStartPoint.Y:F1})");
        lvRules.SelectedItem = rule;
        handle.CaptureMouse();
        e.Handled = true;
    }

    private void Rules_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not UIElement handle || e.LeftButton != MouseButtonState.Pressed || _draggedRule == null || _draggedRuleIndex < 0)
            return;

        var position = e.GetPosition(null);
        if (Math.Abs(position.X - _rulesDragStartPoint.X) >= SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(position.Y - _rulesDragStartPoint.Y) >= SystemParameters.MinimumVerticalDragDistance)
        {
            if (handle.IsMouseCaptured)
                handle.ReleaseMouseCapture();

            try
            {
                var dragData = CreateRuleDragDataObject(_draggedRuleIndex);
                LogRuleDragDiag(
                    $"[RuleDrag] do-drag-drop dragIndex={_draggedRuleIndex} rule='{DescribeRule(_draggedRule)}' " +
                    $"start=({_rulesDragStartPoint.X:F1},{_rulesDragStartPoint.Y:F1}) current=({position.X:F1},{position.Y:F1})");
                DragDrop.DoDragDrop(handle, dragData, DragDropEffects.Move);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                LogException("Rules_MouseMove.DoDragDrop", ex);
                LogRuleDragDiag($"[RuleDrag] do-drag-drop-fail dragIndex={_draggedRuleIndex} rule='{DescribeRule(_draggedRule)}' message='{ex.Message}'");
                tbStatusInfo.Text = "规则拖动失败，请重试。";
            }
            finally
            {
                ClearRuleDropPreview("mouse-move-finally");
                _draggedRule = null;
                _draggedRuleIndex = -1;
            }
        }
    }

    private void RuleDragHandle_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is UIElement handle && handle.IsMouseCaptured)
            handle.ReleaseMouseCapture();

        _draggedRule = null;
        _draggedRuleIndex = -1;
        LogRuleDragDiag("[RuleDrag] mouse-up release");
        ClearRuleDropPreview("mouse-up");
    }

    private void RulesHeaderResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string columnKey })
            return;

        const double minimumUtilityWidth = 28;
        const double minimumNumberWidth = 42;
        const double minimumRuleWidth = 120;

        switch (columnKey)
        {
            case "drag":
                RuleDragColumnWidth = new GridLength(Math.Max(minimumUtilityWidth, RuleDragColumnWidth.Value + e.HorizontalChange));
                break;
            case "check":
                RuleCheckColumnWidth = new GridLength(Math.Max(minimumUtilityWidth, RuleCheckColumnWidth.Value + e.HorizontalChange));
                break;
            case "index":
                RuleIndexColumnWidth = new GridLength(Math.Max(minimumNumberWidth, RuleIndexColumnWidth.Value + e.HorizontalChange));
                break;
            case "rule":
                RuleNameColumnWidth = new GridLength(Math.Max(minimumRuleWidth, RuleNameColumnWidth.Value + e.HorizontalChange));
                break;
        }
    }

    private void Rules_DragOver(object sender, DragEventArgs e)
    {
        if (TryGetDraggedRuleIndex(e.Data, out var dragIndex))
        {
            unchecked
            {
                _ruleDropPreviewEventVersion++;
            }

            LogRuleDragDiag($"[RuleDrag] drag-over version={_ruleDropPreviewEventVersion} dragIndex={dragIndex} pt=({e.GetPosition(lvRules).X:F1},{e.GetPosition(lvRules).Y:F1})");
            UpdateRuleDropPreview(e.GetPosition(lvRules));
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            LogRuleDragDiag("[RuleDrag] drag-over ignored no-data");
            ClearRuleDropPreview("drag-over-no-data");
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void Rules_DragLeave(object sender, DragEventArgs e)
    {
        LogRuleDragDiag("[RuleDrag] drag-leave ignored to prevent preview flicker");
        e.Handled = true;
    }

    private void Rules_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetDraggedRuleIndex(e.Data, out var oldIndex))
        {
            LogRuleDragDiag("[RuleDrag] drop ignored no-data");
            ClearRuleDropPreview("drop-no-data");
            return;
        }

        var dragged = oldIndex >= 0 && oldIndex < Rules.Count
            ? Rules[oldIndex]
            : _draggedRule;
        if (dragged == null)
        {
            LogRuleDragDiag($"[RuleDrag] drop ignored missing-rule oldIndex={oldIndex}");
            ClearRuleDropPreview("drop-missing-rule");
            return;
        }

        var dropPoint = e.GetPosition(lvRules);
        oldIndex = Rules.IndexOf(dragged);
        if (oldIndex < 0)
        {
            LogRuleDragDiag($"[RuleDrag] drop ignored old-index-not-found rule='{DescribeRule(dragged)}'");
            ClearRuleDropPreview("drop-old-index-not-found");
            return;
        }

        int insertionIndex = GetRuleInsertIndex(dropPoint);
        if (insertionIndex > oldIndex)
            insertionIndex--;

        int newIndex = Math.Clamp(insertionIndex, 0, Rules.Count - 1);
        if (newIndex == oldIndex)
        {
            LogRuleDragDiag(
                $"[RuleDrag] drop no-op rule='{DescribeRule(dragged)}' oldIndex={oldIndex} " +
                $"rawInsert={insertionIndex} newIndex={newIndex} pt=({dropPoint.X:F1},{dropPoint.Y:F1})");
            ClearRuleDropPreview("drop-no-op");
            return;
        }

        LogRuleDragDiag(
            $"[RuleDrag] drop move rule='{DescribeRule(dragged)}' oldIndex={oldIndex} " +
            $"rawInsert={insertionIndex} newIndex={newIndex} pt=({dropPoint.X:F1},{dropPoint.Y:F1})");
        Rules.Move(oldIndex, newIndex);
        lvRules.SelectedItem = dragged;
        ClearRuleDropPreview("drop-complete");
        AutoPreviewIfEnabled(sender, new RoutedEventArgs());
    }

    private static DataObject CreateRuleDragDataObject(int ruleIndex)
    {
        var dragData = new DataObject();
        dragData.SetData(RuleDragIndexDataFormat, ruleIndex.ToString(CultureInfo.InvariantCulture));
        return dragData;
    }

    private static bool TryGetDraggedRuleIndex(IDataObject dataObject, out int ruleIndex)
    {
        ruleIndex = -1;
        if (!dataObject.GetDataPresent(RuleDragIndexDataFormat))
            return false;

        var raw = dataObject.GetData(RuleDragIndexDataFormat) as string;
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out ruleIndex);
    }

    private int GetRuleInsertIndex(Point point)
    {
        var row = GetRuleRowAtPoint(point);
        if (row == null)
            return Rules.Count;

        int rowIndex = row.GetIndex();
        if (rowIndex < 0)
            return Rules.Count;

        var rowTopLeft = row.TranslatePoint(new Point(0, 0), lvRules);
        bool insertAfter = point.Y > rowTopLeft.Y + (row.ActualHeight / 2);
        return insertAfter ? rowIndex + 1 : rowIndex;
    }

    private void UpdateRuleDropPreview(Point point)
    {
        if (Rules.Count == 0)
        {
            LogRuleDragDiag($"[RuleDrag] preview skipped reason=no-rules pt=({point.X:F1},{point.Y:F1})");
            ClearRuleDropPreview("preview-no-rules");
            return;
        }

        var row = GetRuleRowAtPoint(point);
        if (row != null)
        {
            int hitRowIndex = row.GetIndex();
            int rawInsertionIndex = GetRuleInsertIndex(point);
            ApplyRuleDropPreview(point, hitRowIndex, rawInsertionIndex);
            return;
        }

        if (lvRules.ItemContainerGenerator.ContainerFromIndex(0) is DataGridRow firstRow &&
            firstRow.DataContext is IRule)
        {
            var firstRowTop = firstRow.TranslatePoint(new Point(0, 0), lvRules).Y;
            if (point.Y <= firstRowTop + (firstRow.ActualHeight / 2))
            {
                ApplyRuleDropPreview(point, 0, 0);
                return;
            }
        }

        if (lvRules.ItemContainerGenerator.ContainerFromIndex(Rules.Count - 1) is DataGridRow lastRow &&
            lastRow.DataContext is IRule)
        {
            var lastRowTop = lastRow.TranslatePoint(new Point(0, 0), lvRules).Y;
            if (point.Y >= lastRowTop + (lastRow.ActualHeight / 2))
            {
                ApplyRuleDropPreview(point, Rules.Count - 1, Rules.Count);
                return;
            }
        }

        LogRuleDragDiag($"[RuleDrag] preview clear reason=no-hit pt=({point.X:F1},{point.Y:F1})");
        ClearRuleDropPreview("preview-no-hit");
    }

    private void ApplyRuleDropPreview(Point point, int? hitRowIndex, int insertionIndex)
    {
        if (Rules.Count == 0)
        {
            LogRuleDragDiag($"[RuleDrag] preview apply skipped reason=no-rules pt=({point.X:F1},{point.Y:F1})");
            ClearRuleDropPreview("apply-preview-no-rules");
            return;
        }

        insertionIndex = Math.Clamp(insertionIndex, 0, Rules.Count);
        if (insertionIndex == 0)
        {
            LogRuleDragPreviewState("preview", point, hitRowIndex, insertionIndex, 0, insertAfter: false);
            SetRuleDropPreview(Rules[0], insertAfter: false);
            return;
        }

        if (insertionIndex >= Rules.Count)
        {
            LogRuleDragPreviewState("preview", point, hitRowIndex, insertionIndex, Rules.Count - 1, insertAfter: true);
            SetRuleDropPreview(Rules[^1], insertAfter: true);
            return;
        }

        LogRuleDragPreviewState("preview", point, hitRowIndex, insertionIndex, insertionIndex, insertAfter: false);
        // 中间插入位置统一预览为“目标行顶部”，避免上一行底部/下一行顶部之间来回闪烁。
        SetRuleDropPreview(Rules[insertionIndex], insertAfter: false);
    }

    private DataGridRow? GetRuleRowAtPoint(Point point)
    {
        var element = lvRules.InputHitTest(point) as DependencyObject;
        if (element == null)
            return null;

        if (ItemsControl.ContainerFromElement(lvRules, element) is DataGridRow row)
            return row;

        while (element != null && element is not DataGridRow)
            element = VisualTreeHelper.GetParent(element);

        return element as DataGridRow;
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
        if (!_appSettings.AutoPreview)
            return;

        if (_wpfFilesBatchDepth > 0)
        {
            _autoPreviewQueuedDuringBatch = true;
            return;
        }

        if (Rules.Count > 0)
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
        CommitPendingFileEdit();

        if (!EnsureRulesReadyForExecution())
            return false;

        var traceId = Interlocked.Increment(ref _previewTraceSequence);
        var totalSw = Stopwatch.StartNew();
        var previewLabel = LanguageService.GetString("Status_Previewing");
        var markedFiles = Files.Where(f => f.IsMarked).ToList();
        var unmarkedFiles = Files.Where(f => !f.IsMarked).ToList();
        var ruleSnapshot = Rules.Where(r => r.IsEnabled).ToList();
        LogImportDiag(
            $"[Preview#{traceId}] start auto={isAutoTrigger} debounceMs={debounceMilliseconds} " +
            $"files={Files.Count} marked={markedFiles.Count} unmarked={unmarkedFiles.Count} enabledRules={ruleSnapshot.Count}");
        var (token, version) = BeginNewPreviewRequest();

        try
        {
            if (debounceMilliseconds > 0)
            {
                await Task.Delay(debounceMilliseconds);
                if (token.IsCancellationRequested || !IsLatestPreviewRequest(version))
                {
                    totalSw.Stop();
                    LogImportDiag($"[Preview#{traceId}] canceled stage=debounce totalMs={totalSw.ElapsedMilliseconds}");
                    return false;
                }
            }

            BeginProgress(previewLabel);
            IProgress<(int current, int total)> progress = new Progress<(int current, int total)>(
                p => UpdateProgress(previewLabel, p.current, p.total));
            int previewProgressStep = Math.Max(1, markedFiles.Count / PreviewProgressUpdateTarget);
            int lastPreviewProgress = 0;

            var computeSw = Stopwatch.StartNew();
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
                    token),
                token);
            computeSw.Stop();

            if (token.IsCancellationRequested || !IsLatestPreviewRequest(version))
            {
                totalSw.Stop();
                LogImportDiag($"[Preview#{traceId}] canceled stage=after-compute computeMs={computeSw.ElapsedMilliseconds} totalMs={totalSw.ElapsedMilliseconds}");
                return false;
            }

            var applySw = new Stopwatch();
            if (!IsWin32FileList)
                BeginWpfFilesBatch();

            var resetSw = new Stopwatch();
            var uiSw = new Stopwatch();
            try
            {
                applySw.Restart();
                foreach (var result in previewResults)
                    result.file.ApplyPreviewName(result.newName);
                applySw.Stop();

                resetSw.Restart();
                foreach (var file in unmarkedFiles)
                    file.ApplyPreviewName(file.OriginalName, force: true);
                resetSw.Stop();

                uiSw.Restart();
                ApplyHighlightChangesState(Files.Where(f => !f.IsRenamed));
                if (IsWin32FileList)
                {
                    RefreshFileList();
                    FlushFileStateUi();
                }
                uiSw.Stop();
            }
            finally
            {
                if (!IsWin32FileList)
                    EndWpfFilesBatch();
            }

            long validateMs = 0;
            if (_appSettings.AutoValidate)
            {
                var validateSw = Stopwatch.StartNew();
                var errors = ValidateMarkedFiles();
                tbStatusInfo.Text = errors.Count == 0
                    ? "Auto validation passed."
                    : $"Auto validation found {errors.Count} issue(s). Run Validate to view details.";
                validateSw.Stop();
                validateMs = validateSw.ElapsedMilliseconds;
            }
            else
            {
                tbStatusInfo.Text = LanguageService.GetString("Status_PreviewCompleted");
            }

            totalSw.Stop();
            LogImportDiag(
                $"[Preview#{traceId}] end auto={isAutoTrigger} marked={markedFiles.Count} unmarked={unmarkedFiles.Count} " +
                $"results={previewResults.Count} computeMs={computeSw.ElapsedMilliseconds} applyMs={applySw.ElapsedMilliseconds} " +
                $"resetMs={resetSw.ElapsedMilliseconds} uiMs={uiSw.ElapsedMilliseconds} validateMs={validateMs} totalMs={totalSw.ElapsedMilliseconds}");
            return true;
        }
        catch (Exception ex)
        {
            totalSw.Stop();
            LogImportDiag($"[Preview#{traceId}] fail auto={isAutoTrigger} message='{ex.Message}' totalMs={totalSw.ElapsedMilliseconds}");
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

        var toRename = Files.Count(f => f.IsMarked && f.HasChanged);
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

        var renameTargets = Files.Where(f => f.IsMarked && f.HasChanged).ToList();
        var renameLabel = LanguageService.GetString("Status_Renaming");
        var (token, version) = BeginNewRenameRequest();

        try
        {
            BeginProgress(renameLabel);
            IProgress<(int current, int total)> progress = new Progress<(int current, int total)>(
                p => UpdateProgress(renameLabel, p.current, p.total));
            int renameProgressStep = Math.Max(1, renameTargets.Count / 100);
            int lastRenameProgress = 0;

            var (success, failed, canceled) = await Task.Run(
                () => _renameService.Rename(
                    renameTargets,
                    (cur, total) =>
                    {
                        if (!ShouldReportProgress(cur, total, lastRenameProgress, renameProgressStep))
                            return;

                        lastRenameProgress = cur;
                        progress.Report((cur, total));
                    },
                    _appSettings.ConflictResolution,
                    token),
                token);

            if (token.IsCancellationRequested || !IsLatestRenameRequest(version))
                return;

            if (canceled)
            {
                tbStatusInfo.Text = "Rename cancelled.";
                return;
            }

            var renamedFiles = renameTargets.Where(f => f.IsRenamed).ToList();
            string? undoLogPath = null;
            if (_appSettings.CreateUndoLog && renamedFiles.Count > 0)
            {
                undoLogPath = CreateUndoLogFile(renamedFiles);
            }

            // 重命名成功后自动取消勾选，避免后续操作混入已完成项。
            if (IsWin32FileList)
            if (IsWin32FileList)
            {
                if (_appSettings.AutoRemoveRenamed && renamedFiles.Count > 0)
                    Files.RemoveRange(renamedFiles);

                RefreshFileList();
                FlushFileStateUi();
            }
            else
            {
                RunWpfFilesBatch(() =>
                {
                    if (_appSettings.AutoRemoveRenamed && renamedFiles.Count > 0)
                        Files.RemoveRange(renamedFiles);
                }, preserveSelection: !_appSettings.AutoRemoveRenamed, clearSelection: _appSettings.AutoRemoveRenamed);
            }

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
        if (IsWin32FileList)
        {
            _renameService.UndoRename(Files);
            RefreshFileList();
            FlushFileStateUi();
            return;
        }

        RunWpfFilesBatch(() => _renameService.UndoRename(Files), preserveSelection: true);
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

    private void Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => HandleFileSelectionChanged();

    private void HandleFileSelectionChanged()
    {
        _selectedFileCount = _fileListView.SelectedCount;
        if (_fileSelectionBatchDepth > 0)
        {
            _fileSelectionStatusDirty = true;
            return;
        }

        RequestStatusBarUpdate();
        RequestActionsRefresh();
    }
    private void FileCheckBox_Click(object sender, RoutedEventArgs e)
    {
        NotifyPreviewRefreshNeededForMarking();
    }

    private void FileHeaderCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox headerCheckBox)
            return;

        var isMarked = headerCheckBox.IsChecked == true;
        SetMarkedInBulk(Files, _ => isMarked, autoPreview: false);
        NotifyPreviewRefreshNeededForMarking();
    }

    private void Files_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsWin32FileList)
            return;

        RenFile? file = null;
        UIElement? dragSource = sender as UIElement;

        if (sender is FrameworkElement handle && handle.DataContext is RenFile handleFile)
        {
            file = handleFile;
            dragSource = handle;
        }
        else
        {
            file = GetFileAtPoint(e.GetPosition(lvFiles));
            dragSource = lvFiles;
        }

        if (file == null || dragSource == null)
        {
            _draggedFile = null;
            _draggedFileIndex = -1;
            return;
        }

        _filesDragStartPoint = e.GetPosition(null);
        _draggedFile = file;
        _draggedFileIndex = Files.IndexOf(file);
        _fileListView.SelectedItem = file;
        dragSource.CaptureMouse();

        if (!ReferenceEquals(dragSource, lvFiles))
            e.Handled = true;
    }

    private void Files_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
            return;

        if (!TryGetFileCellContext(e.OriginalSource as DependencyObject, out var file, out var column))
            return;

        if (!ReferenceEquals(column, colFileName) && !ReferenceEquals(column, colNewName))
            return;

        _fileListView.SelectedItem = file;
        EditNewName_Click(sender, new RoutedEventArgs());
        e.Handled = true;
    }

    private void Files_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not UIElement handle || e.LeftButton != MouseButtonState.Pressed || _draggedFile == null || _draggedFileIndex < 0)
            return;

        var position = e.GetPosition(null);
        if (Math.Abs(position.X - _filesDragStartPoint.X) >= SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(position.Y - _filesDragStartPoint.Y) >= SystemParameters.MinimumVerticalDragDistance)
        {
            if (handle.IsMouseCaptured)
                handle.ReleaseMouseCapture();

            try
            {
                var dragData = CreateFileDragDataObject(_draggedFileIndex);
                DragDrop.DoDragDrop(handle, dragData, DragDropEffects.Move);
            }
            finally
            {
                ClearFileDropPreview();
                _draggedFile = null;
                _draggedFileIndex = -1;
            }
        }
    }

    private void Files_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            ClearFileDropPreview();
            e.Effects = DragDropEffects.Copy;
        }
        else if (TryGetDraggedFileIndex(e.Data, out _))
        {
            UpdateFileDropPreview(e.GetPosition(lvFiles));
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            ClearFileDropPreview();
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void Files_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            ClearFileDropPreview();
            var paths = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            AddFilePaths(paths, recursive: true, forcePreview: _appSettings.AutoPreview);
            return;
        }

        if (!TryGetDraggedFileIndex(e.Data, out var oldIndex))
        {
            ClearFileDropPreview();
            return;
        }

        var dragged = oldIndex >= 0 && oldIndex < Files.Count
            ? Files[oldIndex]
            : _draggedFile;
        if (dragged == null)
        {
            ClearFileDropPreview();
            return;
        }

        var dropPoint = e.GetPosition(lvFiles);
        oldIndex = Files.IndexOf(dragged);
        if (oldIndex < 0)
        {
            ClearFileDropPreview();
            return;
        }

        int insertionIndex = GetFileInsertIndex(dropPoint);
        if (insertionIndex > oldIndex)
            insertionIndex--;

        int newIndex = Math.Clamp(insertionIndex, 0, Files.Count - 1);
        if (newIndex == oldIndex)
        {
            ClearFileDropPreview();
            return;
        }

        Files.Move(oldIndex, newIndex);
        _fileListView.SelectedItem = dragged;
        ClearFileDropPreview();
        NotifyPreviewRefreshNeededForFileOrder();
    }

    private void FileDragHandle_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is UIElement handle && handle.IsMouseCaptured)
            handle.ReleaseMouseCapture();

        _draggedFile = null;
        _draggedFileIndex = -1;
        ClearFileDropPreview();
    }

    private static DataObject CreateFileDragDataObject(int fileIndex)
    {
        var dragData = new DataObject();
        dragData.SetData(FileDragIndexDataFormat, fileIndex.ToString(CultureInfo.InvariantCulture));
        return dragData;
    }

    private static bool TryGetDraggedFileIndex(IDataObject dataObject, out int fileIndex)
    {
        fileIndex = -1;
        if (!dataObject.GetDataPresent(FileDragIndexDataFormat))
            return false;

        var raw = dataObject.GetData(FileDragIndexDataFormat) as string;
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out fileIndex);
    }

    private int GetFileInsertIndex(Point point)
    {
        var row = GetFileRowAtPoint(point);
        if (row == null)
            return Files.Count;

        int rowIndex = row.GetIndex();
        if (rowIndex < 0)
            return Files.Count;

        var rowTopLeft = row.TranslatePoint(new Point(0, 0), lvFiles);
        bool insertAfter = point.Y > rowTopLeft.Y + (row.ActualHeight / 2);
        return insertAfter ? rowIndex + 1 : rowIndex;
    }

    private void UpdateFileDropPreview(Point point)
    {
        if (Files.Count == 0)
        {
            ClearFileDropPreview();
            return;
        }

        var row = GetFileRowAtPoint(point);
        if (row != null)
        {
            ApplyFileDropPreview(GetFileInsertIndex(point));
            return;
        }

        if (lvFiles.ItemContainerGenerator.ContainerFromIndex(0) is DataGridRow firstRow &&
            firstRow.DataContext is RenFile)
        {
            var firstRowTop = firstRow.TranslatePoint(new Point(0, 0), lvFiles).Y;
            if (point.Y <= firstRowTop + (firstRow.ActualHeight / 2))
            {
                ApplyFileDropPreview(0);
                return;
            }
        }

        if (lvFiles.ItemContainerGenerator.ContainerFromIndex(Files.Count - 1) is DataGridRow lastRow &&
            lastRow.DataContext is RenFile)
        {
            var lastRowTop = lastRow.TranslatePoint(new Point(0, 0), lvFiles).Y;
            if (point.Y >= lastRowTop + (lastRow.ActualHeight / 2))
            {
                ApplyFileDropPreview(Files.Count);
                return;
            }
        }

        ClearFileDropPreview();
    }

    private void ApplyFileDropPreview(int insertionIndex)
    {
        if (Files.Count == 0)
        {
            ClearFileDropPreview();
            return;
        }

        insertionIndex = Math.Clamp(insertionIndex, 0, Files.Count);
        if (insertionIndex == 0)
        {
            SetFileDropPreview(Files[0], insertAfter: false);
            return;
        }

        if (insertionIndex >= Files.Count)
        {
            SetFileDropPreview(Files[^1], insertAfter: true);
            return;
        }

        SetFileDropPreview(Files[insertionIndex], insertAfter: false);
    }

    private DataGridRow? GetFileRowAtPoint(Point point)
    {
        var element = lvFiles.InputHitTest(point) as DependencyObject;
        if (element == null)
            return null;

        if (ItemsControl.ContainerFromElement(lvFiles, element) is DataGridRow row)
            return row;

        while (element != null && element is not DataGridRow)
            element = VisualTreeHelper.GetParent(element);

        return element as DataGridRow;
    }

    private RenFile? GetFileAtPoint(Point point)
    {
        var element = lvFiles.InputHitTest(point) as DependencyObject;
        while (element != null && element is not DataGridRow)
            element = VisualTreeHelper.GetParent(element);

        return (element as DataGridRow)?.DataContext as RenFile;
    }

    private static bool TryGetFileCellContext(DependencyObject? origin, out RenFile? file, out DataGridColumn? column)
    {
        file = null;
        column = null;

        var current = origin;
        while (current != null && current is not DataGridCell)
            current = VisualTreeHelper.GetParent(current);

        if (current is not DataGridCell cell || cell.DataContext is not RenFile targetFile)
            return false;

        file = targetFile;
        column = cell.Column;
        return true;
    }

    private void CommitPendingFileEdit()
    {
        if (IsWin32FileList)
            return;
        var committedCell = lvFiles.CommitEdit(DataGridEditingUnit.Cell, true);
        var committedRow = lvFiles.CommitEdit(DataGridEditingUnit.Row, true);
        if (committedCell && committedRow)
            return;

        lvFiles.CancelEdit(DataGridEditingUnit.Cell);
        lvFiles.CancelEdit(DataGridEditingUnit.Row);
    }

    private void FilesColumnHeader_Click(object sender, DataGridSortingEventArgs e)
    {
        if (IsWin32FileList)
        {
            e.Handled = true;
            return;
        }

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
        UpdateWpfLiveSorting(sortBy);

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
        var selected = GetSelectedFiles();
        SetMarkedInBulk(selected, _ => true);
    }

    private void UnmarkSelected_Click(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedFiles();
        SetMarkedInBulk(selected, _ => false);
    }

    private void InvertMarking_Click(object sender, RoutedEventArgs e)
    {
        SetMarkedInBulk(Files, f => !f.IsMarked);
    }

    private void MarkOnlyChanged_Click(object sender, RoutedEventArgs e)
    {
        SetMarkedInBulk(Files, f => f.HasChanged);
    }

    private void MarkOnlySelected_Click(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedFiles().ToHashSet();
        SetMarkedInBulk(Files, f => selected.Contains(f));
    }

    private void RemoveFilesInBulk(IEnumerable<RenFile> files, string reason)
    {
        var targets = files?.Distinct().ToList() ?? new List<RenFile>();
        if (targets.Count == 0)
            return;

        LogImportDiag($"[RemoveBulk] start reason={reason} requested={targets.Count} filesBefore={Files.Count} viewBefore={_viewItems.Count}");
        var useWin32Batch = IsWin32FileList;
        var selectionToRestore = useWin32Batch ? null : GetSelectedFiles().ToList();
        if (useWin32Batch)
            BeginWin32FilesBatch();
        else
            BeginWpfFilesBatch();

        int removed = 0;
        try
        {
            removed = Files.RemoveRange(targets);
        }
        finally
        {
            if (useWin32Batch)
                EndWin32FilesBatch();
            else
                EndWpfFilesBatch(selectionToRestore);
        }

        if (useWin32Batch)
        {
            // Defensive sync: avoid stale virtual rows when the data source is fully cleared.
            if (Files.Count == 0 && _viewItems.Count != 0)
                RebuildViewItems(preserveSelection: false);

            _fileListView.ClearSelection();
            RefreshFileList();
        }

        if (useWin32Batch)
            FlushFileStateUi();
        LogImportDiag($"[RemoveBulk] end reason={reason} requested={targets.Count} removed={removed} filesAfter={Files.Count} viewAfter={_viewItems.Count}");
    }

    private void ClearAll_Click(object sender, RoutedEventArgs e)
    {
        if (!IsWin32FileList)
        {
            RunWpfFilesBatch(() => Files.ReplaceAll(Array.Empty<RenFile>()), clearSelection: true);
            return;
        }

        BeginWin32FilesBatch();
        try
        {
            Files.ReplaceAll(Array.Empty<RenFile>());
        }
        finally
        {
            EndWin32FilesBatch();
        }

        _fileListView.ClearSelection();
    }
    private void ClearRenamed_Click(object sender, RoutedEventArgs e)
        => RemoveFilesInBulk(Files.Where(f => f.IsRenamed).ToList(), "clear-renamed");
    private void ClearFailed_Click(object sender, RoutedEventArgs e)
        => RemoveFilesInBulk(Files.Where(f => f.State == "×").ToList(), "clear-failed");
    private void ClearNotChanged_Click(object sender, RoutedEventArgs e)
        => RemoveFilesInBulk(Files.Where(f => !f.HasChanged).ToList(), "clear-not-changed");
    private void ClearMarked_Click(object sender, RoutedEventArgs e)
        => RemoveFilesInBulk(Files.Where(f => f.IsMarked).ToList(), "clear-marked");
    private void ClearNotMarked_Click(object sender, RoutedEventArgs e)
        => RemoveFilesInBulk(Files.Where(f => !f.IsMarked).ToList(), "clear-not-marked");

    private async void FilesSelectAll_Click(object sender, RoutedEventArgs e)
    {
        if (IsWin32FileList)
        {
            var swWin32 = Stopwatch.StartNew();
            RunFileSelectionBatch(() => _fileListView.SelectAll());
            swWin32.Stop();
            App.LogInputDebug($"[SelectAll] win32 count={Files.Count} totalMs={swWin32.ElapsedMilliseconds}");
            return;
        }

        if (Files.Count <= LargeSelectionThreshold)
        {
            var sw = Stopwatch.StartNew();
            RunFileSelectionBatch(() => _fileListView.SelectAll());
            sw.Stop();
            App.LogInputDebug($"[SelectAll] native count={Files.Count} totalMs={sw.ElapsedMilliseconds}");
            return;
        }

        await SelectAllFilesInChunksAsync();
    }

    private void InvertSelection_Click(object sender, RoutedEventArgs e)
    {
        RunFileSelectionBatch(() =>
        {
            var selected = GetSelectedFiles().ToHashSet();
            var newSelection = Files.Where(f => !selected.Contains(f)).ToList();
            SetSelectedFiles(newSelection);
        });
    }

    private void MoveFileUp_Click(object sender, RoutedEventArgs e)
    {
        if (_fileListView.SelectedCount != 1)
            return;

        var index = _fileListView.SelectedIndex;
        if (index > 0)
        {
            Files.Move(index, index - 1);
            _fileListView.SelectedIndex = index - 1;
            NotifyPreviewRefreshNeededForFileOrder();
        }
    }

    private void MoveFileDown_Click(object sender, RoutedEventArgs e)
    {
        if (_fileListView.SelectedCount != 1)
            return;

        var index = _fileListView.SelectedIndex;
        if (index >= 0 && index < Files.Count - 1)
        {
            Files.Move(index, index + 1);
            _fileListView.SelectedIndex = index + 1;
            NotifyPreviewRefreshNeededForFileOrder();
        }
    }

    private void RemoveSelectedFiles_Click(object sender, RoutedEventArgs e)
    {
        var targets = GetSelectedFiles().ToList();
        if (targets.Count == 0)
            targets = Files.Where(f => f.IsMarked).ToList();

        if (targets.Count == 0)
            return;
        RemoveFilesInBulk(targets, "remove-selected");
        AutoPreviewIfEnabled(sender, e);
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        if (GetSelectedFile() is RenFile file && File.Exists(file.FullPath))
            Process.Start(new ProcessStartInfo(file.FullPath) { UseShellExecute = true });
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (GetSelectedFile() is RenFile file)
            Process.Start("explorer.exe", $"/select,\"{file.FullPath}\"");
    }

    private void EditNewName_Click(object sender, RoutedEventArgs e)
    {
        if (GetSelectedFile() is not RenFile file)
            return;

        var dlg = new TextInputDialog(
            LanguageService.GetString("Dialog_EditNewNameTitle"),
            LanguageService.GetString("Column_NewName"),
            file.NewName,
            validator: text => string.IsNullOrWhiteSpace(text) ? LanguageService.GetString("Dialog_InputRequired") : null,
            originalText: file.OriginalName)
        {
            Owner = this
        };

        if (dlg.ShowDialog() == true)
        {
            if (IsWin32FileList)
            {
                file.NewName = dlg.InputText;
                ApplyHighlightChangesState(file);
                RefreshFileList();
                FlushFileStateUi();
                return;
            }

            RunWpfFilesBatch(() =>
            {
                file.NewName = dlg.InputText;
                ApplyHighlightChangesState(file);
            }, preserveSelection: true);
        }
    }

    #endregion

    #region Menu/Toolbar Actions

    private void NewProject_Click(object sender, RoutedEventArgs e)
    {
        Rules.Clear();
        ClearActiveFileSort();

        if (IsWin32FileList)
        {
            BeginWin32FilesBatch();
            try
            {
                Files.ReplaceAll(Array.Empty<RenFile>());
            }
            finally
            {
                EndWin32FilesBatch();
            }

            _fileListView.ClearSelection();
            return;
        }

        RunWpfFilesBatch(() => Files.ReplaceAll(Array.Empty<RenFile>()), clearSelection: true);
    }

    private void NewInstance_Click()
    {
        try
        {
            var exe = Process.GetCurrentProcess().MainModule?.FileName;
            if (exe != null) Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            LogException("NewInstance_Click", ex);
            ShowBackgroundOperationError($"Failed to launch a new instance: {ex.Message}", showDialog: true);
        }
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
            Filter = "Preset Files (*.json;*.rnp)|*.json;*.rnp|JSON Presets (*.json)|*.json|Legacy Presets (*.rnp)|*.rnp|All Files (*.*)|*.*",
            Title = "Load Preset"
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                LoadPresetFromPath(dialog.FileName, "load preset", showWarningDialog: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load preset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void LoadPresetFromPath(string path, string settingsActionName, bool showWarningDialog, bool updateLastPresetPath = true)
    {
        var content = File.ReadAllText(path);
        var loadedRules = PresetService.LoadFromContent(content, path, out var warningMessage);

        Rules.Clear();
        foreach (var rule in loadedRules) Rules.Add(rule);

        if (updateLastPresetPath)
        {
            _appSettings.LastPresetPath = path;
            SaveSettingsWithFeedback(settingsActionName, showDialog: false);
        }

        if (showWarningDialog && !string.IsNullOrWhiteSpace(warningMessage))
        {
            MessageBox.Show(warningMessage, "Preset Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // 预设加载后立即重算文件表格预览，避免 UI 仍显示旧规则结果。
        if (Files.Count > 0)
        {
            _ = ExecutePreviewAsync(isAutoTrigger: true, debounceMilliseconds: 0);
        }
    }

    private void RestoreLastSessionRulesIfNeeded()
    {
        if (!_appSettings.LoadLastPreset)
            return;

        if (!File.Exists(SessionRulesFile))
            return;

        try
        {
            LoadPresetFromPath(
                SessionRulesFile,
                settingsActionName: "restore last session rules",
                showWarningDialog: false,
                updateLastPresetPath: false);
            tbStatusInfo.Text = LanguageService.GetString("Status_Ready");
        }
        catch (Exception ex)
        {
            tbStatusInfo.Text = $"Failed to restore last rules: {ex.Message}";
            Debug.WriteLine($"[MainWindow] RestoreLastSessionRulesIfNeeded failed: {ex}");
        }
    }

    private void SaveSessionRulesSnapshot()
    {
        try
        {
            var dir = Path.GetDirectoryName(SessionRulesFile);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = PresetService.SaveToJson(Rules);
            File.WriteAllText(SessionRulesFile, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MainWindow] SaveSessionRulesSnapshot failed: {ex}");
        }
    }

    private async void SavePreset_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_appSettings.LastPresetPath))
        {
            SavePresetAs_Click(sender, e);
            return;
        }

        await SavePresetToPathAsync(_appSettings.LastPresetPath, updateLastPath: false);
    }

    private async void SavePresetAs_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "JSON Presets (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Save Preset As"
        };
        if (dialog.ShowDialog() == true)
        {
            await SavePresetToPathAsync(dialog.FileName, updateLastPath: true);
        }
    }

    private async Task SavePresetToPathAsync(string path, bool updateLastPath)
    {
        const string progressLabel = "保存预设";

        try
        {
            BeginProgress(progressLabel);
            var rulesSnapshot = Rules.ToList();
            await Task.Yield();
            await Task.Run(() =>
            {
                var json = PresetService.SaveToJson(rulesSnapshot);
                File.WriteAllText(path, json);
            });

            UpdateProgress(progressLabel, 1, 1);

            if (updateLastPath)
            {
                _appSettings.LastPresetPath = path;
                SaveSettingsWithFeedback("save preset as", showDialog: false);
            }

            tbStatusInfo.Text = $"预设保存成功：{Path.GetFileName(path)}";
            MessageBox.Show($"预设已保存到：\n{path}", "保存预设", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save preset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            EndProgress();
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => RequestAppExit();

    private void ManageFiles_Click(object sender, RoutedEventArgs e) => AddFiles_Click(sender, e);

    private void Filters_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new FiltersDialog(_appSettings)
        {
            Owner = this
        };
        if (dlg.ShowDialog() != true)
            return;

        if (_appSettings.FiltersApplied)
            ApplyFiltersToFiles(Files);
        else
            SetMarkedInBulk(Files, _ => true);
    }

    private void OpenSettingsTab(SettingsTab tab)
    {
        var dlg = new SettingsDialog(_appSettings, tab) { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            SaveSettingsWithFeedback("settings dialog", showDialog: true);
            RuleHelpers.ResolveMetaTagsEnabled = _appSettings.ResolveMetaTags;
            App.SetInputDebugLoggingEnabled(_appSettings.EnableInputDebugLogging);
            UpdateTrayIconState();
            // Apply language if changed
            if (_appSettings.Language == "en-US")
                LanguageService.SwitchToEnglish();
            else
                LanguageService.SwitchToChinese();

            if (IsWin32FileList)
            {
                ApplyHighlightChangesState(Files);
                if (_win32FileListView != null)
                    ApplyWin32Palette(_win32FileListView);
                RefreshFileList();
                FlushFileStateUi();
                return;
            }

            RunWpfFilesBatch(() => ApplyHighlightChangesState(Files), preserveSelection: true);
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
        if (IsWin32FileList)
        {
            for (int i = 0; i < _fileListColumns.Count; i++)
            {
                if (_fileListColumns[i].Visible)
                    AutoSizeWin32Column(i);
            }
            return;
        }

        foreach (var col in lvFiles.Columns)
        {
            if (col.Visibility == Visibility.Visible)
                AutoSizeColumn(col);
        }
    }

    private void FixConflictingNames()
    {
        var groups = Files.Where(f => f.HasChanged)
            .GroupBy(f => Path.Combine(f.FolderPath, f.NewName).ToLowerInvariant())
            .Where(g => g.Count() > 1);
        int fixed_ = 0;

        void ApplyFixes()
        {
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
        }

        if (IsWin32FileList)
        {
            ApplyFixes();
            RefreshFileList();
            FlushFileStateUi();
        }
        else
        {
            RunWpfFilesBatch(ApplyFixes, preserveSelection: true);
        }

        MessageBox.Show($"Fixed {fixed_} conflicting names.", "Fix Conflicts", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void AnalyzeSampleText()
    {
        var dlg = new Window
        {
            Title = "Analyze Sample Text",
            Width = 450,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize
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
        ClearActiveFileSort();

        if (IsWin32FileList)
        {
            BeginWin32FilesBatch();
            try
            {
                Files.ReplaceAll(sorted);
            }
            finally
            {
                EndWin32FilesBatch();
            }

            NotifyPreviewRefreshNeededForFileOrder();
            return;
        }

        RunWpfFilesBatch(
            () => Files.ReplaceAll(sorted),
            preserveSelection: true);
        NotifyPreviewRefreshNeededForFileOrder();
    }

    private void Analyze_Click(object sender, RoutedEventArgs e)
    {
        if (GetSelectedFile() is RenFile file)
        {
            MessageBox.Show(
                $"Original: {file.OriginalName}\nNew Name: {file.NewName}\nExtension: {file.Extension}\n" +
                $"Path: {file.FolderPath}\nSize: {file.SizeDisplay}\nName Length: {file.NameLength}\nDigits: {file.NameDigits}",
                "Analyze Name", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void FilesColumnHeader_RightClick(object sender, MouseButtonEventArgs e)
    {
        if (IsWin32FileList)
            return;
        if (sender is DataGridColumnHeader header && header.Column != null)
        {
            ShowColumnsMenu(header);
            e.Handled = true;
        }
    }

    private void FilesColumnHeader_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (IsWin32FileList)
            return;
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
                {
                    var targetKey = GetColumnKey(targetCol);
                    if (!string.IsNullOrWhiteSpace(targetKey)
                        && _appSettings.ColumnWidths.TryGetValue(targetKey, out var persistedWidth)
                        && persistedWidth > 0)
                    {
                        targetCol.Width = new DataGridLength(persistedWidth);
                    }
                    else if (!string.IsNullOrWhiteSpace(targetKey)
                             && _defaultColumnWidths.TryGetValue(targetKey, out var defaultWidth)
                             && defaultWidth > 0)
                    {
                        targetCol.Width = new DataGridLength(defaultWidth);
                    }
                }

                UpdateColumnWidthCache(targetCol);
            };
            menu.Items.Add(mi);
        }

        menu.Items.Add(new Separator());
        var resetColumnWidths = new MenuItem { Header = LanguageService.GetString("ColumnsMenu_ResetColumnWidths") };
        resetColumnWidths.Click += (_, _) => ResetColumnWidths();
        menu.Items.Add(resetColumnWidths);

        var cancelSort = new MenuItem { Header = LanguageService.GetString("ColumnsMenu_CancelSorting") };
        cancelSort.Click += (_, _) => ClearWpfSort();
        menu.Items.Add(cancelSort);

        menu.PlacementTarget = placementTarget;
        menu.IsOpen = true;
    }

    private void ResetColumnWidths()
    {
        if (IsWin32FileList)
        {
            for (int i = 0; i < _fileListColumns.Count; i++)
            {
                var col = _fileListColumns[i];
                if (!string.IsNullOrEmpty(col.Key) && _defaultColumnWidths.TryGetValue(col.Key, out var width) && width > 0)
                    col.Width = (int)Math.Round(width);

                UpdateColumnWidthCache(col, col.Width);
                _win32FileListView?.UpdateColumnWidth(i, col.Width);
            }
            _win32FileListView?.ApplyColumnVisibility();
            return;
        }

        foreach (var col in lvFiles.Columns)
        {
            var key = GetColumnKey(col);
            if (!string.IsNullOrEmpty(key) && _defaultColumnWidths.TryGetValue(key, out var width) && width > 0)
            {
                col.Width = new DataGridLength(width);
            }

            UpdateColumnWidthCache(col);
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
        UpdateColumnWidthCache(col);
    }

    private void ApplyWin32Palette(Win32FileListViewHost host)
    {
        _win32Palette = BuildWin32Palette();
        host.Palette = _win32Palette;
    }

    private Win32FileListPalette BuildWin32Palette()
    {
        var background = GetResourceColor("CardBrush", SystemColors.WindowColor);
        var alternateBackground = GetResourceColor("SurfaceBrush", SystemColors.ControlLightLightColor);
        var text = EnsureReadableText(
            GetResourceColor("TextBrush", SystemColors.WindowTextColor),
            background,
            SystemColors.WindowTextColor);

        var selectionBackground = GetResourceColor("SelectionBrush", SystemColors.HighlightColor);
        var selectionText = EnsureReadableText(
            GetResourceColor("TextBrush", SystemColors.HighlightTextColor),
            selectionBackground,
            SystemColors.HighlightTextColor);

        var headerBackground = GetResourceColor("SurfaceBrush", SystemColors.ControlColor);
        var headerText = EnsureReadableText(
            GetResourceColor("TextSecondaryBrush", SystemColors.ControlTextColor),
            headerBackground,
            SystemColors.ControlTextColor);
        var border = EnsureReadableBorder(
            GetResourceColor("BorderBrush", SystemColors.ActiveBorderColor),
            background);
        var headerBorder = EnsureReadableBorder(
            GetResourceColor("BorderBrush", SystemColors.ActiveBorderColor),
            headerBackground);

        var headerSortedBackground = GetResourceColor("PrimaryLightBrush", SystemColors.ControlLightColor);
        var headerSortedText = EnsureReadableText(
            GetResourceColor("PrimaryBrush", SystemColors.ControlTextColor),
            headerSortedBackground,
            SystemColors.ControlTextColor);

        return new Win32FileListPalette
        {
            Background = background,
            AlternateRowBackground = alternateBackground,
            Text = text,
            TextSecondary = GetResourceColor("TextSecondaryBrush", SystemColors.GrayTextColor),
            Border = border,
            SelectionBackground = selectionBackground,
            SelectionText = selectionText,
            HoverBackground = GetResourceColor("SurfaceHoverBrush", SystemColors.ControlLightColor),
            Accent = GetResourceColor("PrimaryBrush", SystemColors.HighlightColor),
            Success = GetResourceColor("SemanticSuccessBrush", Colors.ForestGreen),
            Error = GetResourceColor("SemanticErrorBrush", Colors.IndianRed),
            HeaderBackground = headerBackground,
            HeaderText = headerText,
            HeaderBorder = headerBorder,
            HeaderSortedBackground = headerSortedBackground,
            HeaderSortedText = headerSortedText
        };
    }

    private static Color GetResourceColor(string key, Color fallback)
    {
        if (Application.Current?.Resources[key] is SolidColorBrush brush)
            return brush.Color;
        if (Application.Current?.Resources[key] is Color color)
            return color;
        return fallback;
    }

    private static Color EnsureReadableText(Color text, Color background, Color fallback)
    {
        if (GetContrastRatio(text, background) >= 3.0)
            return text;

        if (GetContrastRatio(fallback, background) >= 3.0)
            return fallback;

        return GetRelativeLuminance(background) > 0.5 ? Colors.Black : Colors.White;
    }

    private static Color EnsureReadableBorder(Color border, Color background)
    {
        if (GetContrastRatio(border, background) >= 1.6)
            return border;

        var fallback = GetRelativeLuminance(background) > 0.5
            ? Color.FromRgb(176, 185, 194)
            : Color.FromRgb(74, 85, 96);
        if (GetContrastRatio(fallback, background) >= 1.6)
            return fallback;

        return EnsureReadableText(border, background, fallback);
    }

    private static double GetContrastRatio(Color a, Color b)
    {
        var l1 = GetRelativeLuminance(a);
        var l2 = GetRelativeLuminance(b);
        var lighter = Math.Max(l1, l2);
        var darker = Math.Min(l1, l2);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double GetRelativeLuminance(Color color)
    {
        static double Channel(byte c)
        {
            var srgb = c / 255.0;
            return srgb <= 0.03928 ? srgb / 12.92 : Math.Pow((srgb + 0.055) / 1.055, 2.4);
        }

        var r = Channel(color.R);
        var g = Channel(color.G);
        var b = Channel(color.B);
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    private void AutoSizeWin32Column(int columnIndex)
    {
        if (_win32FileListView == null || columnIndex < 0 || columnIndex >= _fileListColumns.Count)
            return;

        _win32FileListView.AutoSizeColumn(columnIndex);
        var width = _win32FileListView.GetColumnWidth(columnIndex);
        _fileListColumns[columnIndex].Width = width;
        UpdateColumnWidthCache(_fileListColumns[columnIndex], width);
    }

    private string GetWin32CellText(int row, int col)
    {
        if (row < 0 || row >= _viewItems.Count)
            return string.Empty;
        if (col < 0 || col >= _fileListColumns.Count)
            return string.Empty;

        var file = _viewItems[row];
        if (string.Equals(_fileListColumns[col].Key, "IsMarked", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        return _fileListColumns[col].TextGetter?.Invoke(file) ?? string.Empty;
    }

    private Win32FileListViewHost.CellStyle? GetWin32CellStyle(int row, int col, bool isSelected)
    {
        if (row < 0 || row >= _viewItems.Count)
            return null;
        if (col < 0 || col >= _fileListColumns.Count)
            return null;

        var file = _viewItems[row];
        var key = _fileListColumns[col].Key;
        if (string.Equals(key, "State", StringComparison.OrdinalIgnoreCase))
        {
            var palette = _win32Palette;
            return file.State switch
            {
                "✓" => new Win32FileListViewHost.CellStyle(palette?.Success ?? Colors.ForestGreen),
                "×" => new Win32FileListViewHost.CellStyle(palette?.Error ?? Colors.IndianRed),
                "→" => new Win32FileListViewHost.CellStyle(palette?.Accent ?? Colors.DodgerBlue),
                _ => null
            };
        }

        var newNameChanged = !string.Equals(file.NewName, file.OriginalName, StringComparison.Ordinal);
        if (string.Equals(key, "NewName", StringComparison.OrdinalIgnoreCase) && (file.HasChanged || newNameChanged) && !isSelected)
        {
            var palette = _win32Palette;
            return new Win32FileListViewHost.CellStyle(palette?.Accent ?? Colors.DodgerBlue, bold: true);
        }

        return null;
    }

    private bool GetWin32ItemChecked(int row)
    {
        if (row < 0 || row >= _viewItems.Count)
            return false;
        return _viewItems[row].IsMarked;
    }

    private void OnWin32ItemCheckedChanged(int row, bool isChecked)
    {
        if (row < 0 || row >= _viewItems.Count)
            return;
        _viewItems[row].IsMarked = isChecked;
        NotifyPreviewRefreshNeededForMarking();
    }

    private void OnWin32ItemActivated(int row, int col)
    {
        if (row < 0 || row >= _viewItems.Count)
            return;
        if (col < 0 || col >= _fileListColumns.Count)
            return;

        var key = _fileListColumns[col].Key;
        if (!string.Equals(key, "OriginalName", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(key, "NewName", StringComparison.OrdinalIgnoreCase))
            return;

        var file = _viewItems[row];
        _fileListView.SelectedItem = file;
        EditNewName_Click(this, new RoutedEventArgs());
    }

    private void OnWin32ColumnHeaderClick(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= _fileListColumns.Count)
            return;

        var col = _fileListColumns[columnIndex];
        if (string.Equals(col.Key, "IsMarked", StringComparison.OrdinalIgnoreCase))
        {
            ToggleAllMarks();
            return;
        }

        if (!col.Sortable || col.SortValueGetter == null)
            return;

        if (string.Equals(_viewSortKey, col.Key, StringComparison.OrdinalIgnoreCase))
        {
            _viewSortDirection = _viewSortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }
        else
        {
            _viewSortKey = col.Key;
            _viewSortDirection = ListSortDirection.Ascending;
        }

        var selected = GetSelectedFiles().ToList();
        ApplyViewSort();
        _fileListView.Refresh();
        if (selected.Count > 0)
            _fileListView.SetSelectedItems(selected);
        _win32FileListView?.SetSortIndicator(columnIndex, _viewSortDirection);
    }

    private void OnWin32ColumnHeaderAutoSize(int columnIndex)
        => AutoSizeWin32Column(columnIndex);

    private void ShowWin32ColumnsMenuAtCursor()
    {
        var pt = WinForms.Control.MousePosition;
        ShowWin32ColumnsMenu(new Point(pt.X, pt.Y));
    }

    private void ShowWin32ColumnsMenu(Point screenPoint)
    {
        var menu = new ContextMenu();
        for (int i = 2; i < _fileListColumns.Count; i++)
        {
            var col = _fileListColumns[i];
            var header = string.IsNullOrWhiteSpace(col.Header) ? $"Column {i}" : col.Header;
            var mi = new MenuItem { Header = header, IsCheckable = true, IsChecked = col.Visible, Tag = i };
            mi.Click += (s, _) =>
            {
                var index = (int)((MenuItem)s!).Tag;
                var target = _fileListColumns[index];
                target.Visible = ((MenuItem)s!).IsChecked;
                if (target.Visible && target.Width <= 0)
                {
                    if (_appSettings.ColumnWidths.TryGetValue(target.Key, out var persistedWidth) && persistedWidth > 0)
                        target.Width = (int)Math.Round(persistedWidth);
                    else if (_defaultColumnWidths.TryGetValue(target.Key, out var defaultWidth) && defaultWidth > 0)
                        target.Width = (int)Math.Round(defaultWidth);
                }

                UpdateColumnWidthCache(target, target.Width);
                _win32FileListView?.UpdateColumnWidth(index, target.Width);
                _win32FileListView?.ApplyColumnVisibility();
            };
            menu.Items.Add(mi);
        }

        menu.Items.Add(new Separator());
        var resetColumnWidths = new MenuItem { Header = LanguageService.GetString("ColumnsMenu_ResetColumnWidths") };
        resetColumnWidths.Click += (_, _) => ResetColumnWidths();
        menu.Items.Add(resetColumnWidths);

        var cancelSort = new MenuItem { Header = LanguageService.GetString("ColumnsMenu_CancelSorting") };
        cancelSort.Click += (_, _) => ClearWin32Sort();
        menu.Items.Add(cancelSort);

        var wpfPoint = ScreenToWpfPoint(screenPoint);
        menu.Placement = PlacementMode.AbsolutePoint;
        menu.PlacementTarget = this;
        menu.HorizontalOffset = wpfPoint.X;
        menu.VerticalOffset = wpfPoint.Y;
        menu.IsOpen = true;
    }

    private void ShowWin32ContextMenu(Point screenPoint)
    {
        if (_win32FileListView == null)
            return;

        var index = _win32FileListView.HitTestIndexFromScreenPoint(screenPoint);
        if (index >= 0 && index < _viewItems.Count)
        {
            var target = _viewItems[index];
            if (!_fileListView.SelectedItems.Contains(target))
                _fileListView.SelectedItem = target;
        }

        var menu = lvFiles.ContextMenu;
        if (menu == null)
            return;

        var wpfPoint = ScreenToWpfPoint(screenPoint);
        menu.PlacementTarget = this;
        menu.Placement = PlacementMode.AbsolutePoint;
        menu.HorizontalOffset = wpfPoint.X;
        menu.VerticalOffset = wpfPoint.Y;
        menu.IsOpen = true;
    }

    private void OnWin32ReorderRequest(int from, int to)
    {
        if (from < 0 || to < 0 || from >= _viewItems.Count || to >= _viewItems.Count)
            return;

        var dragged = _viewItems[from];
        var target = _viewItems[to];
        int oldIndex = Files.IndexOf(dragged);
        if (oldIndex < 0)
            return;

        int newIndex = Files.IndexOf(target);
        if (newIndex < 0)
            newIndex = Files.Count - 1;
        if (newIndex == oldIndex)
            return;
        if (newIndex > oldIndex)
            newIndex--;

        Files.Move(oldIndex, newIndex);
        _fileListView.SelectedItem = dragged;
        NotifyPreviewRefreshNeededForFileOrder();
    }

    private void OnWin32KeyGesture(Key key, ModifierKeys modifiers)
    {
        if (modifiers == ModifierKeys.Control && key == Key.A)
            TryExecute(CmdFilesSelectAll);
        else if (modifiers == ModifierKeys.None && key == Key.Delete)
            TryExecute(CmdRemoveSelectedFiles);
        else if (modifiers == ModifierKeys.None && key == Key.F5)
            TryExecute(CmdPreview);
    }

    private void ToggleAllMarks()
    {
        if (Files.Count == 0)
            return;
        var allMarked = Files.All(f => f.IsMarked);
        SetMarkedInBulk(Files, _ => !allMarked, autoPreview: false);
        NotifyPreviewRefreshNeededForMarking();
    }

    private void UpdateWin32HeaderCheckState()
    {
        if (!IsWin32FileList || _win32FileListView == null)
            return;

        if (Files.Count == 0)
        {
            _win32FileListView.SetHeaderCheckState(false);
            return;
        }

        var any = Files.Any(f => f.IsMarked);
        var all = any && Files.All(f => f.IsMarked);
        bool? state = all ? true : any ? (bool?)null : false;
        _win32FileListView.SetHeaderCheckState(state);
    }

    private Point ScreenToWpfPoint(Point screenPoint)
    {
        var source = PresentationSource.FromVisual(this);
        if (source?.CompositionTarget != null)
            return source.CompositionTarget.TransformFromDevice.Transform(screenPoint);
        return screenPoint;
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(LanguageService.GetString("Msg_About"),
            LanguageService.GetString("Menu_About"), MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DebugWindow_Click(object sender, RoutedEventArgs e)
    {
        var win = new DebugWindow { Owner = this };
        win.Show();
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
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            LogException($"OpenUrl({url})", ex);
            MessageBox.Show($"Cannot open link: {ex.Message}", "ReNamer", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
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
            AutoPreviewIfEnabled(sender, e);
        }
    }

    private void AddRuleBelow_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddRuleDialog();
        if (dialog.ShowDialog() == true && dialog.SelectedRule != null)
        {
            var idx = lvRules.SelectedIndex < 0 ? Rules.Count : lvRules.SelectedIndex + 1;
            Rules.Insert(idx, dialog.SelectedRule);
            AutoPreviewIfEnabled(sender, e);
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
        if (GetSelectedFile() is RenFile file && File.Exists(file.FullPath))
            Process.Start("notepad.exe", $"\"{file.FullPath}\"");
    }

    private void FileProperties_Click(object sender, RoutedEventArgs e)
    {
        if (GetSelectedFile() is RenFile file && File.Exists(file.FullPath))
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
        var selectedFiles = GetSelectedFiles().ToList();
        if (selectedFiles.Count == 0) return;
        var selected = selectedFiles.Select(f => f.FullPath).ToArray();
        var col = new System.Collections.Specialized.StringCollection();
        col.AddRange(selected);
        Clipboard.SetFileDropList(col);
        RemoveFilesInBulk(selectedFiles, "cut-from-list");
    }

    private void CopyFilesToClipboard_Click(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedFiles().Select(f => f.FullPath).ToArray();
        if (selected.Length == 0) return;
        var col = new System.Collections.Specialized.StringCollection();
        col.AddRange(selected);
        Clipboard.SetFileDropList(col);
    }

    private void DeleteToRecycleBin_Click(object sender, RoutedEventArgs e)
    {
        var selected = GetSelectedFiles().ToList();
        if (selected.Count == 0) return;
        if (MessageBox.Show($"Move {selected.Count} file(s) to Recycle Bin?", "Confirm",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        var failed = new List<string>();
        var successCount = 0;
        var removed = new List<RenFile>();

        foreach (var file in selected)
        {
            if (TrySendToRecycleBin(file.FullPath, out var error))
            {
                removed.Add(file);
                successCount++;
                continue;
            }

            failed.Add($"{file.FullPath} -> {error}");
        }

        if (removed.Count > 0)
        {
            RemoveFilesInBulk(removed, "delete-to-recycle-bin");
            AutoPreviewIfEnabled(sender, e);
        }

        if (failed.Count == 0)
        {
            tbStatusInfo.Text = $"Moved {successCount} file(s) to Recycle Bin.";
            return;
        }

        var maxLines = Math.Min(8, failed.Count);
        var summaryLines = failed.Take(maxLines);
        var suffix = failed.Count > maxLines
            ? $"\n...and {failed.Count - maxLines} more item(s)."
            : string.Empty;
        MessageBox.Show(
            "Some items could not be moved to Recycle Bin:\n" + string.Join("\n", summaryLines) + suffix,
            "Recycle Bin",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
        tbStatusInfo.Text = $"Recycle Bin completed with errors: success {successCount}, failed {failed.Count}.";
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

    private static bool TrySendToRecycleBin(string path, out string error)
    {
        error = string.Empty;
        IFileOperation? fileOp = null;
        IShellItem? item = null;
        try
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                error = "Path does not exist.";
                return false;
            }

            fileOp = (IFileOperation)new FileOperation();
            fileOp.SetOperationFlags(FOS_ALLOWUNDO | FOS_NOCONFIRMATION | FOS_NOERRORUI | FOS_SILENT);

            var iid = typeof(IShellItem).GUID;
            int hr = SHCreateItemFromParsingName(path, IntPtr.Zero, ref iid, out item);
            if (hr != 0 || item == null)
            {
                error = $"Shell create item failed (0x{hr:X8}).";
                return false;
            }

            fileOp.DeleteItem(item, IntPtr.Zero);

            int res = fileOp.PerformOperations();
            if (res != 0)
            {
                error = $"Recycle Bin operation failed (0x{res:X8}).";
                return false;
            }
            fileOp.GetAnyOperationsAborted(out bool aborted);
            if (aborted)
            {
                error = "Operation was canceled by system.";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LogException($"TrySendToRecycleBin({path})", ex);
            error = ex.Message;
            return false;
        }
        finally
        {
            if (item != null)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(item);
            if (fileOp != null)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fileOp);
        }
    }

    // Mark extensions
    private void MarkChangedIncCase_Click(object sender, RoutedEventArgs e)
    {
        SetMarkedInBulk(Files, f => !string.Equals(f.OriginalName, f.NewName, StringComparison.Ordinal));
    }

    private void MarkChangedExcCase_Click(object sender, RoutedEventArgs e)
    {
        SetMarkedInBulk(Files, f => !string.Equals(f.OriginalName, f.NewName, StringComparison.OrdinalIgnoreCase));
    }

    private void MarkByMask_Click(object sender, RoutedEventArgs e)
    {
        var mask = PromptInput(
            LanguageService.GetString("Dialog_MarkByMaskTitle"),
            LanguageService.GetString("Dialog_MarkByMaskPrompt"),
            "*.*");
        if (mask == null) return;
        var regex = WildcardToRegex(mask);
        SetMarkedInBulk(Files, f => regex.IsMatch(f.OriginalName));
    }

    // Clear extensions
    private void ClearValid_Click(object sender, RoutedEventArgs e)
        => RemoveFilesInBulk(Files.Where(f => f.State == "✓" || f.HasChanged).ToList(), "clear-valid");

    private void ClearInvalid_Click(object sender, RoutedEventArgs e)
        => RemoveFilesInBulk(Files.Where(f => f.State == "×" || !string.IsNullOrEmpty(f.Error)).ToList(), "clear-invalid");

    // Select extensions
    private void SelectByNameLength_Click(object sender, RoutedEventArgs e)
    {
        var input = PromptInput(
            LanguageService.GetString("Dialog_SelectByNameLengthTitle"),
            LanguageService.GetString("Dialog_SelectByNameLengthPrompt"),
            "20");
        if (input == null || !int.TryParse(input, out int maxLen)) return;
        RunFileSelectionBatch(() =>
        {
            var selected = Files.Where(f => f.OriginalName.Length <= maxLen).ToList();
            SetSelectedFiles(selected);
        });
    }

    private void SelectByExtension_Click(object sender, RoutedEventArgs e)
    {
        var ext = PromptInput(
            LanguageService.GetString("Dialog_SelectByExtensionTitle"),
            LanguageService.GetString("Dialog_SelectByExtensionPrompt"),
            ".txt");
        if (ext == null) return;
        RunFileSelectionBatch(() =>
        {
            var selected = Files.Where(f => f.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase)).ToList();
            SetSelectedFiles(selected);
        });
    }

    private void SelectByMask_Click(object sender, RoutedEventArgs e)
    {
        var mask = PromptInput(
            LanguageService.GetString("Dialog_SelectByMaskTitle"),
            LanguageService.GetString("Dialog_SelectByMaskPrompt"),
            "*.*");
        if (mask == null) return;
        var regex = WildcardToRegex(mask);
        RunFileSelectionBatch(() =>
        {
            var selected = Files.Where(f => regex.IsMatch(f.OriginalName)).ToList();
            SetSelectedFiles(selected);
        });
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
        Path.Combine(AppContext.BaseDirectory, "Presets");

    private void PopulatePresetMenu
    ()
    {
        try
        {
            while (menuLoadPreset.Items.Count > 0)
                menuLoadPreset.Items.RemoveAt(0);

            if (!Directory.Exists(PresetsDir))
            {
                Directory.CreateDirectory(PresetsDir);
                return;
            }

            var files = Directory
                .GetFiles(PresetsDir, "*.*")
                .Where(path =>
                {
                    var ext = Path.GetExtension(path);
                    return ext.Equals(".json", StringComparison.OrdinalIgnoreCase)
                           || ext.Equals(".rnp", StringComparison.OrdinalIgnoreCase);
                })
                .OrderBy(path => path)
                .ToArray();

            if (files.Length == 0)
            {
                menuLoadPreset.Items.Add(new MenuItem { Header = "(No presets found)", IsEnabled = false });
                return;
            }

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var mi = new MenuItem { Header = name, Tag = file };
                mi.Click += (s, _) =>
                {
                    try
                    {
                        var path = (string)((MenuItem)s!).Tag;
                        LoadPresetFromPath(path, "quick load preset", showWarningDialog: true);
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
        catch (Exception ex)
        {
            LogException("PopulatePresetMenu", ex);
            ShowBackgroundOperationError($"Preset list could not be loaded: {ex.Message}", showDialog: false);
        }
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
            Title = "Manage Presets",
            Width = 400,
            Height = 350,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.CanResize
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

    private void RescanPresets_Click(object sender, RoutedEventArgs e)
    {
        PopulatePresetMenu();
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

    private void RequestAppExit()
    {
        _exitRequestedFromTray = true;
        Close();
    }

    private void HideToTray()
    {
        UpdateTrayIconState();
        Hide();
    }

    private void RestoreFromTray()
    {
        _exitRequestedFromTray = false;
        EnsureWindowVisible();
        WindowState = WindowState.Normal;
    }

    private void UpdateTrayIconState()
    {
        if (!_appSettings.ShowInSystemTray)
        {
            if (_trayIcon != null)
                _trayIcon.Visible = false;
            return;
        }

        EnsureTrayIcon();
        if (_trayIcon != null)
        {
            _trayIcon.Text = "ReNamer";
            if (_trayIcon.ContextMenuStrip?.Items.Count >= 2)
            {
                _trayIcon.ContextMenuStrip.Items[0].Text = LanguageService.GetString("Tray_Open");
                _trayIcon.ContextMenuStrip.Items[1].Text = LanguageService.GetString("Tray_Exit");
            }
            _trayIcon.Visible = true;
        }
    }

    private void EnsureTrayIcon()
    {
        if (_trayIcon != null)
            return;

        _trayIcon = new WinForms.NotifyIcon
        {
            Icon = GetTrayIcon(),
            Visible = false
        };

        _trayIcon.DoubleClick += (_, _) => Dispatcher.BeginInvoke(new Action(RestoreFromTray));

        var menu = new WinForms.ContextMenuStrip();
        menu.Items.Add(LanguageService.GetString("Tray_Open"), null, (_, _) =>
            Dispatcher.BeginInvoke(new Action(RestoreFromTray)));
        menu.Items.Add(LanguageService.GetString("Tray_Exit"), null, (_, _) =>
            Dispatcher.BeginInvoke(new Action(RequestAppExit)));
        _trayIcon.ContextMenuStrip = menu;
    }

    private static System.Drawing.Icon GetTrayIcon()
    {
        try
        {
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrWhiteSpace(exePath))
            {
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                if (icon != null)
                    return icon;
            }
        }
        catch
        {
            // Ignore icon load failures and fallback to default system icon.
        }

        return System.Drawing.SystemIcons.Application;
    }

    private void DisposeTrayIcon()
    {
        if (_trayIcon == null)
            return;

        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _trayIcon = null;
    }

    private void SaveWindowState()
    {
        if (_appSettings.RememberWindowPosition)
        {
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
        }

        SaveColumnState();
        SaveSettingsWithFeedback("window state", showDialog: false);
    }

    private void SaveColumnState()
    {
        _appSettings.ColumnWidths.Clear();
        _appSettings.VisibleColumns.Clear();

        if (IsWin32FileList)
        {
            for (int i = 0; i < _fileListColumns.Count; i++)
            {
                var col = _fileListColumns[i];
                if (string.IsNullOrEmpty(col.Key))
                    continue;

                var width = _win32FileListView?.GetColumnWidth(i) ?? col.Width;
                if (width > 0)
                    _appSettings.ColumnWidths[col.Key] = width;
                if (col.Visible)
                    _appSettings.VisibleColumns.Add(col.Key);
            }
            return;
        }

        foreach (var col in lvFiles.Columns)
        {
            var key = GetColumnKey(col);
            if (string.IsNullOrEmpty(key))
                continue;

            var width = GetPersistedWidth(col);
            if (width > 0)
                _appSettings.ColumnWidths[key] = width;
            if (col.Visibility == Visibility.Visible)
                _appSettings.VisibleColumns.Add(key);
        }
    }

    private void RestoreColumnState()
    {
        if (_appSettings.ColumnWidths.Count == 0 && _appSettings.VisibleColumns.Count == 0)
            return;

        if (IsWin32FileList)
        {
            for (int i = 0; i < _fileListColumns.Count; i++)
            {
                var col = _fileListColumns[i];
                if (string.IsNullOrEmpty(col.Key))
                    continue;

                if (_appSettings.ColumnWidths.TryGetValue(col.Key, out var width) && width > 0)
                    col.Width = (int)Math.Round(width);

                if (_appSettings.VisibleColumns.Count > 0)
                {
                    var isVisible = _appSettings.VisibleColumns.Contains(col.Key) || IsForcedVisibleFileColumn(col.Key);
                    col.Visible = isVisible;
                    if (isVisible && (!_appSettings.ColumnWidths.TryGetValue(col.Key, out var persistedWidth) || persistedWidth <= 0))
                    {
                        if (_defaultColumnWidths.TryGetValue(col.Key, out var defaultWidth) && defaultWidth > 0)
                            col.Width = (int)Math.Round(defaultWidth);
                    }
                }
            }

            for (int i = 0; i < _fileListColumns.Count; i++)
                _win32FileListView?.UpdateColumnWidth(i, _fileListColumns[i].Width);
            _win32FileListView?.ApplyColumnVisibility();
            return;
        }

        foreach (var col in lvFiles.Columns)
        {
            var key = GetColumnKey(col);
            if (string.IsNullOrEmpty(key))
                continue;

            if (_appSettings.ColumnWidths.TryGetValue(key, out var width) && width > 0)
                col.Width = new DataGridLength(width);

            if (_appSettings.VisibleColumns.Count > 0)
            {
                var isVisible = _appSettings.VisibleColumns.Contains(key) || IsForcedVisibleFileColumn(key);
                col.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                if (isVisible && (!_appSettings.ColumnWidths.TryGetValue(key, out var persistedWidth) || persistedWidth <= 0))
                {
                    if (_defaultColumnWidths.TryGetValue(key, out var defaultWidth) && defaultWidth > 0)
                        col.Width = new DataGridLength(defaultWidth);
                }
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
        if (_appSettings.ShowInSystemTray && !_exitRequestedFromTray)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }

        if (_appSettings.ConfirmOnExit)
        {
            var shouldExit = MessageBox.Show(
                LanguageService.GetString("Msg_ConfirmExit"),
                LanguageService.GetString("Menu_Exit"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;
            if (!shouldExit)
            {
                _exitRequestedFromTray = false;
                e.Cancel = true;
                return;
            }
        }

        _exitRequestedFromTray = false;
        DisposeTrayIcon();

        Rules.CollectionChanged -= Rules_CollectionChanged;
        foreach (var rule in Rules)
            rule.PropertyChanged -= Rule_PropertyChanged;
        LanguageService.LanguageChanged -= OnLanguageChanged;
        SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;

        CancelPendingPreview();
        CancelPendingRename();
        SaveSessionRulesSnapshot();
        SaveWindowState();
    }

    #endregion

    #region Modern Window Controls

    private void ShowSystemMenuFrom(Point relativePoint)
    {
        var screenPoint = PointToScreen(relativePoint);
        SystemCommands.ShowSystemMenu(this, screenPoint);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
            return;

        if (e.ClickCount == 2)
        {
            if (ResizeMode != ResizeMode.NoResize)
            {
                if (WindowState == WindowState.Maximized)
                    SystemCommands.RestoreWindow(this);
                else
                    SystemCommands.MaximizeWindow(this);
            }
            return;
        }

        if (WindowState == WindowState.Maximized)
        {
            var mousePos = e.GetPosition(this);
            var horizontalPercent = ActualWidth > 0 ? mousePos.X / ActualWidth : 0.5;

            SystemCommands.RestoreWindow(this);
            Left = PointToScreen(mousePos).X - (Width * horizontalPercent);
            Top = Math.Max(SystemParameters.VirtualScreenTop, PointToScreen(mousePos).Y - (TitleBarBorder.ActualHeight / 2));
        }

        DragMove();
    }

    private void TitleBar_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        ShowSystemMenuFrom(e.GetPosition(this));
        e.Handled = true;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var focusedType = Keyboard.FocusedElement?.GetType().Name ?? "<null>";
        App.LogInputDebug($"WindowPreviewKeyDown key={key} systemKey={e.SystemKey} modifiers={Keyboard.Modifiers} handled={e.Handled} focused={focusedType}");

        if (!e.Handled
            && IsWin32FileList
            && win32FileListHost.IsKeyboardFocusWithin
            && Keyboard.Modifiers == ModifierKeys.None
            && key == Key.Delete)
        {
            if (TryExecute(CmdRemoveSelectedFiles))
                e.Handled = true;
            return;
        }

        if (e.SystemKey != Key.Space || (Keyboard.Modifiers & ModifierKeys.Alt) == 0)
            return;

        ShowSystemMenuFrom(new Point(0, TitleBarBorder.ActualHeight));
        e.Handled = true;
        App.LogInputDebug("WindowPreviewKeyDown handled Alt+Space system menu.");
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            SystemCommands.RestoreWindow(this);
        else
            SystemCommands.MaximizeWindow(this);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    #endregion
}

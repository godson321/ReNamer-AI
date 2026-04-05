using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ReNamer.Services;

namespace ReNamer.Views;

public partial class SettingsDialog : Window
{
    public AppSettings Settings { get; private set; }

    public SettingsDialog(AppSettings settings, SettingsTab? initialTab = null)
    {
        InitializeComponent();
        Settings = settings;
        LoadSettings();
        if (initialTab != null)
            tabSettings.SelectedIndex = (int)initialTab;
    }

    private void LoadSettings()
    {
        chkLoadLastPreset.IsChecked = Settings.LoadLastPreset;
        chkRememberWindowPos.IsChecked = Settings.RememberWindowPosition;
        chkConfirmOnExit.IsChecked = Settings.ConfirmOnExit;
        chkShowInTaskbar.IsChecked = Settings.ShowInSystemTray;

        // Language
        foreach (ComboBoxItem item in cmbLanguage.Items)
        {
            if (item.Tag as string == Settings.Language)
            { cmbLanguage.SelectedItem = item; break; }
        }
        if (cmbLanguage.SelectedIndex < 0) cmbLanguage.SelectedIndex = 0;

        // Theme
        foreach (ComboBoxItem item in cmbTheme.Items)
        {
            if (item.Tag as string == Settings.Theme)
            { cmbTheme.SelectedItem = item; break; }
        }
        if (cmbTheme.SelectedIndex < 0) cmbTheme.SelectedIndex = 0;

        chkAutoPreview.IsChecked = Settings.AutoPreview;
        chkPreviewOnFileAdd.IsChecked = Settings.PreviewOnFileAdd;
        chkHighlightChanges.IsChecked = Settings.HighlightChanges;
        chkAutoValidate.IsChecked = Settings.AutoValidate;
        chkWarnInvalidChars.IsChecked = Settings.WarnInvalidChars;
        chkWarnLongPaths.IsChecked = Settings.WarnLongPaths;

        chkConfirmRename.IsChecked = Settings.ConfirmRename;
        chkAutoRemoveRenamed.IsChecked = Settings.AutoRemoveRenamed;
        chkCreateUndoLog.IsChecked = Settings.CreateUndoLog;
        chkResolveMetaTags.IsChecked = Settings.ResolveMetaTags;
        chkEnableInputDebugLogging.IsChecked = Settings.EnableInputDebugLogging;

        rbConflictSkip.IsChecked = Settings.ConflictResolution == 0;
        rbConflictAddSuffix.IsChecked = Settings.ConflictResolution == 1;
        rbConflictOverwrite.IsChecked = Settings.ConflictResolution == 2;
    }

    private void SaveSettings()
    {
        Settings.LoadLastPreset = chkLoadLastPreset.IsChecked == true;
        Settings.RememberWindowPosition = chkRememberWindowPos.IsChecked == true;
        Settings.ConfirmOnExit = chkConfirmOnExit.IsChecked == true;
        Settings.ShowInSystemTray = chkShowInTaskbar.IsChecked == true;

        if (cmbLanguage.SelectedItem is ComboBoxItem langItem)
            Settings.Language = langItem.Tag as string ?? "zh-CN";

        if (cmbTheme.SelectedItem is ComboBoxItem themeItem)
            Settings.Theme = themeItem.Tag as string ?? "Light";

        Settings.AutoPreview = chkAutoPreview.IsChecked == true;
        Settings.PreviewOnFileAdd = chkPreviewOnFileAdd.IsChecked == true;
        Settings.HighlightChanges = chkHighlightChanges.IsChecked == true;
        Settings.AutoValidate = chkAutoValidate.IsChecked == true;
        Settings.WarnInvalidChars = chkWarnInvalidChars.IsChecked == true;
        Settings.WarnLongPaths = chkWarnLongPaths.IsChecked == true;

        Settings.ConfirmRename = chkConfirmRename.IsChecked == true;
        Settings.AutoRemoveRenamed = chkAutoRemoveRenamed.IsChecked == true;
        Settings.CreateUndoLog = chkCreateUndoLog.IsChecked == true;
        Settings.ResolveMetaTags = chkResolveMetaTags.IsChecked == true;
        Settings.EnableInputDebugLogging = chkEnableInputDebugLogging.IsChecked == true;

        if (rbConflictAddSuffix.IsChecked == true) Settings.ConflictResolution = 1;
        else if (rbConflictOverwrite.IsChecked == true) Settings.ConflictResolution = 2;
        else Settings.ConflictResolution = 0;

        ThemeService.ApplyTheme(Settings.Theme);
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}

public enum SettingsTab
{
    General = 0,
    Preview = 1,
    Rename = 2,
    Misc = 3
}

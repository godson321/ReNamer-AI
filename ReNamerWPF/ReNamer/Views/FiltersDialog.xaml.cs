using System;
using System.Globalization;
using System.Windows;
using ReNamer.Services;

namespace ReNamer.Views;

public partial class FiltersDialog : Window
{
    private readonly AppSettings _settings;

    public FiltersDialog(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        LoadSettings();
    }

    private void LoadSettings()
    {
        chkFiltersApplied.IsChecked = _settings.FiltersApplied;
        txtNamePattern.Text = _settings.FilterNamePattern;
        rbNameInclude.IsChecked = !_settings.FilterNameExclude;
        rbNameExclude.IsChecked = _settings.FilterNameExclude;
        chkNameCaseSensitive.IsChecked = _settings.FilterNameCaseSensitive;

        txtExtensions.Text = _settings.FilterExtensions;

        txtMinSize.Text = _settings.FilterMinSize > 0
            ? _settings.FilterMinSize.ToString(CultureInfo.InvariantCulture)
            : string.Empty;
        txtMaxSize.Text = _settings.FilterMaxSize > 0
            ? _settings.FilterMaxSize.ToString(CultureInfo.InvariantCulture)
            : string.Empty;
        cmbMinUnit.SelectedIndex = CoerceUnitIndex(_settings.FilterMinSizeUnit);
        cmbMaxUnit.SelectedIndex = CoerceUnitIndex(_settings.FilterMaxSizeUnit);

        chkAttrReadOnly.IsChecked = _settings.FilterAttrReadOnly;
        chkAttrHidden.IsChecked = _settings.FilterAttrHidden;
        chkAttrSystem.IsChecked = _settings.FilterAttrSystem;

        chkIncludeAllFiles.IsChecked = _settings.FolderIncludeAllFiles;
        chkIncludeFolderName.IsChecked = _settings.FolderIncludeFolderNames;
        chkIncludeSubFolders.IsChecked = _settings.FolderIncludeSubfolders;
        chkIncludeHiddenFiles.IsChecked = _settings.FolderIncludeHiddenFiles;
        chkIncludeSystemFiles.IsChecked = _settings.FolderIncludeSystemFiles;
        chkIgnoreRootFolder.IsChecked = _settings.FolderIgnoreRootFolder;
        txtIncludePattern.Text = _settings.FolderIncludeMask;
        txtExcludePattern.Text = _settings.FolderExcludeMask;
        chkPatternFileNameOnly.IsChecked = _settings.FolderMaskFileNameOnly;
        chkSaveAsDefault.IsChecked = _settings.FolderSaveAsDefault;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _settings.FiltersApplied = chkFiltersApplied.IsChecked == true;

        _settings.FilterNamePattern = txtNamePattern.Text?.Trim() ?? string.Empty;
        _settings.FilterNameEnabled = !string.IsNullOrWhiteSpace(_settings.FilterNamePattern);
        _settings.FilterNameExclude = rbNameExclude.IsChecked == true;
        _settings.FilterNameCaseSensitive = chkNameCaseSensitive.IsChecked == true;

        _settings.FilterExtensions = txtExtensions.Text?.Trim() ?? string.Empty;
        _settings.FilterExtEnabled = !string.IsNullOrWhiteSpace(_settings.FilterExtensions);

        _settings.FilterMinSize = ParseSizeText(txtMinSize.Text);
        _settings.FilterMaxSize = ParseSizeText(txtMaxSize.Text);
        _settings.FilterSizeEnabled = _settings.FilterMinSize > 0 || _settings.FilterMaxSize > 0;
        _settings.FilterMinSizeUnit = CoerceUnitIndex(cmbMinUnit.SelectedIndex);
        _settings.FilterMaxSizeUnit = CoerceUnitIndex(cmbMaxUnit.SelectedIndex);

        _settings.FilterAttrReadOnly = chkAttrReadOnly.IsChecked == true;
        _settings.FilterAttrHidden = chkAttrHidden.IsChecked == true;
        _settings.FilterAttrSystem = chkAttrSystem.IsChecked == true;
        _settings.FilterAttrEnabled = _settings.FilterAttrReadOnly
            || _settings.FilterAttrHidden
            || _settings.FilterAttrSystem;

        _settings.FolderIncludeAllFiles = chkIncludeAllFiles.IsChecked == true;
        _settings.FolderIncludeFolderNames = chkIncludeFolderName.IsChecked == true;
        _settings.FolderIncludeSubfolders = chkIncludeSubFolders.IsChecked == true;
        _settings.FolderIncludeHiddenFiles = chkIncludeHiddenFiles.IsChecked == true;
        _settings.FolderIncludeSystemFiles = chkIncludeSystemFiles.IsChecked == true;
        _settings.FolderIgnoreRootFolder = chkIgnoreRootFolder.IsChecked == true;
        _settings.FolderIncludeMask = txtIncludePattern.Text?.Trim() ?? string.Empty;
        _settings.FolderExcludeMask = txtExcludePattern.Text?.Trim() ?? string.Empty;
        _settings.FolderMaskFileNameOnly = chkPatternFileNameOnly.IsChecked == true;
        _settings.FolderSaveAsDefault = chkSaveAsDefault.IsChecked == true;
        _settings.Save();

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private static int CoerceUnitIndex(int index)
    {
        if (index < 0 || index > 3)
            return 0;
        return index;
    }

    private static double ParseSizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var raw = text.Trim();
        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariantValue))
            return Math.Max(0, invariantValue);
        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out var cultureValue))
            return Math.Max(0, cultureValue);
        return 0;
    }
}

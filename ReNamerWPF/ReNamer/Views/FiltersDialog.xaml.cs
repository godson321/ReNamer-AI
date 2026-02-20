using System;
using System.Collections.Generic;
using System.Windows;
using ReNamer.Models;
using ReNamer.Services;

namespace ReNamer.Views;

public partial class FiltersDialog : Window
{
    private readonly AppSettings _settings;

    public FiltersDialog(IList<RenFile> files, Action refreshCallback, AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        LoadSettings();
    }

    private void LoadSettings()
    {
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
}

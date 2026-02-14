using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using ReNamer.Models;
using ReNamer.Services;

namespace ReNamer.Views;

public partial class FiltersDialog : Window
{
    private readonly IList<RenFile> _files;
    private readonly Action _refreshCallback;
    private readonly AppSettings _settings;

    public FiltersDialog(IList<RenFile> files, Action refreshCallback, AppSettings settings)
    {
        InitializeComponent();
        _files = files;
        _refreshCallback = refreshCallback;
        _settings = settings;
        LoadSettings();
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        int filtered = 0;

        foreach (var file in _files)
        {
            bool pass = true;

            // Name filter
            if (chkEnableNameFilter.IsChecked == true && !string.IsNullOrEmpty(txtNamePattern.Text))
            {
                bool matches = MatchWildcard(file.OriginalName, txtNamePattern.Text,
                    chkNameCaseSensitive.IsChecked == true);
                bool isExclude = cmbNameFilterMode.SelectedIndex == 1;
                pass = isExclude ? !matches : matches;
            }

            // Extension filter
            if (pass && chkEnableExtFilter.IsChecked == true && !string.IsNullOrEmpty(txtExtensions.Text))
            {
                var exts = txtExtensions.Text.Split(';', ',')
                    .Select(x => x.Trim().ToLowerInvariant())
                    .Where(x => x.Length > 0)
                    .ToHashSet();
                pass = exts.Contains(file.Extension.ToLowerInvariant());
            }

            // Size filter
            if (pass && chkEnableSizeFilter.IsChecked == true)
            {
                long minBytes = ParseSize(txtMinSize.Text, cmbMinSizeUnit.SelectedIndex);
                long maxBytes = ParseSize(txtMaxSize.Text, cmbMaxSizeUnit.SelectedIndex);
                if (minBytes > 0) pass = file.Size >= minBytes;
                if (pass && maxBytes > 0) pass = file.Size <= maxBytes;
            }

            // Attribute filter
            if (pass && chkEnableAttrFilter.IsChecked == true)
            {
                try
                {
                    var attrs = File.GetAttributes(file.FullPath);
                    if (chkAttrReadOnly.IsChecked == true && !attrs.HasFlag(FileAttributes.ReadOnly)) pass = false;
                    if (chkAttrHidden.IsChecked == true && !attrs.HasFlag(FileAttributes.Hidden)) pass = false;
                    if (chkAttrSystem.IsChecked == true && !attrs.HasFlag(FileAttributes.System)) pass = false;
                }
                catch { pass = false; }
            }

            file.IsMarked = pass;
            if (!pass) filtered++;
        }

        _refreshCallback();
        SaveSettingsFromUi(applied: true);
        MessageBox.Show($"Filter applied. {_files.Count - filtered} files matched, {filtered} unmarked.",
            "Filters", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ClearAll_Click(object sender, RoutedEventArgs e)
    {
        chkEnableNameFilter.IsChecked = false;
        chkEnableExtFilter.IsChecked = false;
        chkEnableSizeFilter.IsChecked = false;
        chkEnableAttrFilter.IsChecked = false;
        txtNamePattern.Text = "";
        txtExtensions.Text = "";
        txtMinSize.Text = "0";
        txtMaxSize.Text = "0";

        foreach (var f in _files) f.IsMarked = true;
        _refreshCallback();
        SaveSettingsFromUi(applied: false);
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private static bool MatchWildcard(string input, string pattern, bool caseSensitive)
    {
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        return Regex.IsMatch(input, regexPattern, options);
    }

    private static long ParseSize(string text, int unitIndex)
    {
        if (!double.TryParse(text, out double val) || val <= 0) return 0;
        return unitIndex switch
        {
            0 => (long)val,             // B
            1 => (long)(val * 1024),    // KB
            2 => (long)(val * 1048576), // MB
            3 => (long)(val * 1073741824), // GB
            _ => 0
        };
    }

    private void LoadSettings()
    {
        chkEnableNameFilter.IsChecked = _settings.FilterNameEnabled;
        txtNamePattern.Text = _settings.FilterNamePattern;
        chkNameCaseSensitive.IsChecked = _settings.FilterNameCaseSensitive;
        cmbNameFilterMode.SelectedIndex = _settings.FilterNameExclude ? 1 : 0;

        chkEnableExtFilter.IsChecked = _settings.FilterExtEnabled;
        txtExtensions.Text = _settings.FilterExtensions;

        chkEnableSizeFilter.IsChecked = _settings.FilterSizeEnabled;
        txtMinSize.Text = _settings.FilterMinSize.ToString();
        cmbMinSizeUnit.SelectedIndex = _settings.FilterMinSizeUnit;
        txtMaxSize.Text = _settings.FilterMaxSize.ToString();
        cmbMaxSizeUnit.SelectedIndex = _settings.FilterMaxSizeUnit;

        chkEnableAttrFilter.IsChecked = _settings.FilterAttrEnabled;
        chkAttrReadOnly.IsChecked = _settings.FilterAttrReadOnly;
        chkAttrHidden.IsChecked = _settings.FilterAttrHidden;
        chkAttrSystem.IsChecked = _settings.FilterAttrSystem;
    }

    private void SaveSettingsFromUi(bool applied)
    {
        _settings.FiltersApplied = applied;
        _settings.FilterNameEnabled = chkEnableNameFilter.IsChecked == true;
        _settings.FilterNamePattern = txtNamePattern.Text ?? "";
        _settings.FilterNameCaseSensitive = chkNameCaseSensitive.IsChecked == true;
        _settings.FilterNameExclude = cmbNameFilterMode.SelectedIndex == 1;

        _settings.FilterExtEnabled = chkEnableExtFilter.IsChecked == true;
        _settings.FilterExtensions = txtExtensions.Text ?? "";

        _settings.FilterSizeEnabled = chkEnableSizeFilter.IsChecked == true;
        _settings.FilterMinSize = double.TryParse(txtMinSize.Text, out var min) ? min : 0;
        _settings.FilterMinSizeUnit = cmbMinSizeUnit.SelectedIndex;
        _settings.FilterMaxSize = double.TryParse(txtMaxSize.Text, out var max) ? max : 0;
        _settings.FilterMaxSizeUnit = cmbMaxSizeUnit.SelectedIndex;

        _settings.FilterAttrEnabled = chkEnableAttrFilter.IsChecked == true;
        _settings.FilterAttrReadOnly = chkAttrReadOnly.IsChecked == true;
        _settings.FilterAttrHidden = chkAttrHidden.IsChecked == true;
        _settings.FilterAttrSystem = chkAttrSystem.IsChecked == true;

        _settings.Save();
    }
}

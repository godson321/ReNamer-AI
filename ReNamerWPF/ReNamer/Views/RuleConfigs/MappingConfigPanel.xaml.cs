using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class MappingConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly MappingRule _rule;
    private readonly ObservableCollection<MappingEntry> _entries = new();

    public MappingConfigPanel(MappingRule rule)
    {
        InitializeComponent();
        _rule = rule;
        dgMappings.ItemsSource = _entries;
        LoadConfig();
    }

    private void LoadConfig()
    {
        _entries.Clear();
        foreach (var m in _rule.Mappings)
            _entries.Add(new MappingEntry { Match = m.Match, NewName = m.NewName });

        chkAllowReuse.IsChecked = _rule.AllowReuse;
        chkPartialMatch.IsChecked = _rule.PartialMatch;
        chkInverse.IsChecked = _rule.InverseMapping;
        chkCaseSensitive.IsChecked = _rule.CaseSensitive;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
    }

    public void ApplyConfig()
    {
        _rule.Mappings.Clear();
        foreach (var e in _entries)
        {
            if (!string.IsNullOrEmpty(e.Match) || !string.IsNullOrEmpty(e.NewName))
                _rule.Mappings.Add(new MappingEntry { Match = e.Match, NewName = e.NewName });
        }
        _rule.AllowReuse = chkAllowReuse.IsChecked == true;
        _rule.PartialMatch = chkPartialMatch.IsChecked == true;
        _rule.InverseMapping = chkInverse.IsChecked == true;
        _rule.CaseSensitive = chkCaseSensitive.IsChecked == true;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
    }

    private void BtnTable_Click(object sender, RoutedEventArgs e)
    {
        var menu = new ContextMenu();
        var loadFile = new MenuItem { Header = "Load from file..." };
        loadFile.Click += (_, _) => LoadFromFile();
        var loadClip = new MenuItem { Header = "Load from clipboard" };
        loadClip.Click += (_, _) => LoadFromClipboard();
        var saveFile = new MenuItem { Header = "Save to file..." };
        saveFile.Click += (_, _) => SaveToFile();
        var clear = new MenuItem { Header = "Clear" };
        clear.Click += (_, _) => { _entries.Clear(); };

        menu.Items.Add(loadFile);
        menu.Items.Add(loadClip);
        menu.Items.Add(new Separator());
        menu.Items.Add(saveFile);
        menu.Items.Add(new Separator());
        menu.Items.Add(clear);
        menu.IsOpen = true;
    }

    private void LoadFromFile()
    {
        var dlg = new OpenFileDialog { Filter = "Text files|*.txt;*.csv|All files|*.*" };
        if (dlg.ShowDialog() == true)
        {
            _entries.Clear();
            foreach (var line in File.ReadAllLines(dlg.FileName))
            {
                var parts = line.Split('\t', '|', ',');
                if (parts.Length >= 2)
                    _entries.Add(new MappingEntry { Match = parts[0].Trim(), NewName = parts[1].Trim() });
            }
        }
    }

    private void LoadFromClipboard()
    {
        if (Clipboard.ContainsText())
        {
            _entries.Clear();
            foreach (var line in Clipboard.GetText().Split('\n', '\r'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('\t', '|', ',');
                if (parts.Length >= 2)
                    _entries.Add(new MappingEntry { Match = parts[0].Trim(), NewName = parts[1].Trim() });
            }
        }
    }

    private void SaveToFile()
    {
        var dlg = new SaveFileDialog { Filter = "Text files|*.txt|CSV files|*.csv", DefaultExt = ".txt" };
        if (dlg.ShowDialog() == true)
        {
            var lines = new System.Collections.Generic.List<string>();
            foreach (var e in _entries)
                lines.Add($"{e.Match}\t{e.NewName}");
            File.WriteAllLines(dlg.FileName, lines);
        }
    }
}

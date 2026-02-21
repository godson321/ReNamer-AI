using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Jint;
using ReNamer.Rules;
using ReNamer.Views;

namespace ReNamer.Views.RuleConfigs;

public partial class JavaScriptConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly JavaScriptRule _rule;
    private static readonly string ScriptsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
    private readonly ObservableCollection<ScriptListItem> _scripts = [];
    private CancellationTokenSource? _scriptLoadCts;
    private int _scriptLoadVersion;

    public JavaScriptConfigPanel(JavaScriptRule rule)
    {
        InitializeComponent();
        _rule = rule;
        dgScripts.ItemsSource = _scripts;
        LoadConfig();
    }

    private void LoadConfig()
    {
        EnsureScriptsFolder();
        ReloadScriptList();

        if (!string.IsNullOrWhiteSpace(_rule.ScriptFilePath))
        {
            var selected = _scripts.FirstOrDefault(s => string.Equals(s.FullPath, _rule.ScriptFilePath, StringComparison.OrdinalIgnoreCase));
            if (selected != null)
            {
                dgScripts.SelectedItem = selected;
                UpdateStatus(_rule.IsScriptLoading
                    ? RF("JavaScript_Status_SelectedLoading", selected.Name)
                    : RF("JavaScript_Status_Selected", selected.Name));
                return;
            }
        }

        UpdateStatus(R("JavaScript_Status_SelectHint"));
    }

    public void ApplyConfig()
    {
        // 选择时已实时写入 rule，这里无需额外处理。
    }

    private void EnsureScriptsFolder()
    {
        if (!Directory.Exists(ScriptsFolder))
            Directory.CreateDirectory(ScriptsFolder);
    }

    private void ReloadScriptList()
    {
        _scripts.Clear();
        EnsureScriptsFolder();

        var files = Directory.GetFiles(ScriptsFolder, "*.js", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            _scripts.Add(new ScriptListItem
            {
                Name = Path.GetFileNameWithoutExtension(file.Name),
                FullPath = file.FullName,
                SizeKb = (file.Length / 1024d).ToString("N1", CultureInfo.InvariantCulture),
                LastWriteTime = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            });
        }
    }

    private async void ScriptsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (dgScripts.SelectedItem is not ScriptListItem item)
            return;
        _ = SelectScriptAsync(item);
        if (Window.GetWindow(this) is AddRuleDialog dialog)
            dialog.ConfirmCurrentRule();
    }

    private async Task SelectScriptAsync(ScriptListItem item)
    {
        _scriptLoadCts?.Cancel();
        _scriptLoadCts?.Dispose();
        _scriptLoadCts = new CancellationTokenSource();
        var token = _scriptLoadCts.Token;
        var loadVersion = Interlocked.Increment(ref _scriptLoadVersion);

        _rule.MarkScriptLoading(item.FullPath);
        UpdateStatus(RF("JavaScript_Status_SelectedLoading", item.Name));

        try
        {
            var script = await Task.Run(() => File.ReadAllText(item.FullPath));
            if (token.IsCancellationRequested || loadVersion != _scriptLoadVersion)
                return;

            _rule.MarkScriptLoaded(item.FullPath, script);
            UpdateStatus(RF("JavaScript_Status_SelectedLoaded", item.Name));
        }
        catch (Exception ex)
        {
            _rule.MarkScriptLoadFailed(item.FullPath, ex.Message);
            UpdateStatus(RF("JavaScript_Status_LoadFailed", item.Name, ex.Message));
        }
    }

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        ReloadScriptList();
        UpdateStatus(RF("JavaScript_Status_Reloaded", _scripts.Count));
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        EnsureScriptsFolder();
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = ScriptsFolder,
            UseShellExecute = true
        });
    }

    private void Validate_Click(object sender, RoutedEventArgs e)
    {
        var title = R("Rule_JavaScript");
        if (_rule.IsScriptLoading)
        {
            MessageBox.Show(R("JavaScript_Validate_WaitLoading"), title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!_rule.IsScriptReady)
        {
            var reason = string.IsNullOrWhiteSpace(_rule.ScriptLoadError) ? R("JavaScript_Validate_NotSelected") : _rule.ScriptLoadError;
            MessageBox.Show(RF("JavaScript_Validate_Unavailable", reason), title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var engine = new Engine();
            engine.SetValue("Name", "test");
            engine.SetValue("Ext", ".txt");
            engine.SetValue("FullName", "test.txt");
            engine.SetValue("Path", "C:\\test.txt");
            engine.SetValue("FolderPath", "C:\\");
            engine.SetValue("Folder", "test");
            engine.SetValue("Counter", "001");
            engine.SetValue("Size", 1024L);
            engine.SetValue("SizeKB", "1");
            engine.SetValue("SizeMB", "0");
            engine.SetValue("Created", DateTime.Now.ToString("yyyy-MM-dd"));
            engine.SetValue("Modified", DateTime.Now.ToString("yyyy-MM-dd"));
            engine.SetValue("ExifDate", "");
            engine.SetValue("Result", "");

            engine.SetValue("UpperCase", (Func<string, string>)(s => s?.ToUpperInvariant() ?? ""));
            engine.SetValue("LowerCase", (Func<string, string>)(s => s?.ToLowerInvariant() ?? ""));
            engine.SetValue("Trim", (Func<string, string>)(s => s?.Trim() ?? ""));
            engine.SetValue("Replace", (Func<string, string, string, string>)((s, o, n) => s?.Replace(o, n) ?? ""));
            engine.SetValue("LeftStr", (Func<string, int, string>)((s, n) => s?.Length >= n ? s[..n] : s ?? ""));
            engine.SetValue("RightStr", (Func<string, int, string>)((s, n) => s?.Length >= n ? s[^n..] : s ?? ""));
            engine.SetValue("MidStr", (Func<string, int, int, string>)((s, start, len) =>
                s != null && start >= 0 && start < s.Length
                    ? s.Substring(start, Math.Min(len, s.Length - start))
                    : ""));

            engine.Execute(_rule.ScriptText);
            MessageBox.Show(R("JavaScript_ValidationSuccess"), title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Jint.Runtime.JavaScriptException jsEx)
        {
            var line = jsEx.Location.Start.Line;
            MessageBox.Show(RF("JavaScript_ValidationError", line, jsEx.Message), title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(RF("JavaScript_ValidationErrorSimple", ex.Message), title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Help_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            R("JavaScript_HelpPopupContent"),
            R("JavaScript_HelpTitle"),
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void UpdateStatus(string text)
    {
        txtStatus.Text = text;
    }

    private static string RF(string key, params object[] args)
    {
        return string.Format(CultureInfo.CurrentUICulture, R(key), args);
    }

    private static string R(string key)
    {
        if (Application.Current?.TryFindResource(key) is string value && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return key;
    }

    private sealed class ScriptListItem
    {
        public string Name { get; init; } = string.Empty;
        public string FullPath { get; init; } = string.Empty;
        public string SizeKb { get; init; } = string.Empty;
        public string LastWriteTime { get; init; } = string.Empty;
    }
}

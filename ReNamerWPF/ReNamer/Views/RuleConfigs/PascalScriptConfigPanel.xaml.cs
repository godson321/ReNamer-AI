using System.Windows;
using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class PascalScriptConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly PascalScriptRule _rule;

    public PascalScriptConfigPanel(PascalScriptRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void LoadConfig()
    {
        txtScript.Text = _rule.ScriptText;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
        chkMetaTags.IsChecked = _rule.EnableMetaTags;
        nudCounterStart.Value = _rule.CounterStart;
        nudCounterStep.Value = _rule.CounterStep;
        nudCounterDigits.Value = _rule.CounterDigits;
    }

    public void ApplyConfig()
    {
        _rule.ScriptText = txtScript.Text;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
        _rule.EnableMetaTags = chkMetaTags.IsChecked == true;
        _rule.CounterStart = nudCounterStart.Value;
        _rule.CounterStep = nudCounterStep.Value;
        _rule.CounterDigits = nudCounterDigits.Value;
    }

    private void Scripts_Click(object sender, RoutedEventArgs e)
    {
        if (btnScripts.ContextMenu == null)
            return;
        btnScripts.ContextMenu.PlacementTarget = btnScripts;
        btnScripts.ContextMenu.IsOpen = true;
    }

    private void ScriptBasic_Click(object sender, RoutedEventArgs e)
    {
        SetScript("Result := Name;");
    }

    private void ScriptCounter_Click(object sender, RoutedEventArgs e)
    {
        SetScript("Result := Name + '_' + Counter;");
    }

    private void ScriptFolder_Click(object sender, RoutedEventArgs e)
    {
        SetScript("Result := Folder + '_' + Name;");
    }

    private void SetScript(string script)
    {
        txtScript.Text = script;
        txtScript.CaretIndex = txtScript.Text.Length;
        txtScript.Focus();
    }
}

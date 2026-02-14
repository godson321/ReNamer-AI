using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class TransliterateConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly TransliterateRule _rule;

    public TransliterateConfigPanel(TransliterateRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void LoadConfig()
    {
        txtAlphabet.Text = _rule.Alphabet;
        rbForward.IsChecked = _rule.DirectionForward;
        rbBackward.IsChecked = !_rule.DirectionForward;
        chkAutoCase.IsChecked = _rule.AutoCaseAdjustment;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
    }

    public void ApplyConfig()
    {
        _rule.Alphabet = txtAlphabet.Text;
        _rule.DirectionForward = rbForward.IsChecked == true;
        _rule.AutoCaseAdjustment = chkAutoCase.IsChecked == true;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
    }
}

using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class ChineseNumberConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly ChineseNumberRule _rule;

    public ChineseNumberConfigPanel(ChineseNumberRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void LoadConfig()
    {
        rbStrict.IsChecked = !_rule.AllowLooseForms;
        rbLoose.IsChecked = _rule.AllowLooseForms;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
    }

    public void ApplyConfig()
    {
        _rule.AllowLooseForms = rbLoose.IsChecked == true;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
    }
}

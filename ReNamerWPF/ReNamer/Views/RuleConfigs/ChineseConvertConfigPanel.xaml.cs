using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class ChineseConvertConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly ChineseConvertRule _rule;

    public ChineseConvertConfigPanel(ChineseConvertRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void LoadConfig()
    {
        rbTraditionalToSimplified.IsChecked = _rule.Direction == ChineseConvertDirection.TraditionalToSimplified;
        rbSimplifiedToTraditional.IsChecked = _rule.Direction == ChineseConvertDirection.SimplifiedToTraditional;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
    }

    public void ApplyConfig()
    {
        _rule.Direction = rbSimplifiedToTraditional.IsChecked == true
            ? ChineseConvertDirection.SimplifiedToTraditional
            : ChineseConvertDirection.TraditionalToSimplified;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
    }
}

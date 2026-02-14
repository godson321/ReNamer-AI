using System.Windows;
using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class RearrangeConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly RearrangeRule _rule;

    public RearrangeConfigPanel(RearrangeRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void BtnDelimSeparator_Click(object sender, RoutedEventArgs e) =>
        RuleConfigHelper.InsertSeparator(txtDelimiters);

    private void BtnPatternMetaTag_Click(object sender, RoutedEventArgs e) =>
        RuleConfigHelper.ShowMetaTagMenu(txtNewPattern, btnPatternMetaTag);

    private void LoadConfig()
    {
        rbDelimiters.IsChecked = _rule.SplitMode == RearrangeSplitMode.Delimiters;
        rbPositions.IsChecked = _rule.SplitMode == RearrangeSplitMode.Positions;
        rbExact.IsChecked = _rule.SplitMode == RearrangeSplitMode.ExactPattern;
        txtDelimiters.Text = _rule.Delimiters;
        txtNewPattern.Text = _rule.NewPattern;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
        chkRightToLeft.IsChecked = _rule.RightToLeft;
    }

    public void ApplyConfig()
    {
        _rule.SplitMode = rbPositions.IsChecked == true ? RearrangeSplitMode.Positions
                        : rbExact.IsChecked == true ? RearrangeSplitMode.ExactPattern
                        : RearrangeSplitMode.Delimiters;
        _rule.Delimiters = txtDelimiters.Text;
        _rule.NewPattern = txtNewPattern.Text;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
        _rule.RightToLeft = chkRightToLeft.IsChecked == true;
    }
}

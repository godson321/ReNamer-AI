using System.Windows;
using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class RegexConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly RegexRule _rule;

    public RegexConfigPanel(RegexRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void BtnInsertExpr_Click(object sender, RoutedEventArgs e) =>
        RuleConfigHelper.ShowExpressionMenu(txtExpression, btnInsertExpr);

    private void BtnHelp_Click(object sender, RoutedEventArgs e) =>
        RuleConfigHelper.OpenUrl("https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference");

    private void LoadConfig()
    {
        txtExpression.Text = _rule.Expression;
        txtReplace.Text = _rule.ReplaceText;
        chkCaseSensitive.IsChecked = _rule.CaseSensitive;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
    }

    public void ApplyConfig()
    {
        _rule.Expression = txtExpression.Text;
        _rule.ReplaceText = txtReplace.Text;
        _rule.CaseSensitive = chkCaseSensitive.IsChecked == true;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
    }
}

using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class StripConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly StripRule _rule;

    public StripConfigPanel(StripRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void LoadConfig()
    {
        chkEnglish.IsChecked = _rule.StripEnglishLetters;
        chkDigits.IsChecked = _rule.StripDigits;
        chkSymbols.IsChecked = _rule.StripSymbols;
        chkBrackets.IsChecked = _rule.StripBrackets;
        chkUserDefined.IsChecked = _rule.StripUserDefined;
        txtUserDefined.Text = _rule.UserDefinedChars;
        chkUnicodeRange.IsChecked = _rule.StripUnicodeRange;
        txtUnicodeRange.Text = _rule.UnicodeRange;
        rbEverywhere.IsChecked = _rule.Where == StripWhere.Everywhere;
        rbLeading.IsChecked = _rule.Where == StripWhere.Leading;
        rbTrailing.IsChecked = _rule.Where == StripWhere.Trailing;
        chkAllExcept.IsChecked = _rule.StripAllExceptSelected;
        chkCaseSensitive.IsChecked = _rule.CaseSensitive;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
    }

    public void ApplyConfig()
    {
        _rule.StripEnglishLetters = chkEnglish.IsChecked == true;
        _rule.StripDigits = chkDigits.IsChecked == true;
        _rule.StripSymbols = chkSymbols.IsChecked == true;
        _rule.StripBrackets = chkBrackets.IsChecked == true;
        _rule.StripUserDefined = chkUserDefined.IsChecked == true;
        _rule.UserDefinedChars = txtUserDefined.Text;
        _rule.StripUnicodeRange = chkUnicodeRange.IsChecked == true;
        _rule.UnicodeRange = txtUnicodeRange.Text;
        _rule.Where = rbLeading.IsChecked == true ? StripWhere.Leading
                    : rbTrailing.IsChecked == true ? StripWhere.Trailing
                    : StripWhere.Everywhere;
        _rule.StripAllExceptSelected = chkAllExcept.IsChecked == true;
        _rule.CaseSensitive = chkCaseSensitive.IsChecked == true;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
    }
}

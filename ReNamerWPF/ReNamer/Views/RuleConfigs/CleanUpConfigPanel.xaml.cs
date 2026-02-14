using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class CleanUpConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly CleanUpRule _rule;

    public CleanUpConfigPanel(CleanUpRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void LoadConfig()
    {
        chkRound.IsChecked = _rule.StripRoundBrackets;
        chkSquare.IsChecked = _rule.StripSquareBrackets;
        chkCurly.IsChecked = _rule.StripCurlyBrackets;
        chkDot.IsChecked = _rule.ReplaceDotWithSpace;
        chkComma.IsChecked = _rule.ReplaceCommaWithSpace;
        chkUnderscore.IsChecked = _rule.ReplaceUnderscoreWithSpace;
        chkPlus.IsChecked = _rule.ReplacePlusWithSpace;
        chkHyphen.IsChecked = _rule.ReplaceHyphenWithSpace;
        chkWeb.IsChecked = _rule.ReplaceWeb20WithSpace;
        chkSkipVersions.IsChecked = _rule.SkipVersionNumbers;
        chkFixSpaces.IsChecked = _rule.FixSpaces;
        chkNormalizeSpaces.IsChecked = _rule.NormalizeUnicodeSpaces;
        chkStripEmoji.IsChecked = _rule.StripUnicodeEmoji;
        chkStripMarks.IsChecked = _rule.StripUnicodeMarks;
        chkCamelCase.IsChecked = _rule.InsertSpaceBeforeCapitals;
        chkSharePoint.IsChecked = _rule.PrepareForSharePoint;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
    }

    public void ApplyConfig()
    {
        _rule.StripRoundBrackets = chkRound.IsChecked == true;
        _rule.StripSquareBrackets = chkSquare.IsChecked == true;
        _rule.StripCurlyBrackets = chkCurly.IsChecked == true;
        _rule.ReplaceDotWithSpace = chkDot.IsChecked == true;
        _rule.ReplaceCommaWithSpace = chkComma.IsChecked == true;
        _rule.ReplaceUnderscoreWithSpace = chkUnderscore.IsChecked == true;
        _rule.ReplacePlusWithSpace = chkPlus.IsChecked == true;
        _rule.ReplaceHyphenWithSpace = chkHyphen.IsChecked == true;
        _rule.ReplaceWeb20WithSpace = chkWeb.IsChecked == true;
        _rule.SkipVersionNumbers = chkSkipVersions.IsChecked == true;
        _rule.FixSpaces = chkFixSpaces.IsChecked == true;
        _rule.NormalizeUnicodeSpaces = chkNormalizeSpaces.IsChecked == true;
        _rule.StripUnicodeEmoji = chkStripEmoji.IsChecked == true;
        _rule.StripUnicodeMarks = chkStripMarks.IsChecked == true;
        _rule.InsertSpaceBeforeCapitals = chkCamelCase.IsChecked == true;
        _rule.PrepareForSharePoint = chkSharePoint.IsChecked == true;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
    }
}

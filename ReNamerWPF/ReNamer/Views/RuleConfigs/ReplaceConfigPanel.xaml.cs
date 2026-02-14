using System.Windows;
using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class ReplaceConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly ReplaceRule _rule;

    public ReplaceConfigPanel(ReplaceRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void BtnFindSeparator_Click(object sender, RoutedEventArgs e) =>
        RuleConfigHelper.InsertSeparator(txtFind);

    private void BtnReplaceMetaTag_Click(object sender, RoutedEventArgs e) =>
        RuleConfigHelper.ShowMetaTagMenu(txtReplace, btnReplaceMetaTag);

    private void LoadConfig()
    {
        txtFind.Text = _rule.FindText;
        txtReplace.Text = _rule.ReplaceText;
        chkCaseSensitive.IsChecked = _rule.CaseSensitive;
        chkWholeWords.IsChecked = _rule.WholeWordsOnly;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
        chkUseWildcards.IsChecked = _rule.UseWildcards;

        switch (_rule.Occurrence)
        {
            case ReplaceOccurrence.All: rbAll.IsChecked = true; break;
            case ReplaceOccurrence.First: rbFirst.IsChecked = true; break;
            case ReplaceOccurrence.Last: rbLast.IsChecked = true; break;
        }
    }

    public void ApplyConfig()
    {
        _rule.FindText = txtFind.Text;
        _rule.ReplaceText = txtReplace.Text;
        _rule.CaseSensitive = chkCaseSensitive.IsChecked == true;
        _rule.WholeWordsOnly = chkWholeWords.IsChecked == true;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
        _rule.UseWildcards = chkUseWildcards.IsChecked == true;

        if (rbFirst.IsChecked == true) _rule.Occurrence = ReplaceOccurrence.First;
        else if (rbLast.IsChecked == true) _rule.Occurrence = ReplaceOccurrence.Last;
        else _rule.Occurrence = ReplaceOccurrence.All;
    }
}

using System.Windows;
using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class RemoveConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly RemoveRule _rule;

    public RemoveConfigPanel(RemoveRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void BtnSeparator_Click(object sender, RoutedEventArgs e) =>
        RuleConfigHelper.InsertSeparator(txtPattern);

    private void LoadConfig()
    {
        txtPattern.Text = _rule.Pattern;
        chkCaseSensitive.IsChecked = _rule.CaseSensitive;
        chkWholeWords.IsChecked = _rule.WholeWordsOnly;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
        chkUseWildcards.IsChecked = _rule.UseWildcards;

        switch (_rule.Occurrence)
        {
            case RemoveOccurrence.All: rbAll.IsChecked = true; break;
            case RemoveOccurrence.First: rbFirst.IsChecked = true; break;
            case RemoveOccurrence.Last: rbLast.IsChecked = true; break;
        }
    }

    public void ApplyConfig()
    {
        _rule.Pattern = txtPattern.Text;
        _rule.CaseSensitive = chkCaseSensitive.IsChecked == true;
        _rule.WholeWordsOnly = chkWholeWords.IsChecked == true;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
        _rule.UseWildcards = chkUseWildcards.IsChecked == true;

        if (rbFirst.IsChecked == true) _rule.Occurrence = RemoveOccurrence.First;
        else if (rbLast.IsChecked == true) _rule.Occurrence = RemoveOccurrence.Last;
        else _rule.Occurrence = RemoveOccurrence.All;
    }
}

using System.Windows;
using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class InsertConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly InsertRule _rule;

    public InsertConfigPanel(InsertRule rule)
    {
        InitializeComponent();
        _rule = rule;
        btnMetaTag.Click += BtnMetaTag_Click;
        LoadConfig();
    }

    private void BtnMetaTag_Click(object sender, RoutedEventArgs e) =>
        RuleConfigHelper.ShowMetaTagMenu(txtInsert, btnMetaTag);

    private void LoadConfig()
    {
        txtInsert.Text = _rule.InsertText;
        nudPosition.Value = _rule.Position;
        txtAfterText.Text = _rule.AfterText;
        txtBeforeText.Text = _rule.BeforeText;
        chkRightToLeft.IsChecked = _rule.RightToLeft;
        chkSkipExtension.IsChecked = _rule.SkipExtension;

        switch (_rule.InsertPosition)
        {
            case InsertPositionType.Prefix: rbPrefix.IsChecked = true; break;
            case InsertPositionType.Suffix: rbSuffix.IsChecked = true; break;
            case InsertPositionType.Position: rbPosition.IsChecked = true; break;
            case InsertPositionType.AfterText: rbAfterText.IsChecked = true; break;
            case InsertPositionType.BeforeText: rbBeforeText.IsChecked = true; break;
            case InsertPositionType.ReplaceCurrentName: rbReplaceName.IsChecked = true; break;
        }
    }

    public void ApplyConfig()
    {
        _rule.InsertText = txtInsert.Text;
        _rule.Position = nudPosition.Value;
        _rule.AfterText = txtAfterText.Text;
        _rule.BeforeText = txtBeforeText.Text;
        _rule.RightToLeft = chkRightToLeft.IsChecked == true;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;

        if (rbPrefix.IsChecked == true) _rule.InsertPosition = InsertPositionType.Prefix;
        else if (rbSuffix.IsChecked == true) _rule.InsertPosition = InsertPositionType.Suffix;
        else if (rbPosition.IsChecked == true) _rule.InsertPosition = InsertPositionType.Position;
        else if (rbAfterText.IsChecked == true) _rule.InsertPosition = InsertPositionType.AfterText;
        else if (rbBeforeText.IsChecked == true) _rule.InsertPosition = InsertPositionType.BeforeText;
        else if (rbReplaceName.IsChecked == true) _rule.InsertPosition = InsertPositionType.ReplaceCurrentName;
    }
}

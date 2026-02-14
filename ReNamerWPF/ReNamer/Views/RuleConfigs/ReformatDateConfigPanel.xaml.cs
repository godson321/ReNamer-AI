using System.Windows;
using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class ReformatDateConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly ReformatDateRule _rule;

    public ReformatDateConfigPanel(ReformatDateRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void BtnHelp_Click(object sender, RoutedEventArgs e) =>
        RuleConfigHelper.ShowDateFormatHelpMenu(cboSourceFormat, btnHelp);

    private void LoadConfig()
    {
        cboSourceFormat.Text = _rule.SourceFormat;
        cboTargetFormat.Text = _rule.TargetFormat;
        chkWholeWords.IsChecked = _rule.WholeWordsOnly;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
        chkCustomShortMonths.IsChecked = _rule.UseCustomShortMonths;
        cboCustomShortMonths.Text = _rule.CustomShortMonths;
        chkCustomLongMonths.IsChecked = _rule.UseCustomLongMonths;
        cboCustomLongMonths.Text = _rule.CustomLongMonths;
        chkAdjustTime.IsChecked = _rule.AdjustTime;
        nudAdjustBy.Value = _rule.AdjustTimeBy;
        // Select time part in combo
        foreach (ComboBoxItem item in cboTimePart.Items)
        {
            if (item.Content.ToString() == _rule.AdjustTimePart)
            { cboTimePart.SelectedItem = item; break; }
        }
    }

    public void ApplyConfig()
    {
        _rule.SourceFormat = cboSourceFormat.Text;
        _rule.TargetFormat = cboTargetFormat.Text;
        _rule.WholeWordsOnly = chkWholeWords.IsChecked == true;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
        _rule.UseCustomShortMonths = chkCustomShortMonths.IsChecked == true;
        _rule.CustomShortMonths = cboCustomShortMonths.Text;
        _rule.UseCustomLongMonths = chkCustomLongMonths.IsChecked == true;
        _rule.CustomLongMonths = cboCustomLongMonths.Text;
        _rule.AdjustTime = chkAdjustTime.IsChecked == true;
        _rule.AdjustTimeBy = (int)nudAdjustBy.Value;
        _rule.AdjustTimePart = (cboTimePart.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Hours";
    }
}

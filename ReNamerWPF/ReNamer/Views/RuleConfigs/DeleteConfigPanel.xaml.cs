using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class DeleteConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly DeleteRule _rule;

    public DeleteConfigPanel(DeleteRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void LoadConfig()
    {
        nudFromPosition.Value = _rule.FromPosition;
        txtFromDelimiter.Text = _rule.FromDelimiter;
        if (_rule.FromType == DeleteFromType.Position) rbFromPosition.IsChecked = true;
        else rbFromDelimiter.IsChecked = true;

        nudUntilCount.Value = _rule.UntilCount;
        txtUntilDelimiter.Text = _rule.UntilDelimiter;
        switch (_rule.UntilType)
        {
            case DeleteUntilType.Count: rbUntilCount.IsChecked = true; break;
            case DeleteUntilType.Delimiter: rbUntilDelimiter.IsChecked = true; break;
            case DeleteUntilType.TillEnd: rbUntilEnd.IsChecked = true; break;
        }

        chkDeleteCurrentName.IsChecked = _rule.DeleteCurrentName;
        chkRightToLeft.IsChecked = _rule.RightToLeft;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
        chkLeaveDelimiter.IsChecked = _rule.LeaveDelimiter;
    }

    public void ApplyConfig()
    {
        _rule.FromType = rbFromPosition.IsChecked == true ? DeleteFromType.Position : DeleteFromType.Delimiter;
        _rule.FromPosition = nudFromPosition.Value;
        _rule.FromDelimiter = txtFromDelimiter.Text;

        if (rbUntilCount.IsChecked == true) _rule.UntilType = DeleteUntilType.Count;
        else if (rbUntilDelimiter.IsChecked == true) _rule.UntilType = DeleteUntilType.Delimiter;
        else _rule.UntilType = DeleteUntilType.TillEnd;
        _rule.UntilCount = nudUntilCount.Value;
        _rule.UntilDelimiter = txtUntilDelimiter.Text;

        _rule.DeleteCurrentName = chkDeleteCurrentName.IsChecked == true;
        _rule.RightToLeft = chkRightToLeft.IsChecked == true;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
        _rule.LeaveDelimiter = chkLeaveDelimiter.IsChecked == true;
    }
}

using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class PaddingConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly PaddingRule _rule;

    public PaddingConfigPanel(PaddingRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void LoadConfig()
    {
        chkAddZeroPadding.IsChecked = _rule.AddZeroPadding;
        nudZeroPadLength.Value = _rule.ZeroPaddingLength;
        chkRemoveZeroPadding.IsChecked = _rule.RemoveZeroPadding;
        chkAddTextPadding.IsChecked = _rule.AddTextPadding;
        nudTextPadLength.Value = _rule.TextPaddingLength;
        txtPadChars.Text = _rule.PaddingCharacters;
        rbLeft.IsChecked = _rule.TextPaddingPosition == PaddingPosition.Left;
        rbRight.IsChecked = _rule.TextPaddingPosition == PaddingPosition.Right;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
    }

    public void ApplyConfig()
    {
        _rule.AddZeroPadding = chkAddZeroPadding.IsChecked == true;
        _rule.ZeroPaddingLength = (int)nudZeroPadLength.Value;
        _rule.RemoveZeroPadding = chkRemoveZeroPadding.IsChecked == true;
        _rule.AddTextPadding = chkAddTextPadding.IsChecked == true;
        _rule.TextPaddingLength = (int)nudTextPadLength.Value;
        _rule.PaddingCharacters = txtPadChars.Text;
        _rule.TextPaddingPosition = rbRight.IsChecked == true ? PaddingPosition.Right : PaddingPosition.Left;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
    }
}

using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class UserInputConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly UserInputRule _rule;

    public UserInputConfigPanel(UserInputRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void LoadConfig()
    {
        txtInput.Text = _rule.InputText;
        rbInsertBefore.IsChecked = _rule.Mode == UserInputMode.InsertBefore;
        rbInsertAfter.IsChecked = _rule.Mode == UserInputMode.InsertAfter;
        rbReplace.IsChecked = _rule.Mode == UserInputMode.Replace;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
    }

    public void ApplyConfig()
    {
        _rule.InputText = txtInput.Text;
        _rule.Mode = rbInsertBefore.IsChecked == true ? UserInputMode.InsertBefore
                   : rbInsertAfter.IsChecked == true ? UserInputMode.InsertAfter
                   : UserInputMode.Replace;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
    }
}

using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class RandomizeConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly RandomizeRule _rule;

    public RandomizeConfigPanel(RandomizeRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void LoadConfig()
    {
        nudLength.Value = _rule.Length;
        chkUnique.IsChecked = _rule.Unique;
        chkDigits.IsChecked = _rule.UseDigits;
        chkLetters.IsChecked = _rule.UseEnglishLetters;
        chkUserDefined.IsChecked = _rule.UseUserDefined;
        txtUserDefined.Text = _rule.UserDefinedChars;
        rbPrefix.IsChecked = _rule.InsertWhere == RandomizePosition.Prefix;
        rbSuffix.IsChecked = _rule.InsertWhere == RandomizePosition.Suffix;
        rbPosition.IsChecked = _rule.InsertWhere == RandomizePosition.Position;
        rbReplace.IsChecked = _rule.InsertWhere == RandomizePosition.ReplaceCurrentName;
        nudPosition.Value = _rule.PositionValue;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
    }

    public void ApplyConfig()
    {
        _rule.Length = (int)nudLength.Value;
        _rule.Unique = chkUnique.IsChecked == true;
        _rule.UseDigits = chkDigits.IsChecked == true;
        _rule.UseEnglishLetters = chkLetters.IsChecked == true;
        _rule.UseUserDefined = chkUserDefined.IsChecked == true;
        _rule.UserDefinedChars = txtUserDefined.Text;
        _rule.InsertWhere = rbSuffix.IsChecked == true ? RandomizePosition.Suffix
                          : rbPosition.IsChecked == true ? RandomizePosition.Position
                          : rbReplace.IsChecked == true ? RandomizePosition.ReplaceCurrentName
                          : RandomizePosition.Prefix;
        _rule.PositionValue = (int)nudPosition.Value;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
    }
}

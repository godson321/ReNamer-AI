using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class CaseConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly CaseRule _rule;

    public CaseConfigPanel(CaseRule rule)
    {
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void LoadConfig()
    {
        chkSkipExtension.IsChecked = _rule.SkipExtension;
        chkPreserveCase.IsChecked = _rule.PreserveCase;
        chkForceCase.IsChecked = _rule.ForceCase;
        txtForceCase.Text = _rule.ForceCaseText;
        chkExtAlwaysLower.IsChecked = _rule.ExtensionAlwaysLower;
        chkExtAlwaysUpper.IsChecked = _rule.ExtensionAlwaysUpper;

        switch (_rule.CaseType)
        {
            case CaseType.Capitalize: rbCapitalize.IsChecked = true; break;
            case CaseType.Lower: rbLower.IsChecked = true; break;
            case CaseType.Upper: rbUpper.IsChecked = true; break;
            case CaseType.Invert: rbInvert.IsChecked = true; break;
            case CaseType.FirstLetter: rbFirstLetter.IsChecked = true; break;
            case CaseType.Sentence: rbSentence.IsChecked = true; break;
            case CaseType.None: rbNone.IsChecked = true; break;
        }
    }

    public void ApplyConfig()
    {
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
        _rule.PreserveCase = chkPreserveCase.IsChecked == true;
        _rule.ForceCase = chkForceCase.IsChecked == true;
        _rule.ForceCaseText = txtForceCase.Text;
        _rule.ExtensionAlwaysLower = chkExtAlwaysLower.IsChecked == true;
        _rule.ExtensionAlwaysUpper = chkExtAlwaysUpper.IsChecked == true;

        if (rbCapitalize.IsChecked == true) _rule.CaseType = CaseType.Capitalize;
        else if (rbLower.IsChecked == true) _rule.CaseType = CaseType.Lower;
        else if (rbUpper.IsChecked == true) _rule.CaseType = CaseType.Upper;
        else if (rbInvert.IsChecked == true) _rule.CaseType = CaseType.Invert;
        else if (rbFirstLetter.IsChecked == true) _rule.CaseType = CaseType.FirstLetter;
        else if (rbSentence.IsChecked == true) _rule.CaseType = CaseType.Sentence;
        else if (rbNone.IsChecked == true) _rule.CaseType = CaseType.None;
    }
}

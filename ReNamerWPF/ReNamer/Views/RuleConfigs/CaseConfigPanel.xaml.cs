using System.Windows;
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
        UpdatePinyinOptionsVisibility();
    }

    private void LoadConfig()
    {
        chkSkipExtension.IsChecked = _rule.SkipExtension;
        chkPreserveCase.IsChecked = _rule.PreserveCase;
        chkForceCase.IsChecked = _rule.ForceCase;
        txtForceCase.Text = _rule.ForceCaseText;

        if (_rule.UseSegmentedMode)
        {
            cmbFirstMode.SelectedIndex = _rule.FirstLetterMode switch
            {
                FirstLetterMode.Keep => 0,
                FirstLetterMode.Upper => 1,
                FirstLetterMode.Lower => 2,
                FirstLetterMode.PinyinFirstLetter => 3,
                _ => 0
            };
            cmbRestMode.SelectedIndex = _rule.RemainingLettersMode switch
            {
                RemainingLettersMode.Keep => 0,
                RemainingLettersMode.Upper => 1,
                RemainingLettersMode.Lower => 2,
                RemainingLettersMode.CapitalizeWords => 3,
                RemainingLettersMode.Invert => 4,
                _ => 0
            };
            cmbExtMode.SelectedIndex = _rule.ExtensionLetterMode switch
            {
                ExtensionLetterMode.Keep => 0,
                ExtensionLetterMode.Lower => 1,
                ExtensionLetterMode.Upper => 2,
                _ => 0
            };
        }
        else
        {
            MapLegacyToSegmentedUi();
        }

        nudPinyinIndex.Value = _rule.PinyinChineseIndex;
        cmbPinyinCase.SelectedIndex = _rule.PinyinUpperCase ? 0 : 1;
        cmbPinyinAction.SelectedIndex = _rule.PinyinFirstLetterAction == PinyinFirstLetterAction.InsertPrefix ? 0 : 1;
    }

    private void MapLegacyToSegmentedUi()
    {
        switch (_rule.CaseType)
        {
            case CaseType.Capitalize: cmbFirstMode.SelectedIndex = 1; cmbRestMode.SelectedIndex = 3; break;
            case CaseType.Lower: cmbFirstMode.SelectedIndex = 2; cmbRestMode.SelectedIndex = 2; break;
            case CaseType.Upper: cmbFirstMode.SelectedIndex = 1; cmbRestMode.SelectedIndex = 1; break;
            case CaseType.Invert: cmbFirstMode.SelectedIndex = 0; cmbRestMode.SelectedIndex = 4; break;
            case CaseType.FirstLetter: cmbFirstMode.SelectedIndex = 1; cmbRestMode.SelectedIndex = 0; break;
            case CaseType.Sentence: cmbFirstMode.SelectedIndex = 1; cmbRestMode.SelectedIndex = 2; break;
            case CaseType.PinyinFirstLetter: cmbFirstMode.SelectedIndex = 3; cmbRestMode.SelectedIndex = 0; break;
            default: cmbFirstMode.SelectedIndex = 0; cmbRestMode.SelectedIndex = 0; break;
        }

        if (_rule.ExtensionAlwaysLower) cmbExtMode.SelectedIndex = 1;
        else if (_rule.ExtensionAlwaysUpper) cmbExtMode.SelectedIndex = 2;
        else cmbExtMode.SelectedIndex = 0;
    }

    private void UpdatePinyinOptionsVisibility()
    {
        if (cmbFirstMode == null || pnlPinyinOptions == null)
            return;
        pnlPinyinOptions.Visibility = cmbFirstMode.SelectedIndex == 3 ? Visibility.Visible : Visibility.Collapsed;
    }

    private FirstLetterMode GetFirstMode() => cmbFirstMode.SelectedIndex switch
    {
        1 => FirstLetterMode.Upper,
        2 => FirstLetterMode.Lower,
        3 => FirstLetterMode.PinyinFirstLetter,
        _ => FirstLetterMode.Keep
    };

    private RemainingLettersMode GetRestMode() => cmbRestMode.SelectedIndex switch
    {
        1 => RemainingLettersMode.Upper,
        2 => RemainingLettersMode.Lower,
        3 => RemainingLettersMode.CapitalizeWords,
        4 => RemainingLettersMode.Invert,
        _ => RemainingLettersMode.Keep
    };

    private ExtensionLetterMode GetExtMode() => cmbExtMode.SelectedIndex switch
    {
        1 => ExtensionLetterMode.Lower,
        2 => ExtensionLetterMode.Upper,
        _ => ExtensionLetterMode.Keep
    };

    private bool GetPinyinUpperCase() => cmbPinyinCase.SelectedIndex == 0;

    private PinyinFirstLetterAction GetPinyinAction() => cmbPinyinAction.SelectedIndex == 1
        ? PinyinFirstLetterAction.ReplaceFirstCharacter
        : PinyinFirstLetterAction.InsertPrefix;

    private void CmbFirstMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdatePinyinOptionsVisibility();
    }

    public void ApplyConfig()
    {
        _rule.UseSegmentedMode = true;

        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
        _rule.PreserveCase = chkPreserveCase.IsChecked == true;
        _rule.ForceCase = chkForceCase.IsChecked == true;
        _rule.ForceCaseText = txtForceCase.Text ?? "";

        _rule.FirstLetterMode = GetFirstMode();
        _rule.RemainingLettersMode = GetRestMode();
        _rule.ExtensionLetterMode = GetExtMode();

        _rule.PinyinChineseIndex = nudPinyinIndex.Value;
        _rule.PinyinUpperCase = GetPinyinUpperCase();
        _rule.PinyinFirstLetterAction = GetPinyinAction();

        // 同步旧字段，保障旧逻辑兼容
        _rule.CaseType = CaseType.None;
        _rule.ExtensionAlwaysLower = _rule.ExtensionLetterMode == ExtensionLetterMode.Lower;
        _rule.ExtensionAlwaysUpper = _rule.ExtensionLetterMode == ExtensionLetterMode.Upper;
        _rule.PinyinInsertPosition = PinyinInsertPosition.Prefix;
    }
}

using System.Windows;
using System.Windows.Controls;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class DeleteConfigPanel : UserControl, IRuleConfigPanel
{
    private const string EnglishPresetChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DigitsPresetChars = "0123456789";
    private const string SymbolsPresetChars = "!\"#$%&'*+,-./:;=?@\\^_`|~";
    private const string BracketsPresetChars = "(){}[]<>";
    private readonly DeleteRule _rule;
    private bool _syncingSkipExtension;
    private bool _syncingCaseSensitive;
    private bool _isLoading;

    public DeleteConfigPanel(DeleteRule rule)
    {
        InitializeComponent();
        _rule = rule;
        txtRemovePattern.TextChanged += (_, _) => UpdateMultiHint();
        LoadConfig();
    }

    private void BtnSeparator_Click(object sender, RoutedEventArgs e) =>
        RuleConfigHelper.InsertSeparator(txtRemovePattern);

    private void BtnTextPresetPicker_Click(object sender, RoutedEventArgs e)
    {
        if (popupTextPresets == null)
            return;
        popupTextPresets.IsOpen = !popupTextPresets.IsOpen;
    }

    private void PopupTextPresets_Closed(object sender, System.EventArgs e)
    {
        // Reserved for future popup close behaviors.
    }

    private void LoadConfig()
    {
        _isLoading = true;
        try
        {
            // 位置删除 Tab
            rbFromPosition.IsChecked = _rule.FromType != DeleteFromType.Delimiter;
            rbFromDelimiter.IsChecked = _rule.FromType == DeleteFromType.Delimiter;
            nudFromPosition.Value = _rule.FromPosition;
            txtFromDelimiter.Text = _rule.FromDelimiter;

            rbUntilCount.IsChecked = _rule.UntilType == DeleteUntilType.Count;
            rbUntilDelimiter.IsChecked = _rule.UntilType == DeleteUntilType.Delimiter;
            rbUntilEnd.IsChecked = _rule.UntilType == DeleteUntilType.TillEnd;
            nudUntilCount.Value = _rule.UntilCount;
            txtUntilDelimiter.Text = _rule.UntilDelimiter;

            chkRightToLeft.IsChecked = _rule.RightToLeft;
            chkLeaveDelimiter.IsChecked = _rule.LeaveDelimiter;
            chkSkipExtensionPosition.IsChecked = _rule.SkipExtension;

            // 文本模式 Tab
            txtRemovePattern.Text = _rule.RemovePattern;
            var caseSensitive = _rule.RemoveCaseSensitive || _rule.CaseSensitive;
            chkRemoveCaseSensitive.IsChecked = caseSensitive;
            chkCaseSensitiveCharSet.IsChecked = caseSensitive;
            rbMatchPlain.IsChecked = !_rule.RemoveWholeWordsOnly && !_rule.RemoveUseWildcards;
            rbMatchWholeWords.IsChecked = _rule.RemoveWholeWordsOnly && !_rule.RemoveUseWildcards;
            rbMatchWildcards.IsChecked = _rule.RemoveUseWildcards;
            rbOccurrenceAll.IsChecked = _rule.RemoveOccurrence == RemoveOccurrence.All;
            rbOccurrenceFirst.IsChecked = _rule.RemoveOccurrence == RemoveOccurrence.First;
            rbOccurrenceLast.IsChecked = _rule.RemoveOccurrence == RemoveOccurrence.Last;
            chkSkipExtensionText.IsChecked = _rule.SkipExtension;

            // Popup中的预设选项
            chkEnglishQuick.IsChecked = _rule.StripEnglishLetters;
            chkDigitsQuick.IsChecked = _rule.StripDigits;
            chkSymbolsQuick.IsChecked = _rule.StripSymbols;
            chkBracketsQuick.IsChecked = _rule.StripBrackets;
            chkUserDefinedQuick.IsChecked = _rule.StripUserDefined;

            // 按字符删除模式
            chkCharSetMode.IsChecked = _rule.StripUserDefined && !string.IsNullOrEmpty(_rule.UserDefinedChars);
            if (chkCharSetMode.IsChecked == true && string.IsNullOrEmpty(txtRemovePattern.Text))
                txtRemovePattern.Text = _rule.UserDefinedChars;

            // 字符集 Tab
            txtUnicodeRange.Text = _rule.StripUnicodeRange ? _rule.UnicodeRange : string.Empty;
            rbWhereEverywhere.IsChecked = _rule.Where == StripWhere.Everywhere;
            rbWhereLeading.IsChecked = _rule.Where == StripWhere.Leading;
            rbWhereTrailing.IsChecked = _rule.Where == StripWhere.Trailing;
            chkAllExcept.IsChecked = _rule.StripAllExceptSelected;
            chkSkipExtensionCharSet.IsChecked = _rule.SkipExtension;

            // 根据规则模式选择Tab
            tabDeleteMode.SelectedIndex = DetermineInitialTab();

            UpdatePositionInputState();
            UpdateMultiHint();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private int DetermineInitialTab()
    {
        if (IsLikelyFreshRule())
            return 0; // 默认位置删除

        if (_rule.Mode == DeleteMode.PositionDelete)
            return 0;

        if (_rule.Mode == DeleteMode.CharacterRemove || HasConfiguredCharSetFromRule())
            return 2;

        if (_rule.Mode == DeleteMode.TextRemove || !string.IsNullOrEmpty(_rule.RemovePattern))
            return 1;

        return 0;
    }

    public void ApplyConfig()
    {
        var selectedTab = tabDeleteMode.SelectedIndex;

        // 根据当前Tab决定模式
        switch (selectedTab)
        {
            case 0: // 位置删除
                _rule.Mode = DeleteMode.PositionDelete;
                ApplyPositionConfig();
                ClearTextConfig();
                ClearCharSetConfig();
                break;
            case 1: // 文本模式
                _rule.Mode = DeleteMode.TextRemove;
                ApplyTextConfig();
                ClearPositionConfig();
                ClearCharSetConfig();
                break;
            case 2: // 字符集删除
                _rule.Mode = DeleteMode.CharacterRemove;
                ApplyCharSetConfig();
                ClearPositionConfig();
                ClearTextConfig();
                break;
        }
    }

    private void ApplyPositionConfig()
    {
        _rule.FromType = rbFromDelimiter.IsChecked == true ? DeleteFromType.Delimiter : DeleteFromType.Position;
        _rule.FromPosition = nudFromPosition.Value;
        _rule.FromDelimiter = txtFromDelimiter.Text;
        _rule.UntilType = rbUntilDelimiter.IsChecked == true ? DeleteUntilType.Delimiter
            : rbUntilEnd.IsChecked == true ? DeleteUntilType.TillEnd
            : DeleteUntilType.Count;
        _rule.UntilCount = nudUntilCount.Value;
        _rule.UntilDelimiter = txtUntilDelimiter.Text;
        _rule.RightToLeft = chkRightToLeft.IsChecked == true;
        _rule.LeaveDelimiter = chkLeaveDelimiter.IsChecked == true;
        _rule.SkipExtension = chkSkipExtensionPosition.IsChecked == true;
        _rule.DeleteCurrentName = false;
        _rule.TextFirstInCombined = false;
    }

    private void ApplyTextConfig()
    {
        var isCharSetMode = chkCharSetMode.IsChecked == true;
        var text = txtRemovePattern.Text;

        if (isCharSetMode)
        {
            // 按字符删除模式
            _rule.RemovePattern = string.Empty;
            _rule.StripUserDefined = !string.IsNullOrEmpty(text);
            _rule.UserDefinedChars = text;
            _rule.RemoveUseWildcards = false;
            _rule.RemoveWholeWordsOnly = false;
        }
        else
        {
            // 文本匹配模式
            _rule.RemovePattern = text;
            _rule.StripUserDefined = false;
            _rule.UserDefinedChars = string.Empty;
            _rule.RemoveUseWildcards = rbMatchWildcards.IsChecked == true;
            _rule.RemoveWholeWordsOnly = rbMatchWholeWords.IsChecked == true;
        }

        _rule.RemoveCaseSensitive = chkRemoveCaseSensitive.IsChecked == true;
        _rule.CaseSensitive = _rule.RemoveCaseSensitive;
        _rule.RemoveOccurrence = rbOccurrenceFirst.IsChecked == true ? RemoveOccurrence.First
            : rbOccurrenceLast.IsChecked == true ? RemoveOccurrence.Last
            : RemoveOccurrence.All;
        _rule.SkipExtension = chkSkipExtensionText.IsChecked == true;
    }

    private void ApplyCharSetConfig()
    {
        _rule.StripEnglishLetters = false;
        _rule.StripDigits = false;
        _rule.StripSymbols = false;
        _rule.StripBrackets = false;
        _rule.StripUserDefined = false;
        _rule.UserDefinedChars = string.Empty;
        _rule.StripUnicodeRange = !string.IsNullOrWhiteSpace(txtUnicodeRange.Text);
        _rule.UnicodeRange = txtUnicodeRange.Text;
        _rule.Where = rbWhereLeading.IsChecked == true ? StripWhere.Leading
            : rbWhereTrailing.IsChecked == true ? StripWhere.Trailing
            : StripWhere.Everywhere;
        _rule.StripAllExceptSelected = chkAllExcept.IsChecked == true;
        _rule.CaseSensitive = chkCaseSensitiveCharSet.IsChecked == true;
        _rule.RemoveCaseSensitive = _rule.CaseSensitive;
        _rule.SkipExtension = chkSkipExtensionCharSet.IsChecked == true;
    }

    private void ClearPositionConfig()
    {
        _rule.FromType = DeleteFromType.Position;
        _rule.FromPosition = 1;
        _rule.FromDelimiter = string.Empty;
        _rule.UntilType = DeleteUntilType.Count;
        _rule.UntilCount = 1;
        _rule.UntilDelimiter = string.Empty;
        _rule.RightToLeft = false;
        _rule.LeaveDelimiter = false;
        _rule.DeleteCurrentName = false;
        _rule.TextFirstInCombined = false;
    }

    private void ClearTextConfig()
    {
        _rule.RemovePattern = string.Empty;
        _rule.RemoveCaseSensitive = false;
        _rule.RemoveUseWildcards = false;
        _rule.RemoveWholeWordsOnly = false;
        _rule.RemoveOccurrence = RemoveOccurrence.All;
    }

    private void ClearCharSetConfig()
    {
        _rule.StripEnglishLetters = false;
        _rule.StripDigits = false;
        _rule.StripSymbols = false;
        _rule.StripBrackets = false;
        _rule.StripUserDefined = false;
        _rule.UserDefinedChars = string.Empty;
        _rule.StripUnicodeRange = false;
        _rule.UnicodeRange = string.Empty;
        _rule.Where = StripWhere.Everywhere;
        _rule.StripAllExceptSelected = false;
        _rule.CaseSensitive = false;
        _rule.RemoveCaseSensitive = false;
    }

    private void TabDeleteMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading || e.Source != tabDeleteMode)
            return;
        // Tab切换时可以在这里处理特殊逻辑
    }

    private void FromType_Changed(object sender, RoutedEventArgs e)
    {
        UpdatePositionInputState();
    }

    private void UntilType_Changed(object sender, RoutedEventArgs e)
    {
        UpdatePositionInputState();
    }

    private void UpdatePositionInputState()
    {
        if (rbFromPosition == null || rbFromDelimiter == null || nudFromPosition == null || txtFromDelimiter == null ||
            rbUntilCount == null || rbUntilDelimiter == null || rbUntilEnd == null ||
            nudUntilCount == null || txtUntilDelimiter == null || txtUntilEndHint == null)
            return;

        var fromByDelimiter = rbFromDelimiter.IsChecked == true;
        nudFromPosition.Visibility = fromByDelimiter ? Visibility.Collapsed : Visibility.Visible;
        txtFromDelimiter.Visibility = fromByDelimiter ? Visibility.Visible : Visibility.Collapsed;

        nudUntilCount.Visibility = rbUntilCount.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        txtUntilDelimiter.Visibility = rbUntilDelimiter.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        txtUntilEndHint.Visibility = rbUntilEnd.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SkipExtension_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if (_syncingSkipExtension || chkSkipExtensionPosition == null ||
            chkSkipExtensionText == null || chkSkipExtensionCharSet == null)
            return;

        _syncingSkipExtension = true;
        try
        {
            var source = sender as CheckBox;
            var isChecked = source?.IsChecked == true;

            chkSkipExtensionPosition.IsChecked = isChecked;
            chkSkipExtensionText.IsChecked = isChecked;
            chkSkipExtensionCharSet.IsChecked = isChecked;
        }
        finally
        {
            _syncingSkipExtension = false;
        }
    }

    private void CaseSensitive_CheckedChanged(object sender, RoutedEventArgs e)
    {
        if (_syncingCaseSensitive || chkRemoveCaseSensitive == null || chkCaseSensitiveCharSet == null)
            return;

        _syncingCaseSensitive = true;
        try
        {
            var source = sender as CheckBox;
            var isChecked = source?.IsChecked == true;
            chkRemoveCaseSensitive.IsChecked = isChecked;
            chkCaseSensitiveCharSet.IsChecked = isChecked;
        }
        finally
        {
            _syncingCaseSensitive = false;
        }
    }

    private void TextPresetOption_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
            return;

        // 预设选项变化时更新文本框显示
        if (HasNonCustomPresetSelection())
            txtRemovePattern.Text = BuildPresetDisplayText();
    }

    private void UpdateMultiHint()
    {
        txtMultiRemoveHint.Visibility = RuleHelpers.ContainsUnescapedSeparator(txtRemovePattern.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private bool HasNonCustomPresetSelection()
    {
        return chkEnglishQuick.IsChecked == true ||
               chkDigitsQuick.IsChecked == true ||
               chkSymbolsQuick.IsChecked == true ||
               chkBracketsQuick.IsChecked == true;
    }

    private bool HasConfiguredCharSetFromRule()
    {
        return _rule.StripUnicodeRange ||
               _rule.StripAllExceptSelected ||
               _rule.Where != StripWhere.Everywhere ||
               _rule.CaseSensitive;
    }

    private string BuildPresetDisplayText()
    {
        var text = string.Empty;
        if (chkEnglishQuick.IsChecked == true) text += EnglishPresetChars;
        if (chkDigitsQuick.IsChecked == true) text += DigitsPresetChars;
        if (chkSymbolsQuick.IsChecked == true) text += SymbolsPresetChars;
        if (chkBracketsQuick.IsChecked == true) text += BracketsPresetChars;
        return text;
    }

    private bool IsLikelyFreshRule()
    {
        return _rule.Mode == DeleteMode.PositionDelete &&
               _rule.FromType == DeleteFromType.Position &&
               _rule.FromPosition == 1 &&
               string.IsNullOrEmpty(_rule.FromDelimiter) &&
               _rule.UntilType == DeleteUntilType.Count &&
               _rule.UntilCount == 1 &&
               string.IsNullOrEmpty(_rule.UntilDelimiter) &&
               !_rule.DeleteCurrentName &&
               !_rule.RightToLeft &&
               _rule.SkipExtension &&
               !_rule.LeaveDelimiter &&
               string.IsNullOrEmpty(_rule.RemovePattern) &&
               _rule.RemoveOccurrence == RemoveOccurrence.All &&
               !_rule.RemoveCaseSensitive &&
               !_rule.RemoveWholeWordsOnly &&
               !_rule.RemoveUseWildcards &&
               !_rule.StripEnglishLetters &&
               !_rule.StripDigits &&
               !_rule.StripSymbols &&
               !_rule.StripBrackets &&
               !_rule.StripUserDefined &&
               string.IsNullOrEmpty(_rule.UserDefinedChars) &&
               !_rule.StripUnicodeRange &&
               _rule.Where == StripWhere.Everywhere &&
               !_rule.StripAllExceptSelected &&
               !_rule.CaseSensitive;
    }
}

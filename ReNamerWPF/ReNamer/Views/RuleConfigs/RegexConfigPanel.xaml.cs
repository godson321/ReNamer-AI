using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using ReNamer.Rules;

namespace ReNamer.Views.RuleConfigs;

public partial class RegexConfigPanel : UserControl, IRuleConfigPanel
{
    private readonly RegexRule _rule;
    private bool _isLoading;
    private Match? _lastMatch;

    public RegexConfigPanel(RegexRule rule)
    {
        _isLoading = true;
        InitializeComponent();
        _rule = rule;
        LoadConfig();
    }

    private void BtnInsertExpr_Click(object sender, RoutedEventArgs e) =>
        RuleConfigHelper.ShowExpressionMenu(txtExpression, btnInsertExpr);

    private void BtnHelp_Click(object sender, RoutedEventArgs e) =>
        RuleConfigHelper.OpenUrl("https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference");

    private void LoadConfig()
    {
        _isLoading = true;
        txtExpression.Text = _rule.Expression;
        txtReplace.Text = _rule.ReplaceText;
        chkCaseSensitive.IsChecked = _rule.CaseSensitive;
        chkSkipExtension.IsChecked = _rule.SkipExtension;
        chkReplaceFirstOnly.IsChecked = _rule.ReplaceFirstOnly;
        cmbRegexMode.SelectedIndex = GetRegexModeIndex(_rule.Multiline, _rule.Singleline);
        chkIgnorePatternWhitespace.IsChecked = _rule.IgnorePatternWhitespace;
        chkCultureInvariant.IsChecked = _rule.CultureInvariant;
        _isLoading = false;
        UpdatePreview();
    }

    public void ApplyConfig()
    {
        _rule.Expression = txtExpression.Text;
        _rule.ReplaceText = txtReplace.Text;
        _rule.CaseSensitive = chkCaseSensitive.IsChecked == true;
        _rule.SkipExtension = chkSkipExtension.IsChecked == true;
        _rule.ReplaceFirstOnly = chkReplaceFirstOnly.IsChecked == true;
        var mode = cmbRegexMode.SelectedIndex;
        _rule.Multiline = mode == 1 || mode == 3;
        _rule.Singleline = mode == 2 || mode == 3;
        _rule.IgnorePatternWhitespace = chkIgnorePatternWhitespace.IsChecked == true;
        _rule.CultureInvariant = chkCultureInvariant.IsChecked == true;
    }

    private void PreviewTextChanged(object sender, TextChangedEventArgs e) => UpdatePreview();

    private void PreviewOptionChanged(object sender, RoutedEventArgs e) => UpdatePreview();

    private void PreviewModeChanged(object sender, SelectionChangedEventArgs e) => UpdatePreview();

    private void CmbGroupSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateSelectedGroupValue();

    private void UpdatePreview()
    {
        if (_isLoading)
        {
            return;
        }

        if (txtExpression is null || txtReplace is null || txtSampleInput is null ||
            txtPreviewResult is null || txtGroupValue is null || cmbGroupSelector is null ||
            chkSkipExtension is null || chkReplaceFirstOnly is null ||
            chkCaseSensitive is null || chkIgnorePatternWhitespace is null || chkCultureInvariant is null ||
            cmbRegexMode is null)
        {
            return;
        }

        var expression = txtExpression.Text ?? string.Empty;
        var replaceText = txtReplace.Text ?? string.Empty;
        var sampleInput = txtSampleInput.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(sampleInput))
        {
            txtPreviewResult.Text = string.Empty;
            SetNoMatchGroupState();
            return;
        }

        if (string.IsNullOrWhiteSpace(expression))
        {
            txtPreviewResult.Text = sampleInput;
            SetNoMatchGroupState();
            return;
        }

        try
        {
            var options = BuildRegexOptions();
            var regex = new Regex(expression, options);
            var (baseText, extText) = SplitForPreview(sampleInput, chkSkipExtension.IsChecked == true);
            _lastMatch = regex.Match(baseText);

            var result = chkReplaceFirstOnly.IsChecked == true
                ? regex.Replace(baseText, replaceText, 1)
                : regex.Replace(baseText, replaceText);

            txtPreviewResult.Text = result + extText;
            UpdateGroupSelectorItems();
            UpdateSelectedGroupValue();
        }
        catch
        {
            txtPreviewResult.Text = GetString("Regex_InvalidPattern", "表达式无效");
            SetNoMatchGroupState();
        }
    }

    private RegexOptions BuildRegexOptions()
    {
        var options = RegexOptions.None;
        if (chkCaseSensitive.IsChecked != true) options |= RegexOptions.IgnoreCase;

        var mode = cmbRegexMode.SelectedIndex;
        if (mode == 1 || mode == 3) options |= RegexOptions.Multiline;
        if (mode == 2 || mode == 3) options |= RegexOptions.Singleline;

        if (chkIgnorePatternWhitespace.IsChecked == true) options |= RegexOptions.IgnorePatternWhitespace;
        if (chkCultureInvariant.IsChecked == true) options |= RegexOptions.CultureInvariant;
        return options;
    }

    private void UpdateGroupSelectorItems()
    {
        var previousIndex = (cmbGroupSelector.SelectedItem as GroupItem)?.Index ?? 0;
        cmbGroupSelector.Items.Clear();

        if (_lastMatch is null || !_lastMatch.Success)
        {
            SetNoMatchGroupState();
            return;
        }

        cmbGroupSelector.IsEnabled = true;
        cmbGroupSelector.Items.Add(new GroupItem(0, GetString("Regex_Group_All", "$0 全部匹配")));

        for (var i = 1; i < _lastMatch.Groups.Count; i++)
        {
            var groupName = _lastMatch.Groups[i].Name;
            var display = string.IsNullOrWhiteSpace(groupName) || groupName == i.ToString()
                ? $"${i}"
                : $"{groupName} (${i})";
            cmbGroupSelector.Items.Add(new GroupItem(i, display));
        }

        var target = FindGroupItemByIndex(previousIndex) ?? FindGroupItemByIndex(0);
        cmbGroupSelector.SelectedItem = target;
    }

    private GroupItem? FindGroupItemByIndex(int index)
    {
        foreach (var item in cmbGroupSelector.Items)
        {
            if (item is GroupItem groupItem && groupItem.Index == index)
            {
                return groupItem;
            }
        }
        return null;
    }

    private void SetNoMatchGroupState()
    {
        _lastMatch = null;
        cmbGroupSelector.IsEnabled = false;
        cmbGroupSelector.Items.Clear();
        cmbGroupSelector.Items.Add(new GroupItem(-1, GetString("Regex_Group_None", "无匹配分组")));
        cmbGroupSelector.SelectedIndex = 0;
        txtGroupValue.Text = string.Empty;
    }

    private void UpdateSelectedGroupValue()
    {
        if (_lastMatch is null || !_lastMatch.Success)
        {
            txtGroupValue.Text = string.Empty;
            return;
        }

        if (cmbGroupSelector.SelectedItem is GroupItem item &&
            item.Index >= 0 &&
            item.Index < _lastMatch.Groups.Count)
        {
            txtGroupValue.Text = _lastMatch.Groups[item.Index].Value;
            return;
        }

        txtGroupValue.Text = string.Empty;
    }

    private static (string BaseText, string ExtText) SplitForPreview(string input, bool skipExtension)
    {
        if (!skipExtension || string.IsNullOrEmpty(input))
        {
            return (input, string.Empty);
        }

        var dotIndex = input.LastIndexOf('.');
        if (dotIndex <= 0 || dotIndex >= input.Length - 1)
        {
            return (input, string.Empty);
        }

        return (input[..dotIndex], input[dotIndex..]);
    }

    private static string GetString(string key, string fallback)
    {
        var value = Application.Current.TryFindResource(key) as string;
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static int GetRegexModeIndex(bool multiline, bool singleline)
    {
        if (multiline && singleline) return 3;
        if (multiline) return 1;
        if (singleline) return 2;
        return 0;
    }

    private sealed class GroupItem(int index, string text)
    {
        public int Index { get; } = index;
        public string Text { get; } = text;
        public override string ToString() => Text;
    }
}

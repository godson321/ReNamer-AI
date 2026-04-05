using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReNamer.Rules;
using ReNamer.Services;
using ReNamer.Views.RuleConfigs;

namespace ReNamer.Views;

public partial class AddRuleDialog : Window
{
    public IRule? SelectedRule { get; private set; }

    // 当前配置面板
    private IRuleConfigPanel? _currentPanel;
    // 当前预创建的规则实例
    private IRule? _currentRule;
    // 是否为编辑模式
    private readonly bool _isEditMode;

    // 规则类型列表
    private static readonly List<string> RuleTypes = new()
    {
        "Replace", "Insert", "Delete", "Case", "Serialize",
        "Padding", "CleanUp", "Transliterate", "ChineseConvert", "ChineseNumber", "RegEx",
        "Rearrange", "ReformatDate", "Randomize", "JavaScript", "UserInput", "Mapping"
    };

    // 规则类型图标映射
    private static readonly Dictionary<string, Geometry> RuleIcons = new()
    {
        ["Replace"] = (Geometry)Application.Current.FindResource("Icon_Replace"),
        ["Insert"] = (Geometry)Application.Current.FindResource("Icon_Insert"),
        ["Delete"] = (Geometry)Application.Current.FindResource("Icon_Delete"),
        ["Remove"] = (Geometry)Application.Current.FindResource("Icon_RemoveItem"),
        ["Case"] = (Geometry)Application.Current.FindResource("Icon_Case"),
        ["Serialize"] = (Geometry)Application.Current.FindResource("Icon_Serialize"),
        ["Padding"] = (Geometry)Application.Current.FindResource("Icon_Padding"),
        ["CleanUp"] = (Geometry)Application.Current.FindResource("Icon_CleanUp"),
        ["Transliterate"] = (Geometry)Application.Current.FindResource("Icon_Transliterate"),
        ["ChineseConvert"] = (Geometry)Application.Current.FindResource("Icon_Transliterate"),
        ["ChineseNumber"] = (Geometry)Application.Current.FindResource("Icon_Transliterate"),
        ["RegEx"] = (Geometry)Application.Current.FindResource("Icon_RegEx"),
        ["Rearrange"] = (Geometry)Application.Current.FindResource("Icon_Rearrange"),
        ["ReformatDate"] = (Geometry)Application.Current.FindResource("Icon_ReformatDate"),
        ["Randomize"] = (Geometry)Application.Current.FindResource("Icon_Randomize"),
        ["JavaScript"] = (Geometry)Application.Current.FindResource("Icon_JavaScript"),
        ["UserInput"] = (Geometry)Application.Current.FindResource("Icon_UserInput"),
        ["Mapping"] = (Geometry)Application.Current.FindResource("Icon_Mapping")
    };

    /// <summary>
    /// 新建规则模式
    /// </summary>
    public AddRuleDialog()
    {
        InitializeComponent();
        _isEditMode = false;
        LoadRules();
        LanguageService.LanguageChanged += LoadRules;

        // 默认选中第一个
        if (lbRules.Items.Count > 0)
            lbRules.SelectedIndex = 0;
    }

    public void ConfirmCurrentRule()
    {
        if (_currentRule == null) return;

        // 应用配置
        _currentPanel?.ApplyConfig();
        SelectedRule = _currentRule;
        DialogResult = true;
        Close();
    }

    /// <summary>
    /// 编辑规则模式：左侧高亮对应规则，右侧显示配置
    /// </summary>
    public AddRuleDialog(IRule ruleToEdit) : this()
    {
        _isEditMode = true;
        var editableRule = ruleToEdit is RemoveRule legacyRemove
            ? ConvertLegacyRemoveToDelete(legacyRemove)
            : ruleToEdit;
        _currentRule = editableRule;
        Title = LanguageService.GetString("Dialog_EditRule");
        btnAddRule.Content = LanguageService.GetString("Dialog_OK");

        // 定位到对应规则类型
        var typeKey = GetRuleTypeKey(editableRule);
        for (int i = 0; i < lbRules.Items.Count; i++)
        {
            if (lbRules.Items[i] is RuleInfo info && info.TypeKey == typeKey)
            {
                lbRules.SelectedIndex = i;
                break;
            }
        }

        // 编辑模式禁用规则切换
        lbRules.IsEnabled = false;

        // 加载当前规则的配置到面板
        LoadConfigPanel(editableRule);
    }

    private void LoadRules()
    {
        var selectedIndex = lbRules.SelectedIndex;
        var rules = new List<RuleInfo>();
        foreach (var ruleType in RuleTypes)
        {
            var name = LanguageService.GetString($"Rule_{ruleType}");
            var icon = RuleIcons.TryGetValue(ruleType, out var g) ? g : null;
            rules.Add(new RuleInfo(ruleType, name, icon));
        }
        lbRules.ItemsSource = rules;
        if (selectedIndex >= 0 && selectedIndex < rules.Count)
            lbRules.SelectedIndex = selectedIndex;
    }

    /// <summary>
    /// 左侧规则列表选中变化→右侧动态加载配置面板
    /// </summary>
    private void Rules_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isEditMode) return; // 编辑模式不响应切换

        if (lbRules.SelectedItem is not RuleInfo info) return;

        // 创建新的规则实例
        _currentRule = CreateRuleInstance(info.TypeKey);
        if (_currentRule != null)
        {
            LoadConfigPanel(_currentRule);
        }
        else
        {
            configPanel.Content = new TextBlock
            {
                Text = LanguageService.GetString("Msg_ComingSoon"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Gray
            };
            _currentPanel = null;
        }
    }

    private void LoadConfigPanel(IRule rule)
    {
        _currentPanel = CreateConfigPanel(rule);
        configPanel.Content = _currentPanel as UserControl;
    }

    private void AddRule_Click(object sender, RoutedEventArgs e)
    {
        ConfirmCurrentRule();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ListBox_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        AddRule_Click(sender, e);
    }

    private void RuleDescription_Click(object sender, RoutedEventArgs e)
    {
        if (lbRules.SelectedItem is not RuleInfo info) return;
        var rule = CreateRuleInstance(info.TypeKey);
        if (rule == null) return;
        MessageBox.Show(rule.Description, info.Name, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void AddRuleDialog_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void AddRuleDialog_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var paths = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            if (System.OperatingSystem.IsWindows() && Application.Current.MainWindow is MainWindow mw)
                mw.AddFilePaths(paths, recursive: true);
        }
    }

    private static IRule? CreateRuleInstance(string typeKey)
    {
        return typeKey switch
        {
            "Replace" => new ReplaceRule(),
            "Insert" => new InsertRule(),
            "Delete" => new DeleteRule(),
            "Case" => new CaseRule(),
            "Serialize" => new SerializeRule(),
            "RegEx" => new RegexRule(),
            "Padding" => new PaddingRule(),
            "CleanUp" => new CleanUpRule(),
            "Transliterate" => new TransliterateRule(),
            "ChineseConvert" => new ChineseConvertRule(),
            "ChineseNumber" => new ChineseNumberRule(),
            "Rearrange" => new RearrangeRule(),
            "ReformatDate" => new ReformatDateRule(),
            "Randomize" => new RandomizeRule(),
            "JavaScript" => new JavaScriptRule(),
            "UserInput" => new UserInputRule(),
            "Mapping" => new MappingRule(),
            _ => null
        };
    }

    private static IRuleConfigPanel? CreateConfigPanel(IRule rule)
    {
        return rule switch
        {
            ReplaceRule r => new ReplaceConfigPanel(r),
            InsertRule r => new InsertConfigPanel(r),
            DeleteRule r => new DeleteConfigPanel(r),
            CaseRule r => new CaseConfigPanel(r),
            SerializeRule r => new SerializeConfigPanel(r),
            RegexRule r => new RegexConfigPanel(r),
            PaddingRule r => new PaddingConfigPanel(r),
            CleanUpRule r => new CleanUpConfigPanel(r),
            TransliterateRule r => new TransliterateConfigPanel(r),
            ChineseConvertRule r => new ChineseConvertConfigPanel(r),
            ChineseNumberRule r => new ChineseNumberConfigPanel(r),
            RearrangeRule r => new RearrangeConfigPanel(r),
            ReformatDateRule r => new ReformatDateConfigPanel(r),
            RandomizeRule r => new RandomizeConfigPanel(r),
            JavaScriptRule r => new JavaScriptConfigPanel(r),
            UserInputRule r => new UserInputConfigPanel(r),
            MappingRule r => new MappingConfigPanel(r),
            _ => null
        };
    }

    private static string GetRuleTypeKey(IRule rule)
    {
        return rule switch
        {
            ReplaceRule => "Replace",
            InsertRule => "Insert",
            DeleteRule => "Delete",
            RemoveRule => "Delete",
            CaseRule => "Case",
            SerializeRule => "Serialize",
            RegexRule => "RegEx",
            PaddingRule => "Padding",
            CleanUpRule => "CleanUp",
            TransliterateRule => "Transliterate",
            ChineseConvertRule => "ChineseConvert",
            ChineseNumberRule => "ChineseNumber",
            RearrangeRule => "Rearrange",
            ReformatDateRule => "ReformatDate",
            RandomizeRule => "Randomize",
            JavaScriptRule => "JavaScript",
            UserInputRule => "UserInput",
            MappingRule => "Mapping",
            _ => ""
        };
    }

    private static DeleteRule ConvertLegacyRemoveToDelete(RemoveRule source)
    {
        return new DeleteRule
        {
            Mode = DeleteMode.TextRemove,
            RemovePattern = source.Pattern,
            RemoveOccurrence = source.Occurrence,
            RemoveCaseSensitive = source.CaseSensitive,
            RemoveWholeWordsOnly = source.WholeWordsOnly,
            RemoveUseWildcards = source.UseWildcards,
            SkipExtension = source.SkipExtension,
            IsEnabled = source.IsEnabled
        };
    }

    private record RuleInfo(string TypeKey, string Name, Geometry? Icon);
}

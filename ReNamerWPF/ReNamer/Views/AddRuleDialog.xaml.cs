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
        "Replace", "Insert", "Delete", "Remove", "Case", "Serialize", "Extension",
        "Padding", "Strip", "CleanUp", "Transliterate", "RegEx", 
        "Rearrange", "ReformatDate", "Randomize", "PascalScript", "UserInput", "Mapping"
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
        ["Extension"] = (Geometry)Application.Current.FindResource("Icon_Extension"),
        ["Padding"] = (Geometry)Application.Current.FindResource("Icon_Padding"),
        ["Strip"] = (Geometry)Application.Current.FindResource("Icon_Strip"),
        ["CleanUp"] = (Geometry)Application.Current.FindResource("Icon_CleanUp"),
        ["Transliterate"] = (Geometry)Application.Current.FindResource("Icon_Transliterate"),
        ["RegEx"] = (Geometry)Application.Current.FindResource("Icon_RegEx"),
        ["Rearrange"] = (Geometry)Application.Current.FindResource("Icon_Rearrange"),
        ["ReformatDate"] = (Geometry)Application.Current.FindResource("Icon_ReformatDate"),
        ["Randomize"] = (Geometry)Application.Current.FindResource("Icon_Randomize"),
        ["PascalScript"] = (Geometry)Application.Current.FindResource("Icon_PascalScript"),
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
    
    /// <summary>
    /// 编辑规则模式：左侧高亮对应规则，右侧显示配置
    /// </summary>
    public AddRuleDialog(IRule ruleToEdit) : this()
    {
        _isEditMode = true;
        _currentRule = ruleToEdit;
        Title = LanguageService.GetString("Dialog_EditRule");
        btnAddRule.Content = LanguageService.GetString("Dialog_OK");
        
        // 定位到对应规则类型
        var typeKey = GetRuleTypeKey(ruleToEdit);
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
        LoadConfigPanel(ruleToEdit);
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
                Foreground = System.Windows.Media.Brushes.Gray,
                FontSize = 14
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
        if (_currentRule == null) return;
        
        // 应用配置
        _currentPanel?.ApplyConfig();
        SelectedRule = _currentRule;
        DialogResult = true;
        Close();
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
            if (Application.Current.MainWindow is MainWindow mw)
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
            "Remove" => new RemoveRule(),
            "Case" => new CaseRule(),
            "Serialize" => new SerializeRule(),
            "Extension" => new ExtensionRule(),
            "RegEx" => new RegexRule(),
            "Padding" => new PaddingRule(),
            "Strip" => new StripRule(),
            "CleanUp" => new CleanUpRule(),
            "Transliterate" => new TransliterateRule(),
            "Rearrange" => new RearrangeRule(),
            "ReformatDate" => new ReformatDateRule(),
            "Randomize" => new RandomizeRule(),
            "PascalScript" => new PascalScriptRule(),
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
            RemoveRule r => new RemoveConfigPanel(r),
            CaseRule r => new CaseConfigPanel(r),
            SerializeRule r => new SerializeConfigPanel(r),
            ExtensionRule r => new ExtensionConfigPanel(r),
            RegexRule r => new RegexConfigPanel(r),
            PaddingRule r => new PaddingConfigPanel(r),
            StripRule r => new StripConfigPanel(r),
            CleanUpRule r => new CleanUpConfigPanel(r),
            TransliterateRule r => new TransliterateConfigPanel(r),
            RearrangeRule r => new RearrangeConfigPanel(r),
            ReformatDateRule r => new ReformatDateConfigPanel(r),
            RandomizeRule r => new RandomizeConfigPanel(r),
            PascalScriptRule r => new PascalScriptConfigPanel(r),
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
            RemoveRule => "Remove",
            CaseRule => "Case",
            SerializeRule => "Serialize",
            ExtensionRule => "Extension",
            RegexRule => "RegEx",
            PaddingRule => "Padding",
            StripRule => "Strip",
            CleanUpRule => "CleanUp",
            TransliterateRule => "Transliterate",
            RearrangeRule => "Rearrange",
            ReformatDateRule => "ReformatDate",
            RandomizeRule => "Randomize",
            UserInputRule => "UserInput",
            MappingRule => "Mapping",
            _ => ""
        };
    }

    private record RuleInfo(string TypeKey, string Name, Geometry? Icon);
}

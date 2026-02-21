using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ReNamer.Views.RuleConfigs;

/// <summary>
/// 共享辅助方法：分隔符、Meta Tag 菜单、正则表达式模板、帮助链接
/// </summary>
public static class RuleConfigHelper
{
    /// <summary>
    /// 在 TextBox 光标位置插入分隔符 "|"
    /// </summary>
    public static void InsertSeparator(TextBox target)
    {
        if (target == null) return;
        var pos = target.CaretIndex;
        target.Text = target.Text.Insert(pos, "|");
        target.CaretIndex = pos + 1;
        target.Focus();
    }

    /// <summary>
    /// 显示 Meta Tag 弹出菜单，选中项插入到目标 TextBox
    /// </summary>
    public static void ShowMetaTagMenu(TextBox target, UIElement placementTarget)
    {
        var menu = new ContextMenu();
        // File info tags
        AddTagGroup(menu, R("RuleCfg_MetaTag_Group_FileInfo"),
            ("{:Name:}", R("RuleCfg_MetaTag_Name")),
            ("{:NameNoExt:}", R("RuleCfg_MetaTag_NameNoExt")),
            ("{:Ext:}", R("RuleCfg_MetaTag_Ext")),
            ("{:Path:}", R("RuleCfg_MetaTag_Path")),
            ("{:Folder:}", R("RuleCfg_MetaTag_Folder")),
            ("{:ParentFolder:}", R("RuleCfg_MetaTag_ParentFolder")));
        menu.Items.Add(new Separator());
        // Size/date tags
        AddTagGroup(menu, R("RuleCfg_MetaTag_Group_Attributes"),
            ("{:Size:}", R("RuleCfg_MetaTag_Size")),
            ("{:SizeKB:}", R("RuleCfg_MetaTag_SizeKB")),
            ("{:SizeMB:}", R("RuleCfg_MetaTag_SizeMB")),
            ("{:Created:}", R("RuleCfg_MetaTag_Created")),
            ("{:Modified:}", R("RuleCfg_MetaTag_Modified")),
            ("{:Accessed:}", R("RuleCfg_MetaTag_Accessed")));
        menu.Items.Add(new Separator());
        // Counter/random tags
        AddTagGroup(menu, R("RuleCfg_MetaTag_Group_Dynamic"),
            ("{:Counter:}", R("RuleCfg_MetaTag_Counter")),
            ("{:Random:}", R("RuleCfg_MetaTag_Random")),
            ("{:GUID:}", R("RuleCfg_MetaTag_Guid")),
            ("{:Date:}", R("RuleCfg_MetaTag_Date")),
            ("{:Time:}", R("RuleCfg_MetaTag_Time")));

        menu.PlacementTarget = placementTarget;
        menu.IsOpen = true;

        void AddTagGroup(ContextMenu m, string header, params (string tag, string desc)[] items)
        {
            var group = new MenuItem { Header = header, IsEnabled = false, FontWeight = FontWeights.SemiBold };
            m.Items.Add(group);
            foreach (var (tag, desc) in items)
            {
                var mi = new MenuItem { Header = $"{tag}  — {desc}", Tag = tag };
                mi.Click += (_, _) => InsertTextAtCaret(target, tag);
                m.Items.Add(mi);
            }
        }
    }

    /// <summary>
    /// 显示常用正则表达式模板菜单
    /// </summary>
    public static void ShowExpressionMenu(TextBox target, UIElement placementTarget)
    {
        var menu = new ContextMenu();
        var patterns = new (string pattern, string desc)[]
        {
            (@"\d+", R("RuleCfg_RegexPattern_Digits")),
            (@"\w+", R("RuleCfg_RegexPattern_WordChars")),
            (@"[a-z]+", R("RuleCfg_RegexPattern_Lowercase")),
            (@"[A-Z]+", R("RuleCfg_RegexPattern_Uppercase")),
            (@"[a-zA-Z]+", R("RuleCfg_RegexPattern_AnyLetters")),
            (@".*", R("RuleCfg_RegexPattern_AnyGreedy")),
            (@".+?", R("RuleCfg_RegexPattern_AnyLazy")),
            (@"\s+", R("RuleCfg_RegexPattern_Whitespace")),
            (@"[^.]+", R("RuleCfg_RegexPattern_NotDot")),
            (@"(\d{4})-(\d{2})-(\d{2})", R("RuleCfg_RegexPattern_DateYMD")),
            (@"^(.+?)(\s*-\s*)(.+)$", R("RuleCfg_RegexPattern_NameTitle")),
            (@"^\d+\.\s*", R("RuleCfg_RegexPattern_LeadingNumber")),
            (@"\s*\(.*?\)\s*", R("RuleCfg_RegexPattern_ParensText")),
            (@"\s*\[.*?\]\s*", R("RuleCfg_RegexPattern_BracketsText")),
        };

        foreach (var (pattern, desc) in patterns)
        {
            var mi = new MenuItem { Header = $"{pattern}  — {desc}", Tag = pattern };
            mi.Click += (_, _) => InsertTextAtCaret(target, pattern);
            menu.Items.Add(mi);
        }

        menu.PlacementTarget = placementTarget;
        menu.IsOpen = true;
    }

    /// <summary>
    /// 显示 DateTime 格式帮助菜单
    /// </summary>
    public static void ShowDateFormatHelpMenu(TextBox target, UIElement placementTarget)
    {
        var menu = new ContextMenu();
        // Separator option
        var sepItem = new MenuItem { Header = R("RuleCfg_DateFormat_InsertSeparator") };
        sepItem.Click += (_, _) => InsertSeparator(target);
        menu.Items.Add(sepItem);
        menu.Items.Add(new Separator());

        // Format specifiers
        var formats = new (string fmt, string desc)[]
        {
            ("yyyy", R("RuleCfg_DateFormat_Year4")),
            ("yy", R("RuleCfg_DateFormat_Year2")),
            ("MM", R("RuleCfg_DateFormat_Month2")),
            ("MMM", R("RuleCfg_DateFormat_MonthAbbr")),
            ("MMMM", R("RuleCfg_DateFormat_MonthFull")),
            ("dd", R("RuleCfg_DateFormat_Day2")),
            ("ddd", R("RuleCfg_DateFormat_DayAbbr")),
            ("dddd", R("RuleCfg_DateFormat_DayFull")),
            ("HH", R("RuleCfg_DateFormat_Hour24")),
            ("hh", R("RuleCfg_DateFormat_Hour12")),
            ("mm", R("RuleCfg_DateFormat_Minute")),
            ("ss", R("RuleCfg_DateFormat_Second")),
            ("tt", R("RuleCfg_DateFormat_AmPm")),
        };

        foreach (var (fmt, desc) in formats)
        {
            var mi = new MenuItem { Header = $"{fmt}  — {desc}", Tag = fmt };
            mi.Click += (_, _) => InsertTextAtCaret(target, fmt);
            menu.Items.Add(mi);
        }

        menu.Items.Add(new Separator());
        var helpItem = new MenuItem { Header = R("RuleCfg_DateFormat_ReferenceOnline") };
        helpItem.Click += (_, _) => OpenUrl("https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings");
        menu.Items.Add(helpItem);

        menu.PlacementTarget = placementTarget;
        menu.IsOpen = true;
    }

    /// <summary>
    /// 打开 URL
    /// </summary>
    public static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { /* ignore */ }
    }

    /// <summary>
    /// 显示 DateTime 格式帮助菜单（ComboBox 版本）
    /// </summary>
    public static void ShowDateFormatHelpMenu(ComboBox target, UIElement placementTarget)
    {
        var menu = new ContextMenu();
        var sepItem = new MenuItem { Header = R("RuleCfg_DateFormat_InsertSeparator") };
        sepItem.Click += (_, _) => InsertTextIntoComboBox(target, "|");
        menu.Items.Add(sepItem);
        menu.Items.Add(new Separator());

        var formats = new (string fmt, string desc)[]
        {
            ("yyyy", R("RuleCfg_DateFormat_Year4")), ("yy", R("RuleCfg_DateFormat_Year2")),
            ("MM", R("RuleCfg_DateFormat_Month2")), ("MMM", R("RuleCfg_DateFormat_MonthAbbr")), ("MMMM", R("RuleCfg_DateFormat_MonthFull")),
            ("dd", R("RuleCfg_DateFormat_Day2")), ("ddd", R("RuleCfg_DateFormat_DayAbbr")), ("dddd", R("RuleCfg_DateFormat_DayFull")),
            ("HH", R("RuleCfg_DateFormat_Hour24")), ("hh", R("RuleCfg_DateFormat_Hour12")),
            ("mm", R("RuleCfg_DateFormat_Minute")), ("ss", R("RuleCfg_DateFormat_Second")), ("tt", R("RuleCfg_DateFormat_AmPm")),
        };
        foreach (var (fmt, desc) in formats)
        {
            var mi = new MenuItem { Header = $"{fmt}  — {desc}", Tag = fmt };
            mi.Click += (_, _) => InsertTextIntoComboBox(target, fmt);
            menu.Items.Add(mi);
        }

        menu.Items.Add(new Separator());
        var helpItem = new MenuItem { Header = R("RuleCfg_DateFormat_ReferenceOnline") };
        helpItem.Click += (_, _) => OpenUrl("https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings");
        menu.Items.Add(helpItem);

        menu.PlacementTarget = placementTarget;
        menu.IsOpen = true;
    }

    private static void InsertTextAtCaret(TextBox target, string text)
    {
        if (target == null) return;
        var pos = target.CaretIndex;
        target.Text = target.Text.Insert(pos, text);
        target.CaretIndex = pos + text.Length;
        target.Focus();
    }

    private static void InsertTextIntoComboBox(ComboBox target, string text)
    {
        if (target == null) return;
        var current = target.Text ?? "";
        target.Text = current + text;
        target.Focus();
    }

    private static string R(string key)
    {
        if (Application.Current?.TryFindResource(key) is string value && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return key;
    }
}

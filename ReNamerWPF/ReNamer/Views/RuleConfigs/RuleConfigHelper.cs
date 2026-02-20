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
        AddTagGroup(menu, R("RuleCfg_MetaTag_Group_FileInfo", "File Info"),
            ("{:Name:}", R("RuleCfg_MetaTag_Name", "Original file name")),
            ("{:NameNoExt:}", R("RuleCfg_MetaTag_NameNoExt", "Name without extension")),
            ("{:Ext:}", R("RuleCfg_MetaTag_Ext", "Extension")),
            ("{:Path:}", R("RuleCfg_MetaTag_Path", "Full path")),
            ("{:Folder:}", R("RuleCfg_MetaTag_Folder", "Folder name")),
            ("{:ParentFolder:}", R("RuleCfg_MetaTag_ParentFolder", "Parent folder name")));
        menu.Items.Add(new Separator());
        // Size/date tags
        AddTagGroup(menu, R("RuleCfg_MetaTag_Group_Attributes", "Attributes"),
            ("{:Size:}", R("RuleCfg_MetaTag_Size", "File size (bytes)")),
            ("{:SizeKB:}", R("RuleCfg_MetaTag_SizeKB", "File size (KB)")),
            ("{:SizeMB:}", R("RuleCfg_MetaTag_SizeMB", "File size (MB)")),
            ("{:Created:}", R("RuleCfg_MetaTag_Created", "Created date")),
            ("{:Modified:}", R("RuleCfg_MetaTag_Modified", "Modified date")),
            ("{:Accessed:}", R("RuleCfg_MetaTag_Accessed", "Accessed date")));
        menu.Items.Add(new Separator());
        // Counter/random tags
        AddTagGroup(menu, R("RuleCfg_MetaTag_Group_Dynamic", "Dynamic"),
            ("{:Counter:}", R("RuleCfg_MetaTag_Counter", "Auto-increment counter")),
            ("{:Random:}", R("RuleCfg_MetaTag_Random", "Random characters")),
            ("{:GUID:}", R("RuleCfg_MetaTag_Guid", "Generate GUID")),
            ("{:Date:}", R("RuleCfg_MetaTag_Date", "Current date")),
            ("{:Time:}", R("RuleCfg_MetaTag_Time", "Current time")));

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
            (@"\d+", R("RuleCfg_RegexPattern_Digits", "One or more digits")),
            (@"\w+", R("RuleCfg_RegexPattern_WordChars", "One or more word characters")),
            (@"[a-z]+", R("RuleCfg_RegexPattern_Lowercase", "Lowercase letters")),
            (@"[A-Z]+", R("RuleCfg_RegexPattern_Uppercase", "Uppercase letters")),
            (@"[a-zA-Z]+", R("RuleCfg_RegexPattern_AnyLetters", "Any letters")),
            (@".*", R("RuleCfg_RegexPattern_AnyGreedy", "Any characters (greedy)")),
            (@".+?", R("RuleCfg_RegexPattern_AnyLazy", "Any characters (lazy)")),
            (@"\s+", R("RuleCfg_RegexPattern_Whitespace", "Whitespace")),
            (@"[^.]+", R("RuleCfg_RegexPattern_NotDot", "Everything except dot")),
            (@"(\d{4})-(\d{2})-(\d{2})", R("RuleCfg_RegexPattern_DateYMD", "Date pattern YYYY-MM-DD")),
            (@"^(.+?)(\s*-\s*)(.+)$", R("RuleCfg_RegexPattern_NameTitle", "Name - Title pattern")),
            (@"^\d+\.\s*", R("RuleCfg_RegexPattern_LeadingNumber", "Leading number with dot")),
            (@"\s*\(.*?\)\s*", R("RuleCfg_RegexPattern_ParensText", "Text in parentheses")),
            (@"\s*\[.*?\]\s*", R("RuleCfg_RegexPattern_BracketsText", "Text in brackets")),
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
        var sepItem = new MenuItem { Header = R("RuleCfg_DateFormat_InsertSeparator", "Insert separator |") };
        sepItem.Click += (_, _) => InsertSeparator(target);
        menu.Items.Add(sepItem);
        menu.Items.Add(new Separator());

        // Format specifiers
        var formats = new (string fmt, string desc)[]
        {
            ("yyyy", R("RuleCfg_DateFormat_Year4", "Year (4 digits)")),
            ("yy", R("RuleCfg_DateFormat_Year2", "Year (2 digits)")),
            ("MM", R("RuleCfg_DateFormat_Month2", "Month (01-12)")),
            ("MMM", R("RuleCfg_DateFormat_MonthAbbr", "Month abbreviation")),
            ("MMMM", R("RuleCfg_DateFormat_MonthFull", "Month full name")),
            ("dd", R("RuleCfg_DateFormat_Day2", "Day (01-31)")),
            ("ddd", R("RuleCfg_DateFormat_DayAbbr", "Day abbreviation")),
            ("dddd", R("RuleCfg_DateFormat_DayFull", "Day full name")),
            ("HH", R("RuleCfg_DateFormat_Hour24", "Hour 24h (00-23)")),
            ("hh", R("RuleCfg_DateFormat_Hour12", "Hour 12h (01-12)")),
            ("mm", R("RuleCfg_DateFormat_Minute", "Minute (00-59)")),
            ("ss", R("RuleCfg_DateFormat_Second", "Second (00-59)")),
            ("tt", R("RuleCfg_DateFormat_AmPm", "AM/PM")),
        };

        foreach (var (fmt, desc) in formats)
        {
            var mi = new MenuItem { Header = $"{fmt}  — {desc}", Tag = fmt };
            mi.Click += (_, _) => InsertTextAtCaret(target, fmt);
            menu.Items.Add(mi);
        }

        menu.Items.Add(new Separator());
        var helpItem = new MenuItem { Header = R("RuleCfg_DateFormat_ReferenceOnline", "DateTime Format Reference (online)...") };
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
        var sepItem = new MenuItem { Header = R("RuleCfg_DateFormat_InsertSeparator", "Insert separator |") };
        sepItem.Click += (_, _) => InsertTextIntoComboBox(target, "|");
        menu.Items.Add(sepItem);
        menu.Items.Add(new Separator());

        var formats = new (string fmt, string desc)[]
        {
            ("yyyy", R("RuleCfg_DateFormat_Year4", "Year (4 digits)")), ("yy", R("RuleCfg_DateFormat_Year2", "Year (2 digits)")),
            ("MM", R("RuleCfg_DateFormat_Month2", "Month (01-12)")), ("MMM", R("RuleCfg_DateFormat_MonthAbbr", "Month abbreviation")), ("MMMM", R("RuleCfg_DateFormat_MonthFull", "Month full name")),
            ("dd", R("RuleCfg_DateFormat_Day2", "Day (01-31)")), ("ddd", R("RuleCfg_DateFormat_DayAbbr", "Day abbreviation")), ("dddd", R("RuleCfg_DateFormat_DayFull", "Day full name")),
            ("HH", R("RuleCfg_DateFormat_Hour24", "Hour 24h (00-23)")), ("hh", R("RuleCfg_DateFormat_Hour12", "Hour 12h (01-12)")),
            ("mm", R("RuleCfg_DateFormat_Minute", "Minute (00-59)")), ("ss", R("RuleCfg_DateFormat_Second", "Second (00-59)")), ("tt", R("RuleCfg_DateFormat_AmPm", "AM/PM")),
        };
        foreach (var (fmt, desc) in formats)
        {
            var mi = new MenuItem { Header = $"{fmt}  — {desc}", Tag = fmt };
            mi.Click += (_, _) => InsertTextIntoComboBox(target, fmt);
            menu.Items.Add(mi);
        }

        menu.Items.Add(new Separator());
        var helpItem = new MenuItem { Header = R("RuleCfg_DateFormat_ReferenceOnline", "DateTime Format Reference (online)...") };
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

    private static string R(string key, string fallback)
    {
        if (Application.Current?.TryFindResource(key) is string value && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }
}

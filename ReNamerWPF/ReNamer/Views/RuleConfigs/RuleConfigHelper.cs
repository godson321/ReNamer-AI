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
        AddTagGroup(menu, "File Info",
            ("{:Name:}", "Original file name"),
            ("{:NameNoExt:}", "Name without extension"),
            ("{:Ext:}", "Extension"),
            ("{:Path:}", "Full path"),
            ("{:Folder:}", "Folder name"),
            ("{:ParentFolder:}", "Parent folder name"));
        menu.Items.Add(new Separator());
        // Size/date tags
        AddTagGroup(menu, "Attributes",
            ("{:Size:}", "File size (bytes)"),
            ("{:SizeKB:}", "File size (KB)"),
            ("{:SizeMB:}", "File size (MB)"),
            ("{:Created:}", "Created date"),
            ("{:Modified:}", "Modified date"),
            ("{:Accessed:}", "Accessed date"));
        menu.Items.Add(new Separator());
        // Counter/random tags
        AddTagGroup(menu, "Dynamic",
            ("{:Counter:}", "Auto-increment counter"),
            ("{:Random:}", "Random characters"),
            ("{:GUID:}", "Generate GUID"),
            ("{:Date:}", "Current date"),
            ("{:Time:}", "Current time"));

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
            (@"\d+", "One or more digits"),
            (@"\w+", "One or more word characters"),
            (@"[a-z]+", "Lowercase letters"),
            (@"[A-Z]+", "Uppercase letters"),
            (@"[a-zA-Z]+", "Any letters"),
            (@".*", "Any characters (greedy)"),
            (@".+?", "Any characters (lazy)"),
            (@"\s+", "Whitespace"),
            (@"[^.]+", "Everything except dot"),
            (@"(\d{4})-(\d{2})-(\d{2})", "Date pattern YYYY-MM-DD"),
            (@"^(.+?)(\s*-\s*)(.+)$", "Name - Title pattern"),
            (@"^\d+\.\s*", "Leading number with dot"),
            (@"\s*\(.*?\)\s*", "Text in parentheses"),
            (@"\s*\[.*?\]\s*", "Text in brackets"),
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
        var sepItem = new MenuItem { Header = "Insert separator |" };
        sepItem.Click += (_, _) => InsertSeparator(target);
        menu.Items.Add(sepItem);
        menu.Items.Add(new Separator());

        // Format specifiers
        var formats = new (string fmt, string desc)[]
        {
            ("yyyy", "Year (4 digits)"),
            ("yy", "Year (2 digits)"),
            ("MM", "Month (01-12)"),
            ("MMM", "Month abbreviation"),
            ("MMMM", "Month full name"),
            ("dd", "Day (01-31)"),
            ("ddd", "Day abbreviation"),
            ("dddd", "Day full name"),
            ("HH", "Hour 24h (00-23)"),
            ("hh", "Hour 12h (01-12)"),
            ("mm", "Minute (00-59)"),
            ("ss", "Second (00-59)"),
            ("tt", "AM/PM"),
        };

        foreach (var (fmt, desc) in formats)
        {
            var mi = new MenuItem { Header = $"{fmt}  — {desc}", Tag = fmt };
            mi.Click += (_, _) => InsertTextAtCaret(target, fmt);
            menu.Items.Add(mi);
        }

        menu.Items.Add(new Separator());
        var helpItem = new MenuItem { Header = "DateTime Format Reference (online)..." };
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
        var sepItem = new MenuItem { Header = "Insert separator |" };
        sepItem.Click += (_, _) => InsertTextIntoComboBox(target, "|");
        menu.Items.Add(sepItem);
        menu.Items.Add(new Separator());

        var formats = new (string fmt, string desc)[]
        {
            ("yyyy", "Year (4 digits)"), ("yy", "Year (2 digits)"),
            ("MM", "Month (01-12)"), ("MMM", "Month abbreviation"), ("MMMM", "Month full name"),
            ("dd", "Day (01-31)"), ("ddd", "Day abbreviation"), ("dddd", "Day full name"),
            ("HH", "Hour 24h (00-23)"), ("hh", "Hour 12h (01-12)"),
            ("mm", "Minute (00-59)"), ("ss", "Second (00-59)"), ("tt", "AM/PM"),
        };
        foreach (var (fmt, desc) in formats)
        {
            var mi = new MenuItem { Header = $"{fmt}  — {desc}", Tag = fmt };
            mi.Click += (_, _) => InsertTextIntoComboBox(target, fmt);
            menu.Items.Add(mi);
        }

        menu.Items.Add(new Separator());
        var helpItem = new MenuItem { Header = "DateTime Format Reference (online)..." };
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
}

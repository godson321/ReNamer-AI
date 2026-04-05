using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ReNamer.Models;

namespace ReNamer.Rules;

/// <summary>
/// 规则通用辅助方法
/// </summary>
public static class RuleHelpers
{
    private static int _metaTagCounter = 1;
    public static bool ResolveMetaTagsEnabled { get; set; } = true;

    public static void ResetMetaTagCounter() => _metaTagCounter = 1;

    /// <summary>
    /// 将通配符模式转换为正则表达式
    /// ? → 匹配单个字符, * → 匹配任意字符, [abc] → 字符集
    /// $1..$9 → 正则反向引用 \1..\9
    /// </summary>
    public static string WildcardToRegex(string pattern, bool supportBackreferences = false)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];
            switch (c)
            {
                case '*': sb.Append("(.*)"); break;
                case '?': sb.Append("(.)"); break;
                case '[':
                    // 查找匹配的 ]
                    int end = pattern.IndexOf(']', i + 1);
                    if (end > i)
                    {
                        sb.Append('[');
                        sb.Append(pattern.AsSpan(i + 1, end - i - 1));
                        sb.Append(']');
                        i = end;
                    }
                    else
                    {
                        sb.Append(Regex.Escape(c.ToString()));
                    }
                    break;
                case '$' when supportBackreferences && i + 1 < pattern.Length && char.IsDigit(pattern[i + 1]):
                    sb.Append('\\');
                    sb.Append(pattern[i + 1]);
                    i++;
                    break;
                default:
                    sb.Append(Regex.Escape(c.ToString()));
                    break;
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 构建整词匹配的正则表达式
    /// </summary>
    public static string BuildWholeWordPattern(string text, bool caseSensitive)
    {
        return @"\b" + Regex.Escape(text) + @"\b";
    }

    /// <summary>
    /// 执行整词匹配替换
    /// </summary>
    public static string ReplaceWholeWords(string input, string find, string replace,
        bool caseSensitive, ReplaceOccurrence occurrence)
    {
        var pattern = BuildWholeWordPattern(find, caseSensitive);
        var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        var regex = new Regex(pattern, options);

        return occurrence switch
        {
            ReplaceOccurrence.All => regex.Replace(input, replace),
            ReplaceOccurrence.First => regex.Replace(input, replace, 1),
            ReplaceOccurrence.Last => ReplaceLastMatch(regex, input, replace),
            _ => input
        };
    }

    /// <summary>
    /// 执行通配符替换
    /// </summary>
    public static string ReplaceWildcard(string input, string findPattern, string replacePattern,
        ReplaceOccurrence occurrence, bool caseSensitive)
    {
        var regexPattern = WildcardToRegex(findPattern, supportBackreferences: true);
        var regexReplace = ConvertWildcardReplace(replacePattern);
        var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        var regex = new Regex(regexPattern, options);

        return occurrence switch
        {
            ReplaceOccurrence.All => regex.Replace(input, regexReplace),
            ReplaceOccurrence.First => regex.Replace(input, regexReplace, 1),
            ReplaceOccurrence.Last => ReplaceLastMatch(regex, input, regexReplace),
            _ => input
        };
    }

    /// <summary>
    /// 执行整词匹配移除
    /// </summary>
    public static string RemoveWholeWords(string input, string find,
        bool caseSensitive, RemoveOccurrence occurrence)
    {
        var pattern = BuildWholeWordPattern(find, caseSensitive);
        var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        var regex = new Regex(pattern, options);

        return occurrence switch
        {
            RemoveOccurrence.All => regex.Replace(input, ""),
            RemoveOccurrence.First => regex.Replace(input, "", 1),
            RemoveOccurrence.Last => ReplaceLastMatch(regex, input, ""),
            _ => input
        };
    }

    /// <summary>
    /// 执行通配符移除（不支持反向引用）
    /// </summary>
    public static string RemoveWildcard(string input, string findPattern, RemoveOccurrence occurrence)
    {
        return RemoveWildcard(input, findPattern, occurrence, caseSensitive: false);
    }

    public static string RemoveWildcard(string input, string findPattern, RemoveOccurrence occurrence, bool caseSensitive)
    {
        var regexPattern = WildcardToRegex(findPattern, supportBackreferences: false);
        var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        var regex = new Regex(regexPattern, options);

        return occurrence switch
        {
            RemoveOccurrence.All => regex.Replace(input, ""),
            RemoveOccurrence.First => regex.Replace(input, "", 1),
            RemoveOccurrence.Last => ReplaceLastMatch(regex, input, ""),
            _ => input
        };
    }

    /// <summary>
    /// 替换最后一个匹配
    /// </summary>
    private static string ReplaceLastMatch(Regex regex, string input, string replacement)
    {
        var matches = regex.Matches(input);
        if (matches.Count == 0) return input;
        var last = matches[^1];
        return input[..last.Index] + regex.Replace(last.Value, replacement) + input[(last.Index + last.Length)..];
    }

    /// <summary>
    /// 将通配符替换模式中的 $n 转换为正则 $n
    /// </summary>
    private static string ConvertWildcardReplace(string pattern)
    {
        // $n 在正则替换中本身就是 $n，直接使用
        return pattern;
    }

    public static bool ContainsUnescapedSeparator(string input, char separator = '|')
    {
        if (string.IsNullOrEmpty(input))
            return false;

        bool escaped = false;
        foreach (var c in input)
        {
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (c == separator)
                return true;
        }

        return false;
    }

    public static string[] SplitUnescaped(string input, char separator = '|')
    {
        if (input == null)
            return Array.Empty<string>();

        var parts = new System.Collections.Generic.List<string>();
        var sb = new StringBuilder();
        bool escaped = false;

        foreach (var c in input)
        {
            if (escaped)
            {
                if (c != separator && c != '\\')
                    sb.Append('\\');
                sb.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (c == separator)
            {
                parts.Add(sb.ToString());
                sb.Clear();
                continue;
            }

            sb.Append(c);
        }

        if (escaped)
            sb.Append('\\');

        parts.Add(sb.ToString());
        return parts.ToArray();
    }

    public static string UnescapeSeparatorEscapes(string input, char separator = '|')
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder();
        bool escaped = false;

        foreach (var c in input)
        {
            if (escaped)
            {
                if (c != separator && c != '\\')
                    sb.Append('\\');
                sb.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            sb.Append(c);
        }

        if (escaped)
            sb.Append('\\');

        return sb.ToString();
    }

    /// <summary>
    /// 判断字符是否为单词边界字符
    /// </summary>
    public static bool IsWordBoundary(string input, int index, int length)
    {
        bool startOk = index == 0 || !char.IsLetterOrDigit(input[index - 1]);
        bool endOk = (index + length >= input.Length) || !char.IsLetterOrDigit(input[index + length]);
        return startOk && endOk;
    }

    public static string ResolveMetaTags(string input, RenFile file)
    {
        if (!ResolveMetaTagsEnabled || string.IsNullOrEmpty(input)) return input;

        return Regex.Replace(input, @"\{\:([A-Za-z]+)\:\}", m =>
        {
            var key = m.Groups[1].Value;
            return ResolveTag(key, file);
        });
    }

    private static string ResolveTag(string key, RenFile file)
    {
        switch (key)
        {
            case "Name":
                return file.OriginalName;
            case "NameNoExt":
                return Path.GetFileNameWithoutExtension(file.OriginalName);
            case "Ext":
                return file.Extension;
            case "Path":
                return file.FullPath;
            case "Folder":
                return file.FolderName;
            case "ParentFolder":
            {
                var parent = Directory.GetParent(file.FolderPath);
                return parent?.Name ?? "";
            }
            case "Size":
                return file.Size.ToString();
            case "SizeKB":
                return file.SizeKB;
            case "SizeMB":
                return file.SizeMB;
            case "Created":
                return file.CreatedDisplay;
            case "Modified":
                return file.ModifiedDisplay;
            case "Accessed":
                return GetAccessedDate(file);
            case "Counter":
                return (_metaTagCounter++).ToString();
            case "Random":
                return GetRandomString(8);
            case "GUID":
                return Guid.NewGuid().ToString("N");
            case "Date":
                return DateTime.Now.ToString("yyyy-MM-dd");
            case "Time":
                return DateTime.Now.ToString("HHmmss");
            default:
                return "{:" + key + ":}";
        }
    }

    private static string GetAccessedDate(RenFile file)
    {
        try
        {
            if (File.Exists(file.FullPath))
                return File.GetLastAccessTime(file.FullPath).ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch { }
        return "";
    }

    private static string GetRandomString(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
            sb.Append(chars[Random.Shared.Next(chars.Length)]);
        return sb.ToString();
    }
}

using System;
using System.Text.RegularExpressions;
using ReNamer.Models;

namespace ReNamer.Rules;

/// <summary>
/// 替换规则 - 对应原版 TRuleReplace
/// </summary>
public class ReplaceRule : RuleBase
{
    public string FindText { get; set; } = "";
    public string ReplaceText { get; set; } = "";
    public bool CaseSensitive { get; set; } = false;
    public bool SkipExtension { get; set; } = true;
    public bool UseRegex { get; set; } = false;
    public bool WholeWordsOnly { get; set; } = false;
    public bool UseWildcards { get; set; } = false;
    public ReplaceOccurrence Occurrence { get; set; } = ReplaceOccurrence.All;

    public override string RuleName => "Replace";

    public override string Description => 
        $"Replace \"{FindText}\" with \"{ReplaceText}\"";

    public override string Execute(string fileName, RenFile file)
    {
        if (string.IsNullOrEmpty(FindText))
            return fileName;

        var (baseName, ext) = SplitFileName(fileName, SkipExtension);
        var replaceText = RuleHelpers.ResolveMetaTags(ReplaceText, file);

        string result;
        if (UseWildcards)
        {
            result = RuleHelpers.ReplaceWildcard(baseName, FindText, replaceText, Occurrence);
        }
        else if (UseRegex)
        {
            result = ExecuteRegex(baseName, replaceText);
        }
        else if (WholeWordsOnly)
        {
            result = RuleHelpers.ReplaceWholeWords(baseName, FindText, replaceText, CaseSensitive, Occurrence);
        }
        else
        {
            result = ExecuteSimple(baseName, replaceText);
        }

        return result + ext;
    }

    private string ExecuteSimple(string input, string replaceText)
    {
        var comparison = CaseSensitive 
            ? StringComparison.Ordinal 
            : StringComparison.OrdinalIgnoreCase;

        return Occurrence switch
        {
            ReplaceOccurrence.All => ReplaceAll(input, FindText, replaceText, comparison),
            ReplaceOccurrence.First => ReplaceFirst(input, FindText, replaceText, comparison),
            ReplaceOccurrence.Last => ReplaceLast(input, FindText, replaceText, comparison),
            _ => input
        };
    }

    private string ExecuteRegex(string input, string replaceText)
    {
        try
        {
            var options = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            var regex = new Regex(FindText, options);
            var effectiveReplaceText = ReplaceText.Contains("$")
                ? replaceText
                : replaceText.Replace("$", "$$");

            return Occurrence switch
            {
                ReplaceOccurrence.All => regex.Replace(input, effectiveReplaceText),
                ReplaceOccurrence.First => regex.Replace(input, effectiveReplaceText, 1),
                ReplaceOccurrence.Last => ReplaceLastRegex(input, regex, effectiveReplaceText),
                _ => input
            };
        }
        catch
        {
            return input; // 正则表达式错误时返回原值
        }
    }

    private static string ReplaceAll(string input, string find, string replace, StringComparison comparison)
    {
        var result = input;
        int index;
        while ((index = result.IndexOf(find, comparison)) >= 0)
        {
            result = result.Remove(index, find.Length).Insert(index, replace);
        }
        return result;
    }

    private static string ReplaceFirst(string input, string find, string replace, StringComparison comparison)
    {
        var index = input.IndexOf(find, comparison);
        if (index < 0) return input;
        return input.Remove(index, find.Length).Insert(index, replace);
    }

    private static string ReplaceLast(string input, string find, string replace, StringComparison comparison)
    {
        var index = input.LastIndexOf(find, comparison);
        if (index < 0) return input;
        return input.Remove(index, find.Length).Insert(index, replace);
    }

    private static string ReplaceLastRegex(string input, Regex regex, string replace)
    {
        var matches = regex.Matches(input);
        if (matches.Count == 0) return input;
        
        var lastMatch = matches[^1];
        return input.Remove(lastMatch.Index, lastMatch.Length).Insert(lastMatch.Index, replace);
    }

}

public enum ReplaceOccurrence
{
    All,
    First,
    Last
}

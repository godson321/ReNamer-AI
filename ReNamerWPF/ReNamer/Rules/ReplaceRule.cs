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

        var (baseName, ext) = SplitFileName(fileName, SkipExtension, file.IsFolder);
        var replaceText = RuleHelpers.ResolveMetaTags(ReplaceText, file);
        var hasMultipleItems = RuleHelpers.ContainsUnescapedSeparator(FindText) && !UseRegex;
        var singleFindText = UseRegex ? FindText : RuleHelpers.UnescapeSeparatorEscapes(FindText);
        var singleReplaceText = hasMultipleItems ? replaceText : RuleHelpers.UnescapeSeparatorEscapes(replaceText);

        string result;
        if (hasMultipleItems)
        {
            result = ExecuteMultipleReplace(baseName, replaceText);
        }
        else if (UseWildcards)
        {
            result = RuleHelpers.ReplaceWildcard(baseName, singleFindText, singleReplaceText, Occurrence, CaseSensitive);
        }
        else if (UseRegex)
        {
            result = ExecuteRegex(baseName, replaceText);
        }
        else if (WholeWordsOnly)
        {
            result = RuleHelpers.ReplaceWholeWords(baseName, singleFindText, singleReplaceText, CaseSensitive, Occurrence);
        }
        else
        {
            result = ExecuteSimple(baseName, singleFindText, singleReplaceText);
        }

        return result + ext;
    }

    private string ExecuteSimple(string input, string findText, string replaceText)
    {
        var comparison = CaseSensitive 
            ? StringComparison.Ordinal 
            : StringComparison.OrdinalIgnoreCase;

        return Occurrence switch
        {
            ReplaceOccurrence.All => ReplaceAll(input, findText, replaceText, comparison),
            ReplaceOccurrence.First => ReplaceFirst(input, findText, replaceText, comparison),
            ReplaceOccurrence.Last => ReplaceLast(input, findText, replaceText, comparison),
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
        var replaced = lastMatch.Result(replace);
        return input.Remove(lastMatch.Index, lastMatch.Length).Insert(lastMatch.Index, replaced);
    }

    /// <summary>
    /// 执行多替换模式 - 按|分隔多个查找和替换项
    /// </summary>
    private string ExecuteMultipleReplace(string input, string replaceText)
    {
        var findParts = RuleHelpers.SplitUnescaped(FindText);
        var replaceParts = RuleHelpers.SplitUnescaped(replaceText);
        if (Occurrence != ReplaceOccurrence.All)
            return ExecuteMultipleReplaceGlobalSingle(input, findParts, replaceParts);

        var result = input;
        var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        for (int i = 0; i < findParts.Length; i++)
        {
            var findPart = findParts[i];
            if (string.IsNullOrEmpty(findPart)) continue;

            var replacePart = i < replaceParts.Length
                ? replaceParts[i]
                : replaceParts.Length == 1
                    ? replaceParts[0]
                    : replaceParts[^1];

            if (UseWildcards)
            {
                result = RuleHelpers.ReplaceWildcard(result, findPart, replacePart, Occurrence, CaseSensitive);
            }
            else if (WholeWordsOnly)
            {
                result = RuleHelpers.ReplaceWholeWords(result, findPart, replacePart, CaseSensitive, Occurrence);
            }
            else
            {
                result = Occurrence switch
                {
                    ReplaceOccurrence.All => ReplaceAll(result, findPart, replacePart, comparison),
                    ReplaceOccurrence.First => ReplaceFirst(result, findPart, replacePart, comparison),
                    ReplaceOccurrence.Last => ReplaceLast(result, findPart, replacePart, comparison),
                    _ => result
                };
            }
        }

        return result;
    }

    private string ExecuteMultipleReplaceGlobalSingle(string input, string[] findParts, string[] replaceParts)
    {
        int selectedIndex = -1;
        int selectedPosition = Occurrence == ReplaceOccurrence.First ? int.MaxValue : -1;

        for (int i = 0; i < findParts.Length; i++)
        {
            var findPart = findParts[i];
            if (string.IsNullOrEmpty(findPart)) continue;
            if (!TryGetMatchPosition(input, findPart, out var position)) continue;

            if (Occurrence == ReplaceOccurrence.First)
            {
                if (position < selectedPosition)
                {
                    selectedPosition = position;
                    selectedIndex = i;
                }
            }
            else
            {
                if (position > selectedPosition)
                {
                    selectedPosition = position;
                    selectedIndex = i;
                }
            }
        }

        if (selectedIndex < 0) return input;

        var selectedFind = findParts[selectedIndex];
        var selectedReplace = selectedIndex < replaceParts.Length
            ? replaceParts[selectedIndex]
            : replaceParts.Length == 1
                ? replaceParts[0]
                : replaceParts[^1];

        if (UseWildcards)
            return RuleHelpers.ReplaceWildcard(input, selectedFind, selectedReplace, Occurrence, CaseSensitive);
        if (WholeWordsOnly)
            return RuleHelpers.ReplaceWholeWords(input, selectedFind, selectedReplace, CaseSensitive, Occurrence);

        var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        return Occurrence == ReplaceOccurrence.First
            ? ReplaceFirst(input, selectedFind, selectedReplace, comparison)
            : ReplaceLast(input, selectedFind, selectedReplace, comparison);
    }

    private bool TryGetMatchPosition(string input, string findPart, out int position)
    {
        if (UseWildcards)
        {
            var regexPattern = RuleHelpers.WildcardToRegex(findPart, supportBackreferences: true);
            var options = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            var regex = new Regex(regexPattern, options);
            var matches = regex.Matches(input);
            if (matches.Count == 0)
            {
                position = -1;
                return false;
            }

            position = Occurrence == ReplaceOccurrence.First
                ? matches[0].Index
                : matches[^1].Index;
            return true;
        }

        if (WholeWordsOnly)
        {
            var pattern = RuleHelpers.BuildWholeWordPattern(findPart, CaseSensitive);
            var options = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            var regex = new Regex(pattern, options);
            var matches = regex.Matches(input);
            if (matches.Count == 0)
            {
                position = -1;
                return false;
            }

            position = Occurrence == ReplaceOccurrence.First
                ? matches[0].Index
                : matches[^1].Index;
            return true;
        }

        var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        position = Occurrence == ReplaceOccurrence.First
            ? input.IndexOf(findPart, comparison)
            : input.LastIndexOf(findPart, comparison);
        return position >= 0;
    }

}

public enum ReplaceOccurrence
{
    All,
    First,
    Last
}

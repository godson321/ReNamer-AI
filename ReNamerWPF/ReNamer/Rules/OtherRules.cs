using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ReNamer.Models;

namespace ReNamer.Rules;

// ═══════════════════════════════════════════════════════════════
// InsertRule
// ═══════════════════════════════════════════════════════════════
public class InsertRule : RuleBase
{
    public string InsertText { get; set; } = "";
    public int Position { get; set; } = 1;
    public InsertPositionType InsertPosition { get; set; } = InsertPositionType.Prefix;
    public bool SkipExtension { get; set; } = true;
    public string AfterText { get; set; } = "";
    public string BeforeText { get; set; } = "";
    public bool RightToLeft { get; set; } = false;

    public override string RuleName => "Insert";
    public override string Description => InsertPosition switch
    {
        InsertPositionType.Prefix => $"Insert \"{InsertText}\" as prefix",
        InsertPositionType.Suffix => $"Insert \"{InsertText}\" as suffix",
        InsertPositionType.Position => $"Insert \"{InsertText}\" at position {Position}",
        InsertPositionType.AfterText => $"Insert \"{InsertText}\" after \"{AfterText}\"",
        InsertPositionType.BeforeText => $"Insert \"{InsertText}\" before \"{BeforeText}\"",
        InsertPositionType.ReplaceCurrentName => $"Replace name with \"{InsertText}\"",
        _ => $"Insert \"{InsertText}\""
    };

    public override string Execute(string fileName, RenFile file)
    {
        if (string.IsNullOrEmpty(InsertText)) return fileName;
        var (baseName, ext) = SplitFileName(fileName, SkipExtension);
        var insertText = RuleHelpers.ResolveMetaTags(InsertText, file);
        return InsertPosition switch
        {
            InsertPositionType.Prefix => insertText + baseName + ext,
            InsertPositionType.Suffix => baseName + insertText + ext,
            InsertPositionType.Position => InsertAtPosition(baseName, insertText) + ext,
            InsertPositionType.AfterText => InsertAfter(baseName, insertText) + ext,
            InsertPositionType.BeforeText => InsertBefore(baseName, insertText) + ext,
            InsertPositionType.ReplaceCurrentName => insertText + ext,
            _ => fileName
        };
    }

    private string InsertAtPosition(string baseName, string insertText)
    {
        int pos = RightToLeft
            ? Math.Max(0, baseName.Length - Position)
            : Math.Min(Math.Max(0, Position - 1), baseName.Length);
        return baseName.Insert(pos, insertText);
    }

    private string InsertAfter(string baseName, string insertText)
    {
        if (string.IsNullOrEmpty(AfterText)) return baseName;
        int idx = baseName.IndexOf(AfterText, StringComparison.Ordinal);
        return idx < 0 ? baseName : baseName.Insert(idx + AfterText.Length, insertText);
    }

    private string InsertBefore(string baseName, string insertText)
    {
        if (string.IsNullOrEmpty(BeforeText)) return baseName;
        int idx = baseName.IndexOf(BeforeText, StringComparison.Ordinal);
        return idx < 0 ? baseName : baseName.Insert(idx, insertText);
    }
}

public enum InsertPositionType { Prefix, Suffix, Position, AfterText, BeforeText, ReplaceCurrentName }

// ═══════════════════════════════════════════════════════════════
// DeleteRule - 完整实现 Delimiter/RightToLeft/LeaveDelimiter
// ═══════════════════════════════════════════════════════════════
public class DeleteRule : RuleBase
{
    public DeleteMode Mode { get; set; } = DeleteMode.PositionDelete;

    // Position delete mode
    public DeleteFromType FromType { get; set; } = DeleteFromType.Position;
    public int FromPosition { get; set; } = 1;
    public string FromDelimiter { get; set; } = "";
    public DeleteUntilType UntilType { get; set; } = DeleteUntilType.Count;
    public int UntilCount { get; set; } = 1;
    public string UntilDelimiter { get; set; } = "";
    public bool DeleteCurrentName { get; set; } = false;
    public bool RightToLeft { get; set; } = false;
    public bool SkipExtension { get; set; } = true;
    public bool LeaveDelimiter { get; set; } = false;
    // Character remove mode (merged from Strip)
    public bool StripEnglishLetters { get; set; } = false;
    public bool StripDigits { get; set; } = false;
    public bool StripSymbols { get; set; } = false;
    public bool StripBrackets { get; set; } = false;
    public bool StripUserDefined { get; set; } = false;
    public string UserDefinedChars { get; set; } = "";
    public bool StripUnicodeRange { get; set; } = false;
    public string UnicodeRange { get; set; } = "10000-10FFFF";
    public StripWhere Where { get; set; } = StripWhere.Everywhere;
    public bool StripAllExceptSelected { get; set; } = false;
    public bool CaseSensitive { get; set; } = false;
    // Text remove mode (merged from legacy RemoveRule)
    public string RemovePattern { get; set; } = "";
    public RemoveOccurrence RemoveOccurrence { get; set; } = RemoveOccurrence.All;
    public bool RemoveCaseSensitive { get; set; } = false;
    public bool RemoveWholeWordsOnly { get; set; } = false;
    public bool RemoveUseWildcards { get; set; } = false;
    public bool TextFirstInCombined { get; set; } = false;

    private static readonly string EnglishLetters = "abcdefghijklmnopqrstuvwxyz";
    private static readonly string Digits = "1234567890";
    private static readonly string Symbols = "!\"#$%&'*+,-./:;=?@\\^_`|~";
    private static readonly string Brackets = "(){}[]<>";

    public override string RuleName => "Delete";
    public override string Description => Mode switch
    {
        DeleteMode.Combined => TextFirstInCombined ? "Delete (text then position)" : "Delete (position then text)",
        DeleteMode.CharacterRemove => "Remove selected text/characters",
        DeleteMode.TextRemove => string.IsNullOrEmpty(RemovePattern) ? "Remove selected text/characters" : $"Remove \"{RemovePattern}\"",
        _ => DeleteCurrentName ? "Delete current name" : $"Delete from {FromType} until {UntilType}"
    };

    public override string Execute(string fileName, RenFile file)
    {
        return Mode switch
        {
            DeleteMode.CharacterRemove => ExecuteTextRemove(fileName, file),
            DeleteMode.TextRemove => ExecuteTextRemove(fileName, file),
            DeleteMode.Combined => ExecuteCombined(fileName, file),
            _ => ExecutePositionDelete(fileName)
        };
    }

    private string ExecutePositionDelete(string fileName)
    {
        var (baseName, ext) = SplitFileName(fileName, SkipExtension);
        if (baseName.Length == 0) return fileName;
        if (DeleteCurrentName) return ext;

        string working = RightToLeft ? Reverse(baseName) : baseName;

        int start;
        if (FromType == DeleteFromType.Delimiter)
        {
            string d = RightToLeft ? Reverse(FromDelimiter) : FromDelimiter;
            int idx = working.IndexOf(d, StringComparison.Ordinal);
            if (idx < 0) return baseName + ext;
            start = LeaveDelimiter ? idx + d.Length : idx;
        }
        else
        {
            start = Math.Min(Math.Max(0, FromPosition - 1), working.Length);
        }

        int end;
        if (UntilType == DeleteUntilType.TillEnd)
        {
            end = working.Length;
        }
        else if (UntilType == DeleteUntilType.Delimiter)
        {
            string d = RightToLeft ? Reverse(UntilDelimiter) : UntilDelimiter;
            int idx = working.IndexOf(d, start, StringComparison.Ordinal);
            end = idx < 0 ? working.Length : (LeaveDelimiter ? idx : idx + d.Length);
        }
        else
        {
            end = Math.Min(start + UntilCount, working.Length);
        }

        if (start >= end) return baseName + ext;
        string result = working.Remove(start, end - start);
        if (RightToLeft) result = Reverse(result);
        return result + ext;
    }
    private string ExecuteCharacterRemove(string fileName)
    {
        var (baseName, ext) = SplitFileName(fileName, SkipExtension);
        if (baseName.Length == 0) return fileName;

        var stripChars = BuildStripSet();
        if (Where == StripWhere.Everywhere)
        {
            baseName = StripChars(baseName, stripChars);
        }
        else if (Where == StripWhere.Leading)
        {
            int i = 0;
            while (i < baseName.Length && ShouldStrip(baseName[i], stripChars))
                i++;
            baseName = baseName[i..];
        }
        else
        {
            int i = baseName.Length - 1;
            while (i >= 0 && ShouldStrip(baseName[i], stripChars))
                i--;
            baseName = baseName[..(i + 1)];
        }

        return baseName + ext;
    }
    private string ExecuteTextRemove(string fileName, RenFile file)
    {
        var result = fileName;
        if (!string.IsNullOrEmpty(RemovePattern))
        {
            var removeRule = new RemoveRule
            {
                Pattern = RemovePattern,
                Occurrence = RemoveOccurrence,
                CaseSensitive = RemoveCaseSensitive,
                WholeWordsOnly = RemoveWholeWordsOnly,
                SkipExtension = SkipExtension,
                UseWildcards = RemoveUseWildcards
            };
            result = removeRule.Execute(result, file);
        }

        if (HasStripSelection())
            result = ExecuteCharacterRemove(result);

        return result;
    }

    private string ExecuteCombined(string fileName, RenFile file)
    {
        if (TextFirstInCombined)
            return ExecutePositionDelete(ExecuteTextRemove(fileName, file));
        return ExecuteTextRemove(ExecutePositionDelete(fileName), file);
    }

    private bool HasStripSelection()
    {
        return StripEnglishLetters || StripDigits || StripSymbols || StripBrackets ||
               (StripUserDefined && !string.IsNullOrEmpty(UserDefinedChars)) || StripUnicodeRange;
    }

    private HashSet<char> BuildStripSet()
    {
        var set = new HashSet<char>();
        if (StripEnglishLetters)
        {
            foreach (var c in EnglishLetters)
            {
                set.Add(c);
                if (!CaseSensitive) set.Add(char.ToUpper(c));
            }
        }
        if (StripDigits) foreach (var c in Digits) set.Add(c);
        if (StripSymbols) foreach (var c in Symbols) set.Add(c);
        if (StripBrackets) foreach (var c in Brackets) set.Add(c);
        if (StripUserDefined)
        {
            foreach (var c in UserDefinedChars)
            {
                set.Add(c);
                if (!CaseSensitive)
                {
                    set.Add(char.ToLower(c));
                    set.Add(char.ToUpper(c));
                }
            }
        }
        return set;
    }

    private bool ShouldStrip(char c, HashSet<char> stripChars)
    {
        bool inSet = stripChars.Contains(c);
        if (StripUnicodeRange && IsInUnicodeRange(c)) inSet = true;
        return StripAllExceptSelected ? !inSet : inSet;
    }

    private string StripChars(string input, HashSet<char> stripChars)
    {
        var sb = new StringBuilder();
        foreach (var c in input)
            if (!ShouldStrip(c, stripChars)) sb.Append(c);
        return sb.ToString();
    }

    private bool IsInUnicodeRange(char c)
    {
        if (string.IsNullOrEmpty(UnicodeRange)) return false;
        var parts = UnicodeRange.Split('-');
        if (parts.Length != 2) return false;
        if (int.TryParse(parts[0], NumberStyles.HexNumber, null, out int lo) &&
            int.TryParse(parts[1], NumberStyles.HexNumber, null, out int hi))
            return c >= lo && c <= hi;
        return false;
    }

    private static string Reverse(string s)
    {
        var a = s.ToCharArray(); Array.Reverse(a); return new string(a);
    }
}
public enum DeleteMode { PositionDelete, CharacterRemove, TextRemove, Combined }

public enum DeleteFromType { Position, Delimiter }
public enum DeleteUntilType { Count, Delimiter, TillEnd }

// ═══════════════════════════════════════════════════════════════
// RemoveRule - 完整实现 WholeWordsOnly + UseWildcards
// ═══════════════════════════════════════════════════════════════
public class RemoveRule : RuleBase
{
    public string Pattern { get; set; } = "";
    public RemoveOccurrence Occurrence { get; set; } = RemoveOccurrence.All;
    public bool CaseSensitive { get; set; } = false;
    public bool WholeWordsOnly { get; set; } = false;
    public bool SkipExtension { get; set; } = true;
    public bool UseWildcards { get; set; } = false;

    public override string RuleName => "Remove";
    public override string Description => $"Remove \"{Pattern}\"";

    public override string Execute(string fileName, RenFile file)
    {
        if (string.IsNullOrEmpty(Pattern)) return fileName;
        var (baseName, ext) = SplitFileName(fileName, SkipExtension);
        string result;
        if (UseWildcards)
            result = RuleHelpers.RemoveWildcard(baseName, Pattern, Occurrence);
        else if (WholeWordsOnly)
            result = RuleHelpers.RemoveWholeWords(baseName, Pattern, CaseSensitive, Occurrence);
        else
            result = RemoveSimple(baseName);
        return result + ext;
    }

    private string RemoveSimple(string input)
    {
        var cmp = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var r = input;
        switch (Occurrence)
        {
            case RemoveOccurrence.All:
                int i; while ((i = r.IndexOf(Pattern, cmp)) >= 0) r = r.Remove(i, Pattern.Length); break;
            case RemoveOccurrence.First:
                var fi = r.IndexOf(Pattern, cmp); if (fi >= 0) r = r.Remove(fi, Pattern.Length); break;
            case RemoveOccurrence.Last:
                var li = r.LastIndexOf(Pattern, cmp); if (li >= 0) r = r.Remove(li, Pattern.Length); break;
        }
        return r;
    }
}

public enum RemoveOccurrence { All, First, Last }

// ═══════════════════════════════════════════════════════════════
// CaseRule - 完整实现 PreserveCase + ForceCase
// ═══════════════════════════════════════════════════════════════
public class CaseRule : RuleBase
{
    // 旧版模式（保留兼容）
    public CaseType CaseType { get; set; } = CaseType.Capitalize;
    public bool SkipExtension { get; set; } = true;
    public bool PreserveCase { get; set; } = false;
    public bool ForceCase { get; set; } = false;
    public string ForceCaseText { get; set; } = "";
    public bool ExtensionAlwaysLower { get; set; } = false;
    public bool ExtensionAlwaysUpper { get; set; } = false;

    // 新版三段式模式
    public bool UseSegmentedMode { get; set; } = false;
    public FirstLetterMode FirstLetterMode { get; set; } = FirstLetterMode.Keep;
    public RemainingLettersMode RemainingLettersMode { get; set; } = RemainingLettersMode.Keep;
    public ExtensionLetterMode ExtensionLetterMode { get; set; } = ExtensionLetterMode.Keep;

    // 拼音相关（新旧模式共用）
    public int PinyinChineseIndex { get; set; } = 1;
    public bool PinyinUpperCase { get; set; } = true;
    public PinyinInsertPosition PinyinInsertPosition { get; set; } = PinyinInsertPosition.Prefix; // 旧模式：前/后缀
    public PinyinFirstLetterAction PinyinFirstLetterAction { get; set; } = PinyinFirstLetterAction.InsertPrefix; // 新模式：插入/替换

    public override string RuleName => "Case";
    public override string Description => UseSegmentedMode
        ? $"Segmented: First={FirstLetterMode}, Rest={RemainingLettersMode}, Ext={ExtensionLetterMode}"
        : CaseType switch
        {
            CaseType.Capitalize => "Capitalize Every Word",
            CaseType.Lower => "lowercase",
            CaseType.Upper => "UPPERCASE",
            CaseType.Invert => "iNVERT cASE",
            CaseType.FirstLetter => "First letter capital",
            CaseType.Sentence => "Sentence case",
            CaseType.None => "(none of the above)",
            CaseType.PinyinFirstLetter => $"Pinyin first letter (#{PinyinChineseIndex})",
            _ => "Case"
        };

    public override string Execute(string fileName, RenFile file)
    {
        var (baseName, ext) = SplitFileName(fileName, SkipExtension);

        if (UseSegmentedMode)
        {
            var segmentedResult = ApplySegmentedCase(baseName);
            if (PreserveCase)
                segmentedResult = ApplyPreserve(baseName, segmentedResult);
            if (ForceCase && !string.IsNullOrEmpty(ForceCaseText))
                segmentedResult = ApplyForce(segmentedResult);

            ext = ApplySegmentedExtensionCase(ext);
            return segmentedResult + ext;
        }

        // 旧模式兼容
        var legacyResult = ApplyCase(baseName);
        if (PreserveCase && CaseType != CaseType.None)
            legacyResult = ApplyPreserve(baseName, legacyResult);
        if (ForceCase && !string.IsNullOrEmpty(ForceCaseText))
            legacyResult = ApplyForce(legacyResult);

        if (ExtensionAlwaysLower) ext = ext.ToLowerInvariant();
        else if (ExtensionAlwaysUpper) ext = ext.ToUpperInvariant();
        return legacyResult + ext;
    }

    private string ApplySegmentedCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return FirstLetterMode switch
        {
            FirstLetterMode.Keep => input[0] + ApplyRemainingLetters(input[1..]),
            FirstLetterMode.Upper => char.ToUpper(input[0]) + ApplyRemainingLetters(input[1..]),
            FirstLetterMode.Lower => char.ToLower(input[0]) + ApplyRemainingLetters(input[1..]),
            FirstLetterMode.PinyinFirstLetter => ApplyPinyinFirstLetter(input),
            _ => input[0] + ApplyRemainingLetters(input[1..])
        };
    }

    private string ApplyPinyinFirstLetter(string input)
    {
        var letter = Helpers.PinyinHelper.GetPinyinFirstLetter(input, Math.Max(1, PinyinChineseIndex), PinyinUpperCase);
        if (string.IsNullOrEmpty(letter))
            letter = PinyinUpperCase ? input[0].ToString().ToUpperInvariant() : input[0].ToString().ToLowerInvariant();

        return PinyinFirstLetterAction switch
        {
            PinyinFirstLetterAction.InsertPrefix => letter + ApplyRemainingLetters(input),
            PinyinFirstLetterAction.ReplaceFirstCharacter => letter + ApplyRemainingLetters(input.Length > 1 ? input[1..] : ""),
            _ => letter + ApplyRemainingLetters(input)
        };
    }

    private string ApplyRemainingLetters(string input) => RemainingLettersMode switch
    {
        RemainingLettersMode.Keep => input,
        RemainingLettersMode.Upper => input.ToUpperInvariant(),
        RemainingLettersMode.Lower => input.ToLowerInvariant(),
        RemainingLettersMode.CapitalizeWords => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower()),
        RemainingLettersMode.Invert => new string(input.Select(c => char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)).ToArray()),
        _ => input
    };

    private string ApplySegmentedExtensionCase(string ext) => ExtensionLetterMode switch
    {
        ExtensionLetterMode.Keep => ext,
        ExtensionLetterMode.Lower => ext.ToLowerInvariant(),
        ExtensionLetterMode.Upper => ext.ToUpperInvariant(),
        _ => ext
    };

    private string ApplyCase(string s) => CaseType switch
    {
        CaseType.Capitalize => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower()),
        CaseType.Lower => s.ToLowerInvariant(),
        CaseType.Upper => s.ToUpperInvariant(),
        CaseType.Invert => new string(s.Select(c => char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)).ToArray()),
        CaseType.FirstLetter => s.Length > 0 ? char.ToUpper(s[0]) + s[1..] : s,
        CaseType.Sentence => s.Length > 0 ? char.ToUpper(s[0]) + s[1..].ToLower() : s,
        CaseType.PinyinFirstLetter => ApplyLegacyPinyinCase(s),
        _ => s
    };

    private string ApplyLegacyPinyinCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var letter = Helpers.PinyinHelper.GetPinyinFirstLetter(s, Math.Max(1, PinyinChineseIndex), PinyinUpperCase);
        if (string.IsNullOrEmpty(letter)) return s;
        return PinyinInsertPosition == PinyinInsertPosition.Suffix ? s + letter : letter + s;
    }

    private static string ApplyPreserve(string orig, string transformed)
    {
        var ow = Regex.Split(orig, @"(\b)");
        var tw = Regex.Split(transformed, @"(\b)");
        if (ow.Length != tw.Length) return transformed;
        var sb = new StringBuilder();
        for (int i = 0; i < ow.Length; i++)
        {
            if (ow[i].Length > 1 && ow[i].All(c => !char.IsLetter(c) || char.IsUpper(c)) && ow[i].Any(char.IsLetter))
                sb.Append(ow[i]);
            else
                sb.Append(i < tw.Length ? tw[i] : "");
        }
        return sb.ToString();
    }

    private string ApplyForce(string input)
    {
        var result = input;
        foreach (var f in ForceCaseText.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var t = f.Trim();
            if (t.Length > 0) result = new Regex(Regex.Escape(t), RegexOptions.IgnoreCase).Replace(result, t);
        }
        return result;
    }
}

public enum CaseType { Capitalize, Lower, Upper, Invert, FirstLetter, Sentence, None, PinyinFirstLetter }

public enum PinyinInsertPosition { Prefix, Suffix }
public enum FirstLetterMode { Keep, Upper, Lower, PinyinFirstLetter }
public enum RemainingLettersMode { Keep, Upper, Lower, CapitalizeWords, Invert }
public enum ExtensionLetterMode { Keep, Lower, Upper }
public enum PinyinFirstLetterAction { InsertPrefix, ReplaceFirstCharacter }

// ═══════════════════════════════════════════════════════════════
// SerializeRule - 完整实现 Repeat/Reset/NumberingSystem
// ═══════════════════════════════════════════════════════════════
public class SerializeRule : RuleBase, IStatefulRule
{
    public int StartNumber { get; set; } = 1;
    public int Repeat { get; set; } = 1;
    public int Step { get; set; } = 1;
    public bool ResetEvery { get; set; } = false;
    public int ResetEveryCount { get; set; } = 1;
    public bool ResetIfFolderChanges { get; set; } = false;
    public bool ResetIfFileNameChanges { get; set; } = false;
    public bool PadToLength { get; set; } = false;
    public int PadToLengthValue { get; set; } = 1;
    public SerializePosition InsertWhere { get; set; } = SerializePosition.Prefix;
    public int PositionValue { get; set; } = 1;
    public bool SkipExtension { get; set; } = true;
    public string NumberingSystem { get; set; } = "Decimal";
    public string CustomNumberingSymbols { get; set; } = "";
    public string Prefix { get; set; } = "";
    public string Suffix { get; set; } = "";
    public int Padding { get; set; } = 3;
    public int Increment { get => Step; set => Step = value; }
    public SerializePosition Position { get => InsertWhere; set => InsertWhere = value; }

    private int _currentNumber;
    private int _repeatCounter;
    private int _fileCounter;
    private string _lastFolder = "";
    private string _lastFileName = "";

    public override string RuleName => "Serialize";
    public override string Description => $"Add number ({StartNumber}, +{Step})";

    public void Reset()
    {
        _currentNumber = StartNumber;
        _repeatCounter = 0;
        _fileCounter = 0;
        _lastFolder = "";
        _lastFileName = "";
    }

    public override string Execute(string fileName, RenFile file)
    {
        if (ResetIfFolderChanges && file.FolderPath != _lastFolder && _fileCounter > 0)
        { _currentNumber = StartNumber; _repeatCounter = 0; }
        if (ResetIfFileNameChanges && file.BaseName != _lastFileName && _fileCounter > 0)
        { _currentNumber = StartNumber; _repeatCounter = 0; }
        if (ResetEvery && ResetEveryCount > 0 && _fileCounter > 0 && _fileCounter % ResetEveryCount == 0)
        { _currentNumber = StartNumber; _repeatCounter = 0; }

        _lastFolder = file.FolderPath;
        _lastFileName = file.BaseName;
        _fileCounter++;

        var (baseName, ext) = SplitFileName(fileName, SkipExtension);
        int padLen = PadToLength ? PadToLengthValue : Padding;
        var numStr = FormatNum(_currentNumber, padLen);
        var serial = $"{Prefix}{numStr}{Suffix}";

        _repeatCounter++;
        if (_repeatCounter >= Math.Max(1, Repeat))
        { _currentNumber += Step; _repeatCounter = 0; }

        var result = InsertWhere switch
        {
            SerializePosition.Prefix => serial + baseName,
            SerializePosition.Suffix => baseName + serial,
            SerializePosition.Position => baseName.Insert(Math.Min(Math.Max(0, PositionValue - 1), baseName.Length), serial),
            SerializePosition.ReplaceCurrentName => serial,
            _ => serial + baseName
        };
        return result + ext;
    }

    private string FormatNum(int n, int pad)
    {
        bool neg = n < 0; int abs = Math.Abs(n);
        string s = NumberingSystem switch
        {
            "Hexadecimal" => abs.ToString("X"),
            "Octal" => Convert.ToString(abs, 8),
            "Binary" => Convert.ToString(abs, 2),
            "Custom" when CustomNumberingSymbols.Length >= 2 => ToCustom(abs, CustomNumberingSymbols),
            _ => abs.ToString()
        };
        s = s.PadLeft(pad, '0');
        return neg ? "-" + s : s;
    }

    private static string ToCustom(int n, string sym)
    {
        if (n == 0) return sym[0].ToString();
        int b = sym.Length;
        var sb = new StringBuilder();
        while (n > 0) { sb.Insert(0, sym[n % b]); n /= b; }
        return sb.ToString();
    }
}

public enum SerializePosition { Prefix, Suffix, Position, ReplaceCurrentName }

// ═══════════════════════════════════════════════════════════════
// RegexRule
// ═══════════════════════════════════════════════════════════════
public class RegexRule : RuleBase
{
    public string Expression { get; set; } = "";
    public string ReplaceText { get; set; } = "";
    public bool CaseSensitive { get; set; } = false;
    public bool SkipExtension { get; set; } = true;

    public override string RuleName => "Regular Expressions";
    public override string Description => $"Regex: {Expression} → {ReplaceText}";

    public override string Execute(string fileName, RenFile file)
    {
        if (string.IsNullOrEmpty(Expression)) return fileName;
        var (baseName, ext) = SplitFileName(fileName, SkipExtension);
        try
        {
            var opt = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            return new Regex(Expression, opt).Replace(baseName, ReplaceText) + ext;
        }
        catch { return fileName; }
    }
}

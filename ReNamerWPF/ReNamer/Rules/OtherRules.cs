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

    public override string RuleName => "Delete";
    public override string Description => DeleteCurrentName
        ? "Delete current name" : $"Delete from {FromType} until {UntilType}";

    public override string Execute(string fileName, RenFile file)
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

    private static string Reverse(string s)
    {
        var a = s.ToCharArray(); Array.Reverse(a); return new string(a);
    }
}

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
    public CaseType CaseType { get; set; } = CaseType.Capitalize;
    public bool SkipExtension { get; set; } = true;
    public bool PreserveCase { get; set; } = false;
    public bool ForceCase { get; set; } = false;
    public string ForceCaseText { get; set; } = "";
    public bool ExtensionAlwaysLower { get; set; } = false;
    public bool ExtensionAlwaysUpper { get; set; } = false;

    public override string RuleName => "Case";
    public override string Description => CaseType switch
    {
        CaseType.Capitalize => "Capitalize Every Word",
        CaseType.Lower => "lowercase",
        CaseType.Upper => "UPPERCASE",
        CaseType.Invert => "iNVERT cASE",
        CaseType.FirstLetter => "First letter capital",
        CaseType.Sentence => "Sentence case",
        CaseType.None => "(none of the above)",
        _ => "Case"
    };

    public override string Execute(string fileName, RenFile file)
    {
        var (baseName, ext) = SplitFileName(fileName, SkipExtension);
        var result = ApplyCase(baseName);

        if (PreserveCase && CaseType != CaseType.None)
            result = ApplyPreserve(baseName, result);
        if (ForceCase && !string.IsNullOrEmpty(ForceCaseText))
            result = ApplyForce(result);

        if (ExtensionAlwaysLower) ext = ext.ToLowerInvariant();
        else if (ExtensionAlwaysUpper) ext = ext.ToUpperInvariant();
        return result + ext;
    }

    private string ApplyCase(string s) => CaseType switch
    {
        CaseType.Capitalize => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower()),
        CaseType.Lower => s.ToLowerInvariant(),
        CaseType.Upper => s.ToUpperInvariant(),
        CaseType.Invert => new string(s.Select(c => char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)).ToArray()),
        CaseType.FirstLetter => s.Length > 0 ? char.ToUpper(s[0]) + s[1..] : s,
        CaseType.Sentence => s.Length > 0 ? char.ToUpper(s[0]) + s[1..].ToLower() : s,
        _ => s
    };

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

public enum CaseType { Capitalize, Lower, Upper, Invert, FirstLetter, Sentence, None }

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
// ExtensionRule - 完整实现 RemoveDuplicate/DetectBinSign
// ═══════════════════════════════════════════════════════════════
public class ExtensionRule : RuleBase
{
    public bool NewExtensionEnabled { get; set; } = true;
    public string NewExtension { get; set; } = "";
    public bool AppendToOriginal { get; set; } = false;
    public bool DetectBinarySignature { get; set; } = false;
    public bool RemoveDuplicateExtensions { get; set; } = false;
    public bool CaseSensitive { get; set; } = false;
    public ExtensionAction Action { get; set; } = ExtensionAction.Replace;

    private static readonly Dictionary<string, string> Signatures = new()
    {
        {"89504E47",".png"},{"FFD8FF",".jpg"},{"47494638",".gif"},{"25504446",".pdf"},
        {"504B0304",".zip"},{"52617221",".rar"},{"D0CF11E0",".doc"},{"424D",".bmp"},
        {"49443303",".mp3"},{"1F8B08",".gz"},
    };

    public override string RuleName => "Extension";
    public override string Description => NewExtensionEnabled ? $"Change extension to \"{NewExtension}\"" : "Extension rule";

    public override string Execute(string fileName, RenFile file)
    {
        string result = fileName;
        if (DetectBinarySignature && File.Exists(file.FullPath))
        {
            var det = DetectSig(file.FullPath);
            if (det != null) return Path.GetFileNameWithoutExtension(result) + det;
        }
        if (NewExtensionEnabled)
        {
            var bn = Path.GetFileNameWithoutExtension(result);
            var ext = NewExtension.StartsWith('.') ? NewExtension : "." + NewExtension;
            result = AppendToOriginal ? result + ext : bn + ext;
        }
        if (RemoveDuplicateExtensions) result = RemoveDupExts(result);
        return result;
    }

    private string RemoveDupExts(string fn)
    {
        var exts = new List<string>(); string rem = fn;
        while (true)
        {
            var e = Path.GetExtension(rem);
            if (string.IsNullOrEmpty(e)) break;
            exts.Insert(0, e); rem = rem[..^e.Length];
        }
        var cmp = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var uniq = new List<string>();
        foreach (var e in exts) { if (!uniq.Any(u => string.Equals(u, e, cmp))) uniq.Add(e); }
        return rem + string.Concat(uniq);
    }

    private static string? DetectSig(string path)
    {
        try
        {
            var buf = new byte[8];
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            int n = fs.Read(buf, 0, 8); if (n < 2) return null;
            var hex = BitConverter.ToString(buf, 0, n).Replace("-", "");
            foreach (var s in Signatures) { if (hex.StartsWith(s.Key, StringComparison.OrdinalIgnoreCase)) return s.Value; }
        }
        catch { }
        return null;
    }
}

public enum ExtensionAction { Replace, Add, Remove, Lower, Upper }

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

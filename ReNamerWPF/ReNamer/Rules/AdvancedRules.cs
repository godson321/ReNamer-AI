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
// PaddingRule - 数字零填充 + 文本填充
// ═══════════════════════════════════════════════════════════════
public class PaddingRule : RuleBase
{
    public bool AddZeroPadding { get; set; } = false;
    public int ZeroPaddingLength { get; set; } = 1;
    public bool RemoveZeroPadding { get; set; } = false;
    public bool AddTextPadding { get; set; } = false;
    public int TextPaddingLength { get; set; } = 1;
    public string PaddingCharacters { get; set; } = " ";
    public PaddingPosition TextPaddingPosition { get; set; } = PaddingPosition.Left;
    public bool SkipExtension { get; set; } = true;

    public override string RuleName => "Padding";
    public override string Description
    {
        get
        {
            var parts = new List<string>();
            if (AddZeroPadding) parts.Add($"Zero pad to {ZeroPaddingLength}");
            if (RemoveZeroPadding) parts.Add("Remove zero padding");
            if (AddTextPadding) parts.Add($"Text pad to {TextPaddingLength}");
            return parts.Count > 0 ? string.Join(", ", parts) : "Padding";
        }
    }

    public override string Execute(string fileName, RenFile file)
    {
        var (baseName, ext) = SplitFileName(fileName, SkipExtension, file.IsFolder);

        if (AddZeroPadding)
            baseName = Regex.Replace(baseName, @"\d+", m => m.Value.PadLeft(ZeroPaddingLength, '0'));
        else if (RemoveZeroPadding)
            baseName = Regex.Replace(baseName, @"0+(\d)", m => m.Groups[1].Value);

        if (AddTextPadding && !string.IsNullOrEmpty(PaddingCharacters))
        {
            while (baseName.Length < TextPaddingLength)
            {
                if (TextPaddingPosition == PaddingPosition.Left)
                    baseName = PaddingCharacters + baseName;
                else
                    baseName = baseName + PaddingCharacters;
            }
            if (baseName.Length > TextPaddingLength && TextPaddingLength > 0)
            {
                // Trim excess only from padding side
            }
        }

        return baseName + ext;
    }
}

public enum PaddingPosition { Left, Right }

// ═══════════════════════════════════════════════════════════════
// StripRule - 移除指定类型的字符
// ═══════════════════════════════════════════════════════════════
public class StripRule : RuleBase
{
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
    public bool SkipExtension { get; set; } = true;

    private static readonly string EnglishLetters = "abcdefghijklmnopqrstuvwxyz";
    private static readonly string Digits = "1234567890";
    private static readonly string Symbols = "!\"#$%&'*+,-./:;=?@\\^_`|~";
    private static readonly string Brackets = "(){}[]<>";

    public override string RuleName => "Strip";
    public override string Description
    {
        get
        {
            var parts = new List<string>();
            if (StripEnglishLetters) parts.Add("letters");
            if (StripDigits) parts.Add("digits");
            if (StripSymbols) parts.Add("symbols");
            if (StripBrackets) parts.Add("brackets");
            if (StripUserDefined) parts.Add("custom");
            if (StripUnicodeRange) parts.Add("unicode");
            var prefix = StripAllExceptSelected ? "Keep " : "Strip ";
            return prefix + (parts.Count > 0 ? string.Join(", ", parts) : "nothing");
        }
    }

    public override string Execute(string fileName, RenFile file)
    {
        var (baseName, ext) = SplitFileName(fileName, SkipExtension, file.IsFolder);
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
        else // Trailing
        {
            int i = baseName.Length - 1;
            while (i >= 0 && ShouldStrip(baseName[i], stripChars))
                i--;
            baseName = baseName[..(i + 1)];
        }

        return baseName + ext;
    }

    private HashSet<char> BuildStripSet()
    {
        var set = new HashSet<char>();
        if (StripEnglishLetters)
        {
            foreach (var c in EnglishLetters) { set.Add(c); if (!CaseSensitive) set.Add(char.ToUpper(c)); }
        }
        if (StripDigits) foreach (var c in Digits) set.Add(c);
        if (StripSymbols) foreach (var c in Symbols) set.Add(c);
        if (StripBrackets) foreach (var c in Brackets) set.Add(c);
        if (StripUserDefined) foreach (var c in UserDefinedChars) { set.Add(c); if (!CaseSensitive) { set.Add(char.ToLower(c)); set.Add(char.ToUpper(c)); } }
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
}

public enum StripWhere { Everywhere, Leading, Trailing }

// ═══════════════════════════════════════════════════════════════
// CleanUpRule - 清理文件名
// ═══════════════════════════════════════════════════════════════
public class CleanUpRule : RuleBase
{
    // Brackets stripping
    public bool StripRoundBrackets { get; set; } = false;
    public bool StripSquareBrackets { get; set; } = false;
    public bool StripCurlyBrackets { get; set; } = false;
    // Characters to replace with spaces
    public bool ReplaceDotWithSpace { get; set; } = false;
    public bool ReplaceCommaWithSpace { get; set; } = false;
    public bool ReplaceUnderscoreWithSpace { get; set; } = false;
    public bool ReplacePlusWithSpace { get; set; } = false;
    public bool ReplaceHyphenWithSpace { get; set; } = false;
    public bool ReplaceWeb20WithSpace { get; set; } = false;
    public bool SkipVersionNumbers { get; set; } = false;
    // Space handling
    public bool FixSpaces { get; set; } = true;
    public bool NormalizeUnicodeSpaces { get; set; } = true;
    // Unicode
    public bool StripUnicodeEmoji { get; set; } = false;
    public bool StripUnicodeMarks { get; set; } = false;
    // Other
    public bool InsertSpaceBeforeCapitals { get; set; } = false;
    public bool PrepareForSharePoint { get; set; } = false;
    public bool SkipExtension { get; set; } = true;

    public override string RuleName => "Clean Up";
    public override string Description => "Clean up file names";

    public override string Execute(string fileName, RenFile file)
    {
        var (baseName, ext) = SplitFileName(fileName, SkipExtension, file.IsFolder);
        var result = baseName;

        // Strip bracket contents
        if (StripRoundBrackets) result = Regex.Replace(result, @"\([^)]*\)", "");
        if (StripSquareBrackets) result = Regex.Replace(result, @"\[[^\]]*\]", "");
        if (StripCurlyBrackets) result = Regex.Replace(result, @"\{[^}]*\}", "");

        // Replace characters with spaces
        if (ReplaceDotWithSpace)
        {
            if (SkipVersionNumbers)
                result = Regex.Replace(result, @"(?<!\d)\.(?!\d)", " ");
            else
                result = result.Replace('.', ' ');
        }
        if (ReplaceCommaWithSpace) result = result.Replace(',', ' ');
        if (ReplaceUnderscoreWithSpace) result = result.Replace('_', ' ');
        if (ReplacePlusWithSpace) result = result.Replace('+', ' ');
        if (ReplaceHyphenWithSpace) result = result.Replace('-', ' ');
        if (ReplaceWeb20WithSpace) result = result.Replace("%20", " ");

        // Unicode normalization
        if (NormalizeUnicodeSpaces)
        {
            // Replace various unicode spaces with standard space
            result = Regex.Replace(result, @"[\u00A0\u2000-\u200B\u202F\u205F\u3000\uFEFF]", " ");
        }

        // Strip unicode marks
        if (StripUnicodeMarks)
        {
            var normalized = result.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            result = sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // Strip emoji (basic emoji ranges)
        if (StripUnicodeEmoji)
        {
            result = Regex.Replace(result, @"[\u2600-\u27BF\uD83C-\uDBFF\uDC00-\uDFFF\uFE0F\u200D]", "");
        }

        // Insert space before capitals (CamelCase)
        if (InsertSpaceBeforeCapitals)
        {
            result = Regex.Replace(result, @"(?<=[a-z])(?=[A-Z])", " ");
        }

        // Fix spaces
        if (FixSpaces)
        {
            result = Regex.Replace(result, @"\s{2,}", " ").Trim();
        }

        // SharePoint: remove special chars #%&*:<>?/{|}~
        if (PrepareForSharePoint)
        {
            result = Regex.Replace(result, @"[#%&*:<>?/\\{|}~]", "");
        }

        return result + ext;
    }
}

// ═══════════════════════════════════════════════════════════════
// TransliterateRule - 字母表音译转写
// ═══════════════════════════════════════════════════════════════
public class TransliterateRule : RuleBase
{
    public string Alphabet { get; set; } = "";
    public bool DirectionForward { get; set; } = true;
    public bool AutoCaseAdjustment { get; set; } = true;
    public bool SkipExtension { get; set; } = true;

    // 内置简体拉丁音译表
    private static readonly string DefaultAlphabet =
        "ä=ae\nö=oe\nü=ue\nÄ=Ae\nÖ=Oe\nÜ=Ue\nß=ss\n" +
        "á=a\né=e\ní=i\nó=o\nú=u\nà=a\nè=e\nì=i\nò=o\nù=u\n" +
        "â=a\nê=e\nî=i\nô=o\nû=u\nã=a\nñ=n\nç=c\n" +
        "ą=a\nć=c\nę=e\nł=l\nń=n\nś=s\nź=z\nż=z\n" +
        "č=c\nď=d\ně=e\nň=n\nř=r\nš=s\nť=t\nž=z\nů=u\n" +
        "ø=o\nå=a\næ=ae";

    public override string RuleName => "Transliterate";
    public override string Description => DirectionForward ? "Transliterate (forward)" : "Transliterate (backward)";

    public override string Execute(string fileName, RenFile file)
    {
        var (baseName, ext) = SplitFileName(fileName, SkipExtension, file.IsFolder);
        var alphabet = string.IsNullOrWhiteSpace(Alphabet) ? DefaultAlphabet : Alphabet;
        var pairs = ParseAlphabet(alphabet);

        var result = baseName;
        foreach (var (from, to) in pairs)
        {
            var source = DirectionForward ? from : to;
            var target = DirectionForward ? to : from;
            if (string.IsNullOrEmpty(source)) continue;

            result = result.Replace(source, target);
            if (AutoCaseAdjustment && source.Length == 1 && char.IsLetter(source[0]))
            {
                var upperSource = source.ToUpper();
                var upperTarget = target.Length > 0
                    ? char.ToUpper(target[0]) + target[1..]
                    : target;
                if (upperSource != source)
                    result = result.Replace(upperSource, upperTarget);
            }
        }

        return result + ext;
    }

    private static List<(string from, string to)> ParseAlphabet(string alphabet)
    {
        var pairs = new List<(string, string)>();
        foreach (var line in alphabet.Split('\n', '\r'))
        {
            var trimmed = line.Trim();
            var eq = trimmed.IndexOf('=');
            if (eq > 0)
                pairs.Add((trimmed[..eq], trimmed[(eq + 1)..]));
        }
        return pairs;
    }
}

// ═══════════════════════════════════════════════════════════════
// RearrangeRule - 文件名分段重排
// ═══════════════════════════════════════════════════════════════
public class RearrangeRule : RuleBase
{
    public RearrangeSplitMode SplitMode { get; set; } = RearrangeSplitMode.Delimiters;
    public string Delimiters { get; set; } = " - ";
    public string NewPattern { get; set; } = "$2 - $1";
    public bool SkipExtension { get; set; } = true;
    public bool RightToLeft { get; set; } = false;

    public override string RuleName => "Rearrange";
    public override string Description => $"Rearrange: \"{NewPattern}\"";

    public override string Execute(string fileName, RenFile file)
    {
        if (string.IsNullOrEmpty(NewPattern)) return fileName;
        var (baseName, ext) = SplitFileName(fileName, SkipExtension, file.IsFolder);

        string[] parts;
        if (SplitMode == RearrangeSplitMode.Positions)
        {
            parts = SplitByPositions(baseName);
        }
        else if (SplitMode == RearrangeSplitMode.ExactPattern)
        {
            parts = SplitByExactPattern(baseName);
        }
        else
        {
            parts = SplitByDelimiters(baseName);
        }

        if (parts.Length == 0) return fileName;

        // Apply RightToLeft if enabled
        if (RightToLeft)
        {
            Array.Reverse(parts);
        }

        var result = NewPattern;
        // Replace $-N references (from end)
        for (int i = 1; i <= parts.Length; i++)
        {
            result = result.Replace($"$-{i}", i <= parts.Length ? parts[parts.Length - i] : "");
        }
        // Replace $N references
        for (int i = 1; i <= parts.Length; i++)
        {
            result = result.Replace($"${i}", parts[i - 1]);
        }
        // $0 = original name
        result = result.Replace("$0", baseName);

        result = RuleHelpers.ResolveMetaTags(result, file);
        return result + ext;
    }

    private string[] SplitByDelimiters(string input)
    {
        if (string.IsNullOrEmpty(Delimiters)) return new[] { input };
        // Each char in Delimiters string acts as a delimiter
        var delims = Delimiters.Split('|');
        if (delims.Length == 1 && Delimiters.Length > 0)
        {
            return input.Split(new[] { Delimiters }, StringSplitOptions.None);
        }
        return input.Split(delims, StringSplitOptions.None);
    }

    private string[] SplitByPositions(string input)
    {
        if (string.IsNullOrEmpty(Delimiters)) return new[] { input };
        var positions = new List<int>();
        foreach (var p in Delimiters.Split(',', ';', '|'))
            if (int.TryParse(p.Trim(), out int pos)) positions.Add(pos);
        positions.Sort();
        var parts = new List<string>();
        int prev = 0;
        foreach (var pos in positions)
        {
            if (pos > prev && pos <= input.Length)
            { parts.Add(input[prev..pos]); prev = pos; }
        }
        if (prev < input.Length) parts.Add(input[prev..]);
        return parts.ToArray();
    }

    private string[] SplitByExactPattern(string input)
    {
        if (string.IsNullOrEmpty(Delimiters)) return new[] { input };
        return input.Split(new[] { Delimiters }, StringSplitOptions.None);
    }
}

public enum RearrangeSplitMode { Delimiters, Positions, ExactPattern }

// ═══════════════════════════════════════════════════════════════
// ReformatDateRule - 日期格式化
// ═══════════════════════════════════════════════════════════════
public class ReformatDateRule : RuleBase
{
    public string SourceFormat { get; set; } = "yyyy-MM-dd";
    public string TargetFormat { get; set; } = "dd.MM.yyyy";
    public bool WholeWordsOnly { get; set; } = true;
    public bool SkipExtension { get; set; } = true;
    public bool UseCustomShortMonths { get; set; } = false;
    public string CustomShortMonths { get; set; } = "";
    public bool UseCustomLongMonths { get; set; } = false;
    public string CustomLongMonths { get; set; } = "";
    public bool AdjustTime { get; set; } = false;
    public int AdjustTimeBy { get; set; } = 0;
    public string AdjustTimePart { get; set; } = "Hours";

    public override string RuleName => "Reformat Date";
    public override string Description => $"Date: {SourceFormat} → {TargetFormat}";

    public override string Execute(string fileName, RenFile file)
    {
        if (string.IsNullOrEmpty(SourceFormat) || string.IsNullOrEmpty(TargetFormat))
            return fileName;

        var (baseName, ext) = SplitFileName(fileName, SkipExtension, file.IsFolder);

        // Support multiple source formats separated by |
        var formats = SourceFormat.Split('|').Select(f => f.Trim()).Where(f => f.Length > 0).ToArray();

        string result = baseName;
        foreach (var fmt in formats)
        {
            result = TryReformatDate(result, fmt, TargetFormat);
        }

        return result + ext;
    }

    private string TryReformatDate(string input, string sourceFormat, string targetFormat)
    {
        // Build regex from date format
        var pattern = BuildDatePattern(sourceFormat);
        if (string.IsNullOrEmpty(pattern)) return input;

        var boundary = WholeWordsOnly ? @"\b" : "";
        var regex = new Regex(boundary + pattern + boundary);

        return regex.Replace(input, m =>
        {
            if (DateTime.TryParseExact(m.Value, sourceFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt))
            {
                if (AdjustTime && AdjustTimeBy != 0)
                {
                    dt = AdjustTimePart switch
                    {
                        "Hours" => dt.AddHours(AdjustTimeBy),
                        "Minutes" => dt.AddMinutes(AdjustTimeBy),
                        "Seconds" => dt.AddSeconds(AdjustTimeBy),
                        "Days" => dt.AddDays(AdjustTimeBy),
                        "Months" => dt.AddMonths(AdjustTimeBy),
                        "Years" => dt.AddYears(AdjustTimeBy),
                        _ => dt
                    };
                }
                return dt.ToString(targetFormat, CultureInfo.InvariantCulture);
            }
            return m.Value;
        });
    }

    private static string BuildDatePattern(string format)
    {
        var sb = new StringBuilder();
        int i = 0;
        while (i < format.Length)
        {
            char c = format[i];
            int count = 1;
            while (i + count < format.Length && format[i + count] == c) count++;

            switch (c)
            {
                case 'y': sb.Append(@"\d{" + count + "}"); break;
                case 'M': sb.Append(count >= 3 ? @"\w+" : @"\d{1," + count + "}"); break;
                case 'd': sb.Append(count >= 3 ? @"\w+" : @"\d{1," + count + "}"); break;
                case 'H': case 'h': case 'm': case 's':
                    sb.Append(@"\d{1," + count + "}"); break;
                default:
                    sb.Append(Regex.Escape(new string(c, count))); break;
            }
            i += count;
        }
        return sb.ToString();
    }
}

// ═══════════════════════════════════════════════════════════════
// RandomizeRule - 随机字符串
// ═══════════════════════════════════════════════════════════════
public class RandomizeRule : RuleBase, IStatefulRule
{
    public int Length { get; set; } = 8;
    public bool Unique { get; set; } = true;
    public bool UseDigits { get; set; } = true;
    public bool UseEnglishLetters { get; set; } = true;
    public bool UseUserDefined { get; set; } = false;
    public string UserDefinedChars { get; set; } = "";
    public RandomizePosition InsertWhere { get; set; } = RandomizePosition.Prefix;
    public int PositionValue { get; set; } = 1;
    public bool SkipExtension { get; set; } = true;

    private static readonly Random _rng = new();
    private readonly HashSet<string> _usedStrings = new();

    public override string RuleName => "Randomize";
    public override string Description => $"Random {Length} chars ({InsertWhere})";

    public void Reset() => _usedStrings.Clear();

    public override string Execute(string fileName, RenFile file)
    {
        var (baseName, ext) = SplitFileName(fileName, SkipExtension, file.IsFolder);
        var charset = BuildCharset();
        if (charset.Length == 0) return fileName;

        string rand;
        int maxAttempts = 1000;
        do
        {
            rand = GenerateRandom(charset, Length);
            maxAttempts--;
        } while (Unique && _usedStrings.Contains(rand) && maxAttempts > 0);

        _usedStrings.Add(rand);

        var result = InsertWhere switch
        {
            RandomizePosition.Prefix => rand + baseName,
            RandomizePosition.Suffix => baseName + rand,
            RandomizePosition.Position => baseName.Insert(
                Math.Min(Math.Max(0, PositionValue - 1), baseName.Length), rand),
            RandomizePosition.ReplaceCurrentName => rand,
            _ => rand + baseName
        };

        return result + ext;
    }

    private string BuildCharset()
    {
        var sb = new StringBuilder();
        if (UseDigits) sb.Append("0123456789");
        if (UseEnglishLetters) sb.Append("abcdefghijklmnopqrstuvwxyz");
        if (UseUserDefined && !string.IsNullOrEmpty(UserDefinedChars)) sb.Append(UserDefinedChars);
        return sb.ToString();
    }

    private static string GenerateRandom(string charset, int length)
    {
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
            sb.Append(charset[_rng.Next(charset.Length)]);
        return sb.ToString();
    }
}

public enum RandomizePosition { Prefix, Suffix, Position, ReplaceCurrentName }

// ═══════════════════════════════════════════════════════════════
// UserInputRule - 用户逐行输入新文件名
// ═══════════════════════════════════════════════════════════════
public class UserInputRule : RuleBase, IStatefulRule
{
    public string InputText { get; set; } = "";
    public UserInputMode Mode { get; set; } = UserInputMode.Replace;
    public bool SkipExtension { get; set; } = true;

    private string[]? _lines;
    private int _lineIndex;

    public override string RuleName => "User Input";
    public override string Description => Mode switch
    {
        UserInputMode.InsertBefore => "User Input (prefix)",
        UserInputMode.InsertAfter => "User Input (suffix)",
        _ => "User Input (replace)"
    };

    public void Reset()
    {
        _lines = InputText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        _lineIndex = 0;
    }

    public override string Execute(string fileName, RenFile file)
    {
        if (_lines == null) Reset();
        if (_lines == null || _lines.Length == 0) return fileName;

        var line = _lineIndex < _lines.Length ? _lines[_lineIndex] : "";
        _lineIndex++;

        if (string.IsNullOrEmpty(line)) return fileName;

        var (baseName, ext) = SplitFileName(fileName, SkipExtension, file.IsFolder);
        return Mode switch
        {
            UserInputMode.InsertBefore => line + baseName + ext,
            UserInputMode.InsertAfter => baseName + line + ext,
            UserInputMode.Replace => line + ext,
            _ => fileName
        };
    }
}

public enum UserInputMode { Replace, InsertBefore, InsertAfter }

// ═══════════════════════════════════════════════════════════════
// MappingRule - 映射表重命名
// ═══════════════════════════════════════════════════════════════
public class MappingRule : RuleBase, IStatefulRule
{
    public List<MappingEntry> Mappings { get; set; } = new();
    public bool AllowReuse { get; set; } = false;
    public bool PartialMatch { get; set; } = false;
    public bool InverseMapping { get; set; } = false;
    public bool CaseSensitive { get; set; } = false;
    public bool SkipExtension { get; set; } = false; // 注意：Mapping 默认不忽略扩展名

    private readonly HashSet<int> _usedMappings = new();

    public override string RuleName => "Mapping";
    public override string Description => $"Mapping ({Mappings.Count} entries)";

    public void Reset() => _usedMappings.Clear();

    public override string Execute(string fileName, RenFile file)
    {
        var (baseName, ext) = SplitFileName(fileName, SkipExtension, file.IsFolder);
        var cmp = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        for (int i = 0; i < Mappings.Count; i++)
        {
            if (!AllowReuse && _usedMappings.Contains(i)) continue;

            var mapping = Mappings[i];
            var match = InverseMapping ? mapping.NewName : mapping.Match;
            var replace = InverseMapping ? mapping.Match : mapping.NewName;

            if (string.IsNullOrEmpty(match)) continue;

            if (PartialMatch)
            {
                if (baseName.Contains(match, cmp))
                {
                    baseName = baseName.Replace(match, replace, cmp);
                    _usedMappings.Add(i);
                    if (!AllowReuse) break;
                }
            }
            else
            {
                if (string.Equals(baseName, match, cmp))
                {
                    baseName = replace;
                    _usedMappings.Add(i);
                    break;
                }
            }
        }

        return baseName + ext;
    }
}

public class MappingEntry
{
    public string Match { get; set; } = "";
    public string NewName { get; set; } = "";
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ReNamer.Services;

public static class ChineseNumberConversionService
{
    private static readonly Regex CandidateRegex = new(
        "[零〇○一二三四五六七八九十百千万亿兩两壹贰貳叁參肆伍陆陸柒捌玖拾佰仟萬億廿卅卌]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<char> NormalizedDigits =
    [
        '零', '一', '二', '三', '四', '五', '六', '七', '八', '九'
    ];

    private static readonly HashSet<char> SmallUnits =
    [
        '十', '百', '千'
    ];

    private static readonly HashSet<char> LargeUnits =
    [
        '万', '亿'
    ];

    private static readonly HashSet<char> AmbiguousNeighborChars =
    [
        '几', '多', '来', '余', '半'
    ];

    private static readonly string[] SectionUnits = ["千", "百", "十", ""];
    private static readonly string[] GroupUnits = ["", "万", "亿", "万亿"];
    private static readonly string[] DigitTexts = ["零", "一", "二", "三", "四", "五", "六", "七", "八", "九"];

    public static string ConvertChineseNumbersToArabic(string input, bool allowLooseForms)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return CandidateRegex.Replace(input, match =>
        {
            if (!ShouldConvertMatch(input, match.Index, match.Length))
                return match.Value;

            return TryConvertToken(match.Value, allowLooseForms, out var replacement)
                ? replacement
                : match.Value;
        });
    }

    private static bool ShouldConvertMatch(string input, int index, int length)
    {
        if (StartsWithAt(input, index + length, "分之") ||
            EndsWithAt(input, index, "分之") ||
            EndsWithAt(input, index, "百分之"))
        {
            return false;
        }

        if (index > 0 && AmbiguousNeighborChars.Contains(input[index - 1]))
            return false;

        var endIndex = index + length;
        if (endIndex < input.Length && AmbiguousNeighborChars.Contains(input[endIndex]))
            return false;

        return true;
    }

    private static bool TryConvertToken(string token, bool allowLooseForms, out string replacement)
    {
        replacement = token;
        var normalized = NormalizeToken(token, allowLooseForms);
        if (string.IsNullOrEmpty(normalized))
            return false;

        if (IsDigitSequence(normalized))
        {
            replacement = ConvertDigitSequence(normalized);
            return true;
        }

        if (!ContainsUnits(normalized))
            return false;

        if (TryParseStrictNormalized(normalized, out var strictValue))
        {
            replacement = strictValue.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        if (!allowLooseForms)
            return false;

        if (TryExpandLooseNormalized(normalized, out var expanded) &&
            TryParseStrictNormalized(expanded, out var looseValue))
        {
            replacement = looseValue.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        return false;
    }

    private static string NormalizeToken(string token, bool allowLooseForms)
    {
        var builder = new StringBuilder(token.Length * 2);
        foreach (var c in token)
        {
            switch (c)
            {
                case '零':
                case '〇':
                case '○':
                    builder.Append('零');
                    break;
                case '一':
                case '壹':
                    builder.Append('一');
                    break;
                case '二':
                case '两':
                case '兩':
                case '贰':
                case '貳':
                    builder.Append('二');
                    break;
                case '三':
                case '叁':
                case '參':
                    builder.Append('三');
                    break;
                case '四':
                case '肆':
                    builder.Append('四');
                    break;
                case '五':
                case '伍':
                    builder.Append('五');
                    break;
                case '六':
                case '陆':
                case '陸':
                    builder.Append('六');
                    break;
                case '七':
                case '柒':
                    builder.Append('七');
                    break;
                case '八':
                case '捌':
                    builder.Append('八');
                    break;
                case '九':
                case '玖':
                    builder.Append('九');
                    break;
                case '十':
                case '拾':
                    builder.Append('十');
                    break;
                case '百':
                case '佰':
                    builder.Append('百');
                    break;
                case '千':
                case '仟':
                    builder.Append('千');
                    break;
                case '万':
                case '萬':
                    builder.Append('万');
                    break;
                case '亿':
                case '億':
                    builder.Append('亿');
                    break;
                case '廿' when allowLooseForms:
                    builder.Append("二十");
                    break;
                case '卅' when allowLooseForms:
                    builder.Append("三十");
                    break;
                case '卌' when allowLooseForms:
                    builder.Append("四十");
                    break;
                default:
                    return string.Empty;
            }
        }

        return builder.ToString();
    }

    private static bool IsDigitSequence(string normalized)
    {
        foreach (var c in normalized)
        {
            if (!NormalizedDigits.Contains(c))
                return false;
        }

        return normalized.Length > 0;
    }

    private static bool ContainsUnits(string normalized)
    {
        foreach (var c in normalized)
        {
            if (SmallUnits.Contains(c) || LargeUnits.Contains(c))
                return true;
        }

        return false;
    }

    private static string ConvertDigitSequence(string normalized)
    {
        var builder = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
            builder.Append(MapDigit(c));
        return builder.ToString();
    }

    private static bool TryParseStrictNormalized(string normalized, out long value)
    {
        value = 0;
        if (!TryParseUnitNumber(normalized, out value))
            return false;

        var canonical = ToChinese(value);
        if (normalized == canonical)
            return true;

        return NormalizeStrictEquivalent(normalized) == canonical;
    }

    private static string NormalizeStrictEquivalent(string normalized)
    {
        return normalized.StartsWith("一十", StringComparison.Ordinal)
            ? normalized[1..]
            : normalized;
    }

    private static bool TryExpandLooseNormalized(string normalized, out string expanded)
    {
        expanded = string.Empty;
        if (normalized.Length < 2)
            return false;

        var trailingChar = normalized[^1];
        if (!NormalizedDigits.Contains(trailingChar) || trailingChar == '零')
            return false;

        if (normalized[^2] == '零')
            return false;

        var lastUnit = GetLastExplicitUnit(normalized);
        var impliedUnit = GetLooseImpliedUnit(lastUnit);
        if (string.IsNullOrEmpty(impliedUnit))
            return false;

        expanded = normalized[..^1] + trailingChar + impliedUnit;
        return true;
    }

    private static long GetLastExplicitUnit(string normalized)
    {
        for (var i = normalized.Length - 1; i >= 0; i--)
        {
            var c = normalized[i];
            if (SmallUnits.Contains(c) || LargeUnits.Contains(c))
                return MapUnit(c);
        }

        return 0;
    }

    private static string GetLooseImpliedUnit(long lastUnit)
    {
        return lastUnit switch
        {
            100_000_000 => "千万",
            10_000 => "千",
            1_000 => "百",
            100 => "十",
            _ => string.Empty
        };
    }

    private static bool TryParseUnitNumber(string normalized, out long value)
    {
        value = 0;
        if (string.IsNullOrEmpty(normalized))
            return false;

        long total = 0;
        long section = 0;
        int pendingDigit = -1;
        long lastLargeUnit = long.MaxValue;
        long lastSmallUnit = 10_000;
        var hasUnit = false;

        foreach (var c in normalized)
        {
            if (NormalizedDigits.Contains(c))
            {
                pendingDigit = MapDigit(c);
                continue;
            }

            if (SmallUnits.Contains(c))
            {
                hasUnit = true;
                var unit = MapUnit(c);
                if (unit >= lastSmallUnit)
                    return false;

                if (pendingDigit < 0)
                    pendingDigit = 1;

                if (pendingDigit == 0)
                    return false;

                section += pendingDigit * unit;
                pendingDigit = -1;
                lastSmallUnit = unit;
                continue;
            }

            if (LargeUnits.Contains(c))
            {
                hasUnit = true;
                var unit = MapUnit(c);
                if (unit >= lastLargeUnit)
                    return false;

                if (pendingDigit >= 0)
                {
                    section += pendingDigit;
                    pendingDigit = -1;
                }

                if (section == 0)
                    section = 1;

                total += section * unit;
                section = 0;
                lastLargeUnit = unit;
                lastSmallUnit = 10_000;
                continue;
            }

            return false;
        }

        if (!hasUnit)
            return false;

        if (pendingDigit >= 0)
            section += pendingDigit;

        value = total + section;
        return true;
    }

    private static int MapDigit(char c)
    {
        return c switch
        {
            '零' => 0,
            '一' => 1,
            '二' => 2,
            '三' => 3,
            '四' => 4,
            '五' => 5,
            '六' => 6,
            '七' => 7,
            '八' => 8,
            '九' => 9,
            _ => throw new ArgumentOutOfRangeException(nameof(c), c, "Unsupported normalized digit")
        };
    }

    private static long MapUnit(char c)
    {
        return c switch
        {
            '十' => 10,
            '百' => 100,
            '千' => 1_000,
            '万' => 10_000,
            '亿' => 100_000_000,
            _ => throw new ArgumentOutOfRangeException(nameof(c), c, "Unsupported normalized unit")
        };
    }

    private static string ToChinese(long value)
    {
        if (value == 0)
            return DigitTexts[0];

        var sections = new List<int>();
        while (value > 0)
        {
            sections.Add((int)(value % 10_000));
            value /= 10_000;
        }

        var builder = new StringBuilder();
        var pendingZero = false;

        for (var i = sections.Count - 1; i >= 0; i--)
        {
            var section = sections[i];
            if (section == 0)
            {
                pendingZero = builder.Length > 0;
                continue;
            }

            if (pendingZero || (builder.Length > 0 && section < 1_000))
                builder.Append('零');

            builder.Append(ConvertSection(section));
            if (i > 0)
            {
                var unit = i < GroupUnits.Length ? GroupUnits[i] : string.Empty;
                builder.Append(unit);
            }

            pendingZero = false;
        }

        return builder.ToString();
    }

    private static string ConvertSection(int section)
    {
        int[] digits =
        [
            section / 1_000,
            section / 100 % 10,
            section / 10 % 10,
            section % 10
        ];

        var builder = new StringBuilder();
        var pendingZero = false;

        for (var i = 0; i < digits.Length; i++)
        {
            var digit = digits[i];
            if (digit == 0)
            {
                if (builder.Length > 0)
                    pendingZero = true;
                continue;
            }

            if (pendingZero)
            {
                builder.Append('零');
                pendingZero = false;
            }

            if (!(digit == 1 && i == 2 && builder.Length == 0))
                builder.Append(DigitTexts[digit]);

            builder.Append(SectionUnits[i]);
        }

        return builder.ToString();
    }

    private static bool StartsWithAt(string input, int index, string value)
    {
        return index >= 0 &&
               index + value.Length <= input.Length &&
               string.CompareOrdinal(input, index, value, 0, value.Length) == 0;
    }

    private static bool EndsWithAt(string input, int index, string value)
    {
        return index >= value.Length &&
               string.CompareOrdinal(input, index - value.Length, value, 0, value.Length) == 0;
    }
}

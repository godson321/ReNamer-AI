using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ReNamer.Services;

public static class ChineseScriptConversionService
{
    private const string Big5ToGbScriptName = "ChineseBig5ToGb.js";
    private const string GbToBig5ScriptName = "ChineseGbToBig5.js";

    private static readonly Lazy<MappingBundle> TraditionalToSimplifiedBundle =
        new(() => LoadBundle(
            Big5ToGbScriptName,
            phraseMapName: "PHRASES",
            charMapName: "CHARS"),
            LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<MappingBundle> SimplifiedToTraditionalBundle =
        new(() => LoadBundle(
            GbToBig5ScriptName,
            phraseMapName: "COMMON_PHRASES",
            charMapName: "COMMON_CHARS"),
            LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Regex EntryRegex = new(
        @"'(?<key>(?:\\.|[^'])*)'\s*:\s*'(?<value>(?:\\.|[^'])*)'",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string ConvertTraditionalToSimplified(string input)
    {
        return Convert(input, TraditionalToSimplifiedBundle.Value);
    }

    public static string ConvertSimplifiedToTraditional(string input)
    {
        return Convert(input, SimplifiedToTraditionalBundle.Value);
    }

    private static string Convert(string input, MappingBundle bundle)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var runeStarts = BuildRuneStarts(input);
        if (runeStarts.Length <= 1)
            return input;

        var runeCount = runeStarts.Length - 1;
        var builder = new StringBuilder(input.Length);

        for (var runeIndex = 0; runeIndex < runeCount;)
        {
            var matched = false;
            var maxPhraseLength = Math.Min(bundle.MaxPhraseLength, runeCount - runeIndex);

            for (var phraseLength = maxPhraseLength; phraseLength >= 2; phraseLength--)
            {
                var start = runeStarts[runeIndex];
                var end = runeStarts[runeIndex + phraseLength];
                var segment = input.Substring(start, end - start);
                if (!bundle.PhraseMap.TryGetValue(segment, out var replacement))
                    continue;

                builder.Append(replacement);
                runeIndex += phraseLength;
                matched = true;
                break;
            }

            if (matched)
                continue;

            var currentStart = runeStarts[runeIndex];
            var currentEnd = runeStarts[runeIndex + 1];
            var current = input.Substring(currentStart, currentEnd - currentStart);
            if (bundle.CharMap.TryGetValue(current, out var currentReplacement))
                builder.Append(currentReplacement);
            else
                builder.Append(current);

            runeIndex++;
        }

        return builder.ToString();
    }

    private static int[] BuildRuneStarts(string input)
    {
        var starts = new List<int>(input.Length + 1);
        for (var i = 0; i < input.Length;)
        {
            starts.Add(i);
            i += char.IsSurrogatePair(input, i) ? 2 : 1;
        }

        starts.Add(input.Length);
        return starts.ToArray();
    }

    private static MappingBundle LoadBundle(string scriptFileName, string phraseMapName, string charMapName)
    {
        var scriptPath = ResolveScriptPath(scriptFileName);
        var script = File.ReadAllText(scriptPath, Encoding.UTF8);
        var phraseMap = ParseMap(script, phraseMapName);
        var charMap = ParseMap(script, charMapName);
        var maxPhraseLength = 1;

        foreach (var key in phraseMap.Keys)
            maxPhraseLength = Math.Max(maxPhraseLength, CountRunes(key));

        return new MappingBundle(
            phraseMap.ToFrozenDictionary(StringComparer.Ordinal),
            charMap.ToFrozenDictionary(StringComparer.Ordinal),
            maxPhraseLength);
    }

    private static Dictionary<string, string> ParseMap(string script, string mapName)
    {
        var body = ExtractObjectBody(script, mapName);
        var map = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (Match match in EntryRegex.Matches(body))
        {
            var key = UnescapeJsString(match.Groups["key"].Value);
            var value = UnescapeJsString(match.Groups["value"].Value);
            map[key] = value;
        }

        return map;
    }

    private static string ExtractObjectBody(string script, string mapName)
    {
        var pattern = $@"const\s+{Regex.Escape(mapName)}\s*=\s*\{{(?<body>.*?)\}};";
        var match = Regex.Match(script, pattern, RegexOptions.Singleline | RegexOptions.CultureInvariant);
        if (!match.Success)
            throw new InvalidOperationException($"Cannot find map '{mapName}'.");

        return match.Groups["body"].Value;
    }

    private static string ResolveScriptPath(string scriptFileName)
    {
        foreach (var root in EnumerateCandidateRoots(AppContext.BaseDirectory))
        {
            var directScriptsPath = Path.Combine(root, "Scripts", scriptFileName);
            if (File.Exists(directScriptsPath))
                return directScriptsPath;

            var projectScriptsPath = Path.Combine(root, "ReNamerWPF", "ReNamer", "Scripts", scriptFileName);
            if (File.Exists(projectScriptsPath))
                return projectScriptsPath;

            var fallbackProjectScriptsPath = Path.Combine(root, "ReNamer", "Scripts", scriptFileName);
            if (File.Exists(fallbackProjectScriptsPath))
                return fallbackProjectScriptsPath;
        }

        throw new FileNotFoundException($"Chinese conversion script not found: {scriptFileName}");
    }

    private static IEnumerable<string> EnumerateCandidateRoots(string startDirectory)
    {
        for (var directory = new DirectoryInfo(startDirectory); directory != null; directory = directory.Parent)
            yield return directory.FullName;
    }

    private static string UnescapeJsString(string value)
    {
        if (value.IndexOf('\\') < 0)
            return value;

        var builder = new StringBuilder(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];
            if (current != '\\' || i == value.Length - 1)
            {
                builder.Append(current);
                continue;
            }

            var next = value[++i];
            switch (next)
            {
                case '\\':
                case '\'':
                case '"':
                    builder.Append(next);
                    break;
                case 'n':
                    builder.Append('\n');
                    break;
                case 'r':
                    builder.Append('\r');
                    break;
                case 't':
                    builder.Append('\t');
                    break;
                case 'u' when i + 4 < value.Length:
                {
                    var hex = value.Substring(i + 1, 4);
                    if (ushort.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var codePoint))
                    {
                        builder.Append((char)codePoint);
                        i += 4;
                        break;
                    }

                    builder.Append("\\u");
                    break;
                }
                default:
                    builder.Append(next);
                    break;
            }
        }

        return builder.ToString();
    }

    private static int CountRunes(string value)
    {
        var count = 0;
        foreach (var _ in value.EnumerateRunes())
            count++;

        return count;
    }

    private sealed record MappingBundle(
        FrozenDictionary<string, string> PhraseMap,
        FrozenDictionary<string, string> CharMap,
        int MaxPhraseLength);
}

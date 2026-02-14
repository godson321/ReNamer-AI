using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ReNamer.Models;

namespace ReNamer.Rules;

public class PascalScriptRule : RuleBase, IStatefulRule
{
    private int _counter;

    public string ScriptText { get; set; } = "Result := Name;";
    public bool SkipExtension { get; set; } = true;
    public bool EnableMetaTags { get; set; } = true;
    public int CounterStart { get; set; } = 1;
    public int CounterStep { get; set; } = 1;
    public int CounterDigits { get; set; } = 0;

    public override string RuleName => "PascalScript";

    public override string Description => $"PascalScript: {GetScriptSummary()}";

    public PascalScriptRule()
    {
        Reset();
    }

    public void Reset()
    {
        _counter = CounterStart;
    }

    public override string Execute(string fileName, RenFile file)
    {
        var (baseName, ext) = SplitFileName(fileName, SkipExtension);
        var counterText = CounterDigits > 0
            ? _counter.ToString($"D{CounterDigits}", CultureInfo.InvariantCulture)
            : _counter.ToString(CultureInfo.InvariantCulture);

        var context = new ScriptContext
        {
            Name = baseName,
            Ext = ext.TrimStart('.'),
            FullName = fileName,
            Path = file.FullPath,
            FolderPath = file.FolderPath,
            Folder = file.FolderName,
            Counter = counterText,
            Size = file.Size.ToString(CultureInfo.InvariantCulture),
            SizeKB = file.SizeKB,
            SizeMB = file.SizeMB,
            Created = file.Created.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            Modified = file.Modified.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            ExifDate = file.ExifDate?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty
        };

        var result = PascalScriptEvaluator.Evaluate(ScriptText, context, baseName);
        if (EnableMetaTags)
            result = RuleHelpers.ResolveMetaTags(result, file);

        if (SkipExtension)
            result += ext;

        _counter += CounterStep;
        return result;
    }

    private string GetScriptSummary()
    {
        var line = ScriptText?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(line))
            return "(empty)";
        return line.Length > 40 ? line[..40] + "..." : line;
    }
}

internal sealed class ScriptContext
{
    public string Name { get; init; } = string.Empty;
    public string Ext { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string FolderPath { get; init; } = string.Empty;
    public string Folder { get; init; } = string.Empty;
    public string Counter { get; init; } = string.Empty;
    public string Size { get; init; } = string.Empty;
    public string SizeKB { get; init; } = string.Empty;
    public string SizeMB { get; init; } = string.Empty;
    public string Created { get; init; } = string.Empty;
    public string Modified { get; init; } = string.Empty;
    public string ExifDate { get; init; } = string.Empty;
}

internal static class PascalScriptEvaluator
{
    private static readonly Regex ResultRegex = new(
        @"Result\s*:=\s*(.+?);",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Dictionary<string, Func<ScriptContext, string>> Variables = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Name"] = c => c.Name,
        ["Ext"] = c => c.Ext,
        ["FullName"] = c => c.FullName,
        ["Path"] = c => c.Path,
        ["FolderPath"] = c => c.FolderPath,
        ["Folder"] = c => c.Folder,
        ["Counter"] = c => c.Counter,
        ["Size"] = c => c.Size,
        ["SizeKB"] = c => c.SizeKB,
        ["SizeMB"] = c => c.SizeMB,
        ["Created"] = c => c.Created,
        ["Modified"] = c => c.Modified,
        ["ExifDate"] = c => c.ExifDate
    };

    private static readonly Dictionary<string, Func<List<string>, string>> Functions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["UpperCase"] = args => args.Count > 0 ? args[0].ToUpperInvariant() : string.Empty,
        ["LowerCase"] = args => args.Count > 0 ? args[0].ToLowerInvariant() : string.Empty,
        ["Trim"] = args => args.Count > 0 ? args[0].Trim() : string.Empty,
        ["Replace"] = args => args.Count >= 3 ? args[0].Replace(args[1], args[2]) : string.Empty,
        ["LeftStr"] = args => SliceLeft(args),
        ["RightStr"] = args => SliceRight(args),
        ["MidStr"] = args => SliceMid(args)
    };

    public static string Evaluate(string script, ScriptContext context, string fallback)
    {
        if (string.IsNullOrWhiteSpace(script))
            return fallback;

        var expr = ExtractResultExpression(script);
        if (string.IsNullOrWhiteSpace(expr))
            return fallback;

        try
        {
            return EvaluateExpression(expr, context);
        }
        catch
        {
            return fallback;
        }
    }

    private static string ExtractResultExpression(string script)
    {
        var match = ResultRegex.Match(script);
        if (match.Success)
            return match.Groups[1].Value.Trim();
        return script.Trim();
    }

    private static string EvaluateExpression(string expression, ScriptContext context)
    {
        var parts = SplitTopLevel(expression, '+');
        if (parts.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var part in parts)
        {
            sb.Append(EvaluateTerm(part, context));
        }
        return sb.ToString();
    }

    private static string EvaluateTerm(string term, ScriptContext context)
    {
        var trimmed = term.Trim();
        if (trimmed.Length == 0)
            return string.Empty;

        if (IsStringLiteral(trimmed))
            return UnescapePascalString(trimmed);

        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
            return num.ToString(CultureInfo.InvariantCulture);

        var openIndex = trimmed.IndexOf('(');
        if (openIndex > 0 && trimmed.EndsWith(')'))
        {
            var funcName = trimmed[..openIndex].Trim();
            var argText = trimmed[(openIndex + 1)..^1];
            var args = SplitTopLevel(argText, ',')
                .Select(a => EvaluateExpression(a, context))
                .ToList();
            if (Functions.TryGetValue(funcName, out var func))
                return func(args);
        }

        if (Variables.TryGetValue(trimmed, out var getter))
            return getter(context);

        return string.Empty;
    }

    private static bool IsStringLiteral(string text)
    {
        return text.Length >= 2 && text.StartsWith('\'') && text.EndsWith('\'');
    }

    private static string UnescapePascalString(string text)
    {
        var inner = text[1..^1];
        return inner.Replace("''", "'");
    }

    private static List<string> SplitTopLevel(string input, char separator)
    {
        var parts = new List<string>();
        if (string.IsNullOrEmpty(input))
        {
            parts.Add(string.Empty);
            return parts;
        }

        var sb = new StringBuilder();
        var depth = 0;
        var inString = false;

        for (var i = 0; i < input.Length; i++)
        {
            var ch = input[i];
            if (ch == '\'')
            {
                sb.Append(ch);
                if (inString)
                {
                    if (i + 1 < input.Length && input[i + 1] == '\'')
                    {
                        sb.Append('\'');
                        i++;
                    }
                    else
                    {
                        inString = false;
                    }
                }
                else
                {
                    inString = true;
                }
                continue;
            }

            if (!inString)
            {
                if (ch == '(') depth++;
                else if (ch == ')' && depth > 0) depth--;

                if (depth == 0 && ch == separator)
                {
                    parts.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }
            }

            sb.Append(ch);
        }

        parts.Add(sb.ToString());
        return parts;
    }

    private static int ParseInt(List<string> args, int index, int defaultValue = 0)
    {
        if (args.Count <= index)
            return defaultValue;
        return int.TryParse(args[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)
            ? n
            : defaultValue;
    }

    private static string SliceLeft(List<string> args)
    {
        if (args.Count < 2) return string.Empty;
        var text = args[0];
        var len = Math.Max(0, ParseInt(args, 1));
        return text.Length <= len ? text : text[..len];
    }

    private static string SliceRight(List<string> args)
    {
        if (args.Count < 2) return string.Empty;
        var text = args[0];
        var len = Math.Max(0, ParseInt(args, 1));
        return text.Length <= len ? text : text[^len..];
    }

    private static string SliceMid(List<string> args)
    {
        if (args.Count < 2) return string.Empty;
        var text = args[0];
        var start = Math.Max(1, ParseInt(args, 1));
        var length = args.Count >= 3 ? Math.Max(0, ParseInt(args, 2)) : text.Length;
        var zeroBased = Math.Max(0, start - 1);
        if (zeroBased >= text.Length) return string.Empty;
        var maxLen = Math.Min(length, text.Length - zeroBased);
        return text.Substring(zeroBased, maxLen);
    }
}

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Acornima.Ast;
using Jint;
using Jint.Native;
using ReNamer.Models;

namespace ReNamer.Rules;

public class JavaScriptRule : RuleBase, IStatefulRule, IPreviewParallelRule
{
    private static readonly Regex CounterIdentifierRegex = new(
        @"(?<![\p{L}\p{Nd}_$])Counter(?![\p{L}\p{Nd}_$])",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private int _counter;
    private string _scriptText = "Result = Name;";
    private string? _scriptFilePath;
    private bool _isScriptLoading;
    private string? _scriptLoadError;
    private string? _counterUsageCacheKey;
    private bool _counterUsageCacheValue;

    public string ScriptText
    {
        get => _scriptText;
        set
        {
            if (_scriptText == value)
                return;

            _scriptText = value;
            _counterUsageCacheKey = null;
            OnPropertyChanged(nameof(ScriptText));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(IsScriptReady));
        }
    }
    public string? ScriptFilePath
    {
        get => _scriptFilePath;
        set
        {
            if (_scriptFilePath == value)
                return;
            _scriptFilePath = value;
            OnPropertyChanged(nameof(ScriptFilePath));
            OnPropertyChanged(nameof(ScriptDisplayName));
            OnPropertyChanged(nameof(Description));
        }
    }
    public bool IsScriptLoading
    {
        get => _isScriptLoading;
        set
        {
            if (_isScriptLoading == value)
                return;
            _isScriptLoading = value;
            OnPropertyChanged(nameof(IsScriptLoading));
            OnPropertyChanged(nameof(IsScriptReady));
        }
    }
    public string? ScriptLoadError
    {
        get => _scriptLoadError;
        set
        {
            if (_scriptLoadError == value)
                return;
            _scriptLoadError = value;
            OnPropertyChanged(nameof(ScriptLoadError));
            OnPropertyChanged(nameof(IsScriptReady));
        }
    }
    public bool IsScriptReady => !IsScriptLoading && string.IsNullOrWhiteSpace(ScriptLoadError) && !string.IsNullOrWhiteSpace(ScriptText);
    public string ScriptDisplayName => string.IsNullOrWhiteSpace(ScriptFilePath) ? "(inline)" : Path.GetFileNameWithoutExtension(ScriptFilePath);
    public bool SkipExtension { get; set; } = true;
    public bool EnableMetaTags { get; set; } = true;
    public int CounterStart { get; set; } = 1;
    public int CounterStep { get; set; } = 1;
    public int CounterDigits { get; set; } = 0;
    public bool CanParallelizePreview => !UsesSequentialCounter();

    public override string RuleName => "JavaScript";

    public override string Description => $"JavaScript[{ScriptDisplayName}]: {GetScriptSummary()}";

    public JavaScriptRule()
    {
        Reset();
    }

    public void Reset()
    {
        _counter = CounterStart;
    }

    public void MarkScriptLoading(string filePath)
    {
        ScriptFilePath = filePath;
        IsScriptLoading = true;
        ScriptLoadError = null;
    }

    public void MarkScriptLoaded(string filePath, string scriptText)
    {
        ScriptFilePath = filePath;
        ScriptText = scriptText;
        IsScriptLoading = false;
        ScriptLoadError = null;
    }

    public void MarkScriptLoadFailed(string filePath, string error)
    {
        ScriptFilePath = filePath;
        IsScriptLoading = false;
        ScriptLoadError = error;
    }

    public override string Execute(string fileName, RenFile file)
    {
        var (baseName, ext) = SplitFileName(fileName, SkipExtension, file.IsFolder);
        bool usesCounter = UsesSequentialCounter();
        var counterText = usesCounter
            ? CounterDigits > 0
                ? _counter.ToString($"D{CounterDigits}", CultureInfo.InvariantCulture)
                : _counter.ToString(CultureInfo.InvariantCulture)
            : string.Empty;

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

        var result = JavaScriptEvaluator.Evaluate(ScriptText, context, baseName);
        if (EnableMetaTags)
            result = RuleHelpers.ResolveMetaTags(result, file);

        if (SkipExtension)
            result += ext;

        if (usesCounter)
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

    private bool UsesSequentialCounter()
    {
        var script = ScriptText ?? string.Empty;
        if (string.Equals(_counterUsageCacheKey, script, StringComparison.Ordinal))
            return _counterUsageCacheValue;

        _counterUsageCacheKey = script;
        _counterUsageCacheValue = CounterIdentifierRegex.IsMatch(script);
        return _counterUsageCacheValue;
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

internal static class JavaScriptEvaluator
{
    private static readonly Regex LegacyResultAssignRegex = new(
        @"\bResult\s*:=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex JsResultAssignRegex = new(
        @"\bResult\s*=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly ConcurrentDictionary<string, Prepared<Script>> PreparedScriptCache = new(StringComparer.Ordinal);

    public static string Evaluate(string script, ScriptContext context, string fallback)
    {
        if (string.IsNullOrWhiteSpace(script))
            return fallback;

        try
        {
            var normalized = NormalizeScript(script);
            var prepared = PreparedScriptCache.GetOrAdd(normalized, static source =>
            {
                return Engine.PrepareScript(source);
            });

            var engine = new Engine();
            BindContext(engine, context, fallback);
            engine.Execute(in prepared);

            var result = AsString(engine.GetValue("Result"));
            return string.IsNullOrEmpty(result) ? fallback : result;
        }
        catch
        {
            return fallback;
        }
    }

    private static void BindContext(Engine engine, ScriptContext context, string fallback)
    {
        engine.SetValue("Name", context.Name);
        engine.SetValue("Ext", context.Ext);
        engine.SetValue("FullName", context.FullName);
        engine.SetValue("Path", context.Path);
        engine.SetValue("FolderPath", context.FolderPath);
        engine.SetValue("Folder", context.Folder);
        engine.SetValue("Counter", context.Counter);
        engine.SetValue("Size", context.Size);
        engine.SetValue("SizeKB", context.SizeKB);
        engine.SetValue("SizeMB", context.SizeMB);
        engine.SetValue("Created", context.Created);
        engine.SetValue("Modified", context.Modified);
        engine.SetValue("ExifDate", context.ExifDate);
        engine.SetValue("Result", fallback);

        engine.SetValue("UpperCase", new Func<object?, string>(v => AsString(v).ToUpperInvariant()));
        engine.SetValue("LowerCase", new Func<object?, string>(v => AsString(v).ToLowerInvariant()));
        engine.SetValue("Trim", new Func<object?, string>(v => AsString(v).Trim()));
        engine.SetValue("Replace", new Func<object?, object?, object?, string>((text, oldValue, newValue) =>
            AsString(text).Replace(AsString(oldValue), AsString(newValue), StringComparison.Ordinal)));
        engine.SetValue("LeftStr", new Func<object?, object?, string>((text, len) =>
        {
            var src = AsString(text);
            var count = Math.Max(0, AsInt(len));
            return src.Length <= count ? src : src[..count];
        }));
        engine.SetValue("RightStr", new Func<object?, object?, string>((text, len) =>
        {
            var src = AsString(text);
            var count = Math.Max(0, AsInt(len));
            return src.Length <= count ? src : src[^count..];
        }));
        engine.SetValue("MidStr", new Func<object?, object?, object?, string>((text, start, len) =>
        {
            var src = AsString(text);
            var oneBasedStart = Math.Max(1, AsInt(start, 1));
            var zeroBasedStart = oneBasedStart - 1;
            if (zeroBasedStart >= src.Length)
                return string.Empty;

            var desiredLength = Math.Max(0, AsInt(len, src.Length - zeroBasedStart));
            var maxLength = Math.Min(desiredLength, src.Length - zeroBasedStart);
            return src.Substring(zeroBasedStart, maxLength);
        }));
    }

    private static string NormalizeScript(string script)
    {
        var normalized = script.Trim();
        if (normalized.Length == 0)
            return normalized;

        normalized = LegacyResultAssignRegex.Replace(normalized, "Result =");
        if (!JsResultAssignRegex.IsMatch(normalized))
        {
            var expr = normalized.TrimEnd(';');
            normalized = $"Result = {expr};";
        }

        return normalized;
    }

    private static int AsInt(object? value, int defaultValue = 0)
    {
        if (value == null)
            return defaultValue;

        switch (value)
        {
            case int i:
                return i;
            case long l:
                return (int)l;
            case double d:
                return (int)d;
            case float f:
                return (int)f;
            case decimal m:
                return (int)m;
            case JsValue jsValue when jsValue.IsNumber():
                return (int)jsValue.AsNumber();
            case JsValue jsValue when jsValue.IsString():
                return int.TryParse(jsValue.AsString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var jsParsed)
                    ? jsParsed
                    : defaultValue;
            default:
                return int.TryParse(AsString(value), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : defaultValue;
        }
    }

    private static string AsString(object? value)
    {
        if (value == null)
            return string.Empty;

        if (value is JsValue jsValue)
        {
            if (jsValue.IsNull() || jsValue.IsUndefined())
                return string.Empty;
            return jsValue.ToString();
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }
}

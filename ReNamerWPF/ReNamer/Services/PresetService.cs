using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ReNamer.Rules;

namespace ReNamer.Services;

/// <summary>
/// 预设序列化/反序列化服务
/// </summary>
public static class PresetService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly Regex LegacySectionRegex = new(@"^\[(?<name>[^\]]+)\]$", RegexOptions.Compiled);

    private static readonly Dictionary<string, string> LegacyRuleIdMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Regex"] = "RegEx",
        ["RegEx"] = "RegEx",
        ["CleanUp"] = "CleanUp",
        ["ReformatDate"] = "ReformatDate",
        ["UserInput"] = "UserInput",
        ["JavaScript"] = "JavaScript"
    };

    private static readonly Dictionary<string, string> LegacyPropertyAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SKIPEXTENSION"] = "SkipExtension",
        ["CASESENSITIVE"] = "CaseSensitive",
        ["USEWILDCARDS"] = "UseWildcards",
        ["WHOLEWORDSONLY"] = "WholeWordsOnly",
        ["TEXTWHAT"] = "FindText",
        ["TEXTWITH"] = "ReplaceText",
        ["EXPRESSION"] = "Expression",
        ["REPLACETEXT"] = "ReplaceText",
        ["PATTERN"] = "Pattern",
        ["TEXT"] = "InsertText",
        ["POSITION"] = "Position",
        ["RIGHTTOLEFT"] = "RightToLeft"
    };

    public static string SaveToJson(IEnumerable<IRule> rules)
    {
        var presetRules = rules.Select(r => new PresetRule
        {
            TypeName = r.GetType().Name,
            IsEnabled = r.IsEnabled,
            Properties = SerializeRuleProperties(r)
        }).ToList();

        return JsonSerializer.Serialize(new PresetData { Rules = presetRules }, JsonOptions);
    }

    public static List<IRule> LoadFromContent(string content, string? sourcePath, out string? warningMessage)
    {
        if (LooksLikeLegacyPreset(content, sourcePath))
            return LoadFromLegacyRnp(content, out warningMessage);

        warningMessage = null;
        return LoadFromJson(content);
    }

    public static string ConvertLegacyRnpToJson(string content, out string? warningMessage)
    {
        var rules = LoadFromLegacyRnp(content, out warningMessage);
        return SaveToJson(rules);
    }

    public static List<IRule> LoadFromJson(string json)
    {
        var data = JsonSerializer.Deserialize<PresetData>(json, JsonOptions);
        if (data?.Rules == null) return new List<IRule>();

        var rules = new List<IRule>();
        foreach (var pr in data.Rules)
        {
            var rule = CreateRuleByName(pr.TypeName);
            if (rule != null)
            {
                rule.IsEnabled = pr.IsEnabled;
                if (pr.Properties != null)
                {
                    if (pr.TypeName == "StripRule" && rule is DeleteRule deleteRule)
                        ApplyLegacyStripProperties(deleteRule, pr.Properties);
                    else if (pr.TypeName == "RemoveRule" && rule is DeleteRule deleteRuleFromRemove)
                        ApplyLegacyRemoveProperties(deleteRuleFromRemove, pr.Properties);
                    else
                        ApplyProperties(rule, pr.Properties);
                }
                rules.Add(rule);
            }
        }
        return rules;
    }

    private static Dictionary<string, JsonElement> SerializeRuleProperties(IRule rule)
    {
        var json = JsonSerializer.Serialize(rule, rule.GetType(), JsonOptions);
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, JsonOptions)
                   ?? new Dictionary<string, JsonElement>();
        dict.Remove("isEnabled");
        dict.Remove("ruleName");
        dict.Remove("description");
        return dict;
    }

    private static void ApplyProperties(IRule rule, Dictionary<string, JsonElement> props)
    {
        var ruleType = rule.GetType();
        foreach (var kvp in props)
        {
            var propName = char.ToUpper(kvp.Key[0]) + kvp.Key[1..];
            var prop = ruleType.GetProperty(propName) ?? ruleType.GetProperty(kvp.Key);
            if (prop == null || !prop.CanWrite) continue;

            try
            {
                var value = JsonSerializer.Deserialize(kvp.Value.GetRawText(), prop.PropertyType, JsonOptions);
                prop.SetValue(rule, value);
            }
            catch { }
        }
    }

    private static IRule? CreateRuleByName(string typeName)
    {
        if (typeName == "StripRule" || typeName == "RemoveRule")
            return new DeleteRule();
        return RuleFactory.CreateByTypeName(typeName);
    }

    private static void ApplyLegacyStripProperties(DeleteRule rule, Dictionary<string, JsonElement> props)
    {
        rule.Mode = DeleteMode.CharacterRemove;
        rule.StripEnglishLetters = GetBool(props, "stripEnglishLetters");
        rule.StripDigits = GetBool(props, "stripDigits");
        rule.StripSymbols = GetBool(props, "stripSymbols");
        rule.StripBrackets = GetBool(props, "stripBrackets");
        rule.StripUserDefined = GetBool(props, "stripUserDefined");
        rule.UserDefinedChars = GetString(props, "userDefinedChars", "");
        rule.StripUnicodeRange = GetBool(props, "stripUnicodeRange");
        rule.UnicodeRange = GetString(props, "unicodeRange", "10000-10FFFF");
        rule.Where = GetEnum(props, "where", StripWhere.Everywhere);
        rule.StripAllExceptSelected = GetBool(props, "stripAllExceptSelected");
        rule.CaseSensitive = GetBool(props, "caseSensitive");
        rule.SkipExtension = GetBool(props, "skipExtension", true);
    }

    private static void ApplyLegacyRemoveProperties(DeleteRule rule, Dictionary<string, JsonElement> props)
    {
        rule.Mode = DeleteMode.TextRemove;
        rule.RemovePattern = GetString(props, "pattern", "");
        rule.RemoveOccurrence = GetEnum(props, "occurrence", RemoveOccurrence.All);
        rule.RemoveCaseSensitive = GetBool(props, "caseSensitive");
        rule.RemoveWholeWordsOnly = GetBool(props, "wholeWordsOnly");
        rule.RemoveUseWildcards = GetBool(props, "useWildcards");
        rule.SkipExtension = GetBool(props, "skipExtension", true);
    }

    private static bool LooksLikeLegacyPreset(string content, string? sourcePath)
    {
        if (!string.IsNullOrWhiteSpace(sourcePath) &&
            string.Equals(Path.GetExtension(sourcePath), ".rnp", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return content.Contains("[Rule", StringComparison.OrdinalIgnoreCase)
               && content.Contains("ID=", StringComparison.OrdinalIgnoreCase)
               && content.Contains("Config=", StringComparison.OrdinalIgnoreCase);
    }

    private static List<IRule> LoadFromLegacyRnp(string content, out string? warningMessage)
    {
        var sections = ParseLegacyRuleSections(content);
        var rules = new List<IRule>();
        var unsupportedRules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var section in sections.OrderBy(s => s.Order))
        {
            if (string.IsNullOrWhiteSpace(section.Id))
                continue;

            var rule = CreateRuleByLegacyId(section.Id);
            if (rule == null)
            {
                unsupportedRules.Add(section.Id);
                continue;
            }

            var config = ParseLegacyConfig(section.ConfigRaw);
            ApplyLegacyRuleConfig(rule, section.Id, config);
            rule.IsEnabled = section.Enabled;
            rules.Add(rule);
        }

        if (unsupportedRules.Count == 0)
        {
            warningMessage = null;
        }
        else
        {
            warningMessage = $"以下旧版规则未自动导入：{string.Join("、", unsupportedRules.OrderBy(x => x))}";
        }

        return rules;
    }

    private static List<LegacyRuleSection> ParseLegacyRuleSections(string content)
    {
        var sections = new List<LegacyRuleSection>();
        LegacyRuleSection? currentSection = null;

        var lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal) || line.StartsWith(";", StringComparison.Ordinal))
                continue;

            var match = LegacySectionRegex.Match(line);
            if (match.Success)
            {
                var name = match.Groups["name"].Value.Trim();
                if (name.StartsWith("Rule", StringComparison.OrdinalIgnoreCase))
                {
                    var suffix = name.Length > 4 ? name[4..] : string.Empty;
                    var order = int.TryParse(suffix, out var parsedOrder) ? parsedOrder : int.MaxValue;
                    currentSection = new LegacyRuleSection(order);
                    sections.Add(currentSection);
                }
                else
                {
                    currentSection = null;
                }

                continue;
            }

            if (currentSection == null)
                continue;

            var eqIndex = line.IndexOf('=');
            if (eqIndex <= 0)
                continue;

            var key = line[..eqIndex].Trim();
            var value = line[(eqIndex + 1)..].Trim();

            if (key.Equals("ID", StringComparison.OrdinalIgnoreCase))
            {
                currentSection.Id = value;
            }
            else if (key.Equals("Config", StringComparison.OrdinalIgnoreCase))
            {
                currentSection.ConfigRaw = value;
            }
            else if (key.Equals("Enabled", StringComparison.OrdinalIgnoreCase)
                     || key.Equals("Active", StringComparison.OrdinalIgnoreCase)
                     || key.Equals("IsEnabled", StringComparison.OrdinalIgnoreCase))
            {
                currentSection.Enabled = ParseLegacyBool(value, true);
            }
        }

        return sections;
    }

    private static Dictionary<string, string> ParseLegacyConfig(string config)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(config))
            return dict;

        foreach (var part in config.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var colonIndex = part.IndexOf(':');
            if (colonIndex <= 0)
                continue;

            var key = part[..colonIndex].Trim();
            var rawValue = part[(colonIndex + 1)..];
            var value = WebUtility.UrlDecode(rawValue);
            dict[key] = value;
        }

        return dict;
    }

    private static IRule? CreateRuleByLegacyId(string legacyId)
    {
        if (legacyId.Equals("PascalScript", StringComparison.OrdinalIgnoreCase))
            return null;

        if (legacyId.Equals("Strip", StringComparison.OrdinalIgnoreCase))
            return new DeleteRule { Mode = DeleteMode.CharacterRemove };

        if (legacyId.Equals("Remove", StringComparison.OrdinalIgnoreCase))
            return new DeleteRule { Mode = DeleteMode.TextRemove };

        var normalizedId = NormalizeLegacyRuleId(legacyId);
        if (RuleFactory.TryCreate(normalizedId, out var rule))
            return rule;

        return null;
    }

    private static string NormalizeLegacyRuleId(string legacyId)
    {
        if (LegacyRuleIdMap.TryGetValue(legacyId, out var mapped))
            return mapped;

        return legacyId;
    }

    private static void ApplyLegacyRuleConfig(IRule rule, string legacyId, Dictionary<string, string> config)
    {
        switch (rule)
        {
            case InsertRule insertRule:
                ApplyLegacyInsertConfig(insertRule, config);
                break;
            case ReplaceRule replaceRule:
                ApplyLegacyReplaceConfig(replaceRule, config);
                break;
            case SerializeRule serializeRule:
                ApplyLegacySerializeConfig(serializeRule, config);
                break;
            case RegexRule regexRule:
                ApplyLegacyRegexConfig(regexRule, config);
                break;
            case DeleteRule deleteRule:
                ApplyLegacyDeleteConfig(deleteRule, legacyId, config);
                break;
            case JavaScriptRule javaScriptRule:
                ApplyLegacyJavaScriptConfig(javaScriptRule, config);
                break;
        }

        ApplyLegacyFallbackProperties(rule, config);
    }

    private static void ApplyLegacyInsertConfig(InsertRule rule, Dictionary<string, string> config)
    {
        rule.InsertText = GetLegacyString(config, "TEXT", rule.InsertText);
        rule.Position = Math.Max(1, GetLegacyInt(config, "POSITION", rule.Position));
        rule.InsertPosition = MapLegacyInsertPosition(GetLegacyInt(config, "WHERE", 1));
        rule.AfterText = GetLegacyString(config, "INSERTAFTERTEXT", rule.AfterText);
        rule.BeforeText = GetLegacyString(config, "INSERTBEFORETEXT", rule.BeforeText);
        rule.RightToLeft = GetLegacyBool(config, "RIGHTTOLEFT", rule.RightToLeft);
        rule.SkipExtension = GetLegacyBool(config, "SKIPEXTENSION", rule.SkipExtension);
    }

    private static void ApplyLegacyReplaceConfig(ReplaceRule rule, Dictionary<string, string> config)
    {
        rule.FindText = GetLegacyString(config, "TEXTWHAT", rule.FindText);
        rule.ReplaceText = GetLegacyString(config, "TEXTWITH", rule.ReplaceText);
        rule.CaseSensitive = GetLegacyBool(config, "CASESENSITIVE", rule.CaseSensitive);
        rule.SkipExtension = GetLegacyBool(config, "SKIPEXTENSION", rule.SkipExtension);
        rule.UseWildcards = GetLegacyBool(config, "USEWILDCARDS", rule.UseWildcards);
        rule.WholeWordsOnly = GetLegacyBool(config, "WHOLEWORDSONLY", rule.WholeWordsOnly);
        rule.Occurrence = MapLegacyReplaceOccurrence(GetLegacyInt(config, "WHICH", 3));
    }

    private static void ApplyLegacySerializeConfig(SerializeRule rule, Dictionary<string, string> config)
    {
        rule.StartNumber = GetLegacyInt(config, "INDEX", rule.StartNumber);
        rule.Step = Math.Max(1, GetLegacyInt(config, "STEP", rule.Step));
        rule.Repeat = Math.Max(1, GetLegacyInt(config, "REPEAT", rule.Repeat));
        rule.ResetEvery = GetLegacyBool(config, "RESETEVERY", rule.ResetEvery);
        rule.ResetEveryCount = Math.Max(1, GetLegacyInt(config, "RESETEVERYCOUNT", rule.ResetEveryCount));
        rule.ResetIfFolderChanges = GetLegacyBool(config, "RESETIFFOLDERCHANGES", rule.ResetIfFolderChanges);
        rule.ResetIfFileNameChanges = GetLegacyBool(config, "RESETIFFILENAMECHANGES", rule.ResetIfFileNameChanges);
        rule.PadToLength = GetLegacyBool(config, "PADTOLENGTH", rule.PadToLength);
        rule.PadToLengthValue = Math.Max(1, GetLegacyInt(config, "PADTOLENGTHVALUE", rule.PadToLengthValue));
        rule.InsertWhere = MapLegacySerializePosition(GetLegacyInt(config, "WHERE", 1));
        rule.PositionValue = Math.Max(1, GetLegacyInt(config, "POSITION", rule.PositionValue));
        rule.SkipExtension = GetLegacyBool(config, "SKIPEXTENSION", rule.SkipExtension);

        var numberingRaw = GetLegacyString(config, "NUMERALSYSTEM", "");
        if (string.IsNullOrWhiteSpace(numberingRaw))
            numberingRaw = GetLegacyString(config, "TYPE", "");

        rule.NumberingSystem = MapLegacyNumberingSystem(numberingRaw, rule.NumberingSystem);
        rule.CustomNumberingSymbols = GetLegacyString(config, "CUSTOMSYMBOLS", rule.CustomNumberingSymbols);
        rule.Prefix = GetLegacyString(config, "PREFIX", rule.Prefix);
        rule.Suffix = GetLegacyString(config, "SUFFIX", rule.Suffix);
    }

    private static void ApplyLegacyRegexConfig(RegexRule rule, Dictionary<string, string> config)
    {
        rule.Expression = GetLegacyString(
            config,
            "EXPRESSION",
            GetLegacyString(config, "TEXTWHAT", rule.Expression));
        rule.ReplaceText = GetLegacyString(
            config,
            "REPLACETEXT",
            GetLegacyString(config, "TEXTWITH", rule.ReplaceText));
        rule.CaseSensitive = GetLegacyBool(config, "CASESENSITIVE", rule.CaseSensitive);
        rule.SkipExtension = GetLegacyBool(config, "SKIPEXTENSION", rule.SkipExtension);
    }

    private static void ApplyLegacyDeleteConfig(DeleteRule rule, string legacyId, Dictionary<string, string> config)
    {
        var isStripMode = legacyId.Equals("Strip", StringComparison.OrdinalIgnoreCase)
                          || config.ContainsKey("STRIPENGLISHLETTERS")
                          || config.ContainsKey("STRIPDIGITS")
                          || config.ContainsKey("STRIPSYMBOLS")
                          || config.ContainsKey("STRIPBRACKETS")
                          || config.ContainsKey("STRIPUSERDEFINED")
                          || config.ContainsKey("STRIPUNICODERANGE");

        if (isStripMode)
        {
            rule.Mode = DeleteMode.CharacterRemove;
            rule.StripEnglishLetters = GetLegacyBool(config, "STRIPENGLISHLETTERS", rule.StripEnglishLetters);
            rule.StripDigits = GetLegacyBool(config, "STRIPDIGITS", rule.StripDigits);
            rule.StripSymbols = GetLegacyBool(config, "STRIPSYMBOLS", rule.StripSymbols);
            rule.StripBrackets = GetLegacyBool(config, "STRIPBRACKETS", rule.StripBrackets);
            rule.StripUserDefined = GetLegacyBool(config, "STRIPUSERDEFINED", rule.StripUserDefined);
            rule.UserDefinedChars = GetLegacyString(config, "USERDEFINEDCHARS", rule.UserDefinedChars);
            rule.StripUnicodeRange = GetLegacyBool(config, "STRIPUNICODERANGE", rule.StripUnicodeRange);
            rule.UnicodeRange = GetLegacyString(config, "UNICODERANGE", rule.UnicodeRange);
            rule.CaseSensitive = GetLegacyBool(config, "CASESENSITIVE", rule.CaseSensitive);
            rule.SkipExtension = GetLegacyBool(config, "SKIPEXTENSION", rule.SkipExtension);
            return;
        }

        rule.Mode = DeleteMode.TextRemove;
        rule.RemovePattern = GetLegacyString(
            config,
            "PATTERN",
            GetLegacyString(config, "TEXTWHAT", GetLegacyString(config, "TEXT", rule.RemovePattern)));
        rule.RemoveOccurrence = MapLegacyRemoveOccurrence(GetLegacyInt(config, "WHICH", 3));
        rule.RemoveCaseSensitive = GetLegacyBool(config, "CASESENSITIVE", rule.RemoveCaseSensitive);
        rule.RemoveWholeWordsOnly = GetLegacyBool(config, "WHOLEWORDSONLY", rule.RemoveWholeWordsOnly);
        rule.RemoveUseWildcards = GetLegacyBool(config, "USEWILDCARDS", rule.RemoveUseWildcards);
        rule.SkipExtension = GetLegacyBool(config, "SKIPEXTENSION", rule.SkipExtension);
    }

    private static void ApplyLegacyJavaScriptConfig(JavaScriptRule rule, Dictionary<string, string> config)
    {
        var script = GetLegacyString(
            config,
            "SCRIPT",
            GetLegacyString(config, "SOURCEUTF8", GetLegacyString(config, "SOURCE", rule.ScriptText)));

        if (!string.IsNullOrWhiteSpace(script))
            rule.ScriptText = script;

        rule.SkipExtension = GetLegacyBool(config, "SKIPEXTENSION", rule.SkipExtension);
        rule.EnableMetaTags = GetLegacyBool(config, "ENABLEMETATAGS", rule.EnableMetaTags);
        rule.CounterStart = GetLegacyInt(config, "COUNTERSTART", rule.CounterStart);
        rule.CounterStep = Math.Max(1, GetLegacyInt(config, "COUNTERSTEP", rule.CounterStep));
        rule.CounterDigits = Math.Max(1, GetLegacyInt(config, "COUNTERDIGITS", rule.CounterDigits));
    }

    private static void ApplyLegacyFallbackProperties(IRule rule, Dictionary<string, string> config)
    {
        foreach (var (legacyKey, propertyName) in LegacyPropertyAliases)
        {
            if (!config.TryGetValue(legacyKey, out var rawValue))
                continue;

            TrySetPropertyFromString(rule, propertyName, rawValue);
        }
    }

    private static bool TrySetPropertyFromString(IRule rule, string propertyName, string rawValue)
    {
        var prop = rule.GetType().GetProperty(propertyName);
        if (prop == null || !prop.CanWrite)
            return false;

        try
        {
            object? value;
            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            if (targetType == typeof(string))
            {
                value = rawValue;
            }
            else if (targetType == typeof(bool))
            {
                value = ParseLegacyBool(rawValue, false);
            }
            else if (targetType == typeof(int))
            {
                value = ParseLegacyInt(rawValue, 0);
            }
            else if (targetType.IsEnum)
            {
                if (int.TryParse(rawValue, out var enumInt))
                    value = Enum.ToObject(targetType, enumInt);
                else if (Enum.TryParse(targetType, rawValue, ignoreCase: true, out var enumParsed))
                    value = enumParsed;
                else
                    return false;
            }
            else
            {
                return false;
            }

            prop.SetValue(rule, value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static InsertPositionType MapLegacyInsertPosition(int value)
    {
        return value switch
        {
            2 => InsertPositionType.Suffix,
            3 => InsertPositionType.Position,
            4 => InsertPositionType.AfterText,
            5 => InsertPositionType.BeforeText,
            6 => InsertPositionType.ReplaceCurrentName,
            _ => InsertPositionType.Prefix
        };
    }

    private static SerializePosition MapLegacySerializePosition(int value)
    {
        return value switch
        {
            2 => SerializePosition.Suffix,
            3 => SerializePosition.Position,
            4 => SerializePosition.ReplaceCurrentName,
            _ => SerializePosition.Prefix
        };
    }

    private static ReplaceOccurrence MapLegacyReplaceOccurrence(int value)
    {
        return value switch
        {
            1 => ReplaceOccurrence.First,
            2 => ReplaceOccurrence.Last,
            _ => ReplaceOccurrence.All
        };
    }

    private static RemoveOccurrence MapLegacyRemoveOccurrence(int value)
    {
        return value switch
        {
            1 => RemoveOccurrence.First,
            2 => RemoveOccurrence.Last,
            _ => RemoveOccurrence.All
        };
    }

    private static string MapLegacyNumberingSystem(string rawValue, string defaultValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return defaultValue;

        if (int.TryParse(rawValue, out var numeric))
        {
            return numeric switch
            {
                2 => "Hexadecimal",
                3 => "Octal",
                4 => "Binary",
                5 => "Custom",
                _ => "Decimal"
            };
        }

        return rawValue.Trim().ToLowerInvariant() switch
        {
            "hex" or "hexadecimal" => "Hexadecimal",
            "oct" or "octal" => "Octal",
            "bin" or "binary" => "Binary",
            "custom" => "Custom",
            "decimal" => "Decimal",
            _ => defaultValue
        };
    }

    private static bool GetLegacyBool(IReadOnlyDictionary<string, string> config, string key, bool defaultValue)
    {
        if (!config.TryGetValue(key, out var value))
            return defaultValue;

        return ParseLegacyBool(value, defaultValue);
    }

    private static int GetLegacyInt(IReadOnlyDictionary<string, string> config, string key, int defaultValue)
    {
        if (!config.TryGetValue(key, out var value))
            return defaultValue;

        return ParseLegacyInt(value, defaultValue);
    }

    private static string GetLegacyString(IReadOnlyDictionary<string, string> config, string key, string defaultValue)
    {
        if (!config.TryGetValue(key, out var value))
            return defaultValue;

        return value;
    }

    private static bool ParseLegacyBool(string rawValue, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return defaultValue;

        return rawValue.Trim() switch
        {
            "1" => true,
            "0" => false,
            _ when bool.TryParse(rawValue, out var parsed) => parsed,
            _ => defaultValue
        };
    }

    private static int ParseLegacyInt(string rawValue, int defaultValue)
    {
        return int.TryParse(rawValue, out var value) ? value : defaultValue;
    }

    private static bool GetBool(Dictionary<string, JsonElement> props, string key, bool defaultValue = false)
    {
        if (!props.TryGetValue(key, out var value)) return defaultValue;
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => defaultValue
        };
    }

    private static string GetString(Dictionary<string, JsonElement> props, string key, string defaultValue)
    {
        if (!props.TryGetValue(key, out var value)) return defaultValue;
        return value.ValueKind == JsonValueKind.String ? (value.GetString() ?? defaultValue) : defaultValue;
    }

    private static TEnum GetEnum<TEnum>(Dictionary<string, JsonElement> props, string key, TEnum defaultValue)
        where TEnum : struct, Enum
    {
        if (!props.TryGetValue(key, out var value)) return defaultValue;
        try
        {
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i))
                return (TEnum)Enum.ToObject(typeof(TEnum), i);
            if (value.ValueKind == JsonValueKind.String)
            {
                var s = value.GetString();
                if (!string.IsNullOrWhiteSpace(s) && Enum.TryParse<TEnum>(s, ignoreCase: true, out var e))
                    return e;
            }
        }
        catch { }
        return defaultValue;
    }

    private sealed class LegacyRuleSection
    {
        public LegacyRuleSection(int order)
        {
            Order = order;
        }

        public int Order { get; }
        public string Id { get; set; } = string.Empty;
        public string ConfigRaw { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
    }

    private class PresetData
    {
        public List<PresetRule> Rules { get; set; } = new();
    }

    private class PresetRule
    {
        public string TypeName { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public Dictionary<string, JsonElement>? Properties { get; set; }
    }
}

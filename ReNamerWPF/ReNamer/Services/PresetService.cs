using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        // Remove interface properties that are handled separately
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
            // Try PascalCase property name
            var propName = char.ToUpper(kvp.Key[0]) + kvp.Key[1..];
            var prop = ruleType.GetProperty(propName) ?? ruleType.GetProperty(kvp.Key);
            if (prop == null || !prop.CanWrite) continue;

            try
            {
                var value = JsonSerializer.Deserialize(kvp.Value.GetRawText(), prop.PropertyType, JsonOptions);
                prop.SetValue(rule, value);
            }
            catch { /* Skip properties that can't be deserialized */ }
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

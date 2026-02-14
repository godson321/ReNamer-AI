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
                    ApplyProperties(rule, pr.Properties);
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
        return RuleFactory.CreateByTypeName(typeName);
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

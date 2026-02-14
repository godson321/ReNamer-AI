using System;
using System.Collections.Generic;
using System.Linq;

namespace ReNamer.Rules;

/// <summary>
/// 规则工厂 - 提供规则创建和注册功能
/// </summary>
public static class RuleFactory
{
    private static readonly Dictionary<string, Func<IRule>> _registry = new()
    {
        ["Replace"] = () => new ReplaceRule(),
        ["Insert"] = () => new InsertRule(),
        ["Delete"] = () => new DeleteRule(),
        ["Remove"] = () => new RemoveRule(),
        ["Case"] = () => new CaseRule(),
        ["Serialize"] = () => new SerializeRule(),
        ["RegEx"] = () => new RegexRule(),
        ["Padding"] = () => new PaddingRule(),
        ["CleanUp"] = () => new CleanUpRule(),
        ["Transliterate"] = () => new TransliterateRule(),
        ["Rearrange"] = () => new RearrangeRule(),
        ["ReformatDate"] = () => new ReformatDateRule(),
        ["Randomize"] = () => new RandomizeRule(),
        ["PascalScript"] = () => new PascalScriptRule(),
        ["UserInput"] = () => new UserInputRule(),
        ["Mapping"] = () => new MappingRule()
    };

    // TypeName (ReplaceRule) -> DisplayName (Replace) 映射
    private static readonly Dictionary<string, string> _typeNameToDisplayName = new()
    {
        ["ReplaceRule"] = "Replace",
        ["InsertRule"] = "Insert",
        ["DeleteRule"] = "Delete",
        ["RemoveRule"] = "Remove",
        ["CaseRule"] = "Case",
        ["SerializeRule"] = "Serialize",
        ["RegexRule"] = "RegEx",
        ["PaddingRule"] = "Padding",
        ["StripRule"] = "Delete",
        ["CleanUpRule"] = "CleanUp",
        ["TransliterateRule"] = "Transliterate",
        ["RearrangeRule"] = "Rearrange",
        ["ReformatDateRule"] = "ReformatDate",
        ["RandomizeRule"] = "Randomize",
        ["PascalScriptRule"] = "PascalScript",
        ["UserInputRule"] = "UserInput",
        ["MappingRule"] = "Mapping"
    };

    /// <summary>
    /// 根据显示名称创建规则实例
    /// </summary>
    /// <param name="ruleName">规则显示名称 (如 "Replace", "Insert")</param>
    /// <returns>规则实例，如果不存在则抛出异常</returns>
    /// <exception cref="ArgumentException">未知的规则名称</exception>
    public static IRule Create(string ruleName)
    {
        if (_registry.TryGetValue(ruleName, out var factory))
            return factory();
        throw new ArgumentException($"Unknown rule: {ruleName}");
    }

    /// <summary>
    /// 根据类型名称创建规则实例 (用于反序列化)
    /// </summary>
    /// <param name="typeName">规则类型名称 (如 "ReplaceRule", "InsertRule")</param>
    /// <returns>规则实例，如果不存在则返回 null</returns>
    public static IRule? CreateByTypeName(string typeName)
    {
        if (_typeNameToDisplayName.TryGetValue(typeName, out var displayName))
            return Create(displayName);
        return null;
    }

    /// <summary>
    /// 尝试创建规则实例
    /// </summary>
    /// <param name="ruleName">规则显示名称</param>
    /// <param name="rule">创建的规则实例</param>
    /// <returns>是否成功创建</returns>
    public static bool TryCreate(string ruleName, out IRule? rule)
    {
        if (_registry.TryGetValue(ruleName, out var factory))
        {
            rule = factory();
            return true;
        }
        rule = null;
        return false;
    }

    /// <summary>
    /// 获取所有可用的规则显示名称
    /// </summary>
    /// <returns>规则名称列表</returns>
    public static IEnumerable<string> GetAvailableRules() => _registry.Keys;

    /// <summary>
    /// 获取规则数量
    /// </summary>
    public static int RuleCount => _registry.Count;

    /// <summary>
    /// 检查规则是否存在
    /// </summary>
    public static bool IsRuleAvailable(string ruleName) => _registry.ContainsKey(ruleName);
}

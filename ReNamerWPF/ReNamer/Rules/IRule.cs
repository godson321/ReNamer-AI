using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using ReNamer.Models;
using ReNamer.Services;

namespace ReNamer.Rules;

/// <summary>
/// 规则接口 - 对应原版 TRule
/// </summary>
public interface IRule : INotifyPropertyChanged
{
    /// <summary>规则名称</summary>
    string RuleName { get; }
    
    /// <summary>规则描述</summary>
    string Description { get; }
    
    /// <summary>是否启用</summary>
    bool IsEnabled { get; set; }
    
    /// <summary>
    /// 执行规则，返回新文件名
    /// </summary>
    string Execute(string fileName, RenFile file);
}

/// <summary>
/// 有状态规则接口 - 需要在每次Preview前重置内部状态
/// 用于 SerializeRule, RandomizeRule, PascalScriptRule, UserInputRule, MappingRule
/// </summary>
public interface IStatefulRule : IRule
{
    /// <summary>
    /// 重置规则的内部状态（如计数器、已使用的值等）
    /// </summary>
    void Reset();
}

/// <summary>
/// 预览执行提示接口。
/// 某些规则虽然实现了 IStatefulRule，但在当前配置下并不依赖跨文件共享状态，可以安全并行。
/// </summary>
public interface IPreviewParallelRule : IRule
{
    bool CanParallelizePreview { get; }
}

/// <summary>
/// 规则基类
/// </summary>
public abstract class RuleBase : IRule
{
    private bool _isEnabled = true;
    private string _comment = "";

    public abstract string RuleName { get; }
    public abstract string Description { get; }

    /// <summary>本地化后的规则名称（用于 UI 展示）</summary>
    public string LocalizedRuleName
    {
        get
        {
            var resourceKey = GetRuleNameResourceKey(RuleName);
            var localized = LanguageService.GetString(resourceKey);
            return localized == resourceKey ? RuleName : localized;
        }
    }

    /// <summary>可选注释</summary>
    public string Comment
    {
        get => _comment;
        set
        {
            if (_comment != value)
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
                OnPropertyChanged(nameof(DisplayDescription));
                OnPropertyChanged(nameof(LocalizedDisplayDescription));
            }
        }
    }

    /// <summary>显示描述（包含注释）</summary>
    public string DisplayDescription =>
        string.IsNullOrWhiteSpace(Comment) ? Description : $"{Description}  // {Comment}";

    /// <summary>本地化后的规则说明（用于 UI 展示）</summary>
    public string LocalizedDescription
    {
        get
        {
            if (!LanguageService.CurrentCulture.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
                return Description;
            return GetChineseDescription();
        }
    }

    /// <summary>本地化后的显示说明（包含注释）</summary>
    public string LocalizedDisplayDescription =>
        string.IsNullOrWhiteSpace(Comment) ? LocalizedDescription : $"{LocalizedDescription}  // {Comment}";
    
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
    }

    public abstract string Execute(string fileName, RenFile file);

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    /// <summary>
    /// 分离文件名和扩展名
    /// </summary>
    protected static (string baseName, string extension) SplitFileName(string fileName, bool skipExtension, bool isFolder = false)
    {
        if (!skipExtension || isFolder)
            return (fileName, "");
            
        var ext = Path.GetExtension(fileName);
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        return (baseName, ext);
    }

    public void NotifyLocalizationChanged()
    {
        OnPropertyChanged(nameof(LocalizedRuleName));
        OnPropertyChanged(nameof(LocalizedDescription));
        OnPropertyChanged(nameof(LocalizedDisplayDescription));
    }

    private static string GetRuleNameResourceKey(string ruleName) => ruleName switch
    {
        "Regular Expressions" => "Rule_RegEx",
        "Clean Up" => "Rule_CleanUp",
        "Reformat Date" => "Rule_ReformatDate",
        "User Input" => "Rule_UserInput",
        _ => $"Rule_{ruleName.Replace(" ", string.Empty)}"
    };

    private string GetChineseDescription() => this switch
    {
        ReplaceRule r => $"替换 \"{r.FindText}\" 为 \"{r.ReplaceText}\"",
        InsertRule r => GetInsertDescriptionZh(r),
        DeleteRule r => GetDeleteDescriptionZh(r),
        RemoveRule r => $"移除 \"{r.Pattern}\"",
        CaseRule r => GetCaseDescriptionZh(r),
        SerializeRule r => $"添加序号（{r.StartNumber}, +{r.Step}）",
        RegexRule r => $"正则：{r.Expression} → {r.ReplaceText}",
        PaddingRule r => GetPaddingDescriptionZh(r),
        StripRule r => GetStripDescriptionZh(r),
        CleanUpRule => "清理文件名",
        TransliterateRule r => r.DirectionForward ? "音译（正向）" : "音译（反向）",
        ChineseConvertRule r => r.Direction == ChineseConvertDirection.SimplifiedToTraditional ? "简体转繁体" : "繁体转简体",
        ChineseNumberRule r => r.AllowLooseForms ? "中文数字转阿拉伯数字（宽松）" : "中文数字转阿拉伯数字",
        RearrangeRule r => $"重排：\"{r.NewPattern}\"",
        ReformatDateRule r => $"日期：{r.SourceFormat} → {r.TargetFormat}",
        RandomizeRule r => $"随机 {r.Length} 个字符（{GetRandomizePositionZh(r.InsertWhere)}）",
        UserInputRule r => r.Mode switch
        {
            UserInputMode.InsertBefore => "用户输入（前缀）",
            UserInputMode.InsertAfter => "用户输入（后缀）",
            _ => "用户输入（替换）"
        },
        MappingRule r => $"映射（{r.Mappings.Count} 条）",
        JavaScriptRule r => GetJavaScriptDescriptionZh(r),
        _ => Description
    };

    private static string GetInsertDescriptionZh(InsertRule r) => r.InsertPosition switch
    {
        InsertPositionType.Prefix => $"前缀插入 \"{r.InsertText}\"",
        InsertPositionType.Suffix => $"后缀插入 \"{r.InsertText}\"",
        InsertPositionType.Position => $"在位置 {r.Position} 插入 \"{r.InsertText}\"",
        InsertPositionType.AfterText => $"在 \"{r.AfterText}\" 后插入 \"{r.InsertText}\"",
        InsertPositionType.BeforeText => $"在 \"{r.BeforeText}\" 前插入 \"{r.InsertText}\"",
        InsertPositionType.ReplaceCurrentName => $"用 \"{r.InsertText}\" 替换当前名称",
        _ => $"插入 \"{r.InsertText}\""
    };

    private static string GetDeleteDescriptionZh(DeleteRule r)
    {
        if (r.Mode == DeleteMode.Combined)
            return r.TextFirstInCombined ? "删除（先文本后位置）" : "删除（先位置后文本）";

        if (r.Mode == DeleteMode.CharacterRemove)
            return "删除所选文本/字符";

        if (r.Mode == DeleteMode.TextRemove)
            return string.IsNullOrEmpty(r.RemovePattern) ? "删除所选文本/字符" : $"删除 \"{r.RemovePattern}\"";

        if (r.DeleteCurrentName)
            return "删除当前名称";

        var fromText = r.FromType == DeleteFromType.Delimiter
            ? $"分隔符“{r.FromDelimiter}”"
            : $"位置 {r.FromPosition}";
        var untilText = r.UntilType switch
        {
            DeleteUntilType.Count => $"{r.UntilCount} 个字符",
            DeleteUntilType.Delimiter => $"分隔符“{r.UntilDelimiter}”",
            _ => "末尾"
        };

        return $"从{fromText}删除到{untilText}";
    }

    private static string GetCaseDescriptionZh(CaseRule r)
    {
        if (r.UseSegmentedMode)
        {
            return $"分段大小写：首字母={GetFirstLetterModeZh(r.FirstLetterMode)}，其余={GetRemainingModeZh(r.RemainingLettersMode)}，扩展名={GetExtensionModeZh(r.ExtensionLetterMode)}";
        }

        return r.CaseType switch
        {
            CaseType.Capitalize => "每个单词首字母大写",
            CaseType.Lower => "小写",
            CaseType.Upper => "大写",
            CaseType.Invert => "反转大小写",
            CaseType.FirstLetter => "首字母大写",
            CaseType.Sentence => "句首大写",
            CaseType.None => "不改变",
            CaseType.PinyinFirstLetter => $"拼音首字母（第 {r.PinyinChineseIndex} 个）",
            _ => "大小写"
        };
    }

    private static string GetFirstLetterModeZh(FirstLetterMode mode) => mode switch
    {
        FirstLetterMode.Upper => "大写",
        FirstLetterMode.Lower => "小写",
        FirstLetterMode.PinyinFirstLetter => "拼音首字母",
        _ => "保持不变"
    };

    private static string GetRemainingModeZh(RemainingLettersMode mode) => mode switch
    {
        RemainingLettersMode.Upper => "大写",
        RemainingLettersMode.Lower => "小写",
        RemainingLettersMode.CapitalizeWords => "每个单词首字母大写",
        RemainingLettersMode.Invert => "反转大小写",
        _ => "保持不变"
    };

    private static string GetExtensionModeZh(ExtensionLetterMode mode) => mode switch
    {
        ExtensionLetterMode.Lower => "小写",
        ExtensionLetterMode.Upper => "大写",
        _ => "保持不变"
    };

    private static string GetPaddingDescriptionZh(PaddingRule r)
    {
        var parts = new List<string>();
        if (r.AddZeroPadding) parts.Add($"补零到 {r.ZeroPaddingLength}");
        if (r.RemoveZeroPadding) parts.Add("移除前导零");
        if (r.AddTextPadding) parts.Add($"文本填充到 {r.TextPaddingLength}");
        return parts.Count > 0 ? string.Join("，", parts) : "填充";
    }

    private static string GetStripDescriptionZh(StripRule r)
    {
        var parts = new List<string>();
        if (r.StripEnglishLetters) parts.Add("字母");
        if (r.StripDigits) parts.Add("数字");
        if (r.StripSymbols) parts.Add("符号");
        if (r.StripBrackets) parts.Add("括号");
        if (r.StripUserDefined) parts.Add("自定义");
        if (r.StripUnicodeRange) parts.Add("Unicode");
        var prefix = r.StripAllExceptSelected ? "保留：" : "移除：";
        return prefix + (parts.Count > 0 ? string.Join("、", parts) : "无");
    }

    private static string GetRandomizePositionZh(RandomizePosition position) => position switch
    {
        RandomizePosition.Prefix => "前缀",
        RandomizePosition.Suffix => "后缀",
        RandomizePosition.Position => "指定位置",
        RandomizePosition.ReplaceCurrentName => "替换当前名称",
        _ => "前缀"
    };

    private static string GetJavaScriptDescriptionZh(JavaScriptRule rule)
    {
        var desc = rule.Description;
        const string prefix = "JavaScript";
        if (desc.StartsWith(prefix, StringComparison.Ordinal))
            return "脚本" + desc[prefix.Length..].Replace(":", "：");
        return desc;
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReNamer.Models;
using ReNamer.Rules;

namespace ReNamer.Services;

/// <summary>
/// 重命名服务 - 核心业务逻辑
/// 对应原版的 Preview/Rename/UndoRename 流程
/// </summary>
public class RenameService
{
    /// <summary>
    /// 预览 - 应用所有规则计算新文件名
    /// </summary>
    public void Preview(IEnumerable<RenFile> files, IEnumerable<IRule> rules, System.Action<int, int>? progress = null)
    {
        var ruleList = rules.ToList();
        var fileList = files.ToList();

        RuleHelpers.ResetMetaTagCounter();
        
        // 重置所有有状态规则的内部状态
        // 修复 Bug: 多次 Preview 结果不一致的问题
        foreach (var rule in ruleList)
        {
            if (rule is IStatefulRule statefulRule)
            {
                statefulRule.Reset();
            }
        }
        
        int index = 0;
        int total = fileList.Count;
        foreach (var file in fileList)
        {
            // 从原始名称开始
            var currentName = file.OriginalName;
            
            // 依次应用每个启用的规则
            foreach (var rule in ruleList)
            {
                if (rule.IsEnabled)
                {
                    currentName = rule.Execute(currentName, file);
                }
            }
            
            // 设置新名称
            file.NewName = currentName;
            index++;
            progress?.Invoke(index, total);
        }
    }

    /// <summary>
    /// 执行重命名
    /// </summary>
    public (int success, int failed) Rename(IEnumerable<RenFile> files, System.Action<int, int>? progress = null)
    {
        int success = 0;
        int failed = 0;
        var fileList = files.ToList();
        int index = 0;
        int total = fileList.Count;

        foreach (var file in fileList)
        {
            if (file.IsMarked && file.HasChanged && !file.IsRenamed)
            {
                if (file.Rename())
                    success++;
                else
                    failed++;
            }
            index++;
            progress?.Invoke(index, total);
        }

        return (success, failed);
    }

    /// <summary>
    /// 撤销重命名
    /// </summary>
    public (int success, int failed) UndoRename(IEnumerable<RenFile> files)
    {
        int success = 0;
        int failed = 0;

        // 反向顺序撤销
        foreach (var file in files.Reverse())
        {
            if (file.IsRenamed)
            {
                if (file.UndoRename())
                    success++;
                else
                    failed++;
            }
        }

        return (success, failed);
    }

    /// <summary>
    /// 验证新文件名（检测冲突/非法字符/长路径）
    /// </summary>
    public List<string> ValidateNewNames(
        IEnumerable<RenFile> files,
        bool warnInvalidChars = true,
        bool warnLongPaths = true,
        int longPathThreshold = 260)
    {
        var errors = new List<string>();
        var fileList = files.Where(f => !f.IsRenamed).ToList();
        var newNames = new Dictionary<string, List<RenFile>>();

        foreach (var file in fileList)
        {
            var key = Path.Combine(file.FolderPath, file.NewName).ToLowerInvariant();
            
            if (!newNames.ContainsKey(key))
                newNames[key] = new List<RenFile>();
            
            newNames[key].Add(file);
        }

        // 检查重复
        foreach (var kvp in newNames.Where(x => x.Value.Count > 1))
        {
            errors.Add($"Duplicate name: {kvp.Value[0].NewName} ({kvp.Value.Count} files)");
        }

        if (warnInvalidChars)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var file in fileList.Where(f => f.HasChanged))
            {
                if (file.NewName.IndexOfAny(invalidChars) >= 0)
                {
                    errors.Add($"Invalid characters in: {file.NewName}");
                }
            }
        }

        if (warnLongPaths)
        {
            foreach (var file in fileList.Where(f => f.HasChanged))
            {
                var targetPath = Path.Combine(file.FolderPath, file.NewName);
                if (targetPath.Length > longPathThreshold)
                {
                    errors.Add($"Long path warning ({targetPath.Length}): {targetPath}");
                }
            }
        }

        return errors;
    }
}

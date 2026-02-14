using System.ComponentModel;
using System.IO;
using ReNamer.Models;

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
/// 规则基类
/// </summary>
public abstract class RuleBase : IRule
{
    private bool _isEnabled = true;
    private string _comment = "";

    public abstract string RuleName { get; }
    public abstract string Description { get; }

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
            }
        }
    }

    /// <summary>显示描述（包含注释）</summary>
    public string DisplayDescription =>
        string.IsNullOrWhiteSpace(Comment) ? Description : $"{Description}  // {Comment}";
    
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
    protected static (string baseName, string extension) SplitFileName(string fileName, bool skipExtension)
    {
        if (!skipExtension)
            return (fileName, "");
            
        var ext = Path.GetExtension(fileName);
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        return (baseName, ext);
    }
}

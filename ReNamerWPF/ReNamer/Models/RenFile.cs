using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace ReNamer.Models;

/// <summary>
/// 重命名文件对象 - 对应原版 TRenFile
/// </summary>
public class RenFile : INotifyPropertyChanged
{
    private string _newName;
    private bool _isRenamed;
    private bool _isMarked = true;
    private string _state = "";
    private string _error = "";
    private string _oldPath = "";
    private DateTime? _exifDate;
    private bool _exifLoaded;

    public RenFile(string fullPath)
    {
        FullPath = fullPath;
        OriginalName = Path.GetFileName(fullPath);
        _newName = OriginalName;
        FolderPath = Path.GetDirectoryName(fullPath) ?? "";
        Extension = Path.GetExtension(fullPath);
        
        if (File.Exists(fullPath))
        {
            var info = new FileInfo(fullPath);
            Size = info.Length;
            Created = info.CreationTime;
            Modified = info.LastWriteTime;
            IsFolder = false;
        }
        else if (Directory.Exists(fullPath))
        {
            IsFolder = true;
        }
    }

    #region 属性

    /// <summary>完整路径</summary>
    public string FullPath { get; private set; }

    /// <summary>原始文件名</summary>
    public string OriginalName { get; }

    /// <summary>新文件名</summary>
    public string NewName
    {
        get => _newName;
        set
        {
            if (_newName != value)
            {
                _newName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasChanged));
                OnPropertyChanged(nameof(NewPath));
                OnPropertyChanged(nameof(NewNameLength));
                OnPropertyChanged(nameof(NewPathLength));
            }
        }
    }

    /// <summary>文件夹路径</summary>
    public string FolderPath { get; }

    /// <summary>扩展名</summary>
    public string Extension { get; }

    /// <summary>文件大小</summary>
    public long Size { get; }

    /// <summary>显示大小</summary>
    public string SizeDisplay => FormatSize(Size);

    /// <summary>KB大小</summary>
    public string SizeKB => $"{Size / 1024.0:0.##} KB";

    /// <summary>MB大小</summary>
    public string SizeMB => $"{Size / 1048576.0:0.##} MB";

    /// <summary>创建时间</summary>
    public DateTime Created { get; }

    /// <summary>修改时间</summary>
    public DateTime Modified { get; }

    /// <summary>是否为文件夹</summary>
    public bool IsFolder { get; }

    /// <summary>是否已标记(参与重命名)</summary>
    public bool IsMarked
    {
        get => _isMarked;
        set
        {
            if (_isMarked != value)
            {
                _isMarked = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>是否已重命名</summary>
    public bool IsRenamed
    {
        get => _isRenamed;
        private set
        {
            _isRenamed = value;
            OnPropertyChanged();
            UpdateState();
        }
    }

    /// <summary>名称是否有变化</summary>
    public bool HasChanged => NewName != OriginalName;

    /// <summary>状态标记</summary>
    public string State
    {
        get => _state;
        set
        {
            _state = value;
            OnPropertyChanged();
        }
    }

    /// <summary>错误信息</summary>
    public string Error
    {
        get => _error;
        set
        {
            _error = value;
            OnPropertyChanged();
        }
    }

    /// <summary>旧路径(撤销用)</summary>
    public string OldPath
    {
        get => _oldPath;
        set
        {
            _oldPath = value;
            OnPropertyChanged();
        }
    }

    /// <summary>不含扩展名的基本名称</summary>
    public string BaseName => Path.GetFileNameWithoutExtension(OriginalName);

    /// <summary>父文件夹名称</summary>
    public string FolderName => new DirectoryInfo(FolderPath).Name;

    /// <summary>创建时间显示</summary>
    public string CreatedDisplay => Created.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>修改时间显示</summary>
    public string ModifiedDisplay => Modified.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>Exif 日期</summary>
    public DateTime? ExifDate
    {
        get
        {
            if (!_exifLoaded)
            {
                _exifDate = ReadExifDate();
                _exifLoaded = true;
            }
            return _exifDate;
        }
    }

    /// <summary>Exif 日期显示</summary>
    public string ExifDateDisplay => ExifDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";

    /// <summary>新完整路径</summary>
    public string NewPath => Path.Combine(FolderPath, NewName);

    /// <summary>文件名中的数字</summary>
    public string NameDigits => new string(OriginalName.Where(char.IsDigit).ToArray());

    /// <summary>路径中的数字</summary>
    public string PathDigits => new string(FullPath.Where(char.IsDigit).ToArray());

    /// <summary>文件名长度</summary>
    public int NameLength => OriginalName.Length;

    /// <summary>新文件名长度</summary>
    public int NewNameLength => NewName.Length;

    /// <summary>路径长度</summary>
    public int PathLength => FullPath.Length;

    /// <summary>新路径长度</summary>
    public int NewPathLength => NewPath.Length;

    #endregion

    #region 方法

    /// <summary>
    /// 执行重命名
    /// </summary>
    public bool Rename()
    {
        if (IsRenamed || !HasChanged) return false;

        try
        {
            var newPath = Path.Combine(FolderPath, NewName);
            
            if (File.Exists(newPath))
            {
                State = "×";
                Error = "Target file already exists";
                return false;
            }

            OldPath = FullPath;

            if (IsFolder)
            {
                Directory.Move(FullPath, newPath);
            }
            else
            {
                File.Move(FullPath, newPath);
            }

            FullPath = newPath;
            IsRenamed = true;
            State = "✓";
            Error = "";
            return true;
        }
        catch (Exception ex)
        {
            State = "×";
            Error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// 撤销重命名
    /// </summary>
    public bool UndoRename()
    {
        if (!IsRenamed) return false;

        try
        {
            var originalPath = !string.IsNullOrEmpty(OldPath)
                ? OldPath
                : Path.Combine(FolderPath, OriginalName);

            if (IsFolder)
            {
                Directory.Move(FullPath, originalPath);
            }
            else
            {
                File.Move(FullPath, originalPath);
            }

            FullPath = originalPath;
            IsRenamed = false;
            NewName = OriginalName;
            State = "";
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 重置新名称
    /// </summary>
    public void Reset()
    {
        NewName = OriginalName;
        State = "";
    }

    private void UpdateState()
    {
        if (IsRenamed)
            State = "✓";
        else if (HasChanged)
            State = "→";
        else
            State = "";
    }

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private DateTime? ReadExifDate()
    {
        try
        {
            if (!File.Exists(FullPath)) return null;
            using var stream = File.OpenRead(FullPath);
            var frame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
            if (frame.Metadata is BitmapMetadata metadata)
            {
                var dateTaken = metadata.DateTaken;
                if (!string.IsNullOrEmpty(dateTaken))
                {
                    if (DateTime.TryParse(dateTaken, out var dt)) return dt;
                    var normalized = NormalizeExifDate(dateTaken);
                    if (normalized != null && DateTime.TryParse(normalized, out dt)) return dt;
                }
            }
        }
        catch { }
        return null;
    }

    #endregion

    private static string? NormalizeExifDate(string value)
    {
        var first = value.IndexOf(':');
        if (first < 0) return null;
        var second = value.IndexOf(':', first + 1);
        if (second < 0) return null;
        var chars = value.ToCharArray();
        chars[first] = '-';
        chars[second] = '-';
        return new string(chars);
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}

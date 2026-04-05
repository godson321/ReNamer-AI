using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ReNamer.Models;

/// <summary>
/// 重命名文件对象 - 对应原版 TRenFile
/// </summary>
public class RenFile : INotifyPropertyChanged
{
    private static readonly SemaphoreSlim ExifReadGate = new(2, 2);
    private static readonly HashSet<string> ExifSupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".tif", ".tiff"
    };
    private string _originalName;
    private string _newName;
    private bool _isRenamed;
    private bool _isMarked = true;
    private bool _isNewNameManuallyEdited;
    private bool _isApplyingPreviewName;
    private string _state = "";
    private string _error = "";
    private string _oldPath = "";
    private string _baseName = "";
    private string _nameDigits = "";
    private int _nameLength;
    private string _pathDigits = "";
    private int _pathLength;
    private readonly string _sizeDisplay;
    private readonly string _sizeKb;
    private readonly string _sizeMb;
    private readonly string _createdDisplay;
    private readonly string _modifiedDisplay;
    private readonly string _folderName;
    private DateTime? _exifDate;
    private bool _exifLoaded;
    private bool _exifLoading;

    public RenFile(string fullPath)
        : this(RenFileImportData.FromPathOrPlaceholder(fullPath))
    {
    }

    public RenFile(RenFileImportData importData)
    {
        FullPath = importData.FullPath;
        _originalName = importData.OriginalName;
        _newName = _originalName;
        FolderPath = importData.FolderPath;
        Extension = importData.Extension;
        Size = importData.Size;
        Created = importData.Created;
        Modified = importData.Modified;
        IsFolder = importData.IsFolder;
        _sizeDisplay = FormatSize(Size);
        _sizeKb = $"{Size / 1024.0:0.##} KB";
        _sizeMb = $"{Size / 1048576.0:0.##} MB";
        _createdDisplay = Created == default ? string.Empty : Created.ToString("yyyy-MM-dd HH:mm:ss");
        _modifiedDisplay = Modified == default ? string.Empty : Modified.ToString("yyyy-MM-dd HH:mm:ss");
        _folderName = string.IsNullOrEmpty(FolderPath) ? string.Empty : new DirectoryInfo(FolderPath).Name;
        RefreshNameDerivedValues();
        RefreshPathDerivedValues();
    }

    #region 属性

    /// <summary>完整路径</summary>
    public string FullPath { get; private set; }

    /// <summary>原始文件名</summary>
    public string OriginalName
    {
        get => _originalName;
        private set
        {
            if (string.Equals(_originalName, value, StringComparison.Ordinal))
                return;

            _originalName = value;
            RefreshNameDerivedValues();
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasChanged));
            OnPropertyChanged(nameof(BaseName));
            OnPropertyChanged(nameof(NameDigits));
            OnPropertyChanged(nameof(NameLength));
        }
    }

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

            if (_isApplyingPreviewName)
            {
                if (string.Equals(_newName, OriginalName, StringComparison.Ordinal))
                    _isNewNameManuallyEdited = false;
            }
            else
            {
                _isNewNameManuallyEdited = !string.Equals(_newName, OriginalName, StringComparison.Ordinal);
            }
        }
    }

    public bool IsNewNameManuallyEdited => _isNewNameManuallyEdited;

    public void ApplyPreviewName(string value, bool force = false)
    {
        if (!force && _isNewNameManuallyEdited && !string.Equals(value, _newName, StringComparison.Ordinal))
            return;

        _isApplyingPreviewName = true;
        try
        {
            NewName = value;
        }
        finally
        {
            _isApplyingPreviewName = false;
        }
    }

    /// <summary>文件夹路径</summary>
    public string FolderPath { get; }

    /// <summary>扩展名</summary>
    public string Extension { get; }

    /// <summary>文件大小</summary>
    public long Size { get; }

    /// <summary>显示大小</summary>
    public string SizeDisplay => _sizeDisplay;

    /// <summary>KB大小</summary>
    public string SizeKB => _sizeKb;

    /// <summary>MB大小</summary>
    public string SizeMB => _sizeMb;

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

    public bool SetMarkedSilently(bool value)
    {
        if (_isMarked == value)
            return false;

        _isMarked = value;
        return true;
    }

    /// <summary>是否已重命名</summary>
    public bool IsRenamed
    {
        get => _isRenamed;
        private set
        {
            if (_isRenamed == value)
                return;
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
            if (_state == value)
                return;
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
            if (_error == value)
                return;
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
            if (_oldPath == value)
                return;
            _oldPath = value;
            OnPropertyChanged();
        }
    }

    /// <summary>不含扩展名的基本名称</summary>
    public string BaseName => _baseName;

    /// <summary>父文件夹名称</summary>
    public string FolderName => _folderName;

    /// <summary>创建时间显示</summary>
    public string CreatedDisplay => _createdDisplay;

    /// <summary>修改时间显示</summary>
    public string ModifiedDisplay => _modifiedDisplay;

    /// <summary>Exif 日期</summary>
    public DateTime? ExifDate
    {
        get
        {
            if (!_exifLoaded)
            {
                if (!ShouldAttemptExifRead())
                {
                    _exifLoaded = true;
                    return null;
                }

                _exifDate = ReadExifDate();
                _exifLoaded = true;
            }
            return _exifDate;
        }
    }

    /// <summary>Exif 日期显示</summary>
    public string ExifDateDisplay
    {
        get
        {
            if (!_exifLoaded)
            {
                EnsureExifDateLoadedAsync();
                return "";
            }

            return _exifDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
        }
    }

    /// <summary>新完整路径</summary>
    public string NewPath => Path.Combine(FolderPath, NewName);

    /// <summary>文件名中的数字</summary>
    public string NameDigits => _nameDigits;

    /// <summary>路径中的数字</summary>
    public string PathDigits => _pathDigits;

    /// <summary>文件名长度</summary>
    public int NameLength => _nameLength;

    /// <summary>新文件名长度</summary>
    public int NewNameLength => NewName.Length;

    /// <summary>路径长度</summary>
    public int PathLength => _pathLength;

    /// <summary>新路径长度</summary>
    public int NewPathLength => NewPath.Length;

    #endregion

    #region 方法

    /// <summary>
    /// 执行重命名
    /// </summary>
    public bool Rename()
    {
        if (!HasChanged) return false;

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

            SetFullPath(newPath);
            OriginalName = Path.GetFileName(newPath);
            _isNewNameManuallyEdited = false;
            NewName = OriginalName;
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
            if (string.IsNullOrEmpty(OldPath))
                return false;

            var originalPath = OldPath;

            if (IsFolder)
            {
                Directory.Move(FullPath, originalPath);
            }
            else
            {
                File.Move(FullPath, originalPath);
            }

            SetFullPath(originalPath);
            OriginalName = Path.GetFileName(originalPath);
            _isNewNameManuallyEdited = false;
            IsRenamed = false;
            NewName = OriginalName;
            OldPath = "";
            Error = "";
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

    private void RefreshNameDerivedValues()
    {
        _baseName = Path.GetFileNameWithoutExtension(_originalName);
        _nameDigits = new string(_originalName.Where(char.IsDigit).ToArray());
        _nameLength = _originalName.Length;
    }

    private void RefreshPathDerivedValues()
    {
        _pathDigits = new string(FullPath.Where(char.IsDigit).ToArray());
        _pathLength = FullPath.Length;
    }

    private void SetFullPath(string value)
    {
        if (string.Equals(FullPath, value, StringComparison.Ordinal))
            return;

        FullPath = value;
        RefreshPathDerivedValues();
        OnPropertyChanged(nameof(FullPath));
        OnPropertyChanged(nameof(PathDigits));
        OnPropertyChanged(nameof(PathLength));
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

    private void EnsureExifDateLoadedAsync()
    {
        if (_exifLoaded || _exifLoading)
            return;

        if (!ShouldAttemptExifRead())
        {
            _exifLoaded = true;
            return;
        }

        _exifLoading = true;
        _ = Task.Run(() =>
        {
            DateTime? exifDate = null;
            var gateEntered = false;
            try
            {
                ExifReadGate.Wait();
                gateEntered = true;
                exifDate = ReadExifDate();
            }
            catch
            {
                // Ignore metadata read failures for smoother list scrolling.
            }
            finally
            {
                if (gateEntered)
                    ExifReadGate.Release();
            }

            _exifDate = exifDate;
            _exifLoaded = true;
            _exifLoading = false;

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => OnPropertyChanged(nameof(ExifDateDisplay))));
            }
            else
            {
                OnPropertyChanged(nameof(ExifDateDisplay));
            }
        });
    }

    private bool ShouldAttemptExifRead()
    {
        if (IsFolder)
            return false;

        if (string.IsNullOrWhiteSpace(Extension))
            return false;

        if (!ExifSupportedExtensions.Contains(Extension))
            return false;

        return File.Exists(FullPath);
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

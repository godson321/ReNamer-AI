using System;

namespace ReNamer.Views.FileList;

public sealed class FileListColumn
{
    public FileListColumn(string key, string header)
    {
        Key = key;
        Header = header;
    }

    public string Key { get; }
    public string Header { get; set; }
    public int Width { get; set; } = 100;
    public bool Visible { get; set; } = true;
    public bool IsCheckBox { get; set; }
    public bool Sortable { get; set; } = true;
    public Func<object, IComparable>? SortValueGetter { get; set; }
    public Func<object, string>? TextGetter { get; set; }
}

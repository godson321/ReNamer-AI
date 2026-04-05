using System;
using System.Collections;
using System.Windows;

namespace ReNamer.Views.FileList;

public interface IFileListView
{
    UIElement View { get; }
    IList ItemsSource { get; set; }
    IList SelectedItems { get; }
    object? SelectedItem { get; set; }
    int SelectedIndex { get; set; }
    int SelectedCount { get; }
    void SelectAll();
    void ClearSelection();
    void SetSelectedItems(IList items);
    void Refresh();
    void UpdateLayout();
    void EnsureVisible(int index);
    event EventHandler? SelectionChanged;
}

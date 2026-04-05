using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReNamer.Views.FileList;

public sealed class WpfFileListViewAdapter : IFileListView
{
    private readonly DataGrid _grid;
    private readonly int _logicalSelectionThreshold;
    private readonly HashSet<object> _logicalSelectedItems = [];
    private readonly HashSet<DataGridRow> _realizedRows = [];
    private bool _isLogicalSelectionActive;
    private bool _suppressGridSelectionChanged;
    private object? _logicalCurrentItem;
    private int _logicalAnchorIndex = -1;

    public WpfFileListViewAdapter(DataGrid grid, int logicalSelectionThreshold = 1200)
    {
        _grid = grid;
        _logicalSelectionThreshold = Math.Max(1, logicalSelectionThreshold);
        _grid.SelectionChanged += OnGridSelectionChanged;
        _grid.LoadingRow += OnGridLoadingRow;
        _grid.UnloadingRow += OnGridUnloadingRow;
        _grid.PreviewMouseLeftButtonDown += OnGridPreviewMouseLeftButtonDown;
        _grid.PreviewMouseRightButtonDown += OnGridPreviewMouseRightButtonDown;
    }

    public UIElement View => _grid;

    public IList ItemsSource
    {
        get => _grid.ItemsSource as IList ?? Array.Empty<object>();
        set
        {
            ClearLogicalSelection(raiseSelectionChanged: false);
            _grid.ItemsSource = value;
        }
    }

    public IList SelectedItems => _isLogicalSelectionActive
        ? GetLogicalSelectedItemsSnapshot()
        : _grid.SelectedItems;

    public object? SelectedItem
    {
        get => _isLogicalSelectionActive ? ResolveLogicalCurrentItem() : _grid.SelectedItem;
        set
        {
            if (!_isLogicalSelectionActive)
            {
                _grid.SelectedItem = value;
                return;
            }

            if (value == null)
            {
                ClearSelection();
                return;
            }

            if (_logicalSelectedItems.Contains(value))
            {
                SetLogicalCurrentItem(value);
                _grid.ScrollIntoView(value);
                return;
            }

            SetExplicitSelection(new object[] { value });
        }
    }

    public int SelectedIndex
    {
        get => _isLogicalSelectionActive ? GetItemIndex(ResolveLogicalCurrentItem()) : _grid.SelectedIndex;
        set
        {
            if (!_isLogicalSelectionActive)
            {
                _grid.SelectedIndex = value;
                return;
            }

            if (value < 0 || value >= _grid.Items.Count)
            {
                ClearSelection();
                return;
            }

            var item = _grid.Items[value];
            if (_logicalSelectedItems.Contains(item))
            {
                SetLogicalCurrentItem(item, value);
                _grid.ScrollIntoView(item);
                return;
            }

            SetExplicitSelection(new object[] { item });
        }
    }

    public int SelectedCount => _isLogicalSelectionActive ? _logicalSelectedItems.Count : _grid.SelectedItems.Count;

    public bool IsLogicalSelectAllActive =>
        _isLogicalSelectionActive &&
        _grid.Items.Count > 0 &&
        _logicalSelectedItems.Count == _grid.Items.Count;

    public void SelectAll()
    {
        if (_grid.Items.Count >= _logicalSelectionThreshold)
        {
            SetLogicalSelection(_grid.Items.Cast<object>(), ResolveGridCurrentCandidate());
            return;
        }

        SetExplicitSelection(_grid.Items.Cast<object>().ToList());
    }

    public void ClearSelection()
    {
        if (_isLogicalSelectionActive)
        {
            ClearLogicalSelection(raiseSelectionChanged: true);
            return;
        }

        if (_grid.SelectedItems.Count == 0)
            return;

        _suppressGridSelectionChanged = true;
        try
        {
            _grid.SelectedItems.Clear();
        }
        finally
        {
            _suppressGridSelectionChanged = false;
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSelectedItems(IList items)
    {
        if (items.Count == 0)
        {
            ClearSelection();
            return;
        }

        if (items.Count >= _logicalSelectionThreshold)
        {
            SetLogicalSelection(items.Cast<object>(), items[0]);
            return;
        }

        SetExplicitSelection(items.Cast<object>().ToList());
    }

    public void Refresh()
    {
        _grid.Items.Refresh();
        if (_isLogicalSelectionActive)
            SyncRealizedRowsLogicalSelection();
    }

    public void UpdateLayout()
    {
        _grid.UpdateLayout();
        if (_isLogicalSelectionActive)
            SyncRealizedRowsLogicalSelection();
    }

    public void EnsureVisible(int index)
    {
        if (index < 0 || index >= _grid.Items.Count)
            return;

        _grid.ScrollIntoView(_grid.Items[index]);
    }

    public event EventHandler? SelectionChanged;

    private void OnGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppressGridSelectionChanged)
            return;

        if (_isLogicalSelectionActive)
        {
            if (_grid.SelectedItems.Count == 0)
                return;

            var selected = _grid.SelectedItems.Cast<object>().ToList();
            SetExplicitSelection(selected);
            return;
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnGridLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        _realizedRows.Add(e.Row);
        LogicalSelectionHelper.SetIsLogicalSelected(e.Row, _isLogicalSelectionActive && _logicalSelectedItems.Contains(e.Row.Item));
    }

    private void OnGridUnloadingRow(object? sender, DataGridRowEventArgs e)
    {
        _realizedRows.Remove(e.Row);
        LogicalSelectionHelper.SetIsLogicalSelected(e.Row, false);
    }

    private void OnGridPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isLogicalSelectionActive)
            return;

        if (!TryGetRowItem(e.OriginalSource as DependencyObject, out var item, out var index))
            return;

        var modifiers = Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift);
        switch (modifiers)
        {
            case ModifierKeys.Control:
                ToggleLogicalSelection(item, index);
                e.Handled = true;
                break;
            case ModifierKeys.Shift:
                ApplyLogicalRangeSelection(index, additive: false);
                e.Handled = true;
                break;
            case ModifierKeys.Control | ModifierKeys.Shift:
                ApplyLogicalRangeSelection(index, additive: true);
                e.Handled = true;
                break;
            default:
                SetExplicitSelection(new object[] { item });
                e.Handled = true;
                break;
        }
    }

    private void OnGridPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isLogicalSelectionActive)
            return;

        if (!TryGetRowItem(e.OriginalSource as DependencyObject, out var item, out var index))
            return;

        if (_logicalSelectedItems.Contains(item))
            SetLogicalCurrentItem(item, index);
    }

    private void SetLogicalSelection(IEnumerable<object> items, object? preferredCurrentItem)
    {
        ClearGridSelectionWithoutEvent();
        _logicalSelectedItems.Clear();
        foreach (var item in items)
            _logicalSelectedItems.Add(item);

        _isLogicalSelectionActive = _logicalSelectedItems.Count > 0;
        _logicalCurrentItem = ChooseLogicalCurrentItem(preferredCurrentItem);
        _logicalAnchorIndex = GetItemIndex(_logicalCurrentItem);
        SyncRealizedRowsLogicalSelection();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ClearLogicalSelection(bool raiseSelectionChanged)
    {
        if (!_isLogicalSelectionActive && _logicalSelectedItems.Count == 0)
            return;

        _isLogicalSelectionActive = false;
        _logicalSelectedItems.Clear();
        _logicalCurrentItem = null;
        _logicalAnchorIndex = -1;
        SyncRealizedRowsLogicalSelection();

        if (raiseSelectionChanged)
            SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetExplicitSelection(IList items)
    {
        ClearLogicalSelection(raiseSelectionChanged: false);
        _suppressGridSelectionChanged = true;
        try
        {
            _grid.SelectedItems.Clear();
            if (items.Count == _grid.Items.Count && _grid.Items.Count > 0)
            {
                _grid.SelectAll();
            }
            else
            {
                foreach (var item in items)
                    _grid.SelectedItems.Add(item);
            }
        }
        finally
        {
            _suppressGridSelectionChanged = false;
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ToggleLogicalSelection(object item, int index)
    {
        if (!_logicalSelectedItems.Remove(item))
            _logicalSelectedItems.Add(item);

        _logicalCurrentItem = item;
        _logicalAnchorIndex = index;
        UpdateRealizedRowLogicalSelection(item);

        if (_logicalSelectedItems.Count == 0)
        {
            ClearLogicalSelection(raiseSelectionChanged: true);
            return;
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyLogicalRangeSelection(int targetIndex, bool additive)
    {
        if (targetIndex < 0 || targetIndex >= _grid.Items.Count)
            return;

        var anchorIndex = _logicalAnchorIndex;
        if (anchorIndex < 0 || anchorIndex >= _grid.Items.Count)
            anchorIndex = targetIndex;

        if (!additive)
            _logicalSelectedItems.Clear();

        var start = Math.Min(anchorIndex, targetIndex);
        var end = Math.Max(anchorIndex, targetIndex);
        for (var index = start; index <= end; index++)
            _logicalSelectedItems.Add(_grid.Items[index]);

        _logicalCurrentItem = _grid.Items[targetIndex];
        SyncRealizedRowsLogicalSelection();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetLogicalCurrentItem(object item, int? index = null)
    {
        _logicalCurrentItem = item;
        _logicalAnchorIndex = index ?? GetItemIndex(item);
    }

    private object? ResolveLogicalCurrentItem()
    {
        if (!_isLogicalSelectionActive || _logicalSelectedItems.Count == 0)
            return null;

        if (_logicalCurrentItem != null && _logicalSelectedItems.Contains(_logicalCurrentItem))
            return _logicalCurrentItem;

        var currentCandidate = ResolveGridCurrentCandidate();
        if (currentCandidate != null && _logicalSelectedItems.Contains(currentCandidate))
        {
            _logicalCurrentItem = currentCandidate;
            return currentCandidate;
        }

        foreach (var item in _grid.Items)
        {
            if (!_logicalSelectedItems.Contains(item))
                continue;

            _logicalCurrentItem = item;
            return item;
        }

        return null;
    }

    private object? ChooseLogicalCurrentItem(object? preferredCurrentItem)
    {
        if (preferredCurrentItem != null && _logicalSelectedItems.Contains(preferredCurrentItem))
            return preferredCurrentItem;

        var currentCandidate = ResolveGridCurrentCandidate();
        if (currentCandidate != null && _logicalSelectedItems.Contains(currentCandidate))
            return currentCandidate;

        return _logicalSelectedItems.Count == 0
            ? null
            : _grid.Items.Cast<object>().FirstOrDefault(_logicalSelectedItems.Contains);
    }

    private object? ResolveGridCurrentCandidate()
    {
        if (GetItemIndex(_grid.CurrentItem) >= 0)
            return _grid.CurrentItem;
        return GetItemIndex(_grid.SelectedItem) >= 0 ? _grid.SelectedItem : null;
    }

    private int GetItemIndex(object? item)
    {
        return item == null ? -1 : _grid.Items.IndexOf(item);
    }

    private IList GetLogicalSelectedItemsSnapshot()
    {
        if (_logicalSelectedItems.Count == 0)
            return Array.Empty<object>();

        var items = new List<object>(_logicalSelectedItems.Count);
        foreach (var item in _grid.Items)
        {
            if (_logicalSelectedItems.Contains(item))
                items.Add(item);
        }

        return items;
    }

    private void ClearGridSelectionWithoutEvent()
    {
        _suppressGridSelectionChanged = true;
        try
        {
            _grid.SelectedItems.Clear();
        }
        finally
        {
            _suppressGridSelectionChanged = false;
        }
    }

    private void SyncRealizedRowsLogicalSelection()
    {
        foreach (var row in _realizedRows.ToArray())
            LogicalSelectionHelper.SetIsLogicalSelected(row, _isLogicalSelectionActive && _logicalSelectedItems.Contains(row.Item));
    }

    private void UpdateRealizedRowLogicalSelection(object item)
    {
        if (_grid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
            LogicalSelectionHelper.SetIsLogicalSelected(row, _isLogicalSelectionActive && _logicalSelectedItems.Contains(item));
    }

    private static bool TryGetRowItem(DependencyObject? originalSource, out object item, out int index)
    {
        for (var current = originalSource; current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is DataGridColumnHeader)
                break;

            if (current is DataGridRow row)
            {
                item = row.Item;
                index = row.GetIndex();
                return true;
            }

        }

        item = null!;
        index = -1;
        return false;
    }
}

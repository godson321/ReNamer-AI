using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static ReNamer.Views.FileList.Win32Interop;

namespace ReNamer.Views.FileList;

[SupportedOSPlatform("windows")]
public sealed class Win32FileListViewHost : HwndHost, IFileListView
{
    private readonly List<FileListColumn> _columns;
    private readonly bool _hasCheckBoxColumn;
    private HwndSource? _parentSource;
    private HwndSourceHook? _parentHook;
    private IList _itemsSource = Array.Empty<object>();
    private IntPtr _hwnd;
    private IntPtr _header;
    private bool _suppressSelectionEvent;
    private bool _suppressCheckStateEvent;
    private int _itemCount;
    private int _sortColumn = -1;
    private ListSortDirection? _sortDirection;
    private Win32FileListPalette _palette = Win32FileListPalette.CreateFallback();
    private bool? _headerCheckState;
    private IntPtr _normalFont;
    private IntPtr _boldFont;
    private uint _bgColorRef;
    private uint _altRowBgColorRef;
    private uint _textColorRef;
    private uint _selectionBgColorRef;
    private uint _selectionTextColorRef;
    private uint _headerBgColorRef;
    private uint _headerTextColorRef;
    private uint _headerBorderColorRef;
    private uint _headerSortedBgColorRef;
    private uint _headerSortedTextColorRef;

    public Win32FileListViewHost(List<FileListColumn> columns)
    {
        _columns = columns;
        _hasCheckBoxColumn = _columns.Count > 0
            && (_columns[0].IsCheckBox || string.Equals(_columns[0].Key, "IsMarked", StringComparison.OrdinalIgnoreCase));
        Focusable = true;
        CachePalette();
    }

    public UIElement View => this;

    public IList ItemsSource
    {
        get => _itemsSource;
        set
        {
            _itemsSource = value ?? Array.Empty<object>();
            _itemCount = _itemsSource.Count;
            if (_hwnd != IntPtr.Zero)
            {
                SendMessage(_hwnd, LVM_SETITEMCOUNT, (IntPtr)_itemCount, IntPtr.Zero);
                SyncAllNativeCheckStates();
            }
            Refresh();
        }
    }

    public IList SelectedItems => GetSelectedItemsSnapshot();

    public object? SelectedItem
    {
        get
        {
            int index = GetFirstSelectedIndex();
            if (index < 0 || index >= _itemsSource.Count)
                return null;
            return _itemsSource[index];
        }
        set
        {
            ClearSelection();
            if (value == null)
                return;
            var index = IndexOfItem(value);
            if (index >= 0)
                SetItemSelected(index, true);
        }
    }

    public int SelectedIndex
    {
        get => GetFirstSelectedIndex();
        set
        {
            ClearSelection();
            if (value >= 0 && value < _itemsSource.Count)
                SetItemSelected(value, true);
        }
    }

    public int SelectedCount => _hwnd == IntPtr.Zero ? 0 : (int)SendMessage(_hwnd, LVM_GETSELECTEDCOUNT, IntPtr.Zero, IntPtr.Zero);

    public void SelectAll()
    {
        if (_hwnd == IntPtr.Zero)
            return;
        _suppressSelectionEvent = true;
        SetItemState(-1, LVIS_SELECTED, LVIS_SELECTED);
        _suppressSelectionEvent = false;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearSelection()
    {
        if (_hwnd == IntPtr.Zero)
            return;
        _suppressSelectionEvent = true;
        SetItemState(-1, 0, LVIS_SELECTED);
        _suppressSelectionEvent = false;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSelectedItems(IList items)
    {
        if (_hwnd == IntPtr.Zero)
            return;
        _suppressSelectionEvent = true;
        SetItemState(-1, 0, LVIS_SELECTED);
        foreach (var item in items)
        {
            int idx = IndexOfItem(item);
            if (idx >= 0)
                SetItemSelected(idx, true);
        }
        _suppressSelectionEvent = false;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Refresh()
    {
        if (_hwnd == IntPtr.Zero)
            return;
        InvalidateRect(_hwnd, IntPtr.Zero, true);
    }

    public new void UpdateLayout()
    {
        if (_hwnd == IntPtr.Zero)
            return;
        UpdateWindow(_hwnd);
    }

    public void EnsureVisible(int index)
    {
        if (_hwnd == IntPtr.Zero || index < 0)
            return;
        SendMessage(_hwnd, LVM_ENSUREVISIBLE, (IntPtr)index, new IntPtr(1));
    }

    public event EventHandler? SelectionChanged;

    public readonly struct CellStyle
    {
        public CellStyle(Color? textColor, bool bold = false)
        {
            TextColor = textColor;
            Bold = bold;
        }

        public Color? TextColor { get; }
        public bool Bold { get; }
    }

    public Action<int, int>? ItemActivated { get; set; }
    public Action<int>? ColumnHeaderClick { get; set; }
    public Action<int>? ColumnHeaderRightClick { get; set; }
    public Action<int>? ColumnHeaderAutoSize { get; set; }
    public Action<Point>? ContextMenuRequested { get; set; }
    public Action<int, int>? ReorderRequest { get; set; }
    public Action<string[]>? ExternalFilesDropped { get; set; }
    public Action<Key, ModifierKeys>? KeyGesture { get; set; }
    public Func<int, int, string>? CellTextProvider { get; set; }
    public Func<int, int, bool, CellStyle?>? CellStyleProvider { get; set; }
    public Func<int, bool>? ItemCheckedProvider { get; set; }
    public Action<int, bool>? ItemCheckedChanged { get; set; }
    public Action<int>? HeaderCheckToggle { get; set; }

    public void SetSortIndicator(int columnIndex, ListSortDirection? direction)
    {
        _sortColumn = columnIndex;
        _sortDirection = direction;
        if (_header == IntPtr.Zero)
            return;
        int count = (int)SendMessage(_header, HDM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);
        for (int i = 0; i < count; i++)
        {
            var item = new HDITEM
            {
                mask = HDI_FORMAT
            };
            SendMessage(_header, HDM_GETITEM, (IntPtr)i, ref item);
            item.fmt &= ~(HDF_SORTUP | HDF_SORTDOWN);
            if (i == columnIndex && direction.HasValue)
                item.fmt |= direction == ListSortDirection.Ascending ? HDF_SORTUP : HDF_SORTDOWN;
            SendMessage(_header, HDM_SETITEM, (IntPtr)i, ref item);
        }
        InvalidateHeader();
    }

    public IReadOnlyList<FileListColumn> Columns => _columns;

    public Win32FileListPalette Palette
    {
        get => _palette;
        set
        {
            _palette = value ?? Win32FileListPalette.CreateFallback();
            CachePalette();
            ApplyPaletteToNative();
            Refresh();
            InvalidateHeader();
        }
    }

    public void SetHeaderCheckState(bool? state)
    {
        _headerCheckState = state;
        InvalidateHeader();
    }

    public void ToggleHeaderCheckState()
    {
        _headerCheckState = _headerCheckState == true ? false : true;
        InvalidateHeader();
    }

    public void ApplyColumnVisibility()
    {
        if (_hwnd == IntPtr.Zero)
            return;
        for (int i = 0; i < _columns.Count; i++)
        {
            var col = _columns[i];
            int width = col.Visible ? Math.Max(20, col.Width) : 0;
            SendMessage(_hwnd, LVM_SETCOLUMNWIDTH, (IntPtr)i, (IntPtr)width);
        }
    }

    private void CachePalette()
    {
        _bgColorRef = ToColorRef(_palette.Background);
        _altRowBgColorRef = ToColorRef(_palette.AlternateRowBackground);
        _textColorRef = ToColorRef(_palette.Text);
        _selectionBgColorRef = ToColorRef(_palette.SelectionBackground);
        _selectionTextColorRef = ToColorRef(_palette.SelectionText);
        _headerBgColorRef = ToColorRef(_palette.HeaderBackground);
        _headerTextColorRef = ToColorRef(_palette.HeaderText);
        _headerBorderColorRef = ToColorRef(_palette.HeaderBorder);
        _headerSortedBgColorRef = ToColorRef(_palette.HeaderSortedBackground);
        _headerSortedTextColorRef = ToColorRef(_palette.HeaderSortedText);
    }

    private void ApplyPaletteToNative()
    {
        if (_hwnd == IntPtr.Zero)
            return;
        SendMessage(_hwnd, LVM_SETBKCOLOR, IntPtr.Zero, (IntPtr)_bgColorRef);
        SendMessage(_hwnd, LVM_SETTEXTCOLOR, IntPtr.Zero, (IntPtr)_textColorRef);
        SendMessage(_hwnd, LVM_SETTEXTBKCOLOR, IntPtr.Zero, (IntPtr)_bgColorRef);
    }

    private void InitializeFonts()
    {
        DisposeFonts();
        using var baseFont = System.Drawing.SystemFonts.MessageBoxFont ?? System.Drawing.SystemFonts.DefaultFont;
        _normalFont = baseFont.ToHfont();
        using var boldFont = new System.Drawing.Font(baseFont, System.Drawing.FontStyle.Bold);
        _boldFont = boldFont.ToHfont();
        SendMessage(_hwnd, WM_SETFONT, _normalFont, (IntPtr)1);
        if (_header != IntPtr.Zero)
            SendMessage(_header, WM_SETFONT, _normalFont, (IntPtr)1);
    }

    private void DisposeFonts()
    {
        if (_normalFont != IntPtr.Zero)
        {
            DeleteObject(_normalFont);
            _normalFont = IntPtr.Zero;
        }

        if (_boldFont != IntPtr.Zero)
        {
            DeleteObject(_boldFont);
            _boldFont = IntPtr.Zero;
        }
    }

    private void InvalidateHeader()
    {
        if (_header != IntPtr.Zero)
            InvalidateRect(_header, IntPtr.Zero, true);
    }

    public void UpdateColumnWidth(int columnIndex, int width)
    {
        if (columnIndex < 0 || columnIndex >= _columns.Count)
            return;
        _columns[columnIndex].Width = Math.Max(16, width);
        if (_hwnd != IntPtr.Zero)
            SendMessage(_hwnd, LVM_SETCOLUMNWIDTH, (IntPtr)columnIndex, (IntPtr)_columns[columnIndex].Width);
    }

    public int GetColumnWidth(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= _columns.Count)
            return 0;
        if (_hwnd == IntPtr.Zero)
            return _columns[columnIndex].Width;
        return (int)SendMessage(_hwnd, LVM_GETCOLUMNWIDTH, (IntPtr)columnIndex, IntPtr.Zero);
    }

    public void UpdateColumnHeader(int columnIndex, string text)
    {
        if (_hwnd == IntPtr.Zero || columnIndex < 0 || columnIndex >= _columns.Count)
            return;
        var lvc = new LVCOLUMN
        {
            mask = LVCF_TEXT,
            pszText = text ?? string.Empty,
            cchTextMax = text?.Length ?? 0
        };
        SendMessage(_hwnd, LVM_SETCOLUMN, (IntPtr)columnIndex, ref lvc);
    }

    public void AutoSizeColumn(int columnIndex)
    {
        if (_hwnd == IntPtr.Zero || columnIndex < 0 || columnIndex >= _columns.Count)
            return;
        // Size to header first, then expand based on visible items only.
        SendMessage(_hwnd, LVM_SETCOLUMNWIDTH, (IntPtr)columnIndex, (IntPtr)LVSCW_AUTOSIZE_USEHEADER);
        int headerWidth = (int)SendMessage(_hwnd, LVM_GETCOLUMNWIDTH, (IntPtr)columnIndex, IntPtr.Zero);
        int top = (int)SendMessage(_hwnd, LVM_GETTOPINDEX, IntPtr.Zero, IntPtr.Zero);
        int perPage = (int)SendMessage(_hwnd, LVM_GETCOUNTPERPAGE, IntPtr.Zero, IntPtr.Zero);
        int end = Math.Min(_itemsSource.Count, top + Math.Max(1, perPage));
        int max = headerWidth;
        using var gfx = System.Drawing.Graphics.FromHwnd(_hwnd);
        using var font = System.Drawing.SystemFonts.MessageBoxFont;
        for (int i = top; i < end; i++)
        {
            var text = CellTextProvider?.Invoke(i, columnIndex) ?? string.Empty;
            var size = System.Windows.Forms.TextRenderer.MeasureText(gfx, text, font, new System.Drawing.Size(int.MaxValue, int.MaxValue), System.Windows.Forms.TextFormatFlags.NoPrefix);
            if (size.Width + 12 > max)
                max = size.Width + 12;
        }
        UpdateColumnWidth(columnIndex, max);
    }

    public void ToggleHeaderCheck()
    {
        HeaderCheckToggle?.Invoke(0);
    }

    public int HitTestIndexFromScreenPoint(Point screenPoint)
        => HitTestIndex(screenPoint);

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        InitCommonControls();
        _parentSource = HwndSource.FromHwnd(hwndParent.Handle);
        _parentHook = ParentWndProc;
        _parentSource?.AddHook(_parentHook);

        _hwnd = CreateWindowEx(
            0,
            "SysListView32",
            string.Empty,
            WS_CHILD | WS_VISIBLE | LVS_REPORT | LVS_OWNERDATA | LVS_SHOWSELALWAYS,
            0, 0, 0, 0,
            hwndParent.Handle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

        if (_hwnd == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create Win32 ListView.");

        int extendedStyle = LVS_EX_FULLROWSELECT | LVS_EX_GRIDLINES | LVS_EX_DOUBLEBUFFER;
        if (_hasCheckBoxColumn)
            extendedStyle |= LVS_EX_CHECKBOXES;

        SendMessage(_hwnd, LVM_SETEXTENDEDLISTVIEWSTYLE, IntPtr.Zero,
            (IntPtr)extendedStyle);

        _header = SendMessage(_hwnd, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
        InitializeFonts();
        ApplyPaletteToNative();

        InsertColumns();
        SendMessage(_hwnd, LVM_SETITEMCOUNT, (IntPtr)_itemsSource.Count, IntPtr.Zero);
        SyncAllNativeCheckStates();
        ApplyColumnVisibility();

        DragAcceptFiles(_hwnd, true);

        return new HandleRef(this, _hwnd);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        if (_parentSource != null && _parentHook != null)
            _parentSource.RemoveHook(_parentHook);
        _parentHook = null;
        _parentSource = null;

        if (_hwnd != IntPtr.Zero)
            DestroyWindow(_hwnd);
        _hwnd = IntPtr.Zero;
        DisposeFonts();
    }

    private IntPtr ParentWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WM_NOTIFY || lParam == IntPtr.Zero)
            return IntPtr.Zero;

        var hdr = Marshal.PtrToStructure<NMHDR>(lParam);
        if (hdr.hwndFrom != _hwnd && hdr.hwndFrom != _header)
            return IntPtr.Zero;

        return HandleNotify(lParam, ref handled);
    }

    protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_NOTIFY:
                return HandleNotify(lParam, ref handled);
            case WM_CONTEXTMENU:
                HandleContextMenu(lParam);
                handled = true;
                break;
            case WM_DROPFILES:
                HandleDropFiles(wParam);
                handled = true;
                break;
            case WM_KEYDOWN:
                HandleKeyDown(wParam);
                handled = true;
                break;
        }
        return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
    }

    private void HandleKeyDown(IntPtr wParam)
    {
        var key = KeyInterop.KeyFromVirtualKey(wParam.ToInt32());
        var modifiers = Keyboard.Modifiers;
        KeyGesture?.Invoke(key, modifiers);
    }

    private void HandleContextMenu(IntPtr lParam)
    {
        if (ContextMenuRequested == null)
            return;
        int x = LOWORD(lParam);
        int y = HIWORD(lParam);
        if (x == -1 && y == -1)
        {
            var pt = new POINT();
            GetCursorPos(ref pt);
            x = pt.X;
            y = pt.Y;
        }
        ContextMenuRequested(new Point(x, y));
    }

    private void HandleDropFiles(IntPtr hDrop)
    {
        if (ExternalFilesDropped == null)
            return;
        uint count = DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);
        var files = new List<string>((int)count);
        var sb = new System.Text.StringBuilder(260);
        for (uint i = 0; i < count; i++)
        {
            sb.Clear();
            DragQueryFile(hDrop, i, sb, sb.Capacity);
            files.Add(sb.ToString());
        }
        DragFinish(hDrop);
        ExternalFilesDropped(files.ToArray());
    }

    private IntPtr HandleNotify(IntPtr lParam, ref bool handled)
    {
        var hdr = Marshal.PtrToStructure<NMHDR>(lParam);
        if (hdr.hwndFrom == _hwnd)
        {
            switch ((int)hdr.code)
            {
                case NM_CUSTOMDRAW:
                    return HandleListViewCustomDraw(lParam, ref handled);
                case LVN_GETDISPINFOA:
                case LVN_GETDISPINFOW:
                    HandleGetDispInfo(lParam);
                    handled = true;
                    break;
                case LVN_ITEMCHANGED:
                    HandleItemChanged(lParam);
                    handled = true;
                    break;
                case LVN_COLUMNCLICK:
                    HandleColumnClick(lParam);
                    handled = true;
                    break;
                case NM_CLICK:
                    HandleClick(lParam);
                    handled = true;
                    break;
                case NM_DBLCLK:
                    HandleDoubleClick(lParam);
                    handled = true;
                    break;
                case LVN_BEGINDRAG:
                    HandleBeginDrag(lParam);
                    handled = true;
                    break;
                case NM_RCLICK:
                    HandleContextMenuFromNotify();
                    handled = true;
                    break;
            }
        }
        else if (hdr.hwndFrom == _header)
        {
            switch ((int)hdr.code)
            {
                case NM_CUSTOMDRAW:
                    return HandleHeaderCustomDraw(lParam, ref handled);
                case HDN_ITEMRCLICK:
                    HandleHeaderRightClick(lParam);
                    handled = true;
                    break;
                case HDN_DIVIDERDBLCLICK:
                    HandleHeaderAutoSize(lParam);
                    handled = true;
                    break;
            }
        }
        return IntPtr.Zero;
    }

    private void HandleGetDispInfo(IntPtr lParam)
    {
        var disp = Marshal.PtrToStructure<NMLVDISPINFO>(lParam);
        bool stateUpdated = false;
        int sub = disp.item.iSubItem;
        if ((disp.item.mask & LVIF_TEXT) != 0 && disp.item.pszText != IntPtr.Zero && disp.item.cchTextMax > 0)
        {
            var text = CellTextProvider?.Invoke(disp.item.iItem, sub) ?? string.Empty;
            if (text.Length >= disp.item.cchTextMax)
                text = text.Substring(0, disp.item.cchTextMax - 1);
            var buffer = (text + "\0").ToCharArray();
            Marshal.Copy(buffer, 0, disp.item.pszText, buffer.Length);
        }

        if (_hasCheckBoxColumn
            && ItemCheckedProvider != null
            && sub <= 0
            && disp.item.iItem >= 0
            && (disp.item.mask & LVIF_STATE) != 0)
        {
            disp.item.stateMask = LVIS_STATEIMAGEMASK;
            disp.item.state = StateImageMask(ItemCheckedProvider(disp.item.iItem));
            stateUpdated = true;
        }

        if (stateUpdated)
            Marshal.StructureToPtr(disp, lParam, false);
    }

    private void HandleItemChanged(IntPtr lParam)
    {
        var info = Marshal.PtrToStructure<NMLISTVIEW>(lParam);
        if (!_suppressSelectionEvent && (info.uChanged & LVIF_STATE) != 0)
        {
            var oldSelected = (info.uOldState & LVIS_SELECTED) != 0;
            var newSelected = (info.uNewState & LVIS_SELECTED) != 0;
            if (oldSelected != newSelected)
                SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        if (_suppressCheckStateEvent
            || !_hasCheckBoxColumn
            || ItemCheckedChanged == null
            || ItemCheckedProvider == null
            || info.iItem < 0
            || (info.uChanged & LVIF_STATE) == 0)
            return;

        var oldCheckMask = info.uOldState & LVIS_STATEIMAGEMASK;
        var newCheckMask = info.uNewState & LVIS_STATEIMAGEMASK;
        if (newCheckMask == 0 || oldCheckMask == newCheckMask)
            return;

        bool isChecked = IsStateImageChecked(info.uNewState);
        ItemCheckedChanged(info.iItem, isChecked);

        bool expected = ItemCheckedProvider(info.iItem);
        if (expected != isChecked)
        {
            _suppressCheckStateEvent = true;
            try
            {
                SetNativeItemChecked(info.iItem, expected);
            }
            finally
            {
                _suppressCheckStateEvent = false;
            }
        }

        InvalidateItem(info.iItem);
    }

    private void HandleColumnClick(IntPtr lParam)
    {
        var info = Marshal.PtrToStructure<NMLISTVIEW>(lParam);
        ColumnHeaderClick?.Invoke(info.iSubItem);
    }

    private void HandleDoubleClick(IntPtr lParam)
    {
        var info = Marshal.PtrToStructure<NMITEMACTIVATE>(lParam);
        if (info.iItem >= 0)
            ItemActivated?.Invoke(info.iItem, info.iSubItem);
    }

    private void HandleClick(IntPtr lParam)
    {
        if (_hasCheckBoxColumn)
            return;

        if (ItemCheckedChanged == null || ItemCheckedProvider == null)
            return;

        var info = Marshal.PtrToStructure<NMITEMACTIVATE>(lParam);
        if (info.iItem < 0)
            return;

        int sub = info.iSubItem < 0 ? 0 : info.iSubItem;
        if (!IsCheckBoxColumn(sub))
            return;

        var current = ItemCheckedProvider(info.iItem);
        ItemCheckedChanged(info.iItem, !current);
        InvalidateItem(info.iItem);
    }

    private void HandleBeginDrag(IntPtr lParam)
    {
        var info = Marshal.PtrToStructure<NMLISTVIEW>(lParam);
        if (info.iItem < 0)
            return;
        CaptureMouse();
        _dragIndex = info.iItem;
    }

    private int _dragIndex = -1;

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragIndex < 0 || e.LeftButton != MouseButtonState.Pressed)
            return;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (_dragIndex < 0)
            return;
        ReleaseMouseCapture();
        var pos = e.GetPosition(this);
        int target = HitTestIndex(pos);
        if (target >= 0 && target != _dragIndex)
            ReorderRequest?.Invoke(_dragIndex, target);
        _dragIndex = -1;
    }

    private void HandleContextMenuFromNotify()
    {
        if (ContextMenuRequested == null)
            return;
        var pt = new POINT();
        GetCursorPos(ref pt);
        ContextMenuRequested(new Point(pt.X, pt.Y));
    }

    private void HandleHeaderRightClick(IntPtr lParam)
    {
        var info = Marshal.PtrToStructure<NMHEADER>(lParam);
        ColumnHeaderRightClick?.Invoke(info.iItem);
    }

    private void HandleHeaderAutoSize(IntPtr lParam)
    {
        var info = Marshal.PtrToStructure<NMHEADER>(lParam);
        ColumnHeaderAutoSize?.Invoke(info.iItem);
    }

    private IntPtr HandleListViewCustomDraw(IntPtr lParam, ref bool handled)
    {
        var draw = Marshal.PtrToStructure<NMLVCUSTOMDRAW>(lParam);
        switch (draw.nmcd.dwDrawStage)
        {
            case CDDS_PREPAINT:
                handled = true;
                return (IntPtr)CDRF_NOTIFYITEMDRAW;
            case CDDS_ITEMPREPAINT:
                handled = true;
                return (IntPtr)CDRF_NOTIFYSUBITEMDRAW;
            case CDDS_ITEMPREPAINT | CDDS_SUBITEM:
                {
                    int row = (int)draw.nmcd.dwItemSpec;
                    int col = draw.iSubItem;
                    bool selected = (draw.nmcd.uItemState & CDIS_SELECTED) != 0;

                    uint textColor = selected ? _selectionTextColorRef : _textColorRef;
                    uint backColor = selected
                        ? _selectionBgColorRef
                        : (row % 2 == 0 ? _bgColorRef : _altRowBgColorRef);
                    bool bold = false;

                    if (CellStyleProvider != null)
                    {
                        var style = CellStyleProvider(row, col, selected);
                        if (style.HasValue)
                        {
                            if (style.Value.TextColor.HasValue)
                                textColor = ToColorRef(style.Value.TextColor.Value);
                            bold = style.Value.Bold;
                        }
                    }

                    draw.clrText = textColor;
                    draw.clrTextBk = backColor;
                    Marshal.StructureToPtr(draw, lParam, false);

                    if (_normalFont != IntPtr.Zero)
                        SelectObject(draw.nmcd.hdc, bold && _boldFont != IntPtr.Zero ? _boldFont : _normalFont);

                    handled = true;
                    return _normalFont != IntPtr.Zero ? (IntPtr)CDRF_NEWFONT : (IntPtr)CDRF_DODEFAULT;
                }
        }

        return IntPtr.Zero;
    }

    private IntPtr HandleHeaderCustomDraw(IntPtr lParam, ref bool handled)
    {
        var draw = Marshal.PtrToStructure<NMCUSTOMDRAW>(lParam);
        switch (draw.dwDrawStage)
        {
            case CDDS_PREPAINT:
                handled = true;
                return (IntPtr)CDRF_NOTIFYITEMDRAW;
            case CDDS_ITEMPREPAINT:
                {
                    int index = (int)draw.dwItemSpec;
                    DrawHeaderItem(draw.hdc, draw.rc, index);
                    handled = true;
                    return (IntPtr)CDRF_SKIPDEFAULT;
                }
        }

        return IntPtr.Zero;
    }

    private void DrawHeaderItem(IntPtr hdc, RECT rect, int index)
    {
        if (index < 0 || index >= _columns.Count)
            return;

        bool isSorted = index == _sortColumn && _sortDirection.HasValue;
        uint bg = isSorted ? _headerSortedBgColorRef : _headerBgColorRef;
        uint fg = isSorted ? _headerSortedTextColorRef : _headerTextColorRef;

        using var bgBrush = new GdiObject(CreateSolidBrush(bg));
        FillRect(hdc, ref rect, bgBrush.Handle);

        var borderRect = rect;
        borderRect.top = rect.bottom - 1;
        using var borderBrush = new GdiObject(CreateSolidBrush(_headerBorderColorRef));
        FillRect(hdc, ref borderRect, borderBrush.Handle);
        borderRect = rect;
        borderRect.left = rect.right - 1;
        FillRect(hdc, ref borderRect, borderBrush.Handle);

        if (IsCheckBoxColumn(index))
        {
            DrawHeaderCheckBox(hdc, rect);
            return;
        }

        var text = _columns[index].Header ?? string.Empty;
        var textRect = rect;
        textRect.left += 8;
        textRect.right -= 8;
        if (isSorted)
            textRect.right -= 12;

        SetBkMode(hdc, TRANSPARENT);
        SetTextColor(hdc, fg);
        DrawText(hdc, text, text.Length, ref textRect, DT_SINGLELINE | DT_VCENTER | DT_LEFT | DT_END_ELLIPSIS);

        if (isSorted)
            DrawSortArrow(hdc, rect, fg, _sortDirection == ListSortDirection.Descending);
    }

    private void DrawHeaderCheckBox(IntPtr hdc, RECT rect)
    {
        int size = 15;
        int cx = rect.left + Math.Max(0, (rect.right - rect.left - size) / 2);
        int cy = rect.top + Math.Max(0, (rect.bottom - rect.top - size) / 2);
        var box = new RECT
        {
            left = cx,
            top = cy,
            right = cx + size,
            bottom = cy + size
        };

        uint state = DFCS_BUTTONCHECK;
        if (_headerCheckState == true)
            state |= DFCS_CHECKED;
        else if (_headerCheckState == null)
            state |= DFCS_BUTTON3STATE | DFCS_CHECKED;

        DrawFrameControl(hdc, ref box, DFC_BUTTON, state);
    }

    private void DrawSortArrow(IntPtr hdc, RECT rect, uint color, bool isDescending)
    {
        int size = 7;
        int right = rect.right - 10;
        int centerY = rect.top + (rect.bottom - rect.top) / 2;

        var points = isDescending
            ? new[]
            {
                new POINT { X = right - size, Y = centerY - size / 2 },
                new POINT { X = right, Y = centerY - size / 2 },
                new POINT { X = right - size / 2, Y = centerY + size / 2 }
            }
            : new[]
            {
                new POINT { X = right - size / 2, Y = centerY - size / 2 },
                new POINT { X = right - size, Y = centerY + size / 2 },
                new POINT { X = right, Y = centerY + size / 2 }
            };

        using var brush = new GdiObject(CreateSolidBrush(color));
        var oldBrush = SelectObject(hdc, brush.Handle);
        Polygon(hdc, points, points.Length);
        SelectObject(hdc, oldBrush);
    }

    private bool IsCheckBoxColumn(int index)
    {
        if (index < 0 || index >= _columns.Count)
            return false;
        return _columns[index].IsCheckBox || string.Equals(_columns[index].Key, "IsMarked", StringComparison.OrdinalIgnoreCase);
    }

    private int HitTestIndex(Point position)
    {
        if (_hwnd == IntPtr.Zero)
            return -1;
        var client = PointToClient(position);
        var info = new LVHITTESTINFO
        {
            pt = new POINT { X = (int)client.X, Y = (int)client.Y }
        };
        int index = (int)SendMessage(_hwnd, LVM_HITTEST, IntPtr.Zero, ref info);
        return index;
    }

    private void InvalidateItem(int index)
    {
        if (_hwnd == IntPtr.Zero || index < 0)
            return;
        var rect = new RECT { left = LVIR_BOUNDS };
        SendMessage(_hwnd, LVM_GETITEMRECT, (IntPtr)index, ref rect);
        InvalidateRect(_hwnd, ref rect, false);
    }

    private Point PointToClient(Point position)
    {
        var pt = new POINT { X = (int)position.X, Y = (int)position.Y };
        ScreenToClient(_hwnd, ref pt);
        return new Point(pt.X, pt.Y);
    }

    private IList GetSelectedItemsSnapshot()
    {
        var list = new List<object>();
        if (_hwnd == IntPtr.Zero)
            return list;
        int index = -1;
        while (true)
        {
            index = (int)SendMessage(_hwnd, LVM_GETNEXTITEM, (IntPtr)index, (IntPtr)LVNI_SELECTED);
            if (index < 0)
                break;
            if (index >= 0 && index < _itemsSource.Count)
            {
                var item = _itemsSource[index];
                if (item != null)
                    list.Add(item);
            }
        }
        return list;
    }

    private int GetFirstSelectedIndex()
    {
        if (_hwnd == IntPtr.Zero)
            return -1;
        return (int)SendMessage(_hwnd, LVM_GETNEXTITEM, new IntPtr(-1), (IntPtr)LVNI_SELECTED);
    }

    private void SetItemSelected(int index, bool selected)
    {
        if (_hwnd == IntPtr.Zero)
            return;
        uint state = selected ? (uint)LVIS_SELECTED : 0u;
        SetItemState(index, state, (uint)LVIS_SELECTED);
    }

    private int IndexOfItem(object item)
    {
        for (int i = 0; i < _itemsSource.Count; i++)
        {
            if (ReferenceEquals(_itemsSource[i], item))
                return i;
        }
        return -1;
    }

    private void InsertColumns()
    {
        for (int i = 0; i < _columns.Count; i++)
        {
            var col = _columns[i];
            var lvc = new LVCOLUMN
            {
                mask = LVCF_TEXT | LVCF_WIDTH | LVCF_FMT,
                fmt = IsCheckBoxColumn(i) ? LVCFMT_CENTER : LVCFMT_LEFT,
                pszText = col.Header,
                cx = Math.Max(20, col.Width)
            };
            SendMessage(_hwnd, LVM_INSERTCOLUMN, (IntPtr)i, ref lvc);
        }
    }

    private void SetItemState(int index, uint state, uint mask)
    {
        var item = new LVITEM
        {
            mask = LVIF_STATE,
            stateMask = mask,
            state = state
        };
        SendMessage(_hwnd, LVM_SETITEMSTATE, (IntPtr)index, ref item);
    }

    private static uint StateImageMask(bool isChecked)
        => (uint)((isChecked ? 2 : 1) << 12);

    private static bool IsStateImageChecked(uint state)
        => ((state & LVIS_STATEIMAGEMASK) >> 12) >= 2;

    private void SetNativeItemChecked(int index, bool isChecked)
    {
        if (_hwnd == IntPtr.Zero || index < 0 || index >= _itemCount || !_hasCheckBoxColumn)
            return;
        SetItemState(index, StateImageMask(isChecked), LVIS_STATEIMAGEMASK);
    }

    private void SyncAllNativeCheckStates()
    {
        if (_hwnd == IntPtr.Zero || !_hasCheckBoxColumn || ItemCheckedProvider == null)
            return;

        _suppressCheckStateEvent = true;
        try
        {
            for (int i = 0; i < _itemCount; i++)
                SetNativeItemChecked(i, ItemCheckedProvider(i));
        }
        finally
        {
            _suppressCheckStateEvent = false;
        }
    }

}

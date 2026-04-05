using System;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media;

namespace ReNamer.Views.FileList;

[SupportedOSPlatform("windows")]
internal static class Win32Interop
{
    internal const int WS_CHILD = 0x40000000;
    internal const int WS_VISIBLE = 0x10000000;
    internal const int LVS_REPORT = 0x0001;
    internal const int LVS_OWNERDATA = 0x1000;
    internal const int LVS_SHOWSELALWAYS = 0x0008;
    internal const int LVS_EX_FULLROWSELECT = 0x20;
    internal const int LVS_EX_GRIDLINES = 0x00000001;
    internal const int LVS_EX_DOUBLEBUFFER = 0x00010000;
    internal const int LVS_EX_CHECKBOXES = 0x00000004;

    internal const int WM_NOTIFY = 0x004E;
    internal const int WM_CONTEXTMENU = 0x007B;
    internal const int WM_DROPFILES = 0x0233;
    internal const int WM_KEYDOWN = 0x0100;
    internal const int WM_ERASEBKGND = 0x0014;
    internal const int WM_MOUSEMOVE = 0x0200;
    internal const int WM_MOUSELEAVE = 0x02A3;
    internal const int WM_SETFONT = 0x0030;

    internal const int LVM_FIRST = 0x1000;
    internal const int LVM_SETBKCOLOR = LVM_FIRST + 1;
    internal const int LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
    internal const int LVM_SETTEXTCOLOR = LVM_FIRST + 36;
    internal const int LVM_SETTEXTBKCOLOR = LVM_FIRST + 38;
    internal const int LVM_INSERTCOLUMN = LVM_FIRST + 97;
    internal const int LVM_SETCOLUMN = LVM_FIRST + 26;
    internal const int LVM_SETCOLUMNWIDTH = LVM_FIRST + 30;
    internal const int LVM_GETCOLUMNWIDTH = LVM_FIRST + 29;
    internal const int LVM_SETITEMCOUNT = LVM_FIRST + 47;
    internal const int LVM_GETITEMRECT = LVM_FIRST + 14;
    internal const int LVM_GETNEXTITEM = LVM_FIRST + 12;
    internal const int LVM_GETITEMSTATE = LVM_FIRST + 44;
    internal const int LVM_SETITEMSTATE = LVM_FIRST + 43;
    internal const int LVM_GETSELECTEDCOUNT = LVM_FIRST + 50;
    internal const int LVM_ENSUREVISIBLE = LVM_FIRST + 19;
    internal const int LVM_GETTOPINDEX = LVM_FIRST + 39;
    internal const int LVM_GETCOUNTPERPAGE = LVM_FIRST + 40;
    internal const int LVM_GETHEADER = LVM_FIRST + 31;
    internal const int LVM_HITTEST = LVM_FIRST + 18;

    internal const int LVSCW_AUTOSIZE_USEHEADER = -2;

    internal const int LVN_FIRST = -100;
    internal const int LVN_GETDISPINFOA = LVN_FIRST - 50;
    internal const int LVN_GETDISPINFOW = LVN_FIRST - 77;
    internal const int LVN_ITEMCHANGED = LVN_FIRST - 1;
    internal const int LVN_COLUMNCLICK = LVN_FIRST - 8;
    internal const int LVN_BEGINDRAG = LVN_FIRST - 9;

    internal const int NM_FIRST = 0;
    internal const int NM_CLICK = NM_FIRST - 2;
    internal const int NM_DBLCLK = NM_FIRST - 3;
    internal const int NM_RCLICK = NM_FIRST - 5;
    internal const int NM_CUSTOMDRAW = NM_FIRST - 12;

    internal const int HDN_FIRST = -300;
    internal const int HDN_ITEMRCLICK = HDN_FIRST - 12;
    internal const int HDN_DIVIDERDBLCLICK = HDN_FIRST - 5;

    internal const int HDM_FIRST = 0x1200;
    internal const int HDM_GETITEMCOUNT = HDM_FIRST + 0;
    internal const int HDM_GETITEM = HDM_FIRST + 11;
    internal const int HDM_SETITEM = HDM_FIRST + 12;

    internal const int LVCF_FMT = 0x0001;
    internal const int LVCF_WIDTH = 0x0002;
    internal const int LVCF_TEXT = 0x0004;
    internal const int LVCFMT_LEFT = 0x0000;
    internal const int LVCFMT_CENTER = 0x0002;

    internal const int LVIF_TEXT = 0x0001;
    internal const int LVIF_STATE = 0x0008;
    internal const int LVIS_SELECTED = 0x0002;
    internal const int LVIS_STATEIMAGEMASK = 0xF000;
    internal const int LVNI_SELECTED = 0x0002;

    internal const int HDI_FORMAT = 0x0004;
    internal const int HDF_SORTUP = 0x0400;
    internal const int HDF_SORTDOWN = 0x0200;

    internal const int CDDS_PREPAINT = 0x00000001;
    internal const int CDDS_ITEMPREPAINT = 0x00010001;
    internal const int CDDS_SUBITEM = 0x00020000;
    internal const int CDRF_DODEFAULT = 0x00000000;
    internal const int CDRF_NEWFONT = 0x00000002;
    internal const int CDRF_SKIPDEFAULT = 0x00000004;
    internal const int CDRF_NOTIFYITEMDRAW = 0x00000020;
    internal const int CDRF_NOTIFYSUBITEMDRAW = 0x00000020;
    internal const int CDIS_SELECTED = 0x0001;

    internal const int LVIR_BOUNDS = 0;

    internal const int DT_LEFT = 0x00000000;
    internal const int DT_CENTER = 0x00000001;
    internal const int DT_SINGLELINE = 0x00000020;
    internal const int DT_VCENTER = 0x00000004;
    internal const int DT_END_ELLIPSIS = 0x00008000;

    internal const int TRANSPARENT = 1;

    internal const int DFC_BUTTON = 4;
    internal const int DFCS_BUTTONCHECK = 0x0000;
    internal const int DFCS_CHECKED = 0x0001;
    internal const int DFCS_BUTTON3STATE = 0x0008;

    internal const int TME_LEAVE = 0x00000002;

    internal static void InitCommonControls()
    {
        var icc = new INITCOMMONCONTROLSEX
        {
            dwSize = Marshal.SizeOf<INITCOMMONCONTROLSEX>(),
            dwICC = 0x00000001
        };
        InitCommonControlsEx(ref icc);
    }

    [DllImport("comctl32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr InitCommonControlsEx(ref INITCOMMONCONTROLSEX icc);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr CreateWindowEx(
        int dwExStyle,
        string lpClassName,
        string lpWindowName,
        int dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool DestroyWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    internal static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

    [DllImport("user32.dll")]
    internal static extern bool InvalidateRect(IntPtr hWnd, ref RECT lpRect, bool bErase);

    [DllImport("user32.dll")]
    internal static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref LVCOLUMN lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref LVITEM lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref HDITEM lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref LVHITTESTINFO lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref RECT lParam);

    [DllImport("user32.dll")]
    internal static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll")]
    internal static extern bool GetCursorPos(ref POINT lpPoint);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern int DrawText(IntPtr hdc, string lpString, int nCount, ref RECT lpRect, uint uFormat);

    [DllImport("user32.dll")]
    internal static extern bool FillRect(IntPtr hdc, ref RECT lprc, IntPtr hbr);

    [DllImport("user32.dll")]
    internal static extern bool DrawFrameControl(IntPtr hdc, ref RECT rect, uint uType, uint uState);

    [DllImport("user32.dll")]
    internal static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

    [DllImport("gdi32.dll")]
    internal static extern int SetBkMode(IntPtr hdc, int iBkMode);

    [DllImport("gdi32.dll")]
    internal static extern uint SetTextColor(IntPtr hdc, uint crColor);

    [DllImport("gdi32.dll")]
    internal static extern IntPtr CreateSolidBrush(uint colorRef);

    [DllImport("gdi32.dll")]
    internal static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

    [DllImport("gdi32.dll")]
    internal static extern bool Polygon(IntPtr hdc, POINT[] points, int count);

    [DllImport("shell32.dll")]
    internal static extern void DragAcceptFiles(IntPtr hwnd, bool fAccept);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    internal static extern uint DragQueryFile(IntPtr hDrop, uint iFile, StringBuilder? lpszFile, int cch);

    [DllImport("shell32.dll")]
    internal static extern void DragFinish(IntPtr hDrop);

    [StructLayout(LayoutKind.Sequential)]
    internal struct INITCOMMONCONTROLSEX
    {
        public int dwSize;
        public int dwICC;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct LVCOLUMN
    {
        public int mask;
        public int fmt;
        public int cx;
        public string pszText;
        public int cchTextMax;
        public int iSubItem;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LVITEM
    {
        public uint mask;
        public int iItem;
        public int iSubItem;
        public uint state;
        public uint stateMask;
        public IntPtr pszText;
        public int cchTextMax;
        public int iImage;
        public IntPtr lParam;
        public int iIndent;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NMHDR
    {
        public IntPtr hwndFrom;
        public IntPtr idFrom;
        public int code;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NMLVDISPINFO
    {
        public NMHDR hdr;
        public LVITEM item;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NMLISTVIEW
    {
        public NMHDR hdr;
        public int iItem;
        public int iSubItem;
        public uint uNewState;
        public uint uOldState;
        public uint uChanged;
        public POINT ptAction;
        public IntPtr lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NMITEMACTIVATE
    {
        public NMHDR hdr;
        public int iItem;
        public int iSubItem;
        public uint uNewState;
        public uint uOldState;
        public uint uChanged;
        public POINT ptAction;
        public IntPtr lParam;
        public uint uKeyFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NMHEADER
    {
        public NMHDR hdr;
        public int iItem;
        public int iButton;
        public IntPtr pitem;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NMCUSTOMDRAW
    {
        public NMHDR hdr;
        public int dwDrawStage;
        public IntPtr hdc;
        public RECT rc;
        public IntPtr dwItemSpec;
        public uint uItemState;
        public IntPtr lItemlParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NMLVCUSTOMDRAW
    {
        public NMCUSTOMDRAW nmcd;
        public uint clrText;
        public uint clrTextBk;
        public int iSubItem;
        public uint dwItemType;
        public uint clrFace;
        public int iIconEffect;
        public int iIconPhase;
        public int iPartId;
        public int iStateId;
        public RECT rcText;
        public uint uAlign;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HDITEM
    {
        public int mask;
        public int cxy;
        public IntPtr pszText;
        public IntPtr hbm;
        public int cchTextMax;
        public int fmt;
        public IntPtr lParam;
        public int iImage;
        public int iOrder;
        public uint type;
        public IntPtr pvFilter;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LVHITTESTINFO
    {
        public POINT pt;
        public uint flags;
        public int iItem;
        public int iSubItem;
        public int iGroup;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TRACKMOUSEEVENT
    {
        public int cbSize;
        public int dwFlags;
        public IntPtr hwndTrack;
        public int dwHoverTime;
    }

    internal sealed class GdiObject : IDisposable
    {
        public GdiObject(IntPtr handle) => Handle = handle;
        public IntPtr Handle { get; }
        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
                DeleteObject(Handle);
        }
    }

    internal static uint INDEXTOSTATEIMAGEMASK(int i) => (uint)(i << 12);

    internal static uint ToColorRef(Color color)
        => (uint)(color.R | (color.G << 8) | (color.B << 16));

    internal static int LOWORD(IntPtr value) => (short)((long)value & 0xFFFF);
    internal static int HIWORD(IntPtr value) => (short)(((long)value >> 16) & 0xFFFF);
}


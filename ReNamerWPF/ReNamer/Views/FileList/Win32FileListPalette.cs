using System.Windows;
using System.Windows.Media;

namespace ReNamer.Views.FileList;

public sealed class Win32FileListPalette
{
    public Color Background { get; set; } = SystemColors.WindowColor;
    public Color AlternateRowBackground { get; set; } = SystemColors.ControlLightLightColor;
    public Color Text { get; set; } = SystemColors.WindowTextColor;
    public Color TextSecondary { get; set; } = SystemColors.GrayTextColor;
    public Color Border { get; set; } = SystemColors.ActiveBorderColor;
    public Color SelectionBackground { get; set; } = SystemColors.HighlightColor;
    public Color SelectionText { get; set; } = SystemColors.HighlightTextColor;
    public Color HoverBackground { get; set; } = SystemColors.ControlLightColor;
    public Color Accent { get; set; } = SystemColors.HighlightColor;
    public Color Success { get; set; } = Colors.ForestGreen;
    public Color Error { get; set; } = Colors.IndianRed;
    public Color HeaderBackground { get; set; } = SystemColors.ControlColor;
    public Color HeaderText { get; set; } = SystemColors.ControlTextColor;
    public Color HeaderBorder { get; set; } = SystemColors.ActiveBorderColor;
    public Color HeaderSortedBackground { get; set; } = SystemColors.ControlLightColor;
    public Color HeaderSortedText { get; set; } = SystemColors.ControlTextColor;

    public static Win32FileListPalette CreateFallback() => new();
}

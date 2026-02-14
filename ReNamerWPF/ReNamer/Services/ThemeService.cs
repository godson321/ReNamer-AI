using System;
using System.Windows;
using System.Windows.Media;

namespace ReNamer.Services;

public static class ThemeService
{
    public static void ApplyTheme(string theme)
    {
        var isDark = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase);
        var palette = isDark ? DarkPalette : LightPalette;
        ApplyPalette(palette);
    }

    private static void ApplyPalette(ThemePalette palette)
    {
        SetBrush("PrimaryBrush", palette.Primary);
        SetBrush("PrimaryHoverBrush", palette.PrimaryHover);
        SetBrush("PrimaryPressedBrush", palette.PrimaryPressed);
        SetBrush("AccentBrush", palette.Accent);
        SetBrush("BackgroundBrush", palette.Background);
        SetBrush("CardBrush", palette.Card);
        SetBrush("BorderBrush", palette.Border);
        SetBrush("BorderHoverBrush", palette.BorderHover);
        SetBrush("TextBrush", palette.Text);
        SetBrush("TextSecondaryBrush", palette.TextSecondary);
        SetBrush("TextDisabledBrush", palette.TextDisabled);
        SetBrush("SuccessBrush", palette.Success);
        SetBrush("ErrorBrush", palette.Error);
        SetBrush("WarningBrush", palette.Warning);
        SetBrush("InfoBrush", palette.Info);
        SetBrush("ToolbarBrush", palette.Toolbar);
        SetBrush("SplitterBrush", palette.Splitter);
        SetBrush("SelectionBrush", palette.Selection);
        SetBrush("HoverBrush", palette.Hover);

        // Design system brushes
        SetBrush("PrimaryLightBrush", palette.PrimaryLight);
        SetBrush("SurfaceBrush", palette.Surface);
        SetBrush("SurfaceHoverBrush", palette.SurfaceHover);
        SetBrush("TextMutedBrush", palette.TextMuted);
        SetBrush("SemanticSuccessBrush", palette.SemanticSuccess);
        SetBrush("SemanticWarningBrush", palette.SemanticWarning);
        SetBrush("SemanticErrorBrush", palette.SemanticError);
    }

    private static void SetBrush(string key, Color color)
    {
        var resource = Application.Current.TryFindResource(key);
        if (resource is SolidColorBrush brush && !brush.IsFrozen)
        {
            brush.Color = color;
            return;
        }

        Application.Current.Resources[key] = new SolidColorBrush(color);
    }

    private static readonly ThemePalette LightPalette = new(
        Primary: C("#0078D4"),
        PrimaryHover: C("#106EBE"),
        PrimaryPressed: C("#005A9E"),
        Accent: C("#0078D4"),
        Background: C("#F3F3F3"),
        Card: C("#FFFFFF"),
        Border: C("#D1D1D1"),
        BorderHover: C("#888888"),
        Text: C("#1A1A1A"),
        TextSecondary: C("#666666"),
        TextDisabled: C("#AAAAAA"),
        Success: C("#107C10"),
        Error: C("#D13438"),
        Warning: C("#CA5010"),
        Info: C("#0078D4"),
        Toolbar: C("#F9F9F9"),
        Splitter: C("#E0E0E0"),
        Selection: C("#CCE4F7"),
        Hover: C("#E5E5E5"),
        PrimaryLight: C("#E3F2FD"),
        Surface: C("#FAFAFA"),
        SurfaceHover: C("#F3F3F3"),
        TextMuted: C("#9E9E9E"),
        SemanticSuccess: C("#4CAF50"),
        SemanticWarning: C("#FF9800"),
        SemanticError: C("#F44336")
    );

    private static readonly ThemePalette DarkPalette = new(
        Primary: C("#60A5FA"),
        PrimaryHover: C("#7AB6FB"),
        PrimaryPressed: C("#3B82F6"),
        Accent: C("#60A5FA"),
        Background: C("#121212"),
        Card: C("#1E1E1E"),
        Border: C("#2D2D2D"),
        BorderHover: C("#3A3A3A"),
        Text: C("#FFFFFF"),
        TextSecondary: C("#A0A0A0"),
        TextDisabled: C("#6E6E6E"),
        Success: C("#3FB950"),
        Error: C("#F87171"),
        Warning: C("#FBBF24"),
        Info: C("#60A5FA"),
        Toolbar: C("#1A1A1A"),
        Splitter: C("#2A2A2A"),
        Selection: C("#1E3A8A"),
        Hover: C("#2A2A2A"),
        PrimaryLight: C("#1E293B"),
        Surface: C("#1E1E1E"),
        SurfaceHover: C("#252525"),
        TextMuted: C("#7A7A7A"),
        SemanticSuccess: C("#3FB950"),
        SemanticWarning: C("#FBBF24"),
        SemanticError: C("#F87171")
    );

    private static Color C(string hex)
    {
        return (Color)ColorConverter.ConvertFromString(hex)!;
    }

    private readonly record struct ThemePalette(
        Color Primary,
        Color PrimaryHover,
        Color PrimaryPressed,
        Color Accent,
        Color Background,
        Color Card,
        Color Border,
        Color BorderHover,
        Color Text,
        Color TextSecondary,
        Color TextDisabled,
        Color Success,
        Color Error,
        Color Warning,
        Color Info,
        Color Toolbar,
        Color Splitter,
        Color Selection,
        Color Hover,
        Color PrimaryLight,
        Color Surface,
        Color SurfaceHover,
        Color TextMuted,
        Color SemanticSuccess,
        Color SemanticWarning,
        Color SemanticError);
}

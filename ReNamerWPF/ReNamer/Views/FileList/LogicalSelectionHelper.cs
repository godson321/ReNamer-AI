using System.Windows;

namespace ReNamer.Views.FileList;

public static class LogicalSelectionHelper
{
    public static readonly DependencyProperty IsLogicalSelectedProperty =
        DependencyProperty.RegisterAttached(
            "IsLogicalSelected",
            typeof(bool),
            typeof(LogicalSelectionHelper),
            new FrameworkPropertyMetadata(false));

    public static bool GetIsLogicalSelected(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsLogicalSelectedProperty);
    }

    public static void SetIsLogicalSelected(DependencyObject obj, bool value)
    {
        obj.SetValue(IsLogicalSelectedProperty, value);
    }
}

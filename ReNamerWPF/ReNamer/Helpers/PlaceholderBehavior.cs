using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace ReNamer.Helpers;

public static class PlaceholderBehavior
{
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.RegisterAttached(
            "Placeholder",
            typeof(string),
            typeof(PlaceholderBehavior),
            new PropertyMetadata(string.Empty, OnPlaceholderChanged));

    private static readonly DependencyProperty IsHookedProperty =
        DependencyProperty.RegisterAttached(
            "IsHooked",
            typeof(bool),
            typeof(PlaceholderBehavior),
            new PropertyMetadata(false));

    private static readonly DependencyProperty PlaceholderAdornerProperty =
        DependencyProperty.RegisterAttached(
            "PlaceholderAdorner",
            typeof(PlaceholderAdorner),
            typeof(PlaceholderBehavior),
            new PropertyMetadata(null));

    public static string GetPlaceholder(DependencyObject obj) => (string)obj.GetValue(PlaceholderProperty);
    public static void SetPlaceholder(DependencyObject obj, string value) => obj.SetValue(PlaceholderProperty, value);

    private static bool GetIsHooked(DependencyObject obj) => (bool)obj.GetValue(IsHookedProperty);
    private static void SetIsHooked(DependencyObject obj, bool value) => obj.SetValue(IsHookedProperty, value);

    private static PlaceholderAdorner? GetPlaceholderAdorner(DependencyObject obj) =>
        obj.GetValue(PlaceholderAdornerProperty) as PlaceholderAdorner;

    private static void SetPlaceholderAdorner(DependencyObject obj, PlaceholderAdorner? value) =>
        obj.SetValue(PlaceholderAdornerProperty, value);

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Control control)
        {
            return;
        }

        HookEvents(control);
        UpdatePlaceholder(control);
    }

    private static void HookEvents(Control control)
    {
        if (GetIsHooked(control))
        {
            return;
        }

        control.Loaded += Control_Loaded;
        control.Unloaded += Control_Unloaded;
        control.IsVisibleChanged += Control_IsVisibleChanged;
        control.IsEnabledChanged += Control_IsEnabledChanged;
        control.GotKeyboardFocus += Control_FocusChanged;
        control.LostKeyboardFocus += Control_FocusChanged;

        if (control is TextBox textBox)
        {
            textBox.TextChanged += Control_TextChanged;
        }
        else if (control is ComboBox comboBox)
        {
            comboBox.SelectionChanged += Control_SelectionChanged;
            comboBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(Control_TextChanged));
        }

        SetIsHooked(control, true);
    }

    private static void Control_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Control control)
        {
            UpdatePlaceholder(control);
        }
    }

    private static void Control_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is Control control)
        {
            RemovePlaceholder(control);
        }
    }

    private static void Control_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Control control)
        {
            UpdatePlaceholder(control);
        }
    }

    private static void Control_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Control control)
        {
            UpdatePlaceholder(control);
        }
    }

    private static void Control_FocusChanged(object sender, RoutedEventArgs e)
    {
        if (sender is Control control)
        {
            UpdatePlaceholder(control);
        }
    }

    private static void Control_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is Control control)
        {
            UpdatePlaceholder(control);
        }
    }

    private static void Control_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Control control)
        {
            UpdatePlaceholder(control);
        }
    }

    private static string GetCurrentText(Control control)
    {
        return control switch
        {
            TextBox textBox => textBox.Text,
            ComboBox comboBox when comboBox.IsEditable => comboBox.Text,
            _ => string.Empty
        };
    }

    private static bool SupportsPlaceholder(Control control)
    {
        return control is TextBox || control is ComboBox { IsEditable: true };
    }

    private static void UpdatePlaceholder(Control control)
    {
        if (!SupportsPlaceholder(control))
        {
            return;
        }

        var placeholder = GetPlaceholder(control);
        if (string.IsNullOrWhiteSpace(placeholder))
        {
            RemovePlaceholder(control);
            return;
        }

        var shouldShow = control.IsLoaded
                         && control.IsVisible
                         && control.IsEnabled
                         && !control.IsKeyboardFocusWithin
                         && string.IsNullOrEmpty(GetCurrentText(control));

        if (shouldShow)
        {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer == null)
            {
                return;
            }

            var adorner = GetPlaceholderAdorner(control);
            if (adorner == null)
            {
                adorner = new PlaceholderAdorner(control, placeholder);
                layer.Add(adorner);
                SetPlaceholderAdorner(control, adorner);
            }
            else
            {
                adorner.Placeholder = placeholder;
            }
        }
        else
        {
            RemovePlaceholder(control);
        }
    }

    private static void RemovePlaceholder(Control control)
    {
        var adorner = GetPlaceholderAdorner(control);
        if (adorner == null)
        {
            return;
        }

        var layer = AdornerLayer.GetAdornerLayer(control);
        layer?.Remove(adorner);
        SetPlaceholderAdorner(control, null);
    }

    private sealed class PlaceholderAdorner : Adorner
    {
        private VisualCollection? _visuals;
        private TextBlock? _placeholderText;

        public PlaceholderAdorner(UIElement adornedElement, string placeholder) : base(adornedElement)
        {
            IsHitTestVisible = false;

            _placeholderText = new TextBlock
            {
                Text = placeholder,
                Margin = new Thickness(8, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 0.75,
                Foreground = (Brush?)Application.Current?.TryFindResource("TextSecondaryBrush") ?? Brushes.Gray
            };

            _visuals = new VisualCollection(this) { _placeholderText };
        }

        public string Placeholder
        {
            get => _placeholderText?.Text ?? string.Empty;
            set
            {
                if (_placeholderText != null)
                {
                    _placeholderText.Text = value;
                }
            }
        }

        protected override int VisualChildrenCount => _visuals?.Count ?? 0;

        protected override Visual GetVisualChild(int index)
        {
            if (_visuals == null || index < 0 || index >= _visuals.Count)
            {
                throw new global::System.ArgumentOutOfRangeException(nameof(index));
            }

            return _visuals[index];
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _placeholderText?.Arrange(new Rect(finalSize));
            return finalSize;
        }
    }
}

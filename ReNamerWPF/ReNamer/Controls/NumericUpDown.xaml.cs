using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReNamer.Controls;

public partial class NumericUpDown : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericUpDown),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register(nameof(MinValue), typeof(int), typeof(NumericUpDown),
            new PropertyMetadata(int.MinValue, OnConstraintChanged));

    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(nameof(MaxValue), typeof(int), typeof(NumericUpDown),
            new PropertyMetadata(int.MaxValue, OnConstraintChanged));

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(nameof(Step), typeof(int), typeof(NumericUpDown),
            new PropertyMetadata(1));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int MinValue
    {
        get => (int)GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public int MaxValue
    {
        get => (int)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public int Step
    {
        get => (int)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    #endregion

    public NumericUpDown()
    {
        InitializeComponent();
        UpdateTextBox();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericUpDown nud)
        {
            nud.Value = Clamp((int)e.NewValue, nud.MinValue, nud.MaxValue);
            nud.UpdateTextBox();
        }
    }

    private static void OnConstraintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericUpDown nud)
            nud.Value = Clamp(nud.Value, nud.MinValue, nud.MaxValue);
    }

    private void UpdateTextBox()
    {
        if (PART_TextBox != null && !PART_TextBox.IsFocused)
            PART_TextBox.Text = Value.ToString();
    }

    private void UpButton_Click(object sender, RoutedEventArgs e)
    {
        Value = Clamp(Value + Step, MinValue, MaxValue);
    }

    private void DownButton_Click(object sender, RoutedEventArgs e)
    {
        Value = Clamp(Value - Step, MinValue, MaxValue);
    }

    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // 允许数字和负号
        e.Handled = !Regex.IsMatch(e.Text, @"^[\d\-]$");
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // 输入时不强制同步，等失焦时处理
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(PART_TextBox.Text, out int val))
            Value = Clamp(val, MinValue, MaxValue);
        PART_TextBox.Text = Value.ToString();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        switch (e.Key)
        {
            case Key.Up:
                Value = Clamp(Value + Step, MinValue, MaxValue);
                e.Handled = true;
                break;
            case Key.Down:
                Value = Clamp(Value - Step, MinValue, MaxValue);
                e.Handled = true;
                break;
            case Key.Enter:
                if (int.TryParse(PART_TextBox.Text, out int val))
                    Value = Clamp(val, MinValue, MaxValue);
                PART_TextBox.Text = Value.ToString();
                e.Handled = true;
                break;
        }
    }

    private static int Clamp(int value, int min, int max)
        => Math.Max(min, Math.Min(max, value));
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ReNamer.Services;

namespace ReNamer.Views;

public partial class TextInputDialog : Window
{
    private readonly Func<string, string?>? _validator;

    public string InputText => tbInput.Text;

    public TextInputDialog(
        string title,
        string prompt,
        string initialText = "",
        bool isMultiline = false,
        Func<string, string?>? validator = null,
        string? originalText = null)
    {
        InitializeComponent();

        Title = title;
        tbInput.Text = initialText;
        tbInput.ToolTip = string.IsNullOrWhiteSpace(prompt) ? null : prompt;
        var hasOriginalText = !string.IsNullOrWhiteSpace(originalText);

        _validator = text =>
        {
            if (hasOriginalText && (text.Contains('\r') || text.Contains('\n')))
                return LanguageService.GetString("Dialog_NewNameSingleLineOnly");

            return validator?.Invoke(text);
        };

        btnOk.IsDefault = !isMultiline;

        if (hasOriginalText)
        {
            txtOriginalNameLabel.Text = LanguageService.GetString("Dialog_OriginalFileName");
            tbOriginalName.Text = originalText!;
            pnlOriginalName.Visibility = Visibility.Visible;
            tbInput.AcceptsReturn = true;
            tbInput.TextWrapping = TextWrapping.Wrap;
            tbInput.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            tbInput.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            tbInput.VerticalContentAlignment = VerticalAlignment.Top;
            if (!isMultiline)
            {
                btnOk.IsDefault = false;
                tbInput.PreviewKeyDown += TbInputConfirmOnEnter;
            }
        }
        else if (isMultiline)
        {
            tbInput.AcceptsReturn = true;
            tbInput.TextWrapping = TextWrapping.Wrap;
            tbInput.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            tbInput.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            tbInput.VerticalContentAlignment = VerticalAlignment.Top;
        }
        else
        {
            tbInput.AcceptsReturn = false;
            tbInput.TextWrapping = TextWrapping.NoWrap;
            tbInput.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            tbInput.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            tbInput.VerticalContentAlignment = VerticalAlignment.Center;
        }

        Loaded += (_, _) =>
        {
            tbInput.Focus();
            tbInput.SelectAll();
        };

    }

    private void TbInputConfirmOnEnter(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.None)
            return;

        e.Handled = true;
        Ok_Click(sender, new RoutedEventArgs());
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        var error = _validator?.Invoke(tbInput.Text);
        if (!string.IsNullOrWhiteSpace(error))
        {
            tbInput.Focus();
            return;
        }

        DialogResult = true;
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using ReNamer.Services;

namespace ReNamer.Views;

public partial class TextInputDialog : Window
{
    private readonly Func<string, string?>? _validator;
    private readonly bool _isMultiline;

    public string InputText => tbInput.Text;

    public TextInputDialog(
        string title,
        string prompt,
        string initialText = "",
        bool isMultiline = false,
        Func<string, string?>? validator = null)
    {
        InitializeComponent();

        Title = title;
        txtPrompt.Text = prompt;
        tbInput.Text = initialText;
        _validator = validator;
        _isMultiline = isMultiline;

        btnOk.Content = LanguageService.GetString("Dialog_OK");
        btnCancel.Content = LanguageService.GetString("Dialog_Cancel");
        btnOk.IsDefault = !_isMultiline;

        if (_isMultiline)
        {
            Width = 520;
            Height = 320;
            ResizeMode = ResizeMode.CanResize;
            tbInput.AcceptsReturn = true;
            tbInput.TextWrapping = TextWrapping.Wrap;
            tbInput.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            tbInput.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            tbInput.VerticalContentAlignment = VerticalAlignment.Top;
        }

        Loaded += (_, _) =>
        {
            tbInput.Focus();
            tbInput.SelectAll();
        };

        tbInput.TextChanged += (_, _) =>
        {
            if (txtValidation.Visibility == Visibility.Visible)
                txtValidation.Visibility = Visibility.Collapsed;
        };
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        var error = _validator?.Invoke(tbInput.Text);
        if (!string.IsNullOrWhiteSpace(error))
        {
            txtValidation.Text = error;
            txtValidation.Visibility = Visibility.Visible;
            tbInput.Focus();
            return;
        }

        DialogResult = true;
    }
}

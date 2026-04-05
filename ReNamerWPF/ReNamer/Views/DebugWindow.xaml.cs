using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReNamer.Views
{
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
            AttachLogging(txtA1, "A1");
            AttachLogging(txtA2, "A2");
            AttachLogging(txtB1, "B1");
            AttachLogging(txtB2, "B2");
        }

        private void AttachLogging(TextBox textBox, string name)
        {
            textBox.PreviewKeyDown += (s, e) =>
            {
                var key = e.Key == Key.System ? e.SystemKey : e.Key;
                if (Keyboard.Modifiers == ModifierKeys.Control &&
                    (key == Key.C || key == Key.V || key == Key.X || key == Key.A))
                {
                    Log($"{name}: PreviewKeyDown {key} Handled={e.Handled}");
                }
            };
            textBox.TextChanged += (s, e) =>
            {
                if (textBox.IsKeyboardFocusWithin && !textBox.IsReadOnly)
                    Log($"{name}: TextChanged (len={textBox.Text.Length})");
            };
        }

        private void Log(string message)
        {
            txtLog.AppendText($"{DateTime.Now:HH:mm:ss.fff} {message}\n");
            txtLog.ScrollToEnd();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            txtLog.Clear();
        }
    }
}

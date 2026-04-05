using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ReNamer.Services;

namespace ReNamer;

[SupportedOSPlatform("windows")]
public partial class App : Application
{
    private static readonly object InputLogSync = new();
    private static string? _inputLogPath;
    private static StreamWriter? _inputLogWriter;
    public static bool InputDebugLoggingEnabled { get; private set; }
    private static readonly string AppRootDir = AppContext.BaseDirectory;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        LanguageService.Initialize();
        _inputLogPath = Path.Combine(
            AppRootDir,
            "logs",
            "input-debug.log");

        EventManager.RegisterClassHandler(
            typeof(TextBoxBase),
            UIElement.PreviewKeyDownEvent,
            new KeyEventHandler(OnTextBoxPreviewKeyDown),
            handledEventsToo: true);
    }

    public static void SetInputDebugLoggingEnabled(bool enabled)
    {
        if (InputDebugLoggingEnabled && !enabled)
            LogInputDebug("Input debug logging disabled.");
        InputDebugLoggingEnabled = enabled;
        if (!enabled)
            return;

        LogInputDebug("Input debug logging enabled.");
    }

    private static void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox || textBox.IsReadOnly)
            return;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (!SafeClipboardService.TryHandleShortcut(textBox, key, Keyboard.Modifiers, out var action, LogInputDebug))
            return;

        e.Handled = true;
        LogInputDebug($"[Shortcut] {action} box={DescribeTextBox(textBox)}");
    }

    public static void LogInputDebug(string message)
    {
        if (!InputDebugLoggingEnabled)
            return;

        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [T{Environment.CurrentManagedThreadId}] {message}";
        Debug.WriteLine(line);

        if (string.IsNullOrWhiteSpace(_inputLogPath))
            return;

        lock (InputLogSync)
        {
            try
            {
                if (_inputLogWriter == null)
                {
                    var dir = Path.GetDirectoryName(_inputLogPath);
                    if (!string.IsNullOrWhiteSpace(dir))
                        Directory.CreateDirectory(dir);
                    _inputLogWriter = new StreamWriter(
                        new FileStream(_inputLogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite),
                        Encoding.UTF8)
                    {
                        AutoFlush = true
                    };
                }

                if (_inputLogWriter != null)
                    _inputLogWriter.WriteLine(line);
                else
                    File.AppendAllText(_inputLogPath, line + Environment.NewLine);
            }
            catch
            {
            }
        }
    }

    private static string DescribeTextBox(TextBoxBase textBox)
    {
        var element = (FrameworkElement)textBox;
        var name = string.IsNullOrWhiteSpace(element.Name) ? "<no-name>" : element.Name;
        if (textBox is TextBox plain)
        {
            return $"{textBox.GetType().Name}#{name} focus={plain.IsKeyboardFocusWithin} caret={plain.CaretIndex} sel={plain.SelectionLength} len={plain.Text.Length}";
        }

        return $"{textBox.GetType().Name}#{name} focus={textBox.IsKeyboardFocusWithin}";
    }
}

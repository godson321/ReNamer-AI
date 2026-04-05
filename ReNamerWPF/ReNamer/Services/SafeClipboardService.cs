using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReNamer.Services
{
    [SupportedOSPlatform("windows")]
    public static class SafeClipboardService
    {
        private static string _fallbackText = string.Empty;

        public static bool TryHandleShortcut(
            TextBox textBox,
            Key key,
            ModifierKeys modifiers,
            out string action,
            Action<string>? log = null)
        {
            action = string.Empty;
            if (modifiers != ModifierKeys.Control)
                return false;
            if (key != Key.C && key != Key.X && key != Key.V)
                return false;

            log?.Invoke($"Clipboard shortcut received key={key} selectionLength={textBox.SelectionLength}");

            if (key == Key.C)
            {
                if (string.IsNullOrEmpty(textBox.SelectedText))
                {
                    action = "copy-empty";
                    log?.Invoke("Copy skipped: empty selection");
                    return true;
                }

                _fallbackText = textBox.SelectedText;
                PushClipboardAsync(_fallbackText, log);
                action = $"copy-{_fallbackText.Length}";
                log?.Invoke($"Copy prepared fallbackLength={_fallbackText.Length}");
                return true;
            }

            if (key == Key.X)
            {
                if (string.IsNullOrEmpty(textBox.SelectedText))
                {
                    action = "cut-empty";
                    log?.Invoke("Cut skipped: empty selection");
                    return true;
                }

                _fallbackText = textBox.SelectedText;
                PushClipboardAsync(_fallbackText, log);
                textBox.SelectedText = string.Empty;
                action = $"cut-{_fallbackText.Length}";
                log?.Invoke($"Cut prepared fallbackLength={_fallbackText.Length}");
                return true;
            }

            var (text, source, elapsed) = PullClipboardWithTimeout(15, log);
            if (string.IsNullOrEmpty(text))
            {
                if (!string.IsNullOrEmpty(_fallbackText))
                {
                    text = _fallbackText;
                    source = "fallback";
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                action = "paste-empty";
                log?.Invoke($"Paste skipped: no text source elapsedMs={elapsed}");
                return true;
            }

            textBox.SelectedText = text;
            textBox.CaretIndex += text.Length;
            textBox.SelectionLength = 0;
            action = $"paste-{source}-{text.Length}";
            log?.Invoke($"Paste completed source={source} length={text.Length} elapsedMs={elapsed}");
            return true;
        }

        private static void PushClipboardAsync(string text, Action<string>? log)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var thread = new Thread(() =>
            {
                try
                {
                    Clipboard.SetText(text);
                    log?.Invoke($"Clipboard push success length={text.Length}");
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Clipboard push failed type={ex.GetType().Name} message={ex.Message}");
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            log?.Invoke($"Clipboard push queued length={text.Length}");
        }

        private static (string? text, string source, int elapsedMs) PullClipboardWithTimeout(int timeoutMilliseconds, Action<string>? log)
        {
            string? result = null;
            var completed = false;
            var hasError = false;
            var started = Environment.TickCount64;
            var thread = new Thread(() =>
            {
                try
                {
                    if (Clipboard.ContainsData(DataFormats.UnicodeText) || Clipboard.ContainsText())
                        result = Clipboard.GetText();
                }
                catch (Exception ex)
                {
                    hasError = true;
                    log?.Invoke($"Clipboard pull failed type={ex.GetType().Name} message={ex.Message}");
                }
                finally
                {
                    completed = true;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            for (var i = 0; i < timeoutMilliseconds; i++)
            {
                if (completed)
                    break;
                Thread.Sleep(1);
            }

            var elapsed = (int)(Environment.TickCount64 - started);
            if (!completed)
            {
                log?.Invoke($"Clipboard pull timeout limitMs={timeoutMilliseconds}");
                return (null, "timeout", elapsed);
            }

            if (hasError)
                return (null, "error", elapsed);
            if (string.IsNullOrEmpty(result))
                return (null, "empty", elapsed);
            return (result, "system", elapsed);
        }
    }
}

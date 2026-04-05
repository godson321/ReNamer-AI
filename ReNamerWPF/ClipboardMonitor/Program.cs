using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    [DllImport("user32.dll")]
    static extern IntPtr GetOpenClipboardWindow();

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    static void Main(string[] args)
    {
        Console.WriteLine("Monitoring clipboard locks... Press Ctrl+C to exit.");
        Console.WriteLine("Waiting for clipboard to be locked by another process...");

        IntPtr lastHwnd = IntPtr.Zero;

        while (true)
        {
            try
            {
                IntPtr hwnd = GetOpenClipboardWindow();
                if (hwnd != IntPtr.Zero && hwnd != lastHwnd)
                {
                    uint processId;
                    GetWindowThreadProcessId(hwnd, out processId);

                    if (processId > 0)
                    {
                        try
                        {
                            Process p = Process.GetProcessById((int)processId);
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Clipboard is locked by Window {hwnd:X8} - Process: {p.ProcessName} (PID: {processId})");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Clipboard is locked by Window {hwnd:X8} - Process ID: {processId} (Could not read process name: {ex.Message})");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Clipboard is locked by Window {hwnd:X8} - Unknown Process ID");
                    }
                    
                    lastHwnd = hwnd;
                }
                else if (hwnd == IntPtr.Zero && lastHwnd != IntPtr.Zero)
                {
                    // Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Clipboard released.");
                    lastHwnd = IntPtr.Zero;
                }
            }
            catch { }

            Thread.Sleep(5); // high frequency poll
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ResizableAppWatcher
{
    static class Program
    {
        const int GWL_STYLE = -16;
        const int WS_SIZEBOX = 0x00040000;
        const int WS_MAXIMIZEBOX = 0x00010000;
        const int WS_MINIMIZEBOX = 0x00020000;
        const int WS_SYSMENU = 0x00080000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            NotifyIcon notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Visible = true,
                Text = "OpenResize"
            };

            ContextMenu contextMenu = new ContextMenu();
            MenuItem exitMenuItem = new MenuItem("Exit", (sender, args) => Application.Exit());
            contextMenu.MenuItems.Add(exitMenuItem);

            notifyIcon.ContextMenu = contextMenu;

            Thread watcherThread = new Thread(WatchForApplications)
            {
                IsBackground = true
            };
            watcherThread.Start();

            Application.Run();
        }

        private static void WatchForApplications()
        {
            while (true)
            {
                foreach (var process in GetRunningApplications())
                {
                    IntPtr hWnd = process.MainWindowHandle;
                    if (hWnd != IntPtr.Zero && !AreWindowButtonsEnabled(hWnd))
                    {
                        MakeWindowResizable(hWnd);
                    }
                }

                Thread.Sleep(5000);
            }
        }

        private static Process[] GetRunningApplications()
        {
            return Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .ToArray();
        }

        private static bool AreWindowButtonsEnabled(IntPtr hWnd)
        {
            int style = GetWindowLong(hWnd, GWL_STYLE);
            return (style & WS_SIZEBOX) != 0 &&
                   (style & WS_MAXIMIZEBOX) != 0 &&
                   (style & WS_MINIMIZEBOX) != 0 &&
                   (style & WS_SYSMENU) != 0;
        }

        private static void MakeWindowResizable(IntPtr hWnd)
        {
            int style = GetWindowLong(hWnd, GWL_STYLE);
            SetWindowLong(hWnd, GWL_STYLE, style | WS_SIZEBOX | WS_MAXIMIZEBOX | WS_MINIMIZEBOX | WS_SYSMENU);
        }
    }
}

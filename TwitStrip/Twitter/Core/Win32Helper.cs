using System.Runtime.InteropServices;
using System.Windows.Interop;
using System;

namespace Core
{
    class NativeMethods
    {
        private const int SW_SHOWNOACTIVATE = 4;
        private const int HWND_NONTOPMOST = -2;
        private const int HWND_TOPMOST = -1;
        private const int HWND_TOP = 0;

        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern bool SetWindowPos(
             int hWnd,                  // window handle
             int hWndInsertAfter,       // placement-order handle
             int X,                     // horizontal position
             int Y,                     // vertical position
             int cx,                    // width
             int cy,                    // height
             uint uFlags);              // window positioning flags

        /// <summary> ShowWindow(WindowHandle, SW_SHOWNOACTIVATE); </summary>
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetActiveWindow(IntPtr hWnd);

        public static void ShowWindowTopMost(System.Windows.Window Window) {
            IntPtr WindowHandle = new WindowInteropHelper(Window).Handle;

            SetWindowPos(WindowHandle.ToInt32(),
                         HWND_TOPMOST,
                         Convert.ToInt32(Window.Left),
                         Convert.ToInt32(Window.Top),
                         Convert.ToInt32(Window.Width),
                         Convert.ToInt32(Window.Height),
                         SWP_SHOWWINDOW);

            SetWindowPos(WindowHandle.ToInt32(),
                         HWND_NONTOPMOST,
                         Convert.ToInt32(Window.Left),
                         Convert.ToInt32(Window.Top),
                         Convert.ToInt32(Window.Width),
                         Convert.ToInt32(Window.Height),
                         SWP_SHOWWINDOW);
        }
    }
}

using MCHUB;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace AppUIBasics
{
    /// <summary>
    /// 实用工具类。帮助获取窗口句柄、与系统交互等。
    /// </summary>
    internal static class Win32
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(IntPtr moduleName);

        [DllImport("Shcore.dll", SetLastError = true)]
        public static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

        public const int WM_ACTIVATE = 0x0006;
        public const int WA_ACTIVE = 0x01;
        //static int WA_CLICKACTIVE = 0x02;
        public const int WA_INACTIVE = 0x00;

        public const int WM_SETICON = 0x0080;
        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public static IntPtr MainWindow_Handle;
        public static WindowId MainWindow_ID;
        public static double PixelZoom;

        public static void Init(Window mainWindow)
        {
            MainWindow_Handle = WindowNative.GetWindowHandle(mainWindow);
            MainWindow_ID = Win32Interop.GetWindowIdFromWindow(MainWindow_Handle);

            PixelZoom = GetScaleAdjustment();
        }
        public static MainWindow GetMainWindow()
        {
            return (MainWindow)(Application.Current as App).m_window;
        }
        public static AppWindow GetAppWindow()
        {
            return AppWindow.GetFromWindowId(MainWindow_ID);
        }

        public enum Monitor_DPI_Type : int
        {
            MDT_Effective_DPI = 0,
            MDT_Angular_DPI = 1,
            MDT_Raw_DPI = 2,
            MDT_Default = MDT_Effective_DPI
        }

        /// <summary>
        /// 获取缩放系数。
        /// </summary>
        /// <returns>返回缩放系数，表示缩放后像素大小与原始像素大小之比值。</returns>
        /// <exception cref="Exception">获取缩放系数失败。</exception>
        private static double GetScaleAdjustment()
        {
            DisplayArea displayArea = DisplayArea.GetFromWindowId(MainWindow_ID, DisplayAreaFallback.Primary);
            IntPtr hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

            // Get DPI.
            int result = GetDpiForMonitor(hMonitor, Monitor_DPI_Type.MDT_Default, out uint dpiX, out uint _);
            if (result != 0)
            {
                throw new Exception("Could not get DPI for monitor.");
            }

            uint scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
            return scaleFactorPercent / 100.0;
        }

        /// <summary>
        /// 获取像素数量缩放后对应的像素数量。
        /// </summary>
        /// <param name="pixel">缩放前的像素数量。</param>
        /// <returns>返回缩放后应当由的像素数量。</returns>
        public static int GetActualPixel(double pixel)
        {
            return Convert.ToInt32(pixel * PixelZoom);
        }
    }
}
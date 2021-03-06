global using Microsoft.UI;
global using Microsoft.UI.Windowing;
global using Microsoft.UI.Xaml;
global using Microsoft.UI.Xaml.Controls;
global using Microsoft.UI.Xaml.Input;
global using Microsoft.UI.Xaml.Media;
global using Microsoft.UI.Xaml.Navigation;
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Windows.UI;
using WinRT;
using WinRT.Interop;

namespace MCHUB.Utility;

/// <summary>
/// 实用工具类。帮助获取窗口句柄、与系统交互等。
/// </summary>
public static class UIHelper
{
    #region Consts

    public const int ICON_BIG = 1;
    public const int ICON_SMALL = 0;

    public const int WA_ACTIVE = 0x01;

    //static int WA_CLICKACTIVE = 0x02;
    public const int WA_INACTIVE = 0x00;
    public const int WM_ACTIVATE = 0x0006;
    public const int WM_SETICON = 0x0080;
    public static IntPtr MainWindow_Handle;
    public static WindowId MainWindow_ID;
    public static double PixelZoom;

    private enum Monitor_DPI_Type : int
    {
        MDT_Effective_DPI = 0,
        MDT_Angular_DPI = 1,
        MDT_Raw_DPI = 2,
        MDT_Default = MDT_Effective_DPI
    }

    #endregion

    #region ImportMethods

    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

    [DllImport("Shcore.dll", SetLastError = true)]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

    #endregion

    private static WindowsSystemDispatcherQueueHelper _mWsdqHelper; // See separate sample below for implementation
    private static Microsoft.UI.Composition.SystemBackdrops.MicaController _mMicaController;
    private static Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration _mConfigurationSource;

    /// <summary>
    /// 获取像素数量缩放后对应的像素数量。
    /// </summary>
    /// <param name="pixel">缩放前的像素数量。</param>
    /// <returns>返回缩放后应当由的像素数量。</returns>
    public static int GetActualPixel(double pixel)
    {
        return Convert.ToInt32(pixel * PixelZoom);
    }

    public static AppWindow GetAppWindow()
    {
        return AppWindow.GetFromWindowId(MainWindow_ID);
    }

    public static MainWindow GetMainWindow()
    {
        return (MainWindow)(Application.Current as App).m_window;
    }

    public static void Init(Window mainWindow)
    {
        MainWindow_Handle = WindowNative.GetWindowHandle(mainWindow);
        MainWindow_ID = Win32Interop.GetWindowIdFromWindow(MainWindow_Handle);

        PixelZoom = GetScaleAdjustment();
    }

    /// <summary>
    /// 获取缩放系数。
    /// </summary>
    /// <returns>返回缩放系数，表示缩放后像素大小与原始像素大小之比值。</returns>
    /// <exception cref="Exception">获取缩放系数失败。</exception>
    private static double GetScaleAdjustment()
    {
        var displayArea = DisplayArea.GetFromWindowId(MainWindow_ID, DisplayAreaFallback.Primary);
        var hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

        // Get DPI.
        var result = GetDpiForMonitor(hMonitor, Monitor_DPI_Type.MDT_Default, out var dpiX, out var _);
        if (result != 0) throw new Exception("Could not get DPI for monitor.");

        var scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
        return scaleFactorPercent / 100.0;
    }

    public static bool TrySetMicaBackdrop()
    {
        if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
        {
            _mWsdqHelper = new WindowsSystemDispatcherQueueHelper();
            _mWsdqHelper.EnsureWindowsSystemDispatcherQueueController();

            // Hooking up the policy object
            _mConfigurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
            GetMainWindow().Activated += (_, args) =>
            {
                _mConfigurationSource.IsInputActive =
                    args.WindowActivationState != WindowActivationState.Deactivated;
            };
            ((FrameworkElement)GetMainWindow().Content).ActualThemeChanged += (_, _) =>
            {
                if (_mConfigurationSource != null) SetConfigurationSourceTheme();
            };

            // Initial configuration state.
            _mConfigurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            _mMicaController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            _mMicaController.AddSystemBackdropTarget(GetMainWindow()
                .As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            _mMicaController.SetSystemBackdropConfiguration(_mConfigurationSource);
            return true; // succeeded
        }

        return false; // Mica is not supported on UIHelper.GetMainWindow() system
    }

    private static void SetConfigurationSourceTheme()
    {
        switch (((FrameworkElement)GetMainWindow().Content).ActualTheme)
        {
            case ElementTheme.Dark:
                _mConfigurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark;
                break;
            case ElementTheme.Light:
                _mConfigurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light;
                break;
            case ElementTheme.Default:
                _mConfigurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default;
                break;
        }
    }

    public static void SetTitleBarColor(Color background, Color foreground)
    {
        var res = Application.Current.Resources;
        res["WindowCaptionBackground"] = background;
        res["WindowCaptionForeground"] = foreground;
        if (MainWindow_Handle == GetActiveWindow())
        {
            SendMessage(MainWindow_Handle, WM_ACTIVATE, WA_INACTIVE, IntPtr.Zero);
            SendMessage(MainWindow_Handle, WM_ACTIVATE, WA_ACTIVE, IntPtr.Zero);
        }
        else
        {
            SendMessage(MainWindow_Handle, WM_ACTIVATE, WA_ACTIVE, IntPtr.Zero);
            SendMessage(MainWindow_Handle, WM_ACTIVATE, WA_INACTIVE, IntPtr.Zero);
        }
    }
}

public class WindowsSystemDispatcherQueueHelper
{
    [StructLayout(LayoutKind.Sequential)]
    private struct DispatcherQueueOptions
    {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }

    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options,
        [In] [Out] [MarshalAs(UnmanagedType.IUnknown)]
        ref object dispatcherQueueController);

    private object _mDispatcherQueueController = null;

    public void EnsureWindowsSystemDispatcherQueueController()
    {
        if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            // one already exists, so we'll just use it.
            return;

        if (_mDispatcherQueueController == null)
        {
            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
            options.threadType = 2; // DQTYPE_THREAD_CURRENT
            options.apartmentType = 2; // DQTAT_COM_STA

            CreateDispatcherQueueController(options, ref _mDispatcherQueueController);
        }
    }
}
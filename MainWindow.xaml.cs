using AppUIBasics;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MCHUB
{
    public sealed partial class MainWindow : Window
    {
        private AppWindow AppWindow;
        public static User CurrentUser = null;

        public MainWindow()
        {
            InitializeComponent();
            Win32.Init(this);
            LauncherDataHelper.Init();

            AppWindow = Win32.GetAppWindow();
            AppWindow.Resize(new SizeInt32(Win32.GetActualPixel(800), Win32.GetActualPixel(600)));
            //自定义标题栏。
            Title = "MCHUB";
            //设置标题栏颜色并刷新。
            AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            ExtendsContentIntoTitleBar = true;
            ResourceDictionary res = Application.Current.Resources;
            res["WindowCaptionBackground"] = Colors.Transparent;
            res["WindowCaptionForeground"] = Colors.Black;
            if (Win32.MainWindow_Handle == Win32.GetActiveWindow())
            {
                Win32.SendMessage(Win32.MainWindow_Handle, Win32.WM_ACTIVATE, Win32.WA_INACTIVE, IntPtr.Zero);
                Win32.SendMessage(Win32.MainWindow_Handle, Win32.WM_ACTIVATE, Win32.WA_ACTIVE, IntPtr.Zero);
            }
            else
            {
                Win32.SendMessage(Win32.MainWindow_Handle, Win32.WM_ACTIVATE, Win32.WA_ACTIVE, IntPtr.Zero);
                Win32.SendMessage(Win32.MainWindow_Handle, Win32.WM_ACTIVATE, Win32.WA_INACTIVE, IntPtr.Zero);
            }

            SizeChanged += MainWindow_SizeChanged;

            AccountButton.Click += (sender, args) => { new Flyout() { Content = AccountInfoContent.GetContent() }.ShowAt(AccountButton); };
        }

        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            UpdateDragRects();
        }

        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDragRects();
        }

        /// <summary>
        /// 更新窗口标题栏拖拽部分，便于自定义标题栏控件接收鼠标输入。
        /// 将可拖拽部分分为两部分——自定义控件的左右两边的部分，然后计算两个部分对应的矩形区域，最后注册到窗口标题栏处。
        /// </summary>
        public void UpdateDragRects()
        {
            AppWindowTitleBar titleBar = AppWindow.TitleBar;

            // 标题栏实际尺寸。
            int totalWidth = Win32.GetActualPixel(AppTitleBar.ActualWidth);
            int totalHeight = Win32.GetActualPixel(AppTitleBar.ActualHeight);

            // 自定义控件的左边界相对于整个控件左边界的偏移量。
            int controlLeftOffset = Win32.GetActualPixel(CustomTitleBarControls.ActualOffset.X);

            // 自定义控件的右边界相对于整个控件左边界的偏移量。
            int controlRightOffset = Win32.GetActualPixel(controlLeftOffset + CustomTitleBarControls.ActualWidth);

            int leftSpace = controlLeftOffset;
            int rightSpace = totalWidth - controlLeftOffset - Win32.GetActualPixel(CustomTitleBarControls.ActualWidth);
            int CaptionButtonOcclusionWidthRight = AppWindow.TitleBar.RightInset;
            RightPaddingColumn.Width = new GridLength(CaptionButtonOcclusionWidthRight / Win32.PixelZoom);

            // TODO 计算窗口按钮宽度并排除
            RectInt32 leftRect = new(0, 0, Convert.ToInt32(leftSpace), Convert.ToInt32(totalHeight));
            RectInt32 rightRect = new(Convert.ToInt32(controlRightOffset), 0, Convert.ToInt32(rightSpace - CaptionButtonOcclusionWidthRight), Convert.ToInt32(totalHeight));
            titleBar.SetDragRectangles(new RectInt32[] { leftRect, rightRect });
        }

        //下载命令
        private void DownloadGameCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {

        }

        //导入命令
        private async void ImportGameCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            ImporterGameDialog importerGameDialog = new(Content.XamlRoot);
            if (await importerGameDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                foreach (Minecraft item in Minecraft.GetMinecrafts((importerGameDialog.Content as ImportGameDialogContent).PathBox.Text))
                    Navigation.MenuItems.Add(new NavigationViewItem() { Icon = new SymbolIcon(Symbol.Flag), Content = item.VersionID, Tag = item });
            }
        }

        private void Navigation_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if ((sender.SelectedItem as NavigationViewItem).Tag is Minecraft)
                MainFrame.Navigate(typeof(ManagePanel), (sender.SelectedItem as NavigationViewItem).Tag);
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            LauncherDataHelper.RemoveTempFiles();
        }
    }

    public enum LauncherState
    {
        OK,
        LUNCHING,
        DOWNLOADING,
        GAMING,
        ERROR
    }
}

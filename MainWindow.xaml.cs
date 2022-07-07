using AppUIBasics;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MCHUB
{
    public sealed partial class MainWindow : Window
    {
        private AppWindow AppWindow;
        public List<User> Users;

        public MainWindow()
        {
            InitializeComponent();
            Win32.Init(this);
            AppWindow = AppUIBasics.Win32.GetAppWindow();
            AppWindow.Resize(new SizeInt32(Win32.GetActualPixel(800), Win32.GetActualPixel(600)));

            //自定义标题栏
            Title = "MCHUB";

            //设置标题栏颜色并刷新
            AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            ExtendsContentIntoTitleBar = true;
            var res = Application.Current.Resources;
            res["WindowCaptionBackground"] = Colors.Transparent;
            res["WindowCaptionForeground"] = Colors.Black;
            if (AppUIBasics.Win32.MainWindow_Handle == AppUIBasics.Win32.GetActiveWindow())
            {
                AppUIBasics.Win32.SendMessage(AppUIBasics.Win32.MainWindow_Handle, AppUIBasics.Win32.WM_ACTIVATE, AppUIBasics.Win32.WA_INACTIVE, IntPtr.Zero);
                AppUIBasics.Win32.SendMessage(AppUIBasics.Win32.MainWindow_Handle, AppUIBasics.Win32.WM_ACTIVATE, AppUIBasics.Win32.WA_ACTIVE, IntPtr.Zero);
            }
            else
            {
                AppUIBasics.Win32.SendMessage(AppUIBasics.Win32.MainWindow_Handle, AppUIBasics.Win32.WM_ACTIVATE, AppUIBasics.Win32.WA_ACTIVE, IntPtr.Zero);
                AppUIBasics.Win32.SendMessage(AppUIBasics.Win32.MainWindow_Handle, AppUIBasics.Win32.WM_ACTIVATE, AppUIBasics.Win32.WA_INACTIVE, IntPtr.Zero);
            }

            SizeChanged += MainWindow_SizeChanged;

            Users = new List<User>();
            this.AccountButton.Flyout = new Flyout() { Content = new LoginAccountDialogContent() };
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
            var titleBar = AppWindow.TitleBar;

            // 标题栏实际尺寸。
            var totalWidth = Win32.GetActualPixel(AppTitleBar.ActualWidth);
            var totalHeight = Win32.GetActualPixel(AppTitleBar.ActualHeight);

            // 自定义控件的左边界相对于整个控件左边界的偏移量。
            var controlLeftOffset = Win32.GetActualPixel(CustomTitleBarControls.ActualOffset.X);

            // 自定义控件的右边界相对于整个控件左边界的偏移量。
            var controlRightOffset = Win32.GetActualPixel(controlLeftOffset + CustomTitleBarControls.ActualWidth);

            var leftSpace = controlLeftOffset;
            var rightSpace = totalWidth - controlLeftOffset - Win32.GetActualPixel(CustomTitleBarControls.ActualWidth);
            int CaptionButtonOcclusionWidthRight = AppWindow.TitleBar.RightInset;
            RightPaddingColumn.Width = new GridLength(CaptionButtonOcclusionWidthRight / AppUIBasics.Win32.PixelZoom);

            // TODO 计算窗口按钮宽度并排除
            var leftRect = new RectInt32(0, 0, Convert.ToInt32(leftSpace), Convert.ToInt32(totalHeight));
            var rightRect = new RectInt32(Convert.ToInt32(controlRightOffset), 0, Convert.ToInt32(rightSpace - CaptionButtonOcclusionWidthRight), Convert.ToInt32(totalHeight));
            titleBar.SetDragRectangles(new RectInt32[] { leftRect, rightRect });
        }

        //下载命令
        private void DownloadGameCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {

        }

        //导入命令
        private async void ImportGameCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            ImporterGameDialog importerGameDialog = new ImporterGameDialog(Content.XamlRoot);
            if (await importerGameDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                foreach (var item in Minecraft.GetMinecrafts((importerGameDialog.Content as ImportGameDialogContent).PathBox.Text))
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

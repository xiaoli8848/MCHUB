using Windows.Graphics;
using System.Runtime.InteropServices; // For DllImport
using WinRT; // required to support Window.As<ICompositionSupportsSystemBackdrop>()

namespace MCHUB;

public sealed partial class MainWindow : Window
{
    public static User CurrentUser = null;
    private AppWindow AppWindow;

    public MainWindow()
    {
        InitializeComponent();
        //初始化工具类。
        UIHelper.Init(this);
        LauncherDataHelper.Init();

        AppWindow = UIHelper.GetAppWindow();
        AppWindow.Resize(new SizeInt32(UIHelper.GetActualPixel(800), UIHelper.GetActualPixel(600)));
        //自定义标题栏。
        this.Title = "MCHUB";
        //设置标题栏颜色并刷新。
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        this.ExtendsContentIntoTitleBar = true;
        ResourceDictionary res = Application.Current.Resources;
        res["WindowCaptionBackground"] = Colors.Transparent;
        res["WindowCaptionForeground"] = Colors.Black;
        if (UIHelper.MainWindow_Handle == UIHelper.GetActiveWindow())
        {
            UIHelper.SendMessage(UIHelper.MainWindow_Handle, UIHelper.WM_ACTIVATE, UIHelper.WA_INACTIVE, IntPtr.Zero);
            UIHelper.SendMessage(UIHelper.MainWindow_Handle, UIHelper.WM_ACTIVATE, UIHelper.WA_ACTIVE, IntPtr.Zero);
        }
        else
        {
            UIHelper.SendMessage(UIHelper.MainWindow_Handle, UIHelper.WM_ACTIVATE, UIHelper.WA_ACTIVE, IntPtr.Zero);
            UIHelper.SendMessage(UIHelper.MainWindow_Handle, UIHelper.WM_ACTIVATE, UIHelper.WA_INACTIVE, IntPtr.Zero);
        }

        AccountButton.Click += (_, _) => { new Flyout() { Content = AccountInfoContent.GetContent() }.ShowAt(AccountButton); };
        this.Closed += (_, _) =>
        {
            LauncherDataHelper.RemoveTempFiles();
        };
    }

    /// <summary>
    /// 更新窗口标题栏拖拽部分，便于自定义标题栏控件接收鼠标输入。
    /// <br/>
    /// 将可拖拽部分分为两部分——自定义控件的左右两边的部分，然后计算两个部分对应的矩形区域，最后注册到窗口标题栏处。
    /// </summary>
    public void UpdateDragRects()
    {
        AppWindowTitleBar titleBar = AppWindow.TitleBar;

        // 标题栏实际尺寸。
        var totalWidth = UIHelper.GetActualPixel(AppTitleBar.ActualWidth);
        var totalHeight = UIHelper.GetActualPixel(AppTitleBar.ActualHeight);

        // 自定义控件的左边界相对于整个控件左边界的偏移量。
        var controlLeftOffset = UIHelper.GetActualPixel(CustomTitleBarControls.ActualOffset.X);

        // 自定义控件的右边界相对于整个控件左边界的偏移量。
        var controlRightOffset = UIHelper.GetActualPixel(controlLeftOffset + CustomTitleBarControls.ActualWidth);

        var leftSpace = controlLeftOffset;
        var rightSpace = totalWidth - controlLeftOffset - UIHelper.GetActualPixel(CustomTitleBarControls.ActualWidth);
        var CaptionButtonOcclusionWidthRight = AppWindow.TitleBar.RightInset;
        RightPaddingColumn.Width = new GridLength(CaptionButtonOcclusionWidthRight / UIHelper.PixelZoom);

        // TODO 计算窗口按钮宽度并排除
        RectInt32 leftRect = new(0, 0, Convert.ToInt32(leftSpace), Convert.ToInt32(totalHeight));
        RectInt32 rightRect = new(Convert.ToInt32(controlRightOffset), 0, Convert.ToInt32(rightSpace - CaptionButtonOcclusionWidthRight), Convert.ToInt32(totalHeight));
        titleBar.SetDragRectangles(new RectInt32[] { leftRect, rightRect });
    }

    //下载命令
    private void DownloadGameCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        throw new NotImplementedException();
    }

    //导入命令
    private async void ImportGameCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        ImporterGameDialog importerGameDialog = new(this.Content.XamlRoot);
        if (await importerGameDialog.ShowAsync() == ContentDialogResult.Primary)
        {
            foreach (Minecraft item in Minecraft.GetMinecrafts((importerGameDialog.Content as ImportGameDialogContent).PathBox.Text))
                Navigation.MenuItems.Add(new NavigationViewItem() { Icon = new SymbolIcon(Symbol.Flag), Content = item.VersionID, Tag = item });
        }
    }

    private void MainGrid_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateDragRects();
        UIHelper.TrySetMicaBackdrop();
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        UpdateDragRects();
    }

    private void Navigation_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if ((sender.SelectedItem as NavigationViewItem).Tag is Minecraft)
            MainFrame.Navigate(typeof(ManagePanel), (sender.SelectedItem as NavigationViewItem).Tag);
    }
}

/// <summary>
/// 指示启动器状态。
/// <br/>
/// OK - 就绪。
/// <br/>
/// LAUNCHING - 正在启动游戏。
/// <br/>
/// DOWNLOADING - 正在下载游戏。
/// <br/>
/// GAMING - 游戏中。
/// <br/>
/// ERROR - 出错了。
/// </summary>
public enum LauncherState
{
    OK,
    LAUNCHING,
    DOWNLOADING,
    GAMING,
    ERROR
}
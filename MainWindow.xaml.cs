using MCHUB.Utility;
using Windows.Graphics;
using Windows.UI;

namespace MCHUB;

public sealed partial class MainWindow : Window
{
    private AppWindow AppWindow;

    public MainWindow()
    {
        InitializeComponent();
        //初始化工具类。
        UIHelper.Init(this);
        LauncherDataHelper.Init();


        AccountButton.Click += (_, _) =>
        {
            new Flyout { Content = AccountInfoContent.GetContent() }.ShowAt(AccountButton);
        };
        Closed += (_, _) => { LauncherDataHelper.RemoveTempFiles(); };
        Navigation.ItemsSource = LauncherDataHelper.MinecraftRoots;

        AppWindow = UIHelper.GetAppWindow();
        AppWindow.Resize(new SizeInt32(UIHelper.GetActualPixel(900), UIHelper.GetActualPixel(600)));
        //自定义标题栏。
        Title = "MCHUB";
        //设置标题栏颜色并刷新。
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        ExtendsContentIntoTitleBar = true;
#if DEBUG
        ImportMinecrafts(@"D:\Program Files\Minecraft\1.15\.minecraft");
#endif
    }

    /// <summary>
    /// 更新窗口标题栏拖拽部分，便于自定义标题栏控件接收鼠标输入。
    /// <br/>
    /// 将可拖拽部分分为两部分——自定义控件的左右两边的部分，然后计算两个部分对应的矩形区域，最后注册到窗口标题栏处。
    /// </summary>
    private void UpdateDragRects()
    {
        var titleBar = AppWindow.TitleBar;
        titleBar.SetDragRectangles(new RectInt32[]
        {
            new(0, 0, Convert.ToInt32(UIHelper.GetActualPixel(Content.ActualSize.Length()) - titleBar.RightInset),
                UIHelper.GetActualPixel(AppTitleBar.ActualHeight))
        });
    }

    //下载命令
    private void DownloadGameCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        throw new NotImplementedException();
    }

    //导入命令
    private async void ImportGameCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        ImporterGameDialog importerGameDialog = new(Content.XamlRoot);
        if (await importerGameDialog.ShowAsync() == ContentDialogResult.Primary)
            ImportMinecrafts((importerGameDialog.Content as ImportGameDialogContent).PathBox.Text);
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

    private void Navigation_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        MainFrame.Navigate(typeof(ManagePanel),
            args.InvokedItem is MinecraftRoot
                ? args.InvokedItem
                : LauncherDataHelper.GetMinecraftRoot((Minecraft)args.InvokedItem));
    }

    private void ImportMinecrafts(string path)
    {
        LauncherDataHelper.MinecraftRoots.Add(MinecraftRoot.GetRoot(path));
    }

    public MinecraftRoot GetCurrentMinecraftRoot()
    {
        return Navigation.SelectedItem is MinecraftRoot
            ? (MinecraftRoot)Navigation.SelectedItem
            : LauncherDataHelper.GetMinecraftRoot((Minecraft)Navigation.SelectedItem);
    }

    public Minecraft GetCurrentMinecraft()
    {
        return Navigation.SelectedItem is Minecraft
            ? (Minecraft)Navigation.SelectedItem
            : ((MinecraftRoot)Navigation.SelectedItem).Minecrafts[0];
    }

    private async void LaunchButton_OnClick(object sender, RoutedEventArgs e)
    {
        await GetCurrentMinecraft().LaunchAsync(LauncherDataHelper.CurrentUser.Authenticator, JavaEnvironment.Current);
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
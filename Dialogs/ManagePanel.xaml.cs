namespace MCHUB;

public sealed partial class ManagePanel : Page
{
    private Minecraft Current;

    public ManagePanel()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        MainGrid.DataContext = e.Parameter as Minecraft;
        Current = (Minecraft)e.Parameter;
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        var environment = JavaEnvironment.Current;
        if (environment == null) throw new LaunchArgumentException(Current, "JavaEnvironment", "Null");
        Current.LaunchCommand.Execute(new LaunchArgument()
        {
            Authenticator = MainWindow.CurrentUser.Authenticator, Fullscreen = false,
            JavaEnvironment = JavaEnvironment.Current
        });
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start("explorer.exe", Current.Root.FullName);
    }
}
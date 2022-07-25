namespace MCHUB;

public sealed partial class ManagePanel : Page
{
    private MinecraftRoot _current;

    public ManagePanel()
    {
        InitializeComponent();
        VersionSelector.SelectedIndex = 0;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        MainGrid.DataContext = e.Parameter as MinecraftRoot;
        _current = (MinecraftRoot)e.Parameter;
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start("explorer.exe", _current.Path);
    }
}
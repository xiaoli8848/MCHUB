namespace MCHUB;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AccountInfoContent : Page
{
    private AccountInfoContent()
    {
        InitializeComponent();
    }

    public static UIElement GetContent()
    {
        UIElement content = new AccountInfoContent();
        User user = MainWindow.CurrentUser;
        switch (user)
        {
            case MojangUser mUser:
                (content as AccountInfoContent).Info.Text = "在线用户，" + (mUser.IsAvailabel ? "在线" : "离线");
                (content as AccountInfoContent).RefreshButton.Visibility = Visibility.Visible;
                (content as AccountInfoContent).RefreshButton.Click += (sender, args) => { LauncherDataHelper.RefreshUsers(); };
                break;
            case OfflineUser _:
                (content as AccountInfoContent).Info.Text = "离线用户，离线";
                (content as AccountInfoContent).RefreshButton.Visibility = Visibility.Collapsed;
                break;
            case null:
                StackPanel panel = new();
                TextBlock text = new() { Style = (Style)Application.Current.Resources["BaseTextBlockStyle"], Text = "尚未登录。", Margin = new Thickness(0, 0, 0, 12) };
                panel.Children.Add(text);
                var button = new Button() { Content = "登录" };
                button.Click += async (sender, args) => { await LoginAccountDialog.LoginAsync(UIHelper.GetMainWindow().Content.XamlRoot); };
                panel.Children.Add(button);
                content = panel;
                return content;
        }
        (content as AccountInfoContent).UserName.Text = user.Name;
        (content as AccountInfoContent).LoginButton.Click += async (sender, e) => { await LoginAccountDialog.LoginAsync(UIHelper.GetMainWindow().Content.XamlRoot); };
        return content;
    }
}

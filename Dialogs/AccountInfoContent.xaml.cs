using MCHUB.Utility;

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
        var user = LauncherDataHelper.CurrentUser;
        switch (user)
        {
            case MicrosoftUser mUser:
                (content as AccountInfoContent).Info.Text = "Microsoft用户";
                (content as AccountInfoContent).RefreshButton.Visibility = Visibility.Visible;
                (content as AccountInfoContent).RefreshButton.Click += async (_, _) =>
                {
                    await LauncherDataHelper.RefreshUsersAsync();
                };
                (content as AccountInfoContent).CoName.Text = mUser.IsAvailabel ? "在线" : "离线";
                break;
            case OfflineUser _:
                (content as AccountInfoContent).Info.Text = "离线用户";
                (content as AccountInfoContent).RefreshButton.Visibility = Visibility.Collapsed;
                (content as AccountInfoContent).CoName.Visibility = Visibility.Collapsed;
                break;
            case null:
                StackPanel panel = new();
                TextBlock text = new()
                {
                    Style = (Style)Application.Current.Resources["BaseTextBlockStyle"], Text = "尚未登录。",
                    Margin = new Thickness(0, 0, 0, 12)
                };
                panel.Children.Add(text);
                var offlineLoginButton = new Button() { Content = "登录离线账户" };
                offlineLoginButton.Click += async (sender, args) =>
                {
                    await LoginAccountDialog.LoginAsync(UIHelper.GetMainWindow().Content.XamlRoot);
                };
                panel.Children.Add(offlineLoginButton);
                var onlineLoginButton = new Button() { Content = "登录Microsoft账户", Margin = new Thickness(0, 8, 0, 0) };
                onlineLoginButton.Click += async (sender, args) =>
                {
                    var user = new MicrosoftUser(await MicrosoftUser.GetCodeAsync());
                    LauncherDataHelper.CurrentUser = user;
                    LauncherDataHelper.Users.Add(user);
                };
                panel.Children.Add(onlineLoginButton);
                content = panel;
                return content;
        }

        (content as AccountInfoContent).UserName.Text = user.Name;
        (content as AccountInfoContent).LoginButton.Click += async (_, _) =>
        {
            await LoginAccountDialog.LoginAsync(UIHelper.GetMainWindow().Content.XamlRoot);
        };
        return content;
    }
}
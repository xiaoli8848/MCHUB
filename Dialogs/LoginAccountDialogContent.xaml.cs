using MCHUB.Utility;

namespace MCHUB;

public sealed partial class LoginAccountDialogContent : Page
{
    public LoginAccountDialogContent()
    {
        InitializeComponent();
    }
}

public sealed class LoginAccountDialog : ContentDialog
{
    public LoginAccountDialog(XamlRoot root) : base()
    {
        XamlRoot = root;
        Title = "登录Minecraft离线账户";
        Content = new LoginAccountDialogContent();
        Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        CloseButtonText = "取消";
        DefaultButton = ContentDialogButton.Close;
        IsPrimaryButtonEnabled = false;
        PrimaryButtonText = "登录";

        (Content as LoginAccountDialogContent).Username.TextChanged += OnUsernameChanged;
    }

    /// <summary>
    /// 侦测当用户名与密码都不为空时，登录按钮可用。
    /// </summary>
    private void OnUsernameChanged(object sender, TextChangedEventArgs args)
    {
        IsPrimaryButtonEnabled = (Content as LoginAccountDialogContent).Username.Text.Length != 0;
    }

    public static async Task LoginAsync(XamlRoot root)
    {
        var dialog = new LoginAccountDialog(root);
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            var user_offline = new OfflineUser((dialog.Content as LoginAccountDialogContent).Username.Text);
            LauncherDataHelper.CurrentUser = user_offline;
            LauncherDataHelper.Users.Add(user_offline);
        }
    }
}
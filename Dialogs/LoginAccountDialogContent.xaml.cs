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
        this.XamlRoot = root;
        this.Title = "登录Minecraft离线账户";
        this.Content = new LoginAccountDialogContent();
        this.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        this.CloseButtonText = "取消";
        this.DefaultButton = ContentDialogButton.Close;
        this.IsPrimaryButtonEnabled = false;
        this.PrimaryButtonText = "登录";

        (this.Content as LoginAccountDialogContent).username.TextChanged += OnUsernameChanged;
    }

    /// <summary>
    /// 侦测当用户名与密码都不为空时，登录按钮可用。
    /// </summary>
    private void OnUsernameChanged(object sender, TextChangedEventArgs args)
    {
        this.IsPrimaryButtonEnabled = (this.Content as LoginAccountDialogContent).username.Text.Length != 0;
    }

    public static async Task LoginAsync(XamlRoot root)
    {
        var dialog = new LoginAccountDialog(root);
        if ((await dialog.ShowAsync()) == ContentDialogResult.Primary)
        {
            var user_offline = new OfflineUser((dialog.Content as LoginAccountDialogContent).username.Text);
            MainWindow.CurrentUser = user_offline;
            LauncherDataHelper.Users.Add(user_offline);
        }
    }
}

namespace MCHUB;

public sealed partial class LoginAccountDialogContent : Page
{
    public LoginAccountDialogContent()
    {
        InitializeComponent();
        combobox.SelectionChanged += ComboBox_SelectionChanged;
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        password.Visibility = (e.AddedItems[0] as string) != "离线账户" ? Visibility.Visible : Visibility.Collapsed;
    }
}

public sealed class LoginAccountDialog : ContentDialog
{
    public LoginAccountDialog(XamlRoot root) : base()
    {
        this.XamlRoot = root;
        this.Title = "登录Minecraft账户";
        this.Content = new LoginAccountDialogContent();
        this.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        this.CloseButtonText = "取消";
        this.DefaultButton = ContentDialogButton.Close;
        this.IsPrimaryButtonEnabled = false;
        this.PrimaryButtonText = "登录";

        (this.Content as LoginAccountDialogContent).password.PasswordChanged += OnPasswordChanged;
        (this.Content as LoginAccountDialogContent).username.TextChanged += OnUsernameChanged;
    }

    private void OnUsernameChanged(object sender, TextChangedEventArgs args)
    {
        this.IsPrimaryButtonEnabled =
           (string)(this.Content as LoginAccountDialogContent).combobox.SelectedItem == "离线账户" ?
           (this.Content as LoginAccountDialogContent).username.Text.Length != 0 :
           (this.Content as LoginAccountDialogContent).password.Password.Length != 0 && (this.Content as LoginAccountDialogContent).username.Text.Length != 0;
    }
    private void OnPasswordChanged(object sender, RoutedEventArgs args)
    {
        this.IsPrimaryButtonEnabled =
            (string)(this.Content as LoginAccountDialogContent).combobox.SelectedItem == "离线账户" ?
            (this.Content as LoginAccountDialogContent).username.Text.Length != 0 :
            (this.Content as LoginAccountDialogContent).password.Password.Length != 0 && (this.Content as LoginAccountDialogContent).username.Text.Length != 0;

    }

    public static async Task LoginAsync(XamlRoot root)
    {
        var dialog = new LoginAccountDialog(root);
        if ((await dialog.ShowAsync()) == ContentDialogResult.Primary)
        {
            switch ((string)(dialog.Content as LoginAccountDialogContent).combobox.SelectedItem)
            {
                case "离线账户":
                    var user_offline = new OfflineUser((dialog.Content as LoginAccountDialogContent).username.Text);
                    MainWindow.CurrentUser = user_offline;
                    LauncherDataHelper.Users.Add(user_offline);
                    break;
                case "Mojang账户":
                    var user_mojang = new MojangUser((dialog.Content as LoginAccountDialogContent).username.Text, (dialog.Content as LoginAccountDialogContent).password.Password);
                    MainWindow.CurrentUser = user_mojang;
                    LauncherDataHelper.Users.Add(user_mojang);
                    break;
            }
        }
    }
}

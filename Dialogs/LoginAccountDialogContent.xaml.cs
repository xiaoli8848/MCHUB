using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MCHUB
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginAccountDialogContent : Page
    {
        public LoginAccountDialogContent()
        {
            this.InitializeComponent();
            combobox.SelectionChanged += ComboBox_SelectionChanged;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] as string != "离线账户")
            {
                password.Visibility = Visibility.Visible;
            }
            else
            {
                password.Visibility = Visibility.Collapsed;
            }
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

        public async static Task LoginAsync(XamlRoot root)
        {
            LoginAccountDialog dialog = new LoginAccountDialog(root);
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
}

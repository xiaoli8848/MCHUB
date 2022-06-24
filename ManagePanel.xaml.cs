using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MCHUB
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManagePanel : Page
    {
        private Minecraft Current;
        public ManagePanel()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MainGrid.DataContext = e.Parameter as Minecraft;
            Current = (Minecraft)e.Parameter;
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            JavaEnvironment environment = JavaEnvironment.Current;
            if (environment == null)
            {
                throw new LaunchArgumentException(Current, "JavaEnvironment", "Null");
            }
            Current.LaunchCommand.Execute(new LaunchArgument() { Authenticator = new ModuleLauncher.Re.Authenticators.OfflineAuthenticator("XiaoLi8848"), Fullscreen = false, JavaEnvironment = JavaEnvironment.Current });
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Current.Root.FullName);
        }
    }

}

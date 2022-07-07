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
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MCHUB
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImportGameDialogContent : Page
    {
        public ImportGameDialogContent()
        {
            this.InitializeComponent();
        }
    }

    public sealed class ImporterGameDialog : ContentDialog
    {
        public ImporterGameDialog(XamlRoot root) : base()
        {
            this.Title = "导入Minecraft";
            this.Content = new ImportGameDialogContent();
            this.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            this.CloseButtonText = "取消";
            this.DefaultButton = ContentDialogButton.Close;
            this.IsPrimaryButtonEnabled = false;
            this.PrimaryButtonText = "导入";
            this.XamlRoot = root;

            (this.Content as ImportGameDialogContent).PickerButton.Click += async (sender, arg) => 
            {
                FolderPicker picker = new();
                picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                picker.CommitButtonText = "导入";
                picker.SettingsIdentifier = "settingsIdentifier";

                var hwnd = WindowNative.GetWindowHandle((Application.Current as App).m_window);  // App.m_window?
                InitializeWithWindow.Initialize(picker, hwnd);

                StorageFolder folder = await picker.PickSingleFolderAsync();

                (this.Content as ImportGameDialogContent).PathBox.Text = folder.Path;
                this.IsPrimaryButtonEnabled = true;
            };
        }
    }
}

using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace MCHUB;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ImportGameDialogContent : Page
{
    public ImportGameDialogContent()
    {
        InitializeComponent();
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
            FolderPicker picker = new()
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                CommitButtonText = "导入",
                SettingsIdentifier = "settingsIdentifier"
            };

            IntPtr hwnd = WindowNative.GetWindowHandle((Application.Current as App).m_window);  // App.m_window?
            InitializeWithWindow.Initialize(picker, hwnd);

            StorageFolder folder = await picker.PickSingleFolderAsync();

            (this.Content as ImportGameDialogContent).PathBox.Text = folder.Path;
            this.IsPrimaryButtonEnabled = true;
        };
    }
}

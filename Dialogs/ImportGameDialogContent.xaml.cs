using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace MCHUB;

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
        Title = "导入Minecraft";
        Content = new ImportGameDialogContent();
        Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        CloseButtonText = "取消";
        DefaultButton = ContentDialogButton.Close;
        IsPrimaryButtonEnabled = false;
        PrimaryButtonText = "导入";
        XamlRoot = root;

        (Content as ImportGameDialogContent).PickerButton.Click += async (_, _) =>
        {
            FolderPicker picker = new()
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                CommitButtonText = "导入",
                SettingsIdentifier = "settingsIdentifier"
            };

            var hwnd = WindowNative.GetWindowHandle((Application.Current as App).m_window);
            InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();

            (Content as ImportGameDialogContent).PathBox.Text = folder.Path ?? "";
            IsPrimaryButtonEnabled = true;
        };
    }
}
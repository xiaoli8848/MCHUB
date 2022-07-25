// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MCHUB.Utility;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MCHUBResource
{
    public MCHUBResource()
    {
        InitializeComponent();
    }
}

public class MinecraftTemplateSelector : DataTemplateSelector
{
    public DataTemplate MinecraftTemplate { get; set; }
    public DataTemplate MinecraftRootTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is Minecraft)
            return MinecraftTemplate;
        else
            return MinecraftRootTemplate;
    }
}
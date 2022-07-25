// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MCHUB;

public sealed class ItemInfoBar : Control
{
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(ItemInfoBar),
        new PropertyMetadata(default(string), OnTitleChanged)
    );

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(ItemInfoBar),
        new PropertyMetadata(default(string), OnLabelChanged)
    );

    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(ImageSource),
        typeof(ItemInfoBar),
        new PropertyMetadata(default(ImageSource), OnIconChanged)
    );

    public ItemInfoBar()
    {
        DefaultStyleKey = typeof(ItemInfoBar);
        //this.Background = (Brush)App.Current.Resources["ControlFillColorDefaultBrush"];
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
}
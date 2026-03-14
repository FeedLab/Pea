using System.Windows.Input;

namespace Pea.Meter.Components;

public partial class ToolbarComponent : ContentView
{
    public static readonly BindableProperty Button1ImageSourceProperty =
        BindableProperty.Create(nameof(Button1ImageSource), typeof(ImageSource), typeof(ToolbarComponent), null);

    public static readonly BindableProperty Button2ImageSourceProperty =
        BindableProperty.Create(nameof(Button2ImageSource), typeof(ImageSource), typeof(ToolbarComponent), null);

    public static readonly BindableProperty Button3ImageSourceProperty =
        BindableProperty.Create(nameof(Button3ImageSource), typeof(ImageSource), typeof(ToolbarComponent), null);

    public static readonly BindableProperty Button1CommandProperty =
        BindableProperty.Create(nameof(Button1Command), typeof(ICommand), typeof(ToolbarComponent), null);

    public static readonly BindableProperty Button2CommandProperty =
        BindableProperty.Create(nameof(Button2Command), typeof(ICommand), typeof(ToolbarComponent), null);

    public static readonly BindableProperty Button3CommandProperty =
        BindableProperty.Create(nameof(Button3Command), typeof(ICommand), typeof(ToolbarComponent), null);

    public ImageSource Button1ImageSource
    {
        get => (ImageSource)GetValue(Button1ImageSourceProperty);
        set => SetValue(Button1ImageSourceProperty, value);
    }

    public ImageSource Button2ImageSource
    {
        get => (ImageSource)GetValue(Button2ImageSourceProperty);
        set => SetValue(Button2ImageSourceProperty, value);
    }

    public ImageSource Button3ImageSource
    {
        get => (ImageSource)GetValue(Button3ImageSourceProperty);
        set => SetValue(Button3ImageSourceProperty, value);
    }

    public ICommand Button1Command
    {
        get => (ICommand)GetValue(Button1CommandProperty);
        set => SetValue(Button1CommandProperty, value);
    }

    public ICommand Button2Command
    {
        get => (ICommand)GetValue(Button2CommandProperty);
        set => SetValue(Button2CommandProperty, value);
    }

    public ICommand Button3Command
    {
        get => (ICommand)GetValue(Button3CommandProperty);
        set => SetValue(Button3CommandProperty, value);
    }

    public ToolbarComponent()
    {
        InitializeComponent();
    }

    private void OnQuitTapped(object sender, EventArgs e)
    {
        Application.Current?.Quit();
    }
}

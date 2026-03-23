namespace Pea.Meter.Components;

public partial class SwitchComponent : ContentView
{
    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(SwitchComponent), string.Empty);

    public static readonly BindableProperty IsOnProperty =
        BindableProperty.Create(nameof(IsOn), typeof(bool), typeof(SwitchComponent), false, BindingMode.TwoWay);

    public static readonly BindableProperty TextWhenOnProperty =
        BindableProperty.Create(nameof(TextWhenOn), typeof(string), typeof(SwitchComponent), string.Empty);

    public static readonly BindableProperty TextWhenOffProperty =
        BindableProperty.Create(nameof(TextWhenOff), typeof(string), typeof(SwitchComponent), string.Empty);

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public bool IsOn
    {
        get => (bool)GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    public string TextWhenOn
    {
        get => (string)GetValue(TextWhenOnProperty);
        set => SetValue(TextWhenOnProperty, value);
    }

    public string TextWhenOff
    {
        get => (string)GetValue(TextWhenOffProperty);
        set => SetValue(TextWhenOffProperty, value);
    }

    public SwitchComponent()
    {
        InitializeComponent();
    }
}

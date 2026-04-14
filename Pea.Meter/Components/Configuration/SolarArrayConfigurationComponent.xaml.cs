namespace Pea.Meter.Components.Configuration;

public partial class SolarArrayConfigurationComponent : ContentView
{
    public static readonly BindableProperty TiltProperty =
        BindableProperty.Create(nameof(Tilt), typeof(double), typeof(SolarArrayConfigurationComponent), 0.0, BindingMode.TwoWay);

    public static readonly BindableProperty ArraySizeProperty =
        BindableProperty.Create(nameof(ArraySize), typeof(double), typeof(SolarArrayConfigurationComponent), 5.0, BindingMode.TwoWay);

    public static readonly BindableProperty BatterySizeProperty =
        BindableProperty.Create(nameof(BatterySize), typeof(double), typeof(SolarArrayConfigurationComponent), 10.0, BindingMode.TwoWay);

    public static readonly BindableProperty SelectedDirectionProperty =
        BindableProperty.Create(nameof(SelectedDirection), typeof(int), typeof(SolarArrayConfigurationComponent), 4, BindingMode.TwoWay);

    public double Tilt
    {
        get => (double)GetValue(TiltProperty);
        set => SetValue(TiltProperty, value);
    }

    public double ArraySize
    {
        get => (double)GetValue(ArraySizeProperty);
        set => SetValue(ArraySizeProperty, value);
    }

    public double BatterySize
    {
        get => (double)GetValue(BatterySizeProperty);
        set => SetValue(BatterySizeProperty, value);
    }

    public int SelectedDirection
    {
        get => (int)GetValue(SelectedDirectionProperty);
        set => SetValue(SelectedDirectionProperty, value);
    }

    public event EventHandler<int>? DirectionChanged;

    public SolarArrayConfigurationComponent()
    {
        InitializeComponent();
    }

    private void OnCompassDirectionChanged(object? sender, int direction)
    {
        SelectedDirection = direction;
        DirectionChanged?.Invoke(this, direction);
    }
}

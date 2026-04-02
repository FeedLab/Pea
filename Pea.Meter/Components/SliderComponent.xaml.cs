using System.Globalization;
using Syncfusion.Maui.Sliders;

namespace Pea.Meter.Components;

public partial class SliderComponent : Border
{
    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(SliderComponent), string.Empty);

    public static readonly BindableProperty MinimumProperty =
        BindableProperty.Create(nameof(Minimum), typeof(double), typeof(SliderComponent), 0.0);

    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(double), typeof(SliderComponent), 100.0);

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(SliderComponent), 0.0, BindingMode.TwoWay);

    public static readonly BindableProperty StepSizeProperty =
        BindableProperty.Create(nameof(StepSize), typeof(double), typeof(SliderComponent), 1.0);

    public static readonly BindableProperty IntervalProperty =
        BindableProperty.Create(nameof(Interval), typeof(double), typeof(SliderComponent), 10.0);

    public static readonly BindableProperty NumberFormatProperty =
        BindableProperty.Create(nameof(NumberFormat), typeof(string), typeof(SliderComponent), "0.00");

    public static readonly BindableProperty ThumbNumberFormatProperty =
        BindableProperty.Create(nameof(ThumbNumberFormat), typeof(string), typeof(SliderComponent), "{0:N0}");
    
    public static readonly BindableProperty ThumbColorProperty =
        BindableProperty.Create(nameof(ThumbColor), typeof(Color), typeof(SliderComponent), Colors.Red);

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double StepSize
    {
        get => (double)GetValue(StepSizeProperty);
        set => SetValue(StepSizeProperty, value);
    }

    public double Interval
    {
        get => (double)GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    public string NumberFormat
    {
        get => (string)GetValue(NumberFormatProperty);
        set => SetValue(NumberFormatProperty, value);
    }

    public string ThumbNumberFormat
    {
        get => (string)GetValue(ThumbNumberFormatProperty);
        set => SetValue(ThumbNumberFormatProperty, value);
    }
    
    public Color ThumbColor
    {
        get => (Color)GetValue(ThumbColorProperty);
        set => SetValue(ThumbColorProperty, value);
    }

    public SliderComponent()
    {
        InitializeComponent();
    }

    private void Slider_OnValueChanged(object? sender, SliderValueChangedEventArgs e)
    {
    }
}

public class StringFormatConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is double value && values[1] is string format)
            return string.Format(format, value);
        return values[0]?.ToString();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

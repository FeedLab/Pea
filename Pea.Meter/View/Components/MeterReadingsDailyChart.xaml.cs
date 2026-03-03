using Pea.Meter.Services;
using Pea.Meter.ViewModel.Statistics;

namespace Pea.Meter.View.Components;

public partial class MeterReadingsDailyChart : ContentView
{
    public static readonly BindableProperty ChartTitleProperty =
        BindableProperty.Create(nameof(ChartTitle), typeof(string), typeof(MeterReadingsDailyChart), "Overview Per Day");

    public static readonly BindableProperty YAxisTitleProperty =
        BindableProperty.Create(nameof(YAxisTitle), typeof(string), typeof(MeterReadingsDailyChart), "Energy consumption in KW per Day");

    public static readonly BindableProperty SeriesColorProperty =
        BindableProperty.Create(nameof(SeriesColor), typeof(Color), typeof(MeterReadingsDailyChart), Colors.Green);

    public string ChartTitle
    {
        get => (string)GetValue(ChartTitleProperty);
        set => SetValue(ChartTitleProperty, value);
    }

    public string YAxisTitle
    {
        get => (string)GetValue(YAxisTitleProperty);
        set => SetValue(YAxisTitleProperty, value);
    }

    public Color SeriesColor
    {
        get => (Color)GetValue(SeriesColorProperty);
        set => SetValue(SeriesColorProperty, value);
    }

    public MeterReadingsDailyChart()
    {
        InitializeComponent();

        if (AppService.Current != null)
            BindingContext = AppService.Current.GetRequiredService<MeterReadingsDailyViewModel>();
        else
            throw new InvalidOperationException("AppService is not initialized");
    }
}

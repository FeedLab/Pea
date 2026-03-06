using System.Collections.ObjectModel;
using Pea.Infrastructure.Models;

namespace Pea.Meter.View.Statistics;

public partial class MeterReadingsHourChart : ContentView
{
    public static readonly BindableProperty ChartTitleProperty =
        BindableProperty.Create(nameof(ChartTitle), typeof(string), typeof(MeterReadingsHourChart), string.Empty);

    public static readonly BindableProperty TodayDataProperty =
        BindableProperty.Create(nameof(TodayData), typeof(ObservableCollection<PeaMeterReading>), typeof(MeterReadingsHourChart), null);

    public static readonly BindableProperty Average1DayDataProperty =
        BindableProperty.Create(nameof(Average1DayData), typeof(ObservableCollection<PeaMeterReading>), typeof(MeterReadingsHourChart), null);

    public static readonly BindableProperty Average7DaysDataProperty =
        BindableProperty.Create(nameof(Average7DaysData), typeof(ObservableCollection<PeaMeterReading>), typeof(MeterReadingsHourChart), null);

    public static readonly BindableProperty Average30DaysDataProperty =
        BindableProperty.Create(nameof(Average30DaysData), typeof(ObservableCollection<PeaMeterReading>), typeof(MeterReadingsHourChart), null);

    public string ChartTitle
    {
        get => (string)GetValue(ChartTitleProperty);
        set => SetValue(ChartTitleProperty, value);
    }

    public ObservableCollection<PeaMeterReading> TodayData
    {
        get => (ObservableCollection<PeaMeterReading>)GetValue(TodayDataProperty);
        set => SetValue(TodayDataProperty, value);
    }

    public ObservableCollection<PeaMeterReading> Average1DayData
    {
        get => (ObservableCollection<PeaMeterReading>)GetValue(Average1DayDataProperty);
        set => SetValue(Average1DayDataProperty, value);
    }

    public ObservableCollection<PeaMeterReading> Average7DaysData
    {
        get => (ObservableCollection<PeaMeterReading>)GetValue(Average7DaysDataProperty);
        set => SetValue(Average7DaysDataProperty, value);
    }

    public ObservableCollection<PeaMeterReading> Average30DaysData
    {
        get => (ObservableCollection<PeaMeterReading>)GetValue(Average30DaysDataProperty);
        set => SetValue(Average30DaysDataProperty, value);
    }

    public MeterReadingsHourChart()
    {
        InitializeComponent();
    }
}

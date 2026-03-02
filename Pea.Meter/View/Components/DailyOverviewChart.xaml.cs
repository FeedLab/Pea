using System.Collections.ObjectModel;
using Pea.Infrastructure.Models;

namespace Pea.Meter.View.Components;

public partial class DailyOverviewChart : ContentView
{
    public static readonly BindableProperty ChartTitleProperty =
        BindableProperty.Create(nameof(ChartTitle), typeof(string), typeof(DailyOverviewChart), "Overview Per Day");

    public static readonly BindableProperty YAxisTitleProperty =
        BindableProperty.Create(nameof(YAxisTitle), typeof(string), typeof(DailyOverviewChart), "Energy consumption in KW per Day");

    public static readonly BindableProperty ChartDataProperty =
        BindableProperty.Create(nameof(ChartData), typeof(ObservableCollection<PeaMeterReading>), typeof(DailyOverviewChart), null);

    public static readonly BindableProperty SeriesColorProperty =
        BindableProperty.Create(nameof(SeriesColor), typeof(Color), typeof(DailyOverviewChart), Colors.Green);

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

    public ObservableCollection<PeaMeterReading> ChartData
    {
        get => (ObservableCollection<PeaMeterReading>)GetValue(ChartDataProperty);
        set => SetValue(ChartDataProperty, value);
    }

    public Color SeriesColor
    {
        get => (Color)GetValue(SeriesColorProperty);
        set => SetValue(SeriesColorProperty, value);
    }

    public DailyOverviewChart()
    {
        InitializeComponent();
    }
}

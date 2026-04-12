using Microsoft.Extensions.Logging;
using Pea.Meter.Services;
using Pea.Meter.ViewModel.Statistics;
using Syncfusion.Maui.Buttons;

namespace Pea.Meter.View.Components;

public partial class MeterReadingsDailyChart : ContentView
{
    private readonly MeterReadingsDailyViewModel viewModel;
    private readonly ILogger<MeterReadingsDailyChart> logger;

    public static readonly BindableProperty ChartTitleProperty =
        BindableProperty.Create(nameof(ChartTitle), typeof(string), typeof(MeterReadingsDailyChart), Pea.Meter.Resources.Strings.AppResources.OverviewPerDay);

    public static readonly BindableProperty YAxisTitleProperty =
        BindableProperty.Create(nameof(YAxisTitle), typeof(string), typeof(MeterReadingsDailyChart), Pea.Meter.Resources.Strings.AppResources.EnergyConsumptionInKWPerDay);

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
        logger = AppService.GetRequiredService<ILogger<MeterReadingsDailyChart>>();

        InitializeComponent();

        if (AppService.Current != null)
        {
            viewModel = AppService.Current.GetRequiredService<MeterReadingsDailyViewModel>();
            BindingContext = viewModel;
        }
        else
        {
            throw new InvalidOperationException("AppService is not initialized");
        }
        
        RadioButtonDaily.IsChecked = true;
    }

    private void ToggleButton_OnStateChanged(object? sender, StateChangedEventArgs e)
    {
        var toggleButton = sender as SfRadioButton;
        
        if(toggleButton == null)
            return;

        if (e.IsChecked ?? false)
        {
            if (toggleButton.Value.ToString() == "Daily")
            {
                ColumnSeriesPeek.TooltipTemplate = (DataTemplate)Resources["StackedTooltipTemplateDaily"];
                OffPeakEvening.TooltipTemplate = (DataTemplate)Resources["StackedTooltipTemplateDaily"];
                OffPeakMorning.TooltipTemplate = (DataTemplate)Resources["StackedTooltipTemplateDaily"];
                viewModel.TimeResolutionChanged(MeterReadingsDailyViewModel.TimeResolutionType.Daily);
            }
            else if (toggleButton.Value.ToString() == "Monthly")
            {
                ColumnSeriesPeek.TooltipTemplate = (DataTemplate)Resources["StackedTooltipTemplateMonthly"];
                OffPeakEvening.TooltipTemplate = (DataTemplate)Resources["StackedTooltipTemplateMonthly"];
                OffPeakMorning.TooltipTemplate = (DataTemplate)Resources["StackedTooltipTemplateMonthly"];
                viewModel.TimeResolutionChanged(MeterReadingsDailyViewModel.TimeResolutionType.Monthly);
            }
            else
            {
                logger.LogError("Unknown toggle button value");
            }
        }
    }

    private void OnMoveToFirstTapped(object? sender, EventArgs e)
    {
        viewModel.MoveTo(MeterReadingsDailyViewModel.ChartVisualPositionType.Start);
    }

    private void OnMoveToLastTapped(object? sender, EventArgs e)
    {
        viewModel.MoveTo(MeterReadingsDailyViewModel.ChartVisualPositionType.End);
    }
}

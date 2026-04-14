using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pea.Infrastructure.Helpers;
using Pea.Meter.Services;
using Pea.Meter.ViewModel;
using Syncfusion.Maui.Charts;

namespace Pea.Meter.View.Solar;

public partial class SolarSystemSizingView : ContentView
{
    private readonly SolarSystemSizingViewModel viewModel;

    public SolarSystemSizingView()
    {
        InitializeComponent();
        viewModel = AppService.GetRequiredService<SolarSystemSizingViewModel>();

        BindingContext = viewModel;

        var costTooltip = (DataTemplate)Resources["TooltipTemplateMonthly"];
        CostPeek.TooltipTemplate = costTooltip;
        CostOffPeek.TooltipTemplate = costTooltip;
        CostFlatRate.TooltipTemplate = costTooltip;
        CostPeekSolar.TooltipTemplate = costTooltip;
        CostOffPeekSolar.TooltipTemplate = costTooltip;
        CostFlatRateSolar.TooltipTemplate = costTooltip;

        var kwhTooltip = (DataTemplate)Resources["TooltipTemplateKwh"];
        KwhPeek.TooltipTemplate = kwhTooltip;
        KwhOffPeek.TooltipTemplate = kwhTooltip;
        KwhSolar.TooltipTemplate = kwhTooltip;
        KwhBattery.TooltipTemplate = kwhTooltip;
    }

    private void OnCompassDirectionChanged(object? sender, int e)
    {
    }

    private void ChartAxis_OnLabelCreated(object? sender, ChartAxisLabelEventArgs e)
    {
        e.Label = WattFormatter.Format((decimal)e.Position);
    }

    private void OnYAxisLabelCreated(object? sender, ChartAxisLabelEventArgs e)
    {
        e.Label = WattFormatter.Format((decimal)e.Position);
    }
}
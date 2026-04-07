using Pea.Meter.Services;
using Pea.Meter.ViewModel.Statistics;

namespace Pea.Meter.View.Statistics;

public partial class StatisticsView : ContentView
{
    public StatisticsView()
    {
        InitializeComponent();

        if (AppService.Current != null)
            BindingContext = AppService.Current.GetRequiredService<StatisticsViewModel>();
        else
            throw new InvalidOperationException("AppService is not initialized");

        AllHistoricReadings.Header = $"\U0001F4C8 {Pea.Meter.Resources.Strings.AppResources.AllHistoricReadings}";
        Summary30Days.Header = $"\U0001F4C6 {Pea.Meter.Resources.Strings.AppResources._30DaysSummary}";
    }
}
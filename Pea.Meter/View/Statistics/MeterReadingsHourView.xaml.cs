using Pea.Meter.Services;

namespace Pea.Meter.View.Statistics;

public partial class MeterReadingsHourView : ContentView
{
    public MeterReadingsHourView()
    {
        InitializeComponent();
        
        if (AppService.Current != null)
            BindingContext = AppService.Current.GetRequiredService<ViewModel.Statistics.MeterReadingsHourViewModel>();
        else
            throw new InvalidOperationException("AppService is not initialized");
    }
}

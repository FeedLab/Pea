using Pea.Meter.Services;

namespace Pea.Meter.View.Statistics;

public partial class MeterReadingsDailyView : ContentView
{
    public MeterReadingsDailyView()
    {
        InitializeComponent();
        
        if (AppService.Current != null)
            BindingContext = AppService.Current.GetRequiredService<ViewModel.Statistics.MeterReadingsDailyViewModel>();
        else
            throw new InvalidOperationException("AppService is not initialized");
    }
}

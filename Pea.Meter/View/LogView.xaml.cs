using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter.View;

public partial class LogView : ContentView
{
    public LogView()
    {
        InitializeComponent();

        if (AppService.Current != null)
            BindingContext = AppService.Current.GetRequiredService<LogViewModel>();
        else
            throw new InvalidOperationException("AppService is not initialized");
    }
}

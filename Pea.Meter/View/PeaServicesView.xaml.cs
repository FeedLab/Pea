using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter.View;

public partial class PeaServicesView : ContentView
{
    public PeaServicesView()
    {
        InitializeComponent();
        
        if (AppService.Current != null)
            BindingContext = AppService.Current.GetRequiredService<PeaServicesViewModel>();
        else
            throw new InvalidOperationException("AppService is not initialized");
    }
}
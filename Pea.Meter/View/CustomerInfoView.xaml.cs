using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter.View;

public partial class CustomerInfoView : ContentView
{
    public CustomerInfoView()
    {
        InitializeComponent();
        
        if (AppService.Current != null)
            BindingContext = AppService.Current.GetRequiredService<CustomerInfoViewModel>();
        else
            throw new InvalidOperationException("AppService is not initialized");
    }
}
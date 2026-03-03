using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter.View;

public partial class ErrorMessagePopup : ContentView
{
    public ErrorMessagePopup()
    {
        InitializeComponent();

        if (AppService.Current != null)
            BindingContext = AppService.Current.GetRequiredService<ErrorMessagePopupViewModel>();
        else
            throw new InvalidOperationException("AppService is not initialized");
    }
}

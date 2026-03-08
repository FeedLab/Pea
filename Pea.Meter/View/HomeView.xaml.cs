using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter.View;

public partial class HomeView : ContentView
{
    public HomeView()
    {
        InitializeComponent();
        
        if (AppService.Current != null)
            BindingContext = AppService.Current.GetRequiredService<HomeViewModel>();
        else
            throw new InvalidOperationException("AppService is not initialized");
    }

    private void TapGestureRecognizer_OnTapped(object? sender, TappedEventArgs e)
    {
        var url = "https://www.amr.pea.co.th/AMRWEB/Manual/usernameandpassword190925.pdf";
        Launcher.OpenAsync(new Uri(url));
    }
}
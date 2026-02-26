using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

public partial class PeaServicesViewModel
{
    private readonly IPopupService popupService;

    public PeaServicesViewModel(IPopupService popupService)
    {
        this.popupService = popupService;
    }

    [RelayCommand]
    private async Task OpenUrl(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            Uri uri = new Uri(url);
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unable to open URL: {ex.Message}");
            //await Application.Current.MainPage.DisplayAlert("Error", $"Unable to open URL: {ex.Message}", "OK");
        }
    }
}
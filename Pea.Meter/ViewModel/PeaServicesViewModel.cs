using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Meter.Models;

namespace Pea.Meter.ViewModel;

public partial class PeaServicesViewModel
{
    [RelayCommand]
    private async Task OpenUrl(string url)
    {
        try
        {
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.External);
        }
        catch (Exception ex)
        {
            // Handle exception if needed
            System.Diagnostics.Debug.WriteLine($"Unable to open URL: {ex.Message}");
        }
    }
}
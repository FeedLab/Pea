using CommunityToolkit.Maui;
using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter.Extension;

public static class ErrorMessagePopupExtension
{
    private static readonly IPopupService PopupService = AppService.GetRequiredService<IPopupService>();
    
    public static async Task ShowErrorMessageAsync(this System.Exception exception, string label)
    {
        await ShowPopupAsync(exception.Message, "Error");
    }
    
    public static async Task ShowErrorMessageAsync(this string message, string label)
    {
        await ShowPopupAsync(message, "Error");
    }
    
    public static async Task ShowPopupAsync( string message, string label)
    {
        var queryAttributes = new Dictionary<string, object>
        {
            [nameof(ViewModel.ErrorMessagePopupViewModel.ErrorMessage)] = message,
            [nameof(ViewModel.ErrorMessagePopupViewModel.Label)] = label
        };

        var popupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true
        };
        
        await PopupService.ShowPopupAsync<ErrorMessagePopupViewModel>(
            Shell.Current,
            options: popupOptions,
            queryAttributes);
    }
}

using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Extensions.Logging;
using Pea.Meter.Popup;
using Pea.Meter.Services;
using Pea.Meter.View.Interface;

namespace Pea.Meter.Helpers;

public class PopupHelper
{
    private static ILogger<PopupHelper>? logger;

    public PopupHelper()
    {
        logger = AppService.GetRequiredService<ILogger<PopupHelper>>();
    }

    public async Task<bool?> ShowPopup<TView>(TView contentView, bool canBeDismissedByTappingOutsideOfPopup = false)
        where TView : ContentView, ICloseable
    {
        var popup = new ContentPopup(contentView);

        var popupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = canBeDismissedByTappingOutsideOfPopup
        };

        var windows = Application.Current?.Windows[0].Page;

        if (windows == null)
            return false;

        var popupResult = await windows.ShowPopupAsync<bool?>(popup, popupOptions, CancellationToken.None);

        contentView.CloseAction();

        return popupResult.Result;
    }

    public async Task ShowAutoClosePopup<TView>(TView contentView, Func<Task> action)
        where TView : ContentView, ICloseable
    {
        var popupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = false
        };

        var windows = Application.Current?.Windows[0].Page;

        if (windows == null)
            return;

        var popup = new ContentPopup(contentView);
        windows.ShowPopup(popup, popupOptions);

        await action();

        await MainThread.InvokeOnMainThreadAsync(() => popup.CloseAsync(true));
    }

    public static void ShowErrorPopup(string title, string message)
    {
        try
        {
            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = true
            };

            var windows = Application.Current?.Windows[0].Page;

            if (windows == null)
                return;

            windows.ShowPopup(new ErrorPopup(title, message), popupOptions);
        }
        catch (Exception exception)
        {
            logger?.LogError(exception, "Error showing error popup");
        }
    }
}
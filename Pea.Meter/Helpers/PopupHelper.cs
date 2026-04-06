using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Pea.Meter.Popup;
using Pea.Meter.View.Interface;

namespace Pea.Meter.Helpers;

public class PopupHelper
{
    public async Task<bool?> ShowPopup<TView>(TView contentView, bool canBeDismissedByTappingOutsideOfPopup = false) where TView : ContentView, ICloseable
    {
        var popup = new ContentPopup(contentView);
        
        var popupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = canBeDismissedByTappingOutsideOfPopup
        };

        var windows = Application.Current?.Windows[0].Page;
        
        if(windows == null)
            return false;
        
        var popupResult = await windows.ShowPopupAsync<bool?>(popup, popupOptions, CancellationToken.None);
        
        contentView.CloseAction();
        
        return popupResult.Result;
    }
}
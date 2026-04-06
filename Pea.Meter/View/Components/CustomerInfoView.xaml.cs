using Microsoft.Extensions.Logging;
using Pea.Meter.Helpers;
using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter.View.Components;

public partial class CustomerInfoView : ContentView
{
    private readonly PopupHelper popupHelper = new();
    private readonly ILogger<CustomerInfoView> logger;
    private readonly CustomerInfoViewModel viewModel;


    public CustomerInfoView()
    {
        InitializeComponent();

        viewModel = AppService.GetRequiredService<CustomerInfoViewModel>();
        logger = AppService.GetRequiredService<ILogger<CustomerInfoView>>();
        
        BindingContext = viewModel;
    }

    private async void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            await popupHelper.ShowPopup(new CustomerInfoPopupView(viewModel));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {Method}: {Message}", nameof(OnDoubleTapped), ex.Message);
        }
    }
}
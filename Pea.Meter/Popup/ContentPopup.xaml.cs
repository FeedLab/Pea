using Microsoft.Extensions.Logging;
using Pea.Meter.Services;
using CommunityToolkit.Maui.Views;
using Pea.Meter.View.Interface;

namespace Pea.Meter.Popup;

public partial class ContentPopup : Popup<bool>
{
    private readonly ILogger<ContentPopup> logger;

    public ContentPopup(ContentView contentView)
    {
        InitializeComponent();

        logger = AppService.GetRequiredService<ILogger<ContentPopup>>();

        BindingContext = this;
        
        PopupContentView.Content = contentView;
        
        ((ICloseable)contentView).CloseRequested += OnClose;
    }

    private void OnClose(object? sender, EventArgs e)
    {
        try
        {
            CloseAsync(true);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error closing popup (No)");
        }
    }
}
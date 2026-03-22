using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pea.Meter.Services;
using CommunityToolkit.Maui.Views;

namespace Pea.Meter.Popup;

public partial class QuitPopup : Popup<bool>
{
    private readonly ILogger<QuitPopup> logger;

    public QuitPopup(string message, string yesText = "Yes", string noText = "No")
    {
        InitializeComponent();

        logger = AppService.GetRequiredService<ILogger<QuitPopup>>();

        Message = message;
        YesText = yesText;
        NoText = noText;

        BindingContext = this;
    }

    public string Message { get; }
    public string YesText { get; }
    public string NoText { get; }

    private void OnYes(object sender, EventArgs e)
    {
        try
        {
            CloseAsync(true);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error closing popup (Yes)");
        }
    }

    private void OnNo(object sender, EventArgs e)
    {
        try
        {
            CloseAsync(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error closing popup (No)");
        }
    }
}
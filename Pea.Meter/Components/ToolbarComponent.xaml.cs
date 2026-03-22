using System.Windows.Input;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Extensions.Logging;
using Pea.Meter.Popup;
using Pea.Meter.Services;
using Syncfusion.Maui.Core;

namespace Pea.Meter.Components;

public partial class ToolbarComponent : ContentView
{
    private readonly ILogger<ToolbarComponent> logger;
    private readonly IPopupService popupService;
    private bool isProcessingQuit;
    private bool isProcessingonfiguration;

    public static readonly BindableProperty Button1ImageSourceProperty =
        BindableProperty.Create(nameof(Button1ImageSource), typeof(ImageSource), typeof(ToolbarComponent), null);

    public static readonly BindableProperty Button2ImageSourceProperty =
        BindableProperty.Create(nameof(Button2ImageSource), typeof(ImageSource), typeof(ToolbarComponent), null);

    public static readonly BindableProperty Button3ImageSourceProperty =
        BindableProperty.Create(nameof(Button3ImageSource), typeof(ImageSource), typeof(ToolbarComponent), null);

    public static readonly BindableProperty Button1CommandProperty =
        BindableProperty.Create(nameof(Button1Command), typeof(ICommand), typeof(ToolbarComponent), null);

    public static readonly BindableProperty Button2CommandProperty =
        BindableProperty.Create(nameof(Button2Command), typeof(ICommand), typeof(ToolbarComponent), null);

    public static readonly BindableProperty Button3CommandProperty =
        BindableProperty.Create(nameof(Button3Command), typeof(ICommand), typeof(ToolbarComponent), null);

    public ImageSource Button1ImageSource
    {
        get => (ImageSource)GetValue(Button1ImageSourceProperty);
        set => SetValue(Button1ImageSourceProperty, value);
    }

    public ImageSource Button2ImageSource
    {
        get => (ImageSource)GetValue(Button2ImageSourceProperty);
        set => SetValue(Button2ImageSourceProperty, value);
    }

    public ImageSource Button3ImageSource
    {
        get => (ImageSource)GetValue(Button3ImageSourceProperty);
        set => SetValue(Button3ImageSourceProperty, value);
    }

    public ICommand Button1Command
    {
        get => (ICommand)GetValue(Button1CommandProperty);
        set => SetValue(Button1CommandProperty, value);
    }

    public ICommand Button2Command
    {
        get => (ICommand)GetValue(Button2CommandProperty);
        set => SetValue(Button2CommandProperty, value);
    }

    public ICommand Button3Command
    {
        get => (ICommand)GetValue(Button3CommandProperty);
        set => SetValue(Button3CommandProperty, value);
    }

    public ToolbarComponent()
    {
        InitializeComponent();

        popupService = AppService.GetRequiredService<IPopupService>();
        logger = AppService.GetRequiredService<ILogger<ToolbarComponent>>();
    }

    private async void OnQuitTapped(object sender, EventArgs e)
    {
        try
        {
            if (isProcessingQuit)
                return;

            isProcessingQuit = true;

            var popup = new QuitPopup(
                message: "Quit Current Session?",
                yesText: "Yes, I'm done for today",
                noText: "No, it was my FAT fingers");

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false
            };

            var popupResult = await Window?.Page?.ShowPopupAsync<bool>(popup, popupOptions, CancellationToken.None)!;

            if (popupResult.Result)
            {
                Application.Current?.Quit();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occurred while handling quit popup result");
        }
        finally
        {
            isProcessingQuit = false;
            EffectsViewQuit.Reset();
        }
    }

    private void OnLanguageTapped(object? sender, TappedEventArgs e)
    {
        try
        {
        }
        finally
        {
            // EffectsViewLanguage.IsSelected = !EffectsViewQuit.IsSelected;
            EffectsViewLanguage.Reset();
        }
    }

    private async void OnConfigurationTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (isProcessingonfiguration)
                return;

            isProcessingonfiguration = true;

            var popup = new ConfigurationPopup();

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false
            };

            var popupResult = await Window?.Page?.ShowPopupAsync<bool>(popup, popupOptions, CancellationToken.None)!;

            if (popupResult.Result)
            {
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occurred while handling quit popup result");
        }
        finally
        {
            isProcessingonfiguration = false;
            EffectsViewConfiguration.Reset();
        }
    }

    private void EffectSelectionChanged(object? sender, EventArgs e)
    {
        // if (sender != null)
        // {
        //     var effectsView = (SfEffectsView)sender;
        //
        //     if (effectsView.IsSelected)
        //     {
        //         effectsView.ScaleFactor = 1.0;
        //         effectsView.TouchUpEffects = SfEffects.None;
        //         effectsView.TouchDownEffects = SfEffects.Scale;
        //         effectsView.ScaleAnimationDuration = 250;
        //     }
        //     else
        //     {
        //         effectsView.ScaleFactor = 0.85;
        //         effectsView.TouchDownEffects = SfEffects.Scale;
        //         effectsView.TouchUpEffects = SfEffects.None;
        //         effectsView.ScaleAnimationDuration = 250;
        //     }
        // }
    }

    private void EffectsViewQuit_OnAnimationCompleted(object? sender, EventArgs e)
    {
        if (sender != null)
        {
            var effectsView = (SfEffectsView)sender;

            // if (effectsView.IsSelected)
            // {
            //     effectsView.ScaleFactor = 1.0;
            //     effectsView.TouchUpEffects = SfEffects.None;
            //     effectsView.TouchDownEffects = SfEffects.Scale;
            //     effectsView.ScaleAnimationDuration = 250;
            // }
            // else
            // {
            //     effectsView.ScaleFactor = 0.85;
            //     effectsView.TouchDownEffects = SfEffects.Scale;
            //     effectsView.TouchUpEffects = SfEffects.None;
            //     effectsView.ScaleAnimationDuration = 250;
            // }
        }
    }
}
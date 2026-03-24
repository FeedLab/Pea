using Microsoft.Extensions.Logging;
using Pea.Meter.Services;
using Serilog;

namespace Pea.Meter.Components;

public partial class ServiceBoxComponent : Border
{
    private readonly ILogger<ServiceBoxComponent> logger;

    public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
        nameof(ImageSource),
        typeof(string),
        typeof(ServiceBoxComponent),
        default(string));

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(ServiceBoxComponent),
        default(string));

    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description),
        typeof(string),
        typeof(ServiceBoxComponent),
        default(string));

    public static readonly BindableProperty UrlProperty = BindableProperty.Create(
        nameof(Url),
        typeof(string),
        typeof(ServiceBoxComponent),
        default(string));

    public string ImageSource
    {
        get => (string)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string Url
    {
        get => (string)GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    public ServiceBoxComponent()
    {
        InitializeComponent();

        logger = AppService.GetRequiredService<ILogger<ServiceBoxComponent>>();
    }

    private async void OnTapped(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(Url))
            {
                logger.LogInformation($"Opening URL: {Url}");
                await Launcher.OpenAsync(new Uri(Url));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Unable to open URL: {ex.Message}");
        }

        //         if (string.IsNullOrWhiteSpace(url))
        //             return;
        //
        //         Uri uri = new Uri(url);
        //         await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        //     }
        //     catch (Exception ex)
        //     {
        //         System.Diagnostics.Debug.WriteLine($"Unable to open URL: {ex.Message}");
        //         //await Application.Current.MainPage.DisplayAlert("Error", $"Unable to open URL: {ex.Message}", "OK");
        //     }
        // }
    }
}
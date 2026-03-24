using System.Globalization;
using Microsoft.Extensions.Logging;
using Pea.Meter.Models;
using Pea.Meter.Services;
using Syncfusion.Maui.Buttons;

namespace Pea.Meter.Components.Configuration;

public partial class LanguageComponent : ContentView
{
    private readonly ILogger<LanguageComponent> logger;
    private LanguageItemComponent? oldLanguageItem;
    public ConfigurationLanguageModel LanguageConfiguration { get; }

    public LanguageComponent()
    {
        logger = AppService.GetRequiredService<ILogger<LanguageComponent>>();
        var storageService = AppService.GetRequiredService<StorageService>();

        LanguageConfiguration = storageService.ConfigurationLanguageModel;

        InitializeComponent();

        SetInitialRadioButton();
    }

    private bool isInitializing;

    private void SetInitialRadioButton()
    {
        try
        {
            isInitializing = true;
            foreach (var child in LanguageSelector.Children)
            {
                if (child is LanguageItemComponent item &&
                    item.LanguageValue == LanguageConfiguration.SelectedLanguage)
                {
                    item.IsChecked = true;
                    oldLanguageItem = item;
                    break;
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error setting initial language radio button");
        }
        finally
        {
            isInitializing = false;
        }
    }

    private void OnLanguageTapped(object? sender, StateChangedEventArgs e)
    {
        try
        {
            if (isInitializing)
            {
                return;
            }

            if (sender is not LanguageItemComponent languageItem)
            {
                return;
            }

            if (languageItem.IsChecked)
            {
                if (oldLanguageItem is not null && oldLanguageItem != languageItem)
                {
                    oldLanguageItem.IsChecked = false;
                }

                LanguageConfiguration.Save(
                    languageItem.LanguageValue,
                    languageItem.CultureCode,
                    languageItem.FlagSource);

                oldLanguageItem = languageItem;

                // Set culture globally
                var culture = new CultureInfo(languageItem.CultureCode);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                // Update Shell title
                if (Application.Current?.MainPage is Shell shell)
                {
                    shell.Title = Pea.Meter.Resources.Strings.AppResources.MainTitle;
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occurred while handling language selection");
        }
    }
}
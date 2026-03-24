using Pea.Meter.Models;
using Pea.Meter.Services;
using Syncfusion.Maui.Buttons;

namespace Pea.Meter.Components.Configuration;

public partial class LanguageComponent : ContentView
{
    private LanguageItemComponent? oldLanguageItem;
    public ConfigurationLanguageModel LanguageConfiguration { get; }

    public LanguageComponent()
    {
        var storageService = AppService.GetRequiredService<StorageService>();
        LanguageConfiguration = storageService.ConfigurationLanguageModel;

        InitializeComponent();

        SetInitialRadioButton();
    }

    private void SetInitialRadioButton()
    {
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

    private void OnLanguageTapped(object? sender, StateChangedEventArgs e)
    {
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

            oldLanguageItem = languageItem;
            LanguageConfiguration.SelectedLanguage = languageItem.LanguageValue;
            LanguageConfiguration.FlagSource = languageItem.FlagSource;
        }
    }
}

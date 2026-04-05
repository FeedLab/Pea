using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pea.Meter.Services;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Meter.Models;
using Syncfusion.Maui.TabView;

namespace Pea.Meter.Popup;

public enum SelectedTab
{
    Tariff,
    Language,
    DataImport
};

public partial class ConfigurationPopup : Popup<bool>
{
    private readonly ILogger<QuitPopup> logger;
    private readonly StorageService storageService;
    private readonly ConfigurationTariffModel savedConfigurationTariffModel;

    public ConfigurationPopup(SelectedTab selectedTab)
    {
        InitializeComponent();

        logger = AppService.GetRequiredService<ILogger<QuitPopup>>();
        storageService = AppService.GetRequiredService<StorageService>();

        savedConfigurationTariffModel = storageService.ConfigurationTariffModel.Copy();

        if (selectedTab == SelectedTab.Tariff)
        {
            TabView.SelectedIndex = TabView.Items.IndexOf(Tariff);
        }
        else if (selectedTab == SelectedTab.Language)
        {
            TabView.SelectedIndex = TabView.Items.IndexOf(Language);
        }
        else if (selectedTab == SelectedTab.DataImport)
        {
            TabView.SelectedIndex = TabView.Items.IndexOf(DataImport);
        }
        else
        {
            throw new Exception("Unknown tab");
        }

        Header = "Configuration";
        BindingContext = this;
    }

    public string Header { get; }

    private void OnClose(object? sender, EventArgs e)
    {
        try
        {
            if (storageService.ConfigurationTariffModel.HasChange(savedConfigurationTariffModel))
            {
                WeakReferenceMessenger.Default.Send(
                    new ConfigurationTariffMessage(storageService.ConfigurationTariffModel,
                        savedConfigurationTariffModel));
            }

            CloseAsync(true);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error closing popup (No)");
        }
    }

    private async void TabView_OnSelectionChanged(object? sender, TabSelectionChangedEventArgs e)
    {
        try
        {
            if (sender != null)
            {
                var tabView = sender as SfTabView;

                if (tabView == null)
                {
                    return;
                }

                var newIndex = (int)e.NewIndex;
                var oldIndex = (int)e.OldIndex;
                var selectedTabItem = tabView.Items[newIndex];
                var unSelectedTabItem = tabView.Items[oldIndex];

                var fromImage = GetTabImageFromAutomationId(unSelectedTabItem);
                var toImage = GetTabImageFromAutomationId(selectedTabItem);

                this.AbortAnimation("CrossFade");

                fromImage.Opacity = 0.85;
                toImage.Opacity = 0;

                var animation = new Animation(v =>
                {
                    fromImage.Opacity = 0.85 * (1 - v);
                    toImage.Opacity = 0.85 * v;
                }, 0, 1, Easing.CubicInOut);

                animation.Commit(this, "CrossFade", 16, 250);

                await Task.Delay(250);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error ");
        }
        finally
        {
        }
    }

    private Image GetTabImageFromAutomationId(SfTabItem tabItem)
    {
        var image = tabItem.AutomationId switch
        {
            "Tariff" => TariffImage,
            "Language" => LanguageImage,
            "DataImport" => DataImportImage,
            _ => throw new Exception("Unknown tab")
        };

        return image;
    }

    private void OnHeaderTapped(object? sender, TappedEventArgs e)
    {
    }
}
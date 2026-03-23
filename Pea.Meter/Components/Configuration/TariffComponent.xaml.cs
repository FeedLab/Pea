using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Pea.Meter.Services;

namespace Pea.Meter.Components.Configuration;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0019:Invalid containing type for [ObservableProperty] field or property")]
public partial class TariffComponent : ContentView
{
    public ConfigurationTariffModel TariffConfiguration { get; }

    public TariffComponent()
    {
        TariffConfiguration = ConfigurationTariffModel.Load();

        InitializeComponent();
    }
}

public partial class ConfigurationTariffModel : ObservableObject
{
    private static readonly string SettingsFilePath;

    static ConfigurationTariffModel()
    {
        var roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        SettingsFilePath = Path.Combine(roamingAppDataPath, "Pea.Meter", "TariffConfiguration.json");

        // Ensure directory exists
        var directory = Path.GetDirectoryName(SettingsFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    [ObservableProperty] private bool isTariffTypeTimeOfUse;
    [ObservableProperty] private decimal flatRatePrice;
    [ObservableProperty] private decimal peekPrice;
    [ObservableProperty] private decimal offPeekPrice;

    private readonly ILogger<ConfigurationTariffModel> logger;
    private static bool isLoadingConfiguration;

    public ConfigurationTariffModel()
    {
        logger = AppService.GetRequiredService<ILogger<ConfigurationTariffModel>>();

        IsTariffTypeTimeOfUse = false;
        FlatRatePrice = 3.89m;
        OffPeekPrice = 2.64m;
        PeekPrice = 5.13m;
    }

    partial void OnIsTariffTypeTimeOfUseChanged(bool value)
    {
        Save(this);
    }

    partial void OnFlatRatePriceChanged(decimal value)
    {
        Save(this);
    }

    partial void OnPeekPriceChanged(decimal value)
    {
        Save(this);
    }

    partial void OnOffPeekPriceChanged(decimal value)
    {
        Save(this);
    }

    private void Save(ConfigurationTariffModel configurationTariffModel)
    {
        try
        {
            if (isLoadingConfiguration)
            {
                return;
            }

            var data = new
            {
                IsTariffTypeTimeOfUse = configurationTariffModel.IsTariffTypeTimeOfUse,
                FlatRatePrice = configurationTariffModel.FlatRatePrice,
                PeekPrice = configurationTariffModel.PeekPrice,
                OffPeekPrice = configurationTariffModel.OffPeekPrice
            };
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            // Log or handle exception as needed
            logger.LogError(ex, $"Failed to save tariff configuration: {ex.Message}");
        }
    }

    public static ConfigurationTariffModel Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                isLoadingConfiguration = true;
                var json = File.ReadAllText(SettingsFilePath);
                var data = JsonSerializer.Deserialize<JsonElement>(json);

                var model = new ConfigurationTariffModel();
                if (data.TryGetProperty("IsTariffTypeTimeOfUse", out var isTariffTypeTimeOfUse))
                    model.IsTariffTypeTimeOfUse = isTariffTypeTimeOfUse.GetBoolean();
                if (data.TryGetProperty("FlatRatePrice", out var flatRatePrice))
                    model.FlatRatePrice = flatRatePrice.GetDecimal();
                if (data.TryGetProperty("PeekPrice", out var peekPrice))
                    model.PeekPrice = peekPrice.GetDecimal();
                if (data.TryGetProperty("OffPeekPrice", out var offPeekPrice))
                    model.OffPeekPrice = offPeekPrice.GetDecimal();

                return model;
            }
        }
        catch (Exception ex)
        {
            // Log or handle exception as needed
            System.Diagnostics.Debug.WriteLine($"Failed to load tariff configuration: {ex.Message}");
        }
        finally
        {
            isLoadingConfiguration = false;
        }

        return new ConfigurationTariffModel();
    }
}
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Pea.Meter.Services;

namespace Pea.Meter.Models;

public partial class ConfigurationDataImportModel : ObservableObject
{
    private static readonly string SettingsFilePath;

    static ConfigurationDataImportModel()
    {
        var roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        SettingsFilePath = Path.Combine(roamingAppDataPath, "Pea.Meter", "DataImportConfiguration.json");

        // Ensure directory exists
        var directory = Path.GetDirectoryName(SettingsFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    [ObservableProperty] private DateTime earliestImportedDate;

    private readonly ILogger<ConfigurationDataImportModel> logger;
    private static bool isLoadingConfiguration;

    public ConfigurationDataImportModel()
    {
        logger = AppService.GetRequiredService<ILogger<ConfigurationDataImportModel>>();

        EarliestImportedDate = DateTime.Today.Date.AddDays(-400);
    }

    partial void OnEarliestImportedDateChanged(DateTime value)
    {
        Save(this);
    }
    
    private void Save(ConfigurationDataImportModel dataImportModel)
    {
        try
        {
            if (isLoadingConfiguration)
            {
                return;
            }

            var data = new
            {
                EarliestImportedDate = dataImportModel.EarliestImportedDate
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

    public static ConfigurationDataImportModel Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                isLoadingConfiguration = true;
                var json = File.ReadAllText(SettingsFilePath);
                var data = JsonSerializer.Deserialize<JsonElement>(json);

                var model = new ConfigurationDataImportModel();
                
                if (data.TryGetProperty("EarliestImportedDate", out var earliestImportedDate))
                    model.EarliestImportedDate = earliestImportedDate.GetDateTime();

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

        return new ConfigurationDataImportModel();
    }
}
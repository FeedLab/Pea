using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Pea.Meter.Services;

namespace Pea.Meter.Models;

public partial class ConfigurationLanguageModel : ObservableObject
{
    private static readonly string SettingsFilePath;

    static ConfigurationLanguageModel()
    {
        var roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        SettingsFilePath = Path.Combine(roamingAppDataPath, "Pea.Meter", "LanguageConfiguration.json");

        // Ensure directory exists
        var directory = Path.GetDirectoryName(SettingsFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    [ObservableProperty] private string? selectedLanguage;

    private readonly ILogger<ConfigurationLanguageModel> logger = AppService.GetRequiredService<ILogger<ConfigurationLanguageModel>>();
    private static bool isLoadingConfiguration;

    partial void OnSelectedLanguageChanged(string? value)
    {
        Save(this);
    }


    private void Save(ConfigurationLanguageModel model)
    {
        try
        {
            if (isLoadingConfiguration)
            {
                return;
            }

            var data = new
            {
                SelectedLanguage = model.SelectedLanguage
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

    public static ConfigurationLanguageModel Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                isLoadingConfiguration = true;
                var json = File.ReadAllText(SettingsFilePath);
                var data = JsonSerializer.Deserialize<JsonElement>(json);

                var model = new ConfigurationLanguageModel();

                if (data.TryGetProperty("SelectedLanguage", out var selectedLanguage))
                {
                    model.SelectedLanguage = selectedLanguage.GetString() ?? "English";
                }
                else
                {
                    model.SelectedLanguage = "English";
                }
                return model;
            }
        }
        catch (Exception ex)
        {
            // Log or handle exception as needed
            System.Diagnostics.Debug.WriteLine($"Failed to load language configuration: {ex.Message}");
        }
        finally
        {
            isLoadingConfiguration = false;
        }

        return new ConfigurationLanguageModel();
    }
}
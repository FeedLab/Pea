using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Akavache;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Pea.Meter.Services;

namespace Pea.Meter.Models;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class ConfigurationLanguageModel : ObservableObject
{
    public const string DefaultLanguage = "English";
    public const string DefaultLanguagePng = "gb.png";
    public const string DefaultCultureCode = "gb";

    [ObservableProperty] private string selectedLanguage = DefaultLanguage;
    [ObservableProperty] private string flagSource = DefaultLanguagePng;
    [ObservableProperty] private string cultureCode = DefaultCultureCode;

    private readonly ILogger<ConfigurationLanguageModel>? logger;
    private bool isLoadingConfiguration;

    private sealed record LanguageDto(string SelectedLanguage, string FlagSource, string CultureCode);

    public ConfigurationLanguageModel()
    {
        logger = AppService.GetRequiredService<ILogger<ConfigurationLanguageModel>>();
    }

    partial void OnSelectedLanguageChanged(string? value) => Save();

    public void Save(string languageSelected, string codeOfCulture, string sourceFlag)
    {
        isLoadingConfiguration = true;
        try
        {
            SelectedLanguage = languageSelected;
            CultureCode = codeOfCulture;
            FlagSource = sourceFlag;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to save language configuration: {Message}", ex.Message);
        }
        finally
        {
            isLoadingConfiguration = false;
        }
        Save();
    }

    private async void Save()
    {
        try
        {
            if (isLoadingConfiguration) return;

            await CacheDatabase.UserAccount.InsertObject("LanguageData",
                new LanguageDto(SelectedLanguage, FlagSource, CultureCode));
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to save language configuration: {Message}", ex.Message);
        }
    }

    public async Task Load()
    {
        try
        {
            isLoadingConfiguration = true;
            var dto = await CacheDatabase.UserAccount.GetObject<LanguageDto>("LanguageData");
            SelectedLanguage = dto.SelectedLanguage;
            FlagSource = dto.FlagSource;
            CultureCode = dto.CultureCode;
        }
        catch (KeyNotFoundException)
        {
            await CacheDatabase.UserAccount.InsertObject("LanguageData",
                new LanguageDto(SelectedLanguage, FlagSource, CultureCode));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load language configuration: {ex.Message}");
        }
        finally
        {
            isLoadingConfiguration = false;
        }
    }
}
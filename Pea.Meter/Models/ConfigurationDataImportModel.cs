using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Akavache;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Pea.Meter.Services;

namespace Pea.Meter.Models;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class ConfigurationDataImportModel : ObservableObject
{
    [ObservableProperty] private DateTime earliestImportedDate;

    private readonly ILogger<ConfigurationDataImportModel>? logger;
    private bool isLoadingConfiguration;

    private sealed record DataImportDto(DateTime EarliestImportedDate);

    public ConfigurationDataImportModel()
    {
        logger = AppService.GetRequiredService<ILogger<ConfigurationDataImportModel>>();
        isLoadingConfiguration = true;
        try
        {
            EarliestImportedDate = DateTime.Today.Date.AddDays(-400);
        }
        finally
        {
            isLoadingConfiguration = false;
        }
    }

    partial void OnEarliestImportedDateChanged(DateTime value) => Save();

    private async void Save()
    {
        try
        {
            if (isLoadingConfiguration) return;

            await CacheDatabase.UserAccount.InsertObject("DataImportData",
                new DataImportDto(EarliestImportedDate));
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to save data import configuration: {Message}", ex.Message);
        }
    }

    public async Task Load()
    {
        try
        {
            isLoadingConfiguration = true;
            var dto = await CacheDatabase.UserAccount.GetObject<DataImportDto>("DataImportData");
            EarliestImportedDate = dto.EarliestImportedDate;
        }
        catch (KeyNotFoundException)
        {
            await CacheDatabase.UserAccount.InsertObject("DataImportData",
                new DataImportDto(EarliestImportedDate));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load data import configuration: {ex.Message}");
        }
        finally
        {
            isLoadingConfiguration = false;
        }
    }
}
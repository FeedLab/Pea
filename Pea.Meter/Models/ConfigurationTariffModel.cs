using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Akavache;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Pea.Meter.Services;

namespace Pea.Meter.Models;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class ConfigurationTariffModel : ObservableObject
{
    private const string StoreKey = "TariffData";

    public bool HasChange(ConfigurationTariffModel oldModel)
    {
        return !(oldModel.IsTariffTypeTimeOfUse == IsTariffTypeTimeOfUse &&
                 oldModel.FlatRatePrice == FlatRatePrice &&
                 oldModel.PeekPrice == PeekPrice &&
                 oldModel.OffPeekPrice == OffPeekPrice &&
                 oldModel.InvoiceDayInMonth == InvoiceDayInMonth);
    }

    public ConfigurationTariffModel Copy()
    {
        var copy = new ConfigurationTariffModel();
        copy.isLoadingConfiguration = true;
        try
        {
            copy.IsTariffTypeTimeOfUse = IsTariffTypeTimeOfUse;
            copy.FlatRatePrice = FlatRatePrice;
            copy.PeekPrice = PeekPrice;
            copy.OffPeekPrice = OffPeekPrice;
            copy.InvoiceDayInMonth = InvoiceDayInMonth;
        }
        finally
        {
            copy.isLoadingConfiguration = false;
        }

        return copy;
    }

    [ObservableProperty] private bool isTariffTypeTimeOfUse;
    [ObservableProperty] private decimal flatRatePrice;
    [ObservableProperty] private decimal peekPrice;
    [ObservableProperty] private decimal offPeekPrice;
    [ObservableProperty] private int invoiceDayInMonth;

    private readonly ILogger<ConfigurationTariffModel> logger;
    private bool isLoadingConfiguration;

    private sealed record TariffDto(
        bool IsTariffTypeTimeOfUse,
        decimal FlatRatePrice,
        decimal PeekPrice,
        decimal OffPeekPrice,
        int InvoiceDayInMonth);

    public ConfigurationTariffModel()
    {
        logger = AppService.GetRequiredService<ILogger<ConfigurationTariffModel>>();

        Reset();
    }

    partial void OnIsTariffTypeTimeOfUseChanged(bool value) => Save();
    partial void OnFlatRatePriceChanged(decimal value) => Save();
    partial void OnPeekPriceChanged(decimal value) => Save();
    partial void OnOffPeekPriceChanged(decimal value) => Save();
    partial void OnInvoiceDayInMonthChanged(int value) => Save();

    public void Reset()
    {
        try
        {
            isLoadingConfiguration = true;

            InvoiceDayInMonth = 26;
            IsTariffTypeTimeOfUse = false;
            FlatRatePrice = 3.89m;
            OffPeekPrice = 2.64m;
            PeekPrice = 5.13m;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error initializing tariff configuration: {Message}", e.Message);
        }
        finally
        {
            isLoadingConfiguration = false;
        }
    }

    private async void Save()
    {
        try
        {
            if (isLoadingConfiguration) return;

            await CacheDatabase.UserAccount.InsertObject(StoreKey, new TariffDto(
                IsTariffTypeTimeOfUse, FlatRatePrice, PeekPrice, OffPeekPrice, InvoiceDayInMonth));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save tariff configuration: {Message}", ex.Message);
        }
    }

    public async Task Load()
    {
        try
        {
            isLoadingConfiguration = true;
            var dto = await CacheDatabase.UserAccount.GetObject<TariffDto>(StoreKey);

            if (dto is null)
            {
                logger.LogError("Failed to load tariff configuration: DTO is null. This can not happen.");
                return;
            }

            IsTariffTypeTimeOfUse = dto.IsTariffTypeTimeOfUse;
            FlatRatePrice = dto.FlatRatePrice;
            PeekPrice = dto.PeekPrice;
            OffPeekPrice = dto.OffPeekPrice;
            InvoiceDayInMonth = dto.InvoiceDayInMonth;
        }
        catch (KeyNotFoundException)
        {
            await CacheDatabase.UserAccount.InsertObject(StoreKey, new TariffDto(
                IsTariffTypeTimeOfUse, FlatRatePrice, PeekPrice, OffPeekPrice, InvoiceDayInMonth));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load tariff configuration: {ex.Message}");
        }
        finally
        {
            isLoadingConfiguration = false;
        }
    }
}
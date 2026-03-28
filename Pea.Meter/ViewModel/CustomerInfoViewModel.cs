using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class CustomerInfoViewModel : ObservableObject
{
    private readonly ILogger<CustomerInfoViewModel> logger;
    private readonly PeaAdapter peaAdapter;
    private readonly StorageService storageService;

    [ObservableProperty] private bool isCustomerProfileVisible = false;
    [ObservableProperty] private string customerName = "";
    [ObservableProperty] private string customerId = "";
    [ObservableProperty] private string meterNumber = "";
    [ObservableProperty] private DateTime? periodStart = null;

    public CustomerInfoViewModel(ILogger<CustomerInfoViewModel> logger, PeaAdapter peaAdapter, StorageService storageService)
    {
        this.logger = logger;
        this.peaAdapter = peaAdapter;
        this.storageService = storageService;

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, (r, m) =>
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                IsCustomerProfileVisible = true;

                if (string.IsNullOrEmpty(peaAdapter.CustomerName) ||
                    string.IsNullOrEmpty(peaAdapter.CustomerId) ||
                    string.IsNullOrEmpty(peaAdapter.MeterNumber))
                {
                    logger.LogError("Customer profile not loaded yet");
                    return;
                }

                CustomerName = peaAdapter.CustomerName;
                CustomerId = peaAdapter.CustomerId;
                MeterNumber = peaAdapter.MeterNumber;
            });
        });

        WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this, (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () => { IsCustomerProfileVisible = false; });
            });

        WeakReferenceMessenger.Default.Register<AllAggregationsCompletedMessage>(this, async void (r, m) =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    if (storageService.DailyAggregated.Any())
                    {
                        var meterReading = storageService.DailyAggregated.First();
                        PeriodStart = meterReading.PeriodStart;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError("Error processing AllAggregationsCompletedMessage: {0}", e.Message);
                }
            });
        });
        
        WeakReferenceMessenger.Default.Register<DataImportedEarlierMessage>(this, async void (r, m) =>
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (storageService.DailyAggregated.Any())
                    {
                        var meterReading = storageService.DailyAggregated.First();
                        PeriodStart = meterReading.PeriodStart;
                    }
                    else
                    {
                        logger.LogError("No data available");
                    }

                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing DataImportedEarlierMessage: {0}", e.Message);
            }
        });
    }
}
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Infrastructure.Models;
using Pea.Meter.Extension;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

public partial class SolarSystemSizingViewModel : ObservableObject
{
    private readonly ILogger<SolarSystemSizingViewModel> logger;
    private readonly StorageService storageService;

    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataMonthSummary = [];

    public SolarSystemSizingViewModel(ILogger<SolarSystemSizingViewModel> logger, StorageService storageService)
    {
        this.logger = logger;
        this.storageService = storageService;

        CreateLoggedInSubscription();
        CreateNewDaySubscription();
        CreateDataImportedSubscription();
    }

    private void CreateDataImportedSubscription()
    {
        WeakReferenceMessenger.Default.Register<DataImportedMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await PopulateChartData();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateNewDaySubscription), e.Message);
                    }

                    return Task.CompletedTask;
                });
            });
    }

    private void CreateLoggedInSubscription()
    {
        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await PopulateChartData();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateNewDaySubscription), e.Message);
                    }

                    return Task.CompletedTask;
                });
            });
    }
    
    private void CreateNewDaySubscription()
    {
        WeakReferenceMessenger.Default.Register<DateChangedMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await PopulateChartData();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateNewDaySubscription), e.Message);
                    }

                    return Task.CompletedTask;
                });
            });
    }

    private async Task PopulateChartData()
    {
        var monthlySummaryList = storageService.MonthlyAggregated.TakeLast(15).ToList();

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            MeterDataMonthSummary.Clear();

            MeterDataMonthSummary.AddRange(monthlySummaryList);
        });
    }
}
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Infrastructure.Models;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel.Statistics;

public partial class MeterReadingsDailyViewModel : ObservableObject
{
    private readonly ILogger<MeterReadingsDailyViewModel> logger;
    private readonly StorageService storageService;

    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataDailyAggregated = [];
    [ObservableProperty] private decimal maximumZoomLevel = 1.0m;
    [ObservableProperty] private decimal zoomFactor = 1.0m;
    
    public MeterReadingsDailyViewModel(ILogger<MeterReadingsDailyViewModel> logger, StorageService storageService)
    {
        this.logger = logger;
        this.storageService = storageService;

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async (r, m) =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                DisplayChart();
            });
        });
        
        WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this, (r, m) =>
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                MeterDataDailyAggregated = [];
                
                return Task.CompletedTask;
            });
        });
        
        WeakReferenceMessenger.Default.Register<AllAggregationsCompletedMessage>(this, async void (r, m) =>
        {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        DisplayChart();
                    }
                    catch (Exception exception)
                    {
                        return Task.FromException<Task>(exception);
                    }

                    return Task.CompletedTask;
                });
        });
        
        WeakReferenceMessenger.Default.Register<DateChangedMessage>(this, async (r, m) =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                DisplayChart();
            });
        });

        WeakReferenceMessenger.Default.Register<DataImportedMessage>(this, async (r, m) =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                DisplayChart();
            });
        });
    }

    private void DisplayChart()
    {
        try
        {
            if( storageService.DailyAggregated.Count > 0)
            {
                MaximumZoomLevel = ( storageService.DailyAggregated.Count / 40.0m);
                ZoomFactor = 1.0m / MaximumZoomLevel;
                MeterDataDailyAggregated = storageService.DailyAggregated;
            }
            else
            {
                MeterDataDailyAggregated = [];
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in {Method}: {Message}", nameof(DisplayChart), e.Message);
        }
    }
}
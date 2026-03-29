using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Infrastructure.Models;
using Pea.Meter.Models;
using Pea.Meter.Services;
using System.Collections.ObjectModel;

namespace Pea.Meter.ViewModel.Statistics;

public partial class MeterReadingsDailyViewModel : ObservableObject
{
    private readonly ILogger<MeterReadingsDailyViewModel> logger;
    private readonly StorageService storageService;
    private TimeResolutionType timeResolution;

    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAggregated;
    [ObservableProperty] private decimal maximumZoomLevel = 1.0m;
    [ObservableProperty] private decimal zoomFactor = 1.0m;
    [ObservableProperty] private decimal zoomPosition = 1.0m;
    [ObservableProperty] private string dateFormat = "";
    

    public MeterReadingsDailyViewModel(ILogger<MeterReadingsDailyViewModel> logger, StorageService storageService)
    {
        this.logger = logger;
        this.storageService = storageService;
        
        MeterDataAggregated = new ObservableCollection<PeaMeterReading>();

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
                MeterDataAggregated = [];
                
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
                switch (timeResolution)
                {
                    case TimeResolutionType.Daily:
                        MeterDataAggregated = storageService.DailyAggregated;
                        MaximumZoomLevel = ( storageService.DailyAggregated.Count / 40.0m);
                        ZoomFactor = 1.0m / MaximumZoomLevel;                    
                        ZoomPosition = 1.0m;
                        break;
                    case TimeResolutionType.Monthly:
                        MeterDataAggregated = storageService.MonthlyAggregated;
                        MaximumZoomLevel = ( storageService.MonthlyAggregated.Count / 18.0m);
                        ZoomFactor = 1.0m / MaximumZoomLevel;
                        ZoomPosition = 1.0m;
                    break;
                    default:
                        throw new Exception("Unknown time resolution");
                }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in {Method}: {Message}", nameof(DisplayChart), e.Message);
        }
    }

    public async void TimeResolutionChanged(TimeResolutionType selectedTimeResolution)
    {
        try
        {
            timeResolution = selectedTimeResolution;
            await MainThread.InvokeOnMainThreadAsync(DisplayChart);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in {Method}: {Message}", nameof(TimeResolutionChanged), e.Message);
        }
    }

    public enum TimeResolutionType
    {
        Daily,
        Monthly
    };
    
    public enum ChartVisualPositionType
    {
        Start,
        End
    };

    public void MoveTo(ChartVisualPositionType position)  
    {
        if (position == ChartVisualPositionType.Start)
        {
            ZoomPosition = 0.0m;
        }
        else if (position == ChartVisualPositionType.End)
        {
            ZoomPosition = 1.0m;
        }
        else
        {
            throw new Exception("Unknown position");
        }
    }
}
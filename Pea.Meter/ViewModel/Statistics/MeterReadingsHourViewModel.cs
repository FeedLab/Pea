using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Infrastructure.Models;
using Pea.Meter.Extension;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel.Statistics;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class MeterReadingsHourViewModel : ObservableObject
{
    private readonly ILogger<MeterReadingsHourViewModel> logger;
    private readonly PeaAdapter peaAdapter;
    private readonly StorageService storageService;

    [ObservableProperty] private ObservableCollection<PeaMeterReading> todayData = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage1 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage7 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage30 = [];
    [ObservableProperty] private DateTime dateMeterData = DateTime.Today;

    public MeterReadingsHourViewModel(ILogger<MeterReadingsHourViewModel> logger, PeaAdapter peaAdapter,
        StorageService storageService)
    {
        this.logger = logger;
        this.peaAdapter = peaAdapter;
        this.storageService = storageService;

        CreateLoggedInSubscription();
        CreateLoggedOutSubscription();
        CreateAllAggregationsCompletedSubscription();
        CreateNewDaySubscription();
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
                        DateMeterData = m.NewDate;
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

    private void CreateLoggedOutSubscription()
    {
        WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this, (r, m) =>
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                storageService.DailyPeriodReadings.CollectionChanged -= DailyPeriodPeaMeterDataCollectionChanged;

                TodayData = [];
                MeterDataAverage1 = [];
                MeterDataAverage7 = [];
                MeterDataAverage30 = [];
                
                return Task.CompletedTask;
            });
        });
    }

    private void CreateLoggedInSubscription()
    {
        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async (r, m) =>
        {
            storageService.DailyPeriodReadings.CollectionChanged += DailyPeriodPeaMeterDataCollectionChanged;
        });
    }

    private void CreateAllAggregationsCompletedSubscription()
    {
        WeakReferenceMessenger.Default.Register<AllAggregationsCompletedMessage>(this, async void (r, m) =>
        {
            try
            {
                await PopulateChartData();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateAllAggregationsCompletedSubscription),
                    e.Message);
            }
        });
    }

    private async Task PopulateChartData()
    {
        var today = DateTime.Today;
        var timeStart1 = today.AddDays(-1);
        var timeStart7 = today.AddDays(-7);
        var timeStart30 = today.AddDays(-30);

        var meterDataAverageDays0 = storageService.DailyPeriodReadings.SummaryByHour();
        var meterDataAverageDays1 = storageService.HourlyAggregated.FilterByPeriod(timeStart1, today)
            .AverageByHour();
        var meterDataAverageDays7 = storageService.HourlyAggregated.FilterByPeriod(timeStart7, today)
            .AverageByHour();
        var meterDataAverageDays30 = storageService.HourlyAggregated.FilterByPeriod(timeStart30, today)
            .AverageByHour();

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            TodayData.Clear();
            MeterDataAverage1.Clear();
            MeterDataAverage7.Clear();
            MeterDataAverage30.Clear();

            TodayData.AddRange(meterDataAverageDays0);
            MeterDataAverage1.AddRange(meterDataAverageDays1);
            MeterDataAverage7.AddRange(meterDataAverageDays7);
            MeterDataAverage30.AddRange(meterDataAverageDays30);
        });
    }

    private void DailyPeriodPeaMeterDataCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                TodayData = storageService.DailyPeriodReadings.SummaryByHour();
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems?.Cast<PeaMeterReading>().ToList() ?? [])
                {
                    TodayData.Remove(item);
                }

                break;
                // case NotifyCollectionChangedAction.Replace:
                //     for (var i = 0; i < e.OldItems.Count; i++)
                //     {
                //         var index = MeterData.IndexOf(e.OldItems[i]);
                //         if (index >= 0)
                //         {
                //             MeterData[index] = e.NewItems[i];
                //         }
                //     }
                break;
            case NotifyCollectionChangedAction.Move:
                // For ObservableCollection, typically no action needed as order may not matter
                // or rebuild collection if order is important
                break;
            case NotifyCollectionChangedAction.Reset:
                TodayData.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
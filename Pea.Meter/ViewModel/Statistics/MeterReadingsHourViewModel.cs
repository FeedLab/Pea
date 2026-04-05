using System.Collections.ObjectModel;
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
    private readonly StorageService storageService;
    [ObservableProperty] private DateTime selectedDate;
    [ObservableProperty] private bool isNextDayButtonEnabled;
    [ObservableProperty] private bool isPreviousDayButtonEnabled;
    [ObservableProperty] private DateTime currentTimePickerMaximumDate;
    [ObservableProperty] private DateTime currentTimePickerMinimumDate;

    [ObservableProperty] private ObservableCollection<PeaMeterReading> todayData = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage1 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage7 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage30 = [];

    public MeterReadingsHourViewModel(ILogger<MeterReadingsHourViewModel> logger, PeaAdapter peaAdapter,
        StorageService storageService)
    {
        this.logger = logger;
        this.storageService = storageService;

        CreateLoggedInSubscription();
        CreateLoggedOutSubscription();
        CreateAllAggregationsCompletedSubscription();
        CreateNewDaySubscription();
        CreateAllImportedDataCompletedMessageSubscription();
        CreateDataImportedSubscription();
    }

    private void CreateAllImportedDataCompletedMessageSubscription()
    {
        WeakReferenceMessenger.Default.Register<AllImportedDataCompletedMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await PopulateChartData(DateTime.Today);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateAllImportedDataCompletedMessageSubscription),
                            e.Message);
                    }

                    return Task.CompletedTask;
                });
            });
        
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
                        await PopulateChartData(DateTime.Today);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateDataImportedSubscription),
                            e.Message);
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
                        SelectedDate = m.NewDate;
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
                try
                {
                    TodayData = [];
                    MeterDataAverage1 = [];
                    MeterDataAverage7 = [];
                    MeterDataAverage30 = [];
                
                    return Task.FromResult(Task.CompletedTask);
                }
                catch (Exception exception)
                {
                    return Task.FromException<Task>(exception);
                }
            });
        });
    }

    private void CreateLoggedInSubscription()
    {
        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this,
            async void (r, m) =>
            {
                try
                {
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateLoggedInSubscription), e.Message);
                }
            });
    }

    private void CreateAllAggregationsCompletedSubscription()
    {
        WeakReferenceMessenger.Default.Register<AllAggregationsCompletedMessage>(this, async void (r, m) =>
        {
            try
            {
                SelectedDate = DateTime.Today;
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
        await PopulateChartData(DateTime.Today);
    }

    private async Task PopulateChartData(DateTime date)
    {
        var today = date;
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

            if (date == DateTime.Today)
            {
                TodayData.AddRange(meterDataAverageDays0);
            }

            MeterDataAverage1.AddRange(meterDataAverageDays1);
            MeterDataAverage7.AddRange(meterDataAverageDays7);
            MeterDataAverage30.AddRange(meterDataAverageDays30);
        });
    }

    async partial void OnSelectedDateChanged(DateTime value)
    {
        try
        {
            if(storageService.HourlyAggregated.Count == 0)
            {
                logger.LogWarning("HourlyAggregated is empty");
                return;
            }

            var orderedPeaMeterReadingList = storageService.HourlyAggregated
                .OrderBy(o => o.PeriodStart)
                .ToList();

            var firstRecord = orderedPeaMeterReadingList.First();
            var lastRecord = orderedPeaMeterReadingList.Last();

            var firstDate = firstRecord.PeriodStart.Date;
            var lastDate = DateTime.Today.Date;
            // var lastDate = lastRecord.PeriodStart.Date;

            if (value >= lastDate.Date)
            {
                IsNextDayButtonEnabled = false;
                IsPreviousDayButtonEnabled = true;
            }
            else if (value <= firstDate.Date)
            {
                IsNextDayButtonEnabled = true;
                IsPreviousDayButtonEnabled = false;
            }
            else
            {
                IsNextDayButtonEnabled = true;
                IsPreviousDayButtonEnabled = true;
            }

            CurrentTimePickerMinimumDate = firstRecord.PeriodStart.Date;
            CurrentTimePickerMaximumDate = lastDate;

            await PopulateChartData(value);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in {Method}: {Message}", nameof(OnSelectedDateChanged), e.Message);
        }
    }
}
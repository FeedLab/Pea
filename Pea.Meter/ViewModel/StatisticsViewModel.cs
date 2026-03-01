using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly PeaAdapter peaAdapter;
    private readonly StorageService storageService;

    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataToday = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage1 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage7 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage30 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataDailyAggregated = [];
    [ObservableProperty] private DateTime dateMeterData = DateTime.Today;

    public StatisticsViewModel(PeaAdapter peaAdapter, StorageService storageService)
    {
        this.peaAdapter = peaAdapter;
        this.storageService = storageService;
        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async (r, m) =>
        {
            var dailyReadings = await peaAdapter.ShowDailyReadings(DateTime.Today);

            var today = DateTime.Today;
            var timeStart1 = today.AddDays(-1);
            var timeStart7 = today.AddDays(-7);
            var timeStart30 = today.AddDays(-30);

            var meterDataAverageDays1 = storageService.FetchAverageHourlyReadingsForPeriodAsync(timeStart1, today);
            var meterDataAverageDays7 = storageService.FetchAverageHourlyReadingsForPeriodAsync(timeStart7, today);
            var meterDataAverageDays30 = storageService.FetchAverageHourlyReadingsForPeriodAsync(timeStart30, today);
            var meterDataAggregatedPerDayAll = storageService.FetchDailyAggregatedForPeriodAsync(DateTime.MinValue, DateTime.MaxValue);

            var hourlyTotals = dailyReadings
                .GroupBy(meterReading => new DateTime(meterReading.PeriodStart.Year, meterReading.PeriodStart.Month,
                    meterReading.PeriodStart.Day, meterReading.PeriodStart.Hour, 0, 0))
                .Select(g => new PeaMeterReading(
                    g.Key,
                    g.Sum(reading => reading.RateA),
                    g.Sum(reading => reading.RateB),
                    g.Sum(reading => reading.RateC),
                    60 * 24
                ))
                .OrderBy(peaMeterReading => peaMeterReading.PeriodStart)
                .ToList();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MeterDataToday = new ObservableCollection<PeaMeterReading>(hourlyTotals);
                MeterDataAverage1 = new ObservableCollection<PeaMeterReading>(meterDataAverageDays1);
                MeterDataAverage7 = new ObservableCollection<PeaMeterReading>(meterDataAverageDays7);
                MeterDataAverage30 = new ObservableCollection<PeaMeterReading>(meterDataAverageDays30);
                MeterDataAverage30 = new ObservableCollection<PeaMeterReading>(meterDataAverageDays30);
                MeterDataDailyAggregated = new ObservableCollection<PeaMeterReading>(meterDataAggregatedPerDayAll);
                
                return Task.CompletedTask;
            });
        });
    }
}
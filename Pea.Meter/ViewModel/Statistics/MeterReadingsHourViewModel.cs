using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ObservableCollections;
using Pea.Infrastructure.Models;
using Pea.Meter.Extension;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel.Statistics;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class MeterReadingsHourViewModel : ObservableObject
{
    private readonly PeaAdapter peaAdapter;
    private readonly StorageService storageService;

    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataToday = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage1 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage7 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage30 = [];
    [ObservableProperty] private DateTime dateMeterData = DateTime.Today;

    public MeterReadingsHourViewModel(PeaAdapter peaAdapter, StorageService storageService)
    {
        this.peaAdapter = peaAdapter;
        this.storageService = storageService;

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async (r, m) => { await LoadDataAsync(); });
    }

    private async Task LoadDataAsync()
    {
        var dailyReadings = storageService.DailyReadings;

        var today = DateTime.Today;
        var timeStart1 = today.AddDays(-1);
        var timeStart7 = today.AddDays(-7);
        var timeStart30 = today.AddDays(-30);

        var meterDataAverageDays1 = storageService.HourlyAggregated.AverageByHour().FilterByPeriod(timeStart1, today);
        var meterDataAverageDays7 = storageService.HourlyAggregated.AverageByHour().FilterByPeriod(timeStart7, today);
        var meterDataAverageDays30 = storageService.HourlyAggregated.AverageByHour().FilterByPeriod(timeStart30, today);

        var hourlyTotals = dailyReadings
            .GroupBy(meterReading => new DateTime(meterReading.PeriodStart.Year, meterReading.PeriodStart.Month,
                meterReading.PeriodStart.Day, meterReading.PeriodStart.Hour, 0, 0))
            .Select(g => new PeaMeterReading(
                g.Key,
                g.Sum(reading => reading.RateA),
                g.Sum(reading => reading.RateB),
                g.Sum(reading => reading.RateC),
                60
            ))
            .OrderBy(peaMeterReading => peaMeterReading.PeriodStart)
            .ToList();

        await MainThread.InvokeOnMainThreadAsync((Func<Task>)(() =>
        {
            MeterDataToday = new ObservableCollection<PeaMeterReading>(hourlyTotals);
            MeterDataAverage1 = new ObservableCollection<PeaMeterReading>(meterDataAverageDays1);
            MeterDataAverage7 = new ObservableCollection<PeaMeterReading>(meterDataAverageDays7);
            MeterDataAverage30 = new ObservableCollection<PeaMeterReading>(meterDataAverageDays30);

            return Task.CompletedTask;
        }));
    }
}
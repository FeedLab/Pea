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
    
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataToday = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage1 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage7 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage30 = [];
    [ObservableProperty] private DateTime dateMeterData = DateTime.Today;

    public StatisticsViewModel(PeaAdapter peaAdapter, PeaDbContextFactory dbContextFactory)
    {
        this.peaAdapter = peaAdapter;
        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async (r, m) =>
            {

                    var dailyReadingsTask = peaAdapter.ShowDailyReadings(DateTime.Today);

                    var today = DateTime.Today;
                    var timeStart1 = today.AddDays(-1);
                    var timeStart7 = today.AddDays(-7);
                    var timeStart30 = today.AddDays(-30);

                    var meterDataAverageDays1Task = FetchDailyAverageReadingsAsync(dbContextFactory, timeStart1, today, m.AuthData.Username);
                    var meterDataAverageDays7Task = FetchDailyAverageReadingsAsync(dbContextFactory, timeStart7, today, m.AuthData.Username);
                    var meterDataAverageDays30Task = FetchDailyAverageReadingsAsync(dbContextFactory, timeStart30, today, m.AuthData.Username);

                    await Task.WhenAll(dailyReadingsTask, meterDataAverageDays1Task, meterDataAverageDays7Task, meterDataAverageDays30Task);

                    var meterDataAverageDays1 = meterDataAverageDays1Task.Result;
                    var meterDataAverageDays7 = meterDataAverageDays7Task.Result;
                    var meterDataAverageDays30 = meterDataAverageDays30Task.Result;
                    var dailyReadings = dailyReadingsTask.Result;
                    
                    var hourlyTotals = dailyReadings
                        .GroupBy(meterReading => new DateTime(meterReading.PeriodStart.Year, meterReading.PeriodStart.Month, meterReading.PeriodStart.Day, meterReading.PeriodStart.Hour, 0, 0))
                        .Select(g => new PeaMeterReading(
                            g.Key,
                            g.Sum(reading => reading.RateA),
                            g.Sum(reading => reading.RateB),
                            g.Sum(reading => reading.RateC),
                            60
                        ))
                        .OrderBy(peaMeterReading => peaMeterReading.PeriodStart)
                        .ToList();
                    
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {       
                        MeterDataToday = new ObservableCollection<PeaMeterReading>(hourlyTotals);
                        MeterDataAverage1 = new ObservableCollection<PeaMeterReading>(meterDataAverageDays1);
                        MeterDataAverage7 = new ObservableCollection<PeaMeterReading>(meterDataAverageDays7);
                        MeterDataAverage30 = new ObservableCollection<PeaMeterReading>(meterDataAverageDays30);
                        return Task.CompletedTask;
                    });
            });
    }
    
    private static Task<IList<PeaMeterReading>> FetchDailyAverageReadingsAsync(PeaDbContextFactory dbContextFactory, DateTime timeStart, DateTime timeEnd, string userName)
    {
        var meterDataAverageDaysTask = Task.Run(async () =>
        {
            using var dbContext = dbContextFactory.CreateDbContext(userName);
            var repo = new MeterReadingRepository(dbContext);
            return await repo.GetHourlyAveragesDuringPeriodAsync(timeStart, timeEnd, userName);
        });
        
        return meterDataAverageDaysTask;
    }
}
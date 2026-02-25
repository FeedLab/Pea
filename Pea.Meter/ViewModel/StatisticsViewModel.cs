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

                    // Create separate DbContext instances for each concurrent query to avoid threading issues
                    var meterDataAverageDays1Task = Task.Run(async () =>
                    {
                        using var dbContext = dbContextFactory.CreateDbContext(m.AuthData.Username);
                        var repo = new MeterReadingRepository(dbContext);
                        return await repo.GetHourlyAveragesDuringPeriodAsync(timeStart1, today, m.AuthData.Username);
                    });

                    var meterDataAverageDays7Task = Task.Run(async () =>
                    {
                        using var dbContext = dbContextFactory.CreateDbContext(m.AuthData.Username);
                        var repo = new MeterReadingRepository(dbContext);
                        return await repo.GetHourlyAveragesDuringPeriodAsync(timeStart7, today, m.AuthData.Username);
                    });

                    var meterDataAverageDays30Task = Task.Run(async () =>
                    {
                        using var dbContext = dbContextFactory.CreateDbContext(m.AuthData.Username);
                        var repo = new MeterReadingRepository(dbContext);
                        return await repo.GetHourlyAveragesDuringPeriodAsync(timeStart30, today, m.AuthData.Username);
                    });

                    await Task.WhenAll(dailyReadingsTask, meterDataAverageDays1Task, meterDataAverageDays7Task, meterDataAverageDays30Task);

                    var meterDataAverageDays1 = meterDataAverageDays1Task.Result;
                    var meterDataAverageDays7 = meterDataAverageDays7Task.Result;
                    var meterDataAverageDays30 = meterDataAverageDays30Task.Result;
                    var dailyReadings = dailyReadingsTask.Result;
                    
                    var hourlyTotals = dailyReadings
                        .GroupBy(r => new DateTime(r.PeriodStart.Year, r.PeriodStart.Month, r.PeriodStart.Day, r.PeriodStart.Hour, 0, 0))
                        .Select(g => new PeaMeterReading(
                            g.Key,
                            g.Sum(r => r.RateA),
                            g.Sum(r => r.RateB),
                            g.Sum(r => r.RateC),
                            60
                        ))
                        .OrderBy(r => r.PeriodStart)
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
}
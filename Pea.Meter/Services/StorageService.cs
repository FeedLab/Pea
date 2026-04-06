using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reactive.Linq;
using System.Text.Json;
using Akavache;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure;
using Pea.Infrastructure.Models;
using Pea.Meter.Extension;
using Pea.Meter.Helpers;
using Pea.Meter.Models;

namespace Pea.Meter.Services;

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class StorageService : ObservableObject
{
    [ObservableProperty] private ObservableCollection<PeaMeterReading> allMeterReadingsAsync = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> hourlyAggregated = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> dailyAggregated = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> weeklyAggregated = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> monthlyAggregated = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> dailyPeriodReadings = [];

    [ObservableProperty] private ConfigurationTariffModel configurationTariffModel;
    [ObservableProperty] private ConfigurationLanguageModel configurationLanguageModel;
    [ObservableProperty] private ConfigurationDataImportModel configurationDataImportModel;

    private readonly ILogger logger;
    private readonly ILoginHelper loginHelper;
    private readonly PeaDbContextFactory dbContextFactory;
    private readonly PeaAdapter peaAdapter;
    private CancellationTokenSource newDayCancellationTokenSource;
    public bool IsAuthenticated { get; set; }

    public StorageService(ILogger<StorageService> logger,
        ILoginHelper loginHelper,
        PeaDbContextFactory dbContextFactory,
        PeaAdapter peaAdapter,
        ConfigurationTariffModel configurationTariffModel,
        ConfigurationLanguageModel configurationLanguageModel,
        ConfigurationDataImportModel configurationDataImportModel)
    {
        this.logger = logger;
        this.loginHelper = loginHelper;
        this.dbContextFactory = dbContextFactory;
        this.peaAdapter = peaAdapter;
        ConfigurationTariffModel = configurationTariffModel;
        ConfigurationLanguageModel = configurationLanguageModel;
        ConfigurationDataImportModel = configurationDataImportModel;

        try
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(ConfigurationLanguageModel.CultureCode);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(ConfigurationLanguageModel.CultureCode);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error setting culture: {Message}", e.Message);
        }

        newDayCancellationTokenSource = new CancellationTokenSource();
        
        WeakReferenceMessenger.Default.Register<UserAccountRemovedMessage>(this, (r, m) =>
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var meterReadingRepository = new MeterReadingRepository(dbContextFactory);

                    logger.LogInformation("ConfigurationTariffModel.Reset from UserAccountRemovedMessage event message");
                    ConfigurationTariffModel.Reset();
                
                    logger.LogInformation("ConfigurationLanguageModel.Reset from UserAccountRemovedMessage event message");
                    ConfigurationLanguageModel.Reset();
                
                    logger.LogInformation("ConfigurationDataImportModel.Reset from UserAccountRemovedMessage event message");
                    ConfigurationDataImportModel.Reset();
                
                    logger.LogInformation("Clearing all ObservableCollection, from UserAccountRemovedMessage event message");
                    DailyPeriodReadings.Clear();
                    AllMeterReadingsAsync.Clear();
                    HourlyAggregated.Clear();
                    DailyAggregated.Clear();
                    WeeklyAggregated.Clear();
                    MonthlyAggregated.Clear();
                
                    logger.LogInformation("Deleting all meter readings from database, from UserAccountRemovedMessage event message");
                    await meterReadingRepository.DeleteAllAsync();
                
                    logger.LogInformation("Clearing auth data, from UserAccountRemovedMessage event message");
                    await loginHelper.ClearAuthDataAsync();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error executing reset after receive a UserAccountRemovedMessage event: {Message}", e.Message);
                }
            });
        });
    }

    public async Task ResetHistoricalData()
    {
        var meterReadingRepository = new MeterReadingRepository(dbContextFactory);
        var readingsFromDb = await meterReadingRepository.GetAllMeterReadingsAsync();

        var todayPeaMeterReadings =
            await peaAdapter.ShowDailyReadings(DateTime.Today.Date) ?? new List<PeaMeterReading>();

        if (todayPeaMeterReadings.Any())
        {
            var newReadings = todayPeaMeterReadings.Where(r => r.Total > 0).ToList();
            var newReadingsFiltered = GetDiffBetweenDailyAndAllReadings(newReadings);

            await UpdatePeriodDataAndProcessAggregations(newReadingsFiltered, readingsFromDb.ToList());
        }
    }

    private void ProcessAggregations()
    {
        HourlyAggregated.Clear();
        DailyAggregated.Clear();
        WeeklyAggregated.Clear();
        MonthlyAggregated.Clear();

        // Aggregate by hour
        var hourlyList = AllMeterReadingsAsync.SummaryByHour();
        HourlyAggregated.AddRange(hourlyList);
        WeakReferenceMessenger.Default.Send(new HourlyAggregationCompletedMessage(HourlyAggregated));

        // Aggregate by day
        var dailyList = AllMeterReadingsAsync.SummaryByDay();
        DailyAggregated.AddRange(dailyList);
        WeakReferenceMessenger.Default.Send(new DailyAggregationCompletedMessage(DailyAggregated));

        // Aggregate by week
        var weeklyList = AllMeterReadingsAsync.SummaryByWeek();
        WeeklyAggregated.AddRange(weeklyList);
        WeakReferenceMessenger.Default.Send(new WeeklyAggregationCompletedMessage(WeeklyAggregated));

        // Aggregate by month
        var monthlyList = AllMeterReadingsAsync.SummaryByMonth();
        MonthlyAggregated.AddRange(monthlyList);
        WeakReferenceMessenger.Default.Send(new MonthlyAggregationCompletedMessage(MonthlyAggregated));
    }

    /// <summary>
    /// Creates a diff list between all database readings and current day's live readings.
    /// Returns readings that exist in dailyReadings but not yet in the database (allMeterReadingsAsync).
    /// </summary>
    private List<PeaMeterReading> GetDiffBetweenDailyAndAllReadings(List<PeaMeterReading> readings)
    {
        // Get readings from dailyReadings that don't exist in allMeterReadingsAsync
        // Compare by PeriodStart as unique identifier
        var allReadingTimes = new HashSet<DateTime>(AllMeterReadingsAsync.Select(r => r.PeriodStart));

        var newReadings = readings
            .Where(dr => !allReadingTimes.Contains(dr.PeriodStart))
            .ToList();

        return newReadings;
    }

    public async Task<string> ExportAllMeterReadingsToJsonAsync()
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(AllMeterReadingsAsync, jsonOptions);
            var fileName = $"AllMeterReadings_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            await File.WriteAllTextAsync(filePath, json);

            logger.LogInformation("Exported AllMeterReadingsAsync to {FilePath}", filePath);

            return filePath;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error exporting AllMeterReadingsAsync to JSON: {Message}", e.Message);
            throw;
        }
    }

    public async Task UpdatePeriodDataAndProcessAggregations(List<PeaMeterReading> newReadingsFiltered,
        List<PeaMeterReading>? allReadings = null)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            try
            {
                if (allReadings != null)
                {
                    AllMeterReadingsAsync.Clear();
                    AllMeterReadingsAsync.AddRange(allReadings);
                    AllMeterReadingsAsync.AddRange(newReadingsFiltered);
                }
                else
                {
                    AllMeterReadingsAsync.AddRange(newReadingsFiltered);
                }

                var isDailyCollectionChanged = DailyPeriodReadings.Count != newReadingsFiltered.Count;
                DailyPeriodReadings.Clear();
                DailyPeriodReadings.AddRange(newReadingsFiltered);

                ProcessAggregations();
                
                if(isDailyCollectionChanged)
                {
                    WeakReferenceMessenger.Default.Send(new DailyPeriodsChangedMessage(DailyPeriodReadings));
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error updating period data and processing aggregations: {Message}", e.Message);
            }

            return Task.CompletedTask;
        });
    }

    public async Task ReloadHistoricalDayReadingsFromDb()
    {
        var meterReadingRepository = new MeterReadingRepository(dbContextFactory);

        var readingsFromDb = await meterReadingRepository.GetAllMeterReadingsAsync();

        AllMeterReadingsAsync.Clear();
        AllMeterReadingsAsync.AddRange(readingsFromDb);

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            try
            {
                ProcessAggregations();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error updating period data and processing aggregations: {Message}", e.Message);
            }

            return Task.CompletedTask;
        });
    }
}

public record ConfigurationTariffMessage(ConfigurationTariffModel newModel, ConfigurationTariffModel oldModel);
public record DailyPeriodsChangedMessage(ObservableCollection<PeaMeterReading> Data);
public record HourlyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record DailyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record WeeklyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record MonthlyAggregationCompletedMessage(ObservableCollection<PeaMeterReading> Data);

public record AllAggregationsCompletedMessage();

public record AllImportedDataCompletedMessage();

public record DateChangedMessage(DateTime OldDate, DateTime NewDate);
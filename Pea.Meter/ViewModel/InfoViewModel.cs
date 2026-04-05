using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Infrastructure.Models;
using Pea.Meter.Extension;
using Pea.Meter.Helpers;
using Pea.Meter.Models;
using Pea.Meter.Services;
using static Microsoft.Maui.ApplicationModel.MainThread;
using Debug = System.Diagnostics.Debug;

namespace Pea.Meter.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class InfoViewModel : ObservableObject
{
    private readonly ILogger<InfoViewModel> logger;
    private readonly StorageService storageService;

    [ObservableProperty] private IAuthData? authData;


    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterData = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage1 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage7 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage30 = [];

    [ObservableProperty] private bool isAddMeterVisible = true;
    [ObservableProperty] private bool isMeterDataVisible;
    [ObservableProperty] private bool isRemoveMeterVisible;
    [ObservableProperty] private bool isCustomerProfileViewVisible;
    [ObservableProperty] private DateTime dateMeterData = DateTime.Today;

    public InfoViewModel(ILogger<InfoViewModel> logger,
        AuthDataOptions authDataOptions,
        StorageService storageService)
    {
        this.logger = logger;
        this.storageService = storageService;

        // Initialize AuthData from saved settings
        AuthData = authDataOptions.AuthData;
        Debug.WriteLine($"Constructor: AuthData={(AuthData == null ? "null" : $"{AuthData.Username}")}");
        Debug.WriteLine($"Constructor: IsCustomerProfileViewVisible={IsCustomerProfileViewVisible}");

        // Ensure visibility properties are set correctly on startup
        OnAuthDataChanged(AuthData);
        Debug.WriteLine($"After OnAuthDataChanged: IsCustomerProfileViewVisible={IsCustomerProfileViewVisible}");

        CreateLoggedInSubscription();
        CreateLoggedOutSubscription();
        CreateAllAggregationsCompletedSubscription();
        CreateNewDaySubscription();
        CreateDataImportedSubscription();
        CreateDailyPeriodsChangedSubscription();
    }

    private void CreateDataImportedSubscription()
    {
        WeakReferenceMessenger.Default.Register<DataImportedMessage>(this,
            (_, _) =>
            {
                InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await PopulateChartData();
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
            (_, m) =>
            {
                InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        logger.LogInformation("DateChangedMessage received: {NewDate}", m.NewDate);

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
        WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this, (_, _) =>
        {
            InvokeOnMainThreadAsync(() =>
            {
                IsAddMeterVisible = true;
                IsMeterDataVisible = false;
                IsCustomerProfileViewVisible = true;

                MeterData = [];
                MeterDataAverage1 = [];
                MeterDataAverage7 = [];
                MeterDataAverage30 = [];
                return Task.CompletedTask;
            });
        });
    }

    private void CreateLoggedInSubscription()
    {
        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async (_, _) =>
        {
            IsAddMeterVisible = false;
            IsMeterDataVisible = true;
            IsCustomerProfileViewVisible = false;
        });
    }

    private void CreateDailyPeriodsChangedSubscription()
    {
        WeakReferenceMessenger.Default.Register<DailyPeriodsChangedMessage>(this,
            async void (_, _) =>
            {
                try
                {
                    await PopulateChartData();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateLoggedInSubscription), e.Message);
                }
            });
    }

    private void CreateAllAggregationsCompletedSubscription()
    {
        WeakReferenceMessenger.Default.Register<AllAggregationsCompletedMessage>(this, async void (_, _) =>
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

        var meterDataAverageDays0 = storageService.DailyPeriodReadings.AverageBy15MinutesPeriod();
        var meterDataAverageDays1 = storageService.AllMeterReadingsAsync.FilterByPeriod(timeStart1, today)
            .AverageBy15MinutesPeriod();
        var meterDataAverageDays7 = storageService.AllMeterReadingsAsync.FilterByPeriod(timeStart7, today)
            .AverageBy15MinutesPeriod();
        var meterDataAverageDays30 = storageService.AllMeterReadingsAsync.FilterByPeriod(timeStart30, today)
            .AverageBy15MinutesPeriod();

        await InvokeOnMainThreadAsync(() =>
        {
            MeterData.Clear();
            MeterDataAverage1.Clear();
            MeterDataAverage7.Clear();
            MeterDataAverage30.Clear();

            MeterData.AddRange(meterDataAverageDays0);
            MeterDataAverage1.AddRange(meterDataAverageDays1);
            MeterDataAverage7.AddRange(meterDataAverageDays7);
            MeterDataAverage30.AddRange(meterDataAverageDays30);
        });
    }
}
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0019:Invalid containing type for [ObservableProperty] field or property")]
[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class TouVsFlatRateViewModel : ObservableObject
{
    private readonly ILogger<TouVsFlatRateViewModel> logger;
    private readonly StorageService storageService;
    [ObservableProperty] private List<Models.CostCompare> costCompares = [];
    [ObservableProperty] private decimal touTotalCost;
    [ObservableProperty] private decimal flatRateTotalCost;
    [ObservableProperty] private decimal diffInCurrency;
    [ObservableProperty] private decimal diffInPercent;
    [ObservableProperty] private bool isFlatRateVisible;
    [ObservableProperty] private bool isTouVisible;
    [ObservableProperty] private DateTime startDate;
    [ObservableProperty] private DateTime endDate;
    [ObservableProperty] private DateTime startTimePickerMaximumDate;
    [ObservableProperty] private DateTime startTimePickerMinimumDate;
    [ObservableProperty] private DateTime endTimePickerMaximumDate;
    [ObservableProperty] private DateTime endTimePickerMinimumDate;

    public TouVsFlatRateViewModel(ILogger<TouVsFlatRateViewModel> logger, StorageService storageService)
    {
        this.logger = logger;
        this.storageService = storageService;

        UserLoggedInSubscription();
        UserLoggedOutSubscription();
        DateChangedSubscription();
        DataImportedSubscription();
    }

    private void DataImportedSubscription()
    {
        WeakReferenceMessenger.Default.Register<DataImportedMessage>(this, async void (r, m) =>
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        StartTimePickerMinimumDate = m.Date;

                        if (StartDate < StartTimePickerMinimumDate)
                        {
                            StartDate = StartTimePickerMinimumDate;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}",
                            $"{nameof(DataImportedSubscription)}:InvokeOnMainThreadAsync", e.Message);
                    }

                    return Task.FromResult(Task.CompletedTask);
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in {Method}: {Message}", nameof(DataImportedSubscription), e.Message);
            }
        });
    }
    
    
    private void DateChangedSubscription()
    {
        WeakReferenceMessenger.Default.Register<DateChangedMessage>(this, async void (r, m) =>
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        InitializeDateRange();
                        await CalculateCostComparisons();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}",
                            $"{nameof(DateChangedSubscription)}:InvokeOnMainThreadAsync", e.Message);
                    }

                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in {Method}: {Message}", nameof(DateChangedSubscription), e.Message);
            }
        });
    }
    
    private void UserLoggedInSubscription()
    {
        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async void (r, m) =>
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        InitializeDateRange();
                        await CalculateCostComparisons();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}",
                            $"{nameof(UserLoggedInSubscription)}:InvokeOnMainThreadAsync", e.Message);
                    }

                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in {Method}: {Message}", nameof(UserLoggedInSubscription), e.Message);
            }
        });
    }
    
    private void UserLoggedOutSubscription()
    {
        WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this, async void (r, m) =>
        {
            try
            {
                // await MainThread.InvokeOnMainThreadAsync(async () =>
                // {
                //     try
                //     {
                //
                //     }
                //     catch (Exception e)
                //     {
                //         logger.LogError(e, "Error in {Method}: {Message}",
                //             $"{nameof(UserLoggedOutSubscription)}:InvokeOnMainThreadAsync", e.Message);
                //     }
                //
                //     return Task.CompletedTask;
                // });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in {Method}: {Message}", nameof(UserLoggedOutSubscription), e.Message);
            }
        });
    }

    private void InitializeDateRange()
    {
        var meterReading = storageService.DailyAggregated;

        var today = DateTime.Now;
        StartDate = today.AddYears(-1);
        EndDate = today;

        if (meterReading.Any())
        {
            StartTimePickerMinimumDate = meterReading.First().PeriodStart.Date;
            StartTimePickerMaximumDate = EndDate.AddDays(-1);

            if (StartDate < StartTimePickerMinimumDate)
            {
                StartDate = StartTimePickerMinimumDate;
            }

            EndTimePickerMinimumDate = StartDate.AddDays(1);
            EndTimePickerMaximumDate = today;
        }
    }

    public Task CalculateCostComparisons()
    {
        IsFlatRateVisible = false;
        IsTouVisible = false;

        var meterReading = storageService
            .DailyAggregated
            .Where(w => w.PeriodStart >= StartDate && w.PeriodStart < EndDate)
            .ToList();

        if (meterReading.Count == 0)
        {
            return Task.CompletedTask;
        }

        CostCompares = meterReading
            .Where(w => w.Total > 0)
            .Select(s => new Models.CostCompare(s, 3.9086m, 5.1135m, 2.6037m))
            .ToList();

        TouTotalCost = CostCompares.Sum(c => c.TouCost);
        FlatRateTotalCost = CostCompares.Sum(c => c.FlatRateCost);
        DiffInCurrency = FlatRateTotalCost - TouTotalCost;
        var touAverageCostPerDay = TouTotalCost / CostCompares.Count;
        var flatRateAverageCostPerDay = FlatRateTotalCost / CostCompares.Count;
        DiffInPercent = (flatRateAverageCostPerDay - touAverageCostPerDay) / flatRateAverageCostPerDay * 100;

        if (DiffInCurrency < 0)
        {
            IsFlatRateVisible = true;
            IsTouVisible = false;
        }
        else
        {
            IsFlatRateVisible = false;
            IsTouVisible = true;
        }

        return Task.CompletedTask;
    }
}
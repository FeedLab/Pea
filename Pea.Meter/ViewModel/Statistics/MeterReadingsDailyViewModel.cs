using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Infrastructure.Models;
using Pea.Meter.Extension;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel.Statistics;

public partial class MeterReadingsDailyViewModel : ObservableObject
{
    private readonly ILogger<MeterReadingsDailyViewModel> logger;
    private readonly StorageService storageService;

    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataDailyAggregated = [];

    public MeterReadingsDailyViewModel(ILogger<MeterReadingsDailyViewModel> logger, StorageService storageService)
    {
        this.logger = logger;
        this.storageService = storageService;

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async (r, m) =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                storageService.DailyAggregated.CollectionChanged += MeterDataAverageDailyOnCollectionChanged;

                MeterDataDailyAggregated.AddRange(storageService.DailyAggregated);
                return Task.CompletedTask;
            });
        });
        
        WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this, (r, m) =>
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                storageService.DailyAggregated.CollectionChanged -= MeterDataAverageDailyOnCollectionChanged;

                MeterDataDailyAggregated = [];
                
                return Task.CompletedTask;
            });
        });
        
        WeakReferenceMessenger.Default.Register<AllAggregationsCompletedMessage>(this, async void (r, m) =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        MeterDataDailyAggregated.Clear();

                        MeterDataDailyAggregated.AddRange(storageService.DailyAggregated);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", "MeterDataDailyAggregated", e.Message);
                    }
                    
                    return Task.CompletedTask;
                });
                
                return Task.CompletedTask;
            });
        });
        
        WeakReferenceMessenger.Default.Register<DateChangedMessage>(this, async (r, m) =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MeterDataDailyAggregated.Clear();
                MeterDataDailyAggregated.AddRange(storageService.DailyAggregated);
            });
        });

        WeakReferenceMessenger.Default.Register<DataImportedMessage>(this, async (r, m) =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var aggregated = m.Readings
                    .GroupBy(r => r.PeriodStart.Date)
                    .Select(g => new PeaMeterReading(
                        g.Key,
                        g.Sum(x => x.RateA),
                        g.Sum(x => x.RateB),
                        g.Sum(x => x.RateC)
                    ))
                    .ToList();

                //        var meterDataAggregatedPerDayAll = storageService.DailyAggregated.FilterByPeriod(DateTime.MinValue, DateTime.MaxValue);
                MeterDataDailyAggregated.Insert(0, aggregated.First());

                // MeterDataDailyAggregated = meterDataAggregatedPerDayAll
                //     .OrderBy(o => o.PeriodStart)
                //     .ToObservableCollection();

                return Task.CompletedTask;
            });
        });
    }

    private void MeterDataAverageDailyOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItems = e.NewItems?.Cast<PeaMeterReading>().ToList() ?? [];
                    MeterDataDailyAggregated.AddRange(newItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (PeaMeterReading item in e.OldItems?.Cast<PeaMeterReading>().ToList() ?? [])
                    {
                        MeterDataDailyAggregated.Remove(item);
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    // for (var i = 0; i < e.OldItems.Length; i++)
                    // {
                    //     var index = MeterDataAverage30.IndexOf(e.OldItems[i]);
                    //     if (index >= 0)
                    //     {
                    //         MeterDataAverage30[index] = e.NewItems[i];
                    //     }
                    // }
                    break;
                case NotifyCollectionChangedAction.Move:
                    // For ObservableCollection, typically no action needed as order may not matter
                    // or rebuild collection if order is important
                    break;
                case NotifyCollectionChangedAction.Reset:
                    MeterDataDailyAggregated.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        });
    }
}
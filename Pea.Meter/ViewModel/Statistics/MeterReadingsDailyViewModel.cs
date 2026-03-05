using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ObservableCollections;
using Pea.Infrastructure.Models;
using Pea.Meter.Extension;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel.Statistics;

public partial class MeterReadingsDailyViewModel : ObservableObject
{
    private readonly StorageService storageService;

    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataDailyAggregated = [];

    public MeterReadingsDailyViewModel(StorageService storageService)
    {
        this.storageService = storageService;

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async (r, m) =>
        {
            var meterDataAggregatedPerDayAll =
                storageService.DailyAggregated.FilterByPeriod(DateTime.MinValue, DateTime.MaxValue);

            storageService.DailyAggregated.CollectionChanged += MeterDataAverageDailyOnCollectionChanged;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MeterDataDailyAggregated.AddRange(meterDataAggregatedPerDayAll);
                return Task.CompletedTask;
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

                var meterDataAggregatedPerDayAll = storageService.DailyAggregated.FilterByPeriod(DateTime.MinValue, DateTime.MaxValue);
                meterDataAggregatedPerDayAll.Add(aggregated.First());

                MeterDataDailyAggregated = meterDataAggregatedPerDayAll
                    .OrderBy(o => o.PeriodStart)
                    .ToObservableCollection();
                
                return Task.CompletedTask;
            });
        });
    }

    private void MeterDataAverageDailyOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var newItems = e.NewItems as List<PeaMeterReading> ?? [];
                MeterDataDailyAggregated.AddRange(newItems);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (PeaMeterReading item in e.OldItems)
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
    }
}
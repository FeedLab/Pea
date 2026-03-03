using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Infrastructure.Models;
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
            var meterDataAggregatedPerDayAll = storageService.FetchDailyAggregatedForPeriodAsync(DateTime.MinValue, DateTime.MaxValue);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MeterDataDailyAggregated = new ObservableCollection<PeaMeterReading>(meterDataAggregatedPerDayAll);
                return Task.CompletedTask;
            });
        });
    }
}

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using ObservableCollections;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;
using Pea.Infrastructure.Repositories;
using Pea.Meter.Extension;
using Pea.Meter.Helper;
using Pea.Meter.Models;
using Pea.Meter.Services;
using Debug = System.Diagnostics.Debug;

namespace Pea.Meter.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class InfoViewModel : ObservableObject
{
    private readonly CustomerProfileViewModel customerProfile;
    private readonly ILoginHelper loginHelper;
    private readonly IPopupService popupService;
    private readonly HistoricDataImportService historicDataImportService;
    private readonly StorageService storageService;

    [ObservableProperty] private IAuthData? authData;


    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterData = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage1 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage7 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage30 = [];

    [ObservableProperty] private bool isAddMeterVisible = true;
    [ObservableProperty] private bool isMeterDataVisible = false;
    [ObservableProperty] private bool isRemoveMeterVisible = false;
    [ObservableProperty] private bool isCustomerProfileViewVisible = false;
    [ObservableProperty] private DateTime dateMeterData = DateTime.Today;
    private AuthData? authDataLogin;

    public InfoViewModel(CustomerProfileViewModel customerProfile, AuthDataOptions authDataOptions,
        ILoginHelper loginHelper, IPopupService popupService,
        HistoricDataImportService historicDataImportService,
        StorageService storageService)
    {
        this.customerProfile = customerProfile;
        this.loginHelper = loginHelper;
        this.popupService = popupService;
        this.historicDataImportService = historicDataImportService;
        this.storageService = storageService;

        // Initialize AuthData from saved settings
        AuthData = authDataOptions.AuthData;
        Debug.WriteLine($"Constructor: AuthData={(AuthData == null ? "null" : $"{AuthData.Username}")}");
        Debug.WriteLine($"Constructor: IsCustomerProfileViewVisible={IsCustomerProfileViewVisible}");

        // Ensure visibility properties are set correctly on startup
        OnAuthDataChanged(AuthData);
        Debug.WriteLine($"After OnAuthDataChanged: IsCustomerProfileViewVisible={IsCustomerProfileViewVisible}");

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, async (r, m) =>
        {
            IsAddMeterVisible = false;
            IsMeterDataVisible = true;
            IsCustomerProfileViewVisible = false;

            storageService.DailyReadings.CollectionChanged += DailyPeaMeterDataCollectionChanged;
            storageService.DailyReadings.CollectionChanged += MeterDataAverage1OnCollectionChanged;
            storageService.DailyReadings.CollectionChanged += MeterDataAverage7OnCollectionChanged;
            storageService.DailyReadings.CollectionChanged += MeterDataAverage30OnCollectionChanged;
        });

        WeakReferenceMessenger.Default.Register<AllAggregationsCompletedMessage>(this, async (r, m) =>
        {
            var today = DateTime.Today;
            var timeStart1 = today.AddDays(-1);
            var timeStart7 = today.AddDays(-7);
            var timeStart30 = today.AddDays(-30);

            var meterDataAverageDays0 = storageService.DailyReadings;
            var meterDataAverageDays1 = storageService.AllMeterReadingsAsync.FilterByPeriod(timeStart1, today).AverageBy15MinutesPeriod();
            var meterDataAverageDays7 = storageService.AllMeterReadingsAsync.FilterByPeriod(timeStart7, today).AverageBy15MinutesPeriod();
            var meterDataAverageDays30 = storageService.AllMeterReadingsAsync.FilterByPeriod(timeStart30, today).AverageBy15MinutesPeriod();

            await MainThread.InvokeOnMainThreadAsync(() =>
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

            // var meterDataAverageDays7 = storageService.FetchAverageQuarterlyReadingsForPeriodAsync(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(-1) );
            // var meterDataAverageDays30 = storageService.FetchAverageQuarterlyReadingsForPeriodAsync(DateTime.Today.AddDays(-30), DateTime.Today.AddDays(-1));


            // await MainThread.InvokeOnMainThreadAsync(() =>
            // {
            //     MeterData = dailyReadings;
            //     MeterDataAverage7 = meterDataAverageDays7;
            //     MeterDataAverage30 = meterDataAverageDays30;
            //
            //     return Task.CompletedTask;
            // });
        });

        WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    IsAddMeterVisible = true;
                    IsMeterDataVisible = false;
                    IsCustomerProfileViewVisible = true;
                });
            });
    }

    private void DailyPeaMeterDataCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var newItems = e.NewItems as List<PeaMeterReading> ?? [];
                MeterData.AddRange(newItems);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (PeaMeterReading item in e.OldItems)
                {
                    MeterData.Remove(item);
                }
                break;
            // case NotifyCollectionChangedAction.Replace:
            //     for (var i = 0; i < e.OldItems.Count; i++)
            //     {
            //         var index = MeterData.IndexOf(e.OldItems[i]);
            //         if (index >= 0)
            //         {
            //             MeterData[index] = e.NewItems[i];
            //         }
            //     }
                break;
            case NotifyCollectionChangedAction.Move:
                // For ObservableCollection, typically no action needed as order may not matter
                // or rebuild collection if order is important
                break;
            case NotifyCollectionChangedAction.Reset:
                MeterData.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void MeterDataAverage30OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var data30Day = DateTime.Today.AddDays(-30);
        var dataYesterday = DateTime.Today.AddDays(-1);
            
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var newItems = e.NewItems as List<PeaMeterReading> ?? [];
                var filteredData = newItems.Where(x => x.PeriodStart >= data30Day && x.PeriodStart < dataYesterday);
                MeterDataAverage30.AddRange(filteredData);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (PeaMeterReading item in e.OldItems)
                {
                    MeterDataAverage30.Remove(item);
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
                MeterDataAverage30.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void MeterDataAverage1OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var data30Day = DateTime.Today.AddDays(-30);
        var dataYesterday = DateTime.Today.AddDays(-1);
            
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var newItems = e.NewItems as List<PeaMeterReading> ?? [];
                var filteredData = newItems.Where(x => x.PeriodStart >= data30Day && x.PeriodStart < dataYesterday);
                MeterDataAverage30.AddRange(filteredData);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (PeaMeterReading item in e.OldItems)
                {
                    MeterDataAverage30.Remove(item);
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
                MeterDataAverage30.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void MeterDataAverage7OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var data7Day = DateTime.Today.AddDays(-7);
        var dataYesterday = DateTime.Today.AddDays(-1);
        
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var newItems = e.NewItems as List<PeaMeterReading> ?? [];
                var filteredData = newItems.Where(x => x.PeriodStart >= data7Day && x.PeriodStart < dataYesterday);
                MeterDataAverage7.AddRange(filteredData);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (PeaMeterReading item in e.OldItems)
                {
                    MeterDataAverage7.Remove(item);
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                // for (var i = 0; i < e.OldItems.Length; i++)
                // {
                //     var index = MeterDataAverage7.IndexOf(e.OldItems[i]);
                //     if (index >= 0)
                //     {
                //         MeterDataAverage7[index] = e.NewItems[i];
                //     }
                // }
                break;
            case NotifyCollectionChangedAction.Move:
                // For ObservableCollection, typically no action needed as order may not matter
                // or rebuild collection if order is important
                break;
            case NotifyCollectionChangedAction.Reset:
                MeterDataAverage7.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    [RelayCommand]
    private async Task AddAccount()
    {
        await DisplayLoginPopup();

        if (string.IsNullOrEmpty(authDataLogin.Username) || string.IsNullOrEmpty(authDataLogin.Password))
        {
            return;
        }

        Debug.WriteLine($"AddAccount: Credentials valid, refreshing profile");
        await customerProfile.RefreshProfile(authDataLogin.Username, authDataLogin.Password);

        AuthData = await loginHelper.SaveAuthDataAsync(authDataLogin.Username, authDataLogin.Password);
        Debug.WriteLine($"AddAccount: AuthData saved. IsCustomerProfileViewVisible={IsCustomerProfileViewVisible}");

        WeakReferenceMessenger.Default.Send(new UserLoggedInMessage(AuthData));
    }

    // partial void OnAuthDataChanged(IAuthData? value)
    // {
    //     Debug.WriteLine($"OnAuthDataChanged called: value={(value == null ? "null" : $"{value.Username}")}");
    //
    //     IsAddMeterVisible = value == null;
    //     Debug.WriteLine($"{nameof(IsAddMeterVisible)}={IsAddMeterVisible}");
    //
    //     IsRemoveMeterVisible = value != null;
    //     Debug.WriteLine($"{nameof(IsRemoveMeterVisible)}={IsRemoveMeterVisible}");
    //
    //     IsCustomerProfileViewVisible = value != null;
    //     Debug.WriteLine($"{nameof(IsCustomerProfileViewVisible)}={IsCustomerProfileViewVisible}");
    // }

    private async Task DisplayLoginPopup()
    {
        authDataLogin = new AuthData("", "");
        var queryAttributes = new Dictionary<string, object>
        {
            [nameof(AuthData)] = authDataLogin
        };

        var popupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = false
        };

        await popupService.ShowPopupAsync<LoginPopupViewModel>(
            Shell.Current,
            options: popupOptions,
            shellParameters: queryAttributes);
    }

    [RelayCommand]
    private async Task ImportHistoricData()
    {
        try
        {
            Console.WriteLine("Starting historic data import...");

            // Import data for 7 days starting from yesterday
            var historicData = await historicDataImportService.ImportHistoricDataAsync(7);

            Console.WriteLine($"Import completed. Total days imported: {historicData.Count}");

            // Optionally display the total readings imported
            var totalReadings = historicData.Sum(kvp => kvp.Value.Count);
            Console.WriteLine($"Total readings imported: {totalReadings}");

            // You can update the UI or store the data as needed
            // For now, just logging the results
            foreach (var dateData in historicData.OrderByDescending(x => x.Key))
            {
                Console.WriteLine($"{dateData.Key:yyyy-MM-dd}: {dateData.Value.Count} readings");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during historic data import: {ex.Message}");
        }
    }
}
using System.Collections.ObjectModel;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Models;
using Pea.Infrastructure.Repositories;
using Pea.Meter.Helper;
using Pea.Meter.Models;
using Pea.Meter.Services;
using Debug = System.Diagnostics.Debug;

namespace Pea.Meter.ViewModel;

public partial class InfoViewModel : ObservableObject
{
    private readonly CustomerProfileViewModel customerProfile;
    private readonly AuthDataOptions authDataOptions;
    private readonly PeaAdapter peaAdapter;
    private readonly ILoginHelper loginHelper;
    private readonly IPopupService popupService;
    private readonly HistoricDataImportService historicDataImportService;
    private readonly HistoricDataBackgroundService historicDataBackgroundService;
    private readonly PeaDbContextFactory dbContextFactory;

    [ObservableProperty] private IAuthData? authData;
    
    
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterData = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage7 = [];
    [ObservableProperty] private ObservableCollection<PeaMeterReading> meterDataAverage30 = [];
    
    [ObservableProperty] private bool isAddMeterVisible = true;
    [ObservableProperty] private bool isMeterDataVisible = false;
    [ObservableProperty] private bool isRemoveMeterVisible = false;
    [ObservableProperty] private bool isCustomerProfileViewVisible = false;
    [ObservableProperty] private DateTime dateMeterData = DateTime.Today;
    private AuthData? authDataLogin;
    
    public InfoViewModel(CustomerProfileViewModel customerProfile, AuthDataOptions authDataOptions,
        PeaAdapter peaAdapter, ILoginHelper loginHelper, IPopupService popupService,
        HistoricDataImportService historicDataImportService, HistoricDataBackgroundService historicDataBackgroundService,
        PeaDbContextFactory dbContextFactory)
    {
        this.customerProfile = customerProfile;
        this.authDataOptions = authDataOptions;
        this.peaAdapter = peaAdapter;
        this.loginHelper = loginHelper;
        this.popupService = popupService;
        this.historicDataImportService = historicDataImportService;
        this.historicDataBackgroundService = historicDataBackgroundService;
        this.dbContextFactory = dbContextFactory;

        // Initialize AuthData from saved settings
        AuthData = authDataOptions.AuthData;
        Debug.WriteLine($"Constructor: AuthData={(AuthData == null ? "null" : $"{AuthData.Username}")}");
        Debug.WriteLine($"Constructor: IsCustomerProfileViewVisible={IsCustomerProfileViewVisible}");

        // Ensure visibility properties are set correctly on startup
        OnAuthDataChanged(AuthData);
        Debug.WriteLine($"After OnAuthDataChanged: IsCustomerProfileViewVisible={IsCustomerProfileViewVisible}");

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    IsAddMeterVisible = false;
                    IsMeterDataVisible = true;
                    IsCustomerProfileViewVisible = false;

                    var dailyReadings = await peaAdapter.ShowDailyReadings(DateTime.Today);

                    MeterData = new ObservableCollection<PeaMeterReading>(dailyReadings);
                    
                    // Create user-specific DbContext and repository
                    using var dbContext = dbContextFactory.CreateDbContext(m.AuthData.Username);
                    var meterReadingRepository = new MeterReadingRepository(dbContext);
                    
                    MeterDataAverage7 = new ObservableCollection<PeaMeterReading>(await meterReadingRepository.GetAverageReadingsByTimeOfDayAsync(7, m.AuthData.Username));
                    MeterDataAverage30 = new ObservableCollection<PeaMeterReading>(await meterReadingRepository.GetAverageReadingsByTimeOfDayAsync(30, m.AuthData.Username));

                    // Trigger background import of historic data
                    if (m.AuthData?.Username != null)
                    {
                        historicDataBackgroundService.TriggerImport(m.AuthData.Username);
                    }
                });
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
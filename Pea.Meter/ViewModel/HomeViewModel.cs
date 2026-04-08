using System.Diagnostics;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Meter.Helpers;
using Pea.Meter.Helpers;
using Pea.Meter.Models;
using Pea.Meter.Services;
using AuthData = Pea.Meter.Helpers.AuthData;

namespace Pea.Meter.ViewModel;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty] private IAuthData? authData;

    private readonly CustomerProfileViewModel customerProfile;
    private readonly AuthDataOptions authDataOptions;
    private readonly ILoginHelper loginHelper;
    private readonly IPopupService popupService;
    private readonly StorageService storageService;
    private readonly HistoricDataBackgroundService historicDataBackgroundService;
    private readonly PeaDbContextFactory dbContextFactory;
    private readonly PeaAdapterRouter peaAdapterRouter;
    private readonly DailyPeaReadingsTimer dailyPeaReadingsTimer;
    private readonly NewDayBackgroundTimer newDayBackgroundTimer;
    private AuthData? authDataLogin;

    public HomeViewModel(CustomerProfileViewModel customerProfile, AuthDataOptions authDataOptions,
        ILoginHelper loginHelper, IPopupService popupService,
        StorageService storageService, HistoricDataBackgroundService historicDataBackgroundService,
        IPeaAdapter peaAdapterRouter,
        PeaDbContextFactory dbContextFactory,
        DailyPeaReadingsTimer dailyPeaReadingsTimer, NewDayBackgroundTimer newDayBackgroundTimer)
    {
        this.customerProfile = customerProfile;
        this.authDataOptions = authDataOptions;
        this.loginHelper = loginHelper;
        this.popupService = popupService;
        this.storageService = storageService;
        this.historicDataBackgroundService = historicDataBackgroundService;
        this.dbContextFactory = dbContextFactory;
        this.peaAdapterRouter = (PeaAdapterRouter)peaAdapterRouter;
        this.dailyPeaReadingsTimer = dailyPeaReadingsTimer;
        this.newDayBackgroundTimer = newDayBackgroundTimer;

        AuthData = authDataOptions.AuthData;
    }

    [RelayCommand]
    private async Task AddDemoAccount()
    {
        var loggerRepository = AppService.GetRequiredService<ILogger<MeterReadingRepository>>();
        var meterReadingRepository = new MeterReadingRepository(loggerRepository, dbContextFactory);
        
        peaAdapterRouter.UseDemo(true);
        await peaAdapterRouter.LoginUser("demo", "demo");
        authDataLogin = new AuthData("demo", "demo");
        var meterNumber = peaAdapterRouter.MeterNumber ?? throw new InvalidOperationException();

        await meterReadingRepository.DeleteAllAsync();
        await meterReadingRepository.DeleteAllAsync(meterNumber);
        var readings = await peaAdapterRouter.GetAllReadings(DateTime.Today);
        await meterReadingRepository.AddRangeAsync(readings, meterNumber);
        
        storageService.IsAuthenticated = true;
        AuthData = await loginHelper.SaveAuthDataAsync(authDataLogin.Username, authDataLogin.Password);

        WeakReferenceMessenger.Default.Send(new UserLoggedInMessage(AuthData));

        await storageService.ResetHistoricalData();
        WeakReferenceMessenger.Default.Send(new AllAggregationsCompletedMessage());

        newDayBackgroundTimer.Start();
        await dailyPeaReadingsTimer.Start();
        historicDataBackgroundService.Start(1);
    }

    [RelayCommand]
    private async Task AddAccount()
    {
        peaAdapterRouter.UseDemo(false);

        await DisplayLoginPopup();

        if (string.IsNullOrEmpty(authDataLogin?.Username) || string.IsNullOrEmpty(authDataLogin?.Password))
        {
            return;
        }

        storageService.IsAuthenticated =
            await customerProfile.RefreshProfile(authDataLogin.Username, authDataLogin.Password);


        if (storageService.IsAuthenticated)
        {
            AuthData = await loginHelper.SaveAuthDataAsync(authDataLogin.Username, authDataLogin.Password);

            WeakReferenceMessenger.Default.Send(new UserLoggedInMessage(AuthData));

            await storageService.ResetHistoricalData();
            WeakReferenceMessenger.Default.Send(new AllAggregationsCompletedMessage());

            newDayBackgroundTimer.Start();
            await dailyPeaReadingsTimer.Start();
            historicDataBackgroundService.Start(1);
        }
    }

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
}
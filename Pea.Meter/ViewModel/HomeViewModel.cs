using System.Diagnostics;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
    private AuthData? authDataLogin;
    
    public HomeViewModel(CustomerProfileViewModel customerProfile, AuthDataOptions authDataOptions,
        ILoginHelper loginHelper, IPopupService popupService, StorageService storageService, HistoricDataBackgroundService historicDataBackgroundService)
    {
        this.customerProfile = customerProfile;
        this.authDataOptions = authDataOptions;
        this.loginHelper = loginHelper;
        this.popupService = popupService;
        this.storageService = storageService;
        this.historicDataBackgroundService = historicDataBackgroundService;

        AuthData = authDataOptions.AuthData;
    }

    [RelayCommand]
    private async Task AddAccount()
    {
        await DisplayLoginPopup();

        if (string.IsNullOrEmpty(authDataLogin.Username) || string.IsNullOrEmpty(authDataLogin.Password))
        {
            return;
        }

        storageService.IsAuthenticated = await customerProfile.RefreshProfile(authDataLogin.Username, authDataLogin.Password);


        if (storageService.IsAuthenticated)
        {
            AuthData = await loginHelper.SaveAuthDataAsync(authDataLogin.Username, authDataLogin.Password);
    //        await storageService.Init();
            WeakReferenceMessenger.Default.Send(new UserLoggedInMessage(AuthData));
            historicDataBackgroundService.Start();
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
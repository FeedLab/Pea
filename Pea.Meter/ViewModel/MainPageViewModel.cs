using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Pea.Meter.Helpers;
using Pea.Meter.Helpers;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

public partial class MainPageViewModel : ObservableObject
{
    private readonly CustomerProfileViewModel customerProfile;
    private readonly AuthDataOptions authDataOptions;
    private readonly PeaAdapter peaAdapter;
    private readonly ILoginHelper loginHelper;
    private readonly IPopupService popupService;
    private readonly StorageService storageService;
    private readonly ILogger<MainPageViewModel> logger;

    public MainPageViewModel(ILogger<MainPageViewModel> logger, CustomerProfileViewModel customerProfile,
        AuthDataOptions authDataOptions, PeaAdapter peaAdapter, ILoginHelper loginHelper, IPopupService popupService,
        StorageService storageService)
    {
        this.customerProfile = customerProfile;
        this.authDataOptions = authDataOptions;
        this.peaAdapter = peaAdapter;
        this.loginHelper = loginHelper;
        this.popupService = popupService;
        this.storageService = storageService;
        this.logger = logger;

        CreateAllAggregationsCompletedSubscription();
        CreateUserLoggedInSubscription();
        CreateUserLoggedOutSubscription();
    }

    private void CreateUserLoggedInSubscription()
    {
        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, (r, m) =>
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                IsRemoveMeterVisible = true;
                IsCustomerProfileViewVisible = true;
            });
        });
    }

    private void CreateUserLoggedOutSubscription()
    {
        WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this, (r, m) =>
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    return Task.CompletedTask;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateUserLoggedOutSubscription), e.Message);
                    return Task.FromException(e);
                }
            });
        });
    }
    
    private void CreateAllAggregationsCompletedSubscription()
    {
        WeakReferenceMessenger.Default.Register<AllAggregationsCompletedMessage>(this, async void (_, _) =>
        {
            try
            {
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in {Method}: {Message}", nameof(CreateAllAggregationsCompletedSubscription),
                    e.Message);
            }
        });
    }

    [RelayCommand]
    private async Task RemoveAccount()
    {
        await loginHelper.ClearAuthDataAsync();
    }

    // partial void OnAuthDataChanged(IAuthData? value)
    // {
    //     Debug.WriteLine($"OnAuthDataChanged called: value={(value == null ? "null" : $"{value.Username}")}");
    //
    //     IsRemoveMeterVisible = value != null;
    //     Debug.WriteLine($"{nameof(IsRemoveMeterVisible)}={IsRemoveMeterVisible}");
    //
    //     IsCustomerProfileViewVisible = value != null;
    //     Debug.WriteLine($"{nameof(IsCustomerProfileViewVisible)}={IsCustomerProfileViewVisible}");
    // }

    [ObservableProperty] private bool isRemoveMeterVisible = false;
    [ObservableProperty] private bool isCustomerProfileViewVisible = false;
}
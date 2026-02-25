using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Meter.Helper;
using Pea.Meter.Models;
using Pea.Meter.Services;
using Debug = System.Diagnostics.Debug;

namespace Pea.Meter.ViewModel;

public partial class MainPageViewModel : ObservableObject
{
    private readonly CustomerProfileViewModel customerProfile;
    private readonly AuthDataOptions authDataOptions;
    private readonly PeaAdapter peaAdapter;
    private readonly ILoginHelper loginHelper;
    private readonly IPopupService popupService;

    public MainPageViewModel(CustomerProfileViewModel customerProfile, AuthDataOptions authDataOptions, PeaAdapter peaAdapter, ILoginHelper loginHelper, IPopupService popupService)
    {
        this.customerProfile = customerProfile;
        this.authDataOptions = authDataOptions;
        this.peaAdapter = peaAdapter;
        this.loginHelper = loginHelper;
        this.popupService = popupService;
        
        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, (r, m) =>
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                IsRemoveMeterVisible = true;
                IsCustomerProfileViewVisible = true;
            }); 
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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Meter.Helper;
using Pea.Infrastructure.Models;
using Pea.Meter.Models;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

/// <summary>
/// ViewModel that wraps CustomerProfile model and provides UI logic
/// </summary>
public partial class CustomerProfileViewModel : ObservableObject
{
    [ObservableProperty] private CustomerProfile? customerProfile;

    [ObservableProperty] private bool isLoading;

    [ObservableProperty] private string? errorMessage;

    private readonly PeaAdapter peaAdapter;
    private readonly ILoginHelper loginHelper;

    /// <summary>
    /// ViewModel that wraps CustomerProfile model and provides UI logic
    /// </summary>
    public CustomerProfileViewModel(PeaAdapter peaAdapter, ILoginHelper loginHelper)
    {
        this.peaAdapter = peaAdapter;
        this.loginHelper = loginHelper;

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    // await peaAdapter.CustomerOverviewSelect();
                });
            });
    }

    [RelayCommand]
    private async Task RemoveAccount()
    {
        await loginHelper.ClearAuthDataAsync();
        WeakReferenceMessenger.Default.Send(new UserLoggedOutMessage());
    }

    private async Task LoadCustomerProfile(string user, string pwd)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Ensure login and profile data is loaded
            await peaAdapter.LoginUser(user, pwd);

            // Get the complete customer profile model
            CustomerProfile = peaAdapter.GetCustomerProfileModel();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load customer profile: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }


    public async Task RefreshProfile(string user, string pwd)
    {
        await LoadCustomerProfile(user, pwd);
    }
}
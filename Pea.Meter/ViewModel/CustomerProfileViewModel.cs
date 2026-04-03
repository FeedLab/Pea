using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Meter.Helpers;
using Pea.Infrastructure.Models;
using Pea.Meter.Helpers;
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
    private readonly PeaDbContextFactory dbContextFactory;
    private readonly HistoricDataBackgroundService historicDataBackgroundService;
    private string userName;

    /// <summary>
    /// ViewModel that wraps CustomerProfile model and provides UI logic
    /// </summary>
    public CustomerProfileViewModel(PeaAdapter peaAdapter, ILoginHelper loginHelper,
        PeaDbContextFactory dbContextFactory, HistoricDataBackgroundService historicDataBackgroundService)
    {
        this.peaAdapter = peaAdapter;
        this.loginHelper = loginHelper;
        this.dbContextFactory = dbContextFactory;
        this.historicDataBackgroundService = historicDataBackgroundService;

        WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this,
            (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    userName = m.AuthData.Username;
                    // await peaAdapter.CustomerOverviewSelect();
                });
            });
    }

    [RelayCommand]
    private async Task RemoveAccount()
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var repository = new MeterReadingRepository(dbContext);

        await repository.DeleteAllAsync();
        await loginHelper.ClearAuthDataAsync();
        historicDataBackgroundService.Stop();
        WeakReferenceMessenger.Default.Send(new UserLoggedOutMessage());
    }

    private async Task<bool> LoadCustomerProfile(string user, string pwd)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Ensure login and profile data is loaded
            var isAuthenticated = await peaAdapter.LoginUser(user, pwd);

            if (!isAuthenticated)
            {
                return false;
            }

            // Get the complete customer profile model
            CustomerProfile = peaAdapter.GetCustomerProfileModel();

            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load customer profile: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }

        return false;
    }


    public async Task<bool> RefreshProfile(string user, string pwd)
    {
        return await LoadCustomerProfile(user, pwd);
    }
}
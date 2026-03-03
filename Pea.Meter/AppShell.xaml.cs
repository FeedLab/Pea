using CommunityToolkit.Mvvm.Messaging;
using Pea.Meter.Helper;
using Pea.Meter.Models;
using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            var storageService  = AppService.Current.GetRequiredService<StorageService>();
            var authDataOptions = AppService.Current.GetRequiredService<AuthDataOptions>();
            var customerProfile = AppService.Current.GetRequiredService<CustomerProfileViewModel>();

            var authData = authDataOptions.AuthData;

            _ = InitializeAsync(authData, customerProfile, storageService);
        }

        private async Task InitializeAsync(IAuthData? authData, CustomerProfileViewModel customerProfile, StorageService storageService)
        {
            await InitializeUserSession(authData, customerProfile, storageService);
            await GoToAsync("//MainPage");
        }

        private static async Task InitializeUserSession(IAuthData? authData, CustomerProfileViewModel customerProfile,
            StorageService storageService)
        {
            if (authData is not null && authData.Username != "" && authData.Password != "")
            {
                storageService.IsAuthenticated = await customerProfile.RefreshProfile(authData.Username, authData.Password);

                if (storageService.IsAuthenticated)
                {
                    await storageService.Init();
                }

                //        WeakReferenceMessenger.Default.Send(new UserLoggedInMessage(authData));
            }
        }
    }
}

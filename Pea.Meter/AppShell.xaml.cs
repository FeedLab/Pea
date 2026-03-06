using Pea.Meter.Helper;
using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            var storageService = AppService.Current.GetRequiredService<StorageService>();
            var authDataOptions = AppService.Current.GetRequiredService<AuthDataOptions>();
            var customerProfile = AppService.Current.GetRequiredService<CustomerProfileViewModel>();
            var historicDataBackgroundService = AppService.Current.GetRequiredService<HistoricDataBackgroundService>();

            var authData = authDataOptions.AuthData;

            _ = InitializeAsync(authData, customerProfile, storageService, historicDataBackgroundService);
        }

        private async Task InitializeAsync(IAuthData? authData, CustomerProfileViewModel customerProfile,
            StorageService storageService, HistoricDataBackgroundService historicDataBackgroundService)
        {
            await InitializeUserSession(authData, customerProfile, storageService, historicDataBackgroundService);
            await GoToAsync("//MainPage");
        }

        private static async Task InitializeUserSession(IAuthData? authData, CustomerProfileViewModel customerProfile,
            StorageService storageService, HistoricDataBackgroundService historicDataBackgroundService)
        {
            if (authData is not null && authData.Username != "" && authData.Password != "")
            {
                storageService.IsAuthenticated =
                    await customerProfile.RefreshProfile(authData.Username, authData.Password);

                if (storageService.IsAuthenticated)
                {

                    await storageService.Init();
                
                    historicDataBackgroundService.TriggerImport();
                }
            }
        }
    }
}
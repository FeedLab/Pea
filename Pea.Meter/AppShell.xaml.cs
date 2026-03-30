using Pea.Meter.Helpers;
using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            var storageService = AppService.GetRequiredService<StorageService>();
            var authDataOptions = AppService.GetRequiredService<AuthDataOptions>();
            var customerProfile = AppService.GetRequiredService<CustomerProfileViewModel>();
            var historicDataBackgroundService = AppService.GetRequiredService<HistoricDataBackgroundService>();

            // Set title from current culture (after StorageService initializes the culture)
            Title = Pea.Meter.Resources.Strings.AppResources.MainTitle;

            var authData = authDataOptions.AuthData;

            // Ensure LoadingPage is displayed before starting initialization
            Dispatcher.Dispatch(async void () =>
            {
                try
                {
                    await InitializeAsync(authData, customerProfile, storageService, historicDataBackgroundService);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        private async Task InitializeAsync(IAuthData? authData, CustomerProfileViewModel customerProfile,
            StorageService storageService, HistoricDataBackgroundService historicDataBackgroundService)
        {
            // LoadingPage is shown by default (first ShellContent in AppShell.xaml)
            await InitializeUserSession(authData, customerProfile, storageService, historicDataBackgroundService);
            
            // Navigate to MainPage after initialization completes
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
                }
            }
        }
    }
}
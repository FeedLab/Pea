using Microsoft.Extensions.Logging;
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
            var logger = AppService.GetRequiredService<ILogger<AppShell>>();

            // Set title from current culture (after StorageService initializes the culture)
            Title = Pea.Meter.Resources.Strings.AppResources.MainTitle;

            var authData = authDataOptions.AuthData;

            // Ensure LoadingPage is displayed before starting initialization
            Dispatcher.Dispatch(async void () =>
            {
                try
                {
                    await InitializeAsync(authData, customerProfile, storageService);
                    
                    if (storageService.IsAuthenticated)
                    {
                        await Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Delay(1000);
                                await storageService.Init();
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, "Error in {Method}: {Message}", nameof(OnAppearing), e.Message);
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        private async Task InitializeAsync(IAuthData? authData, CustomerProfileViewModel customerProfile,
            StorageService storageService)
        {
            if (authData is not null && authData.Username != "" && authData.Password != "")
            {
                storageService.IsAuthenticated =
                    await customerProfile.RefreshProfile(authData.Username, authData.Password);
            }
            
            // Navigate to MainPage after initialization completes
            await GoToAsync("//MainPage");
        }
    }
}
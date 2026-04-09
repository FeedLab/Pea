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

            var peaAdapterRouter = (PeaAdapterRouter)AppService.GetRequiredService<IPeaAdapter>();
            var storageService = AppService.GetRequiredService<StorageService>();
            var authDataOptions = AppService.GetRequiredService<AuthDataOptions>();
            var customerProfile = AppService.GetRequiredService<CustomerProfileViewModel>();
            var logger = AppService.GetRequiredService<ILogger<AppShell>>();

            // Set title from current culture (after StorageService initializes the culture)
            Title = Pea.Meter.Resources.Strings.AppResources.MainTitle;

            var authData = authDataOptions.AuthData;

            if (authData is not null && authData.Username == "demo" && authData.Password == "demo")
            {
                peaAdapterRouter.UseDemo(true);
            }
            else
            {
                peaAdapterRouter.UseDemo(false);
            }
            
            // Ensure LoadingPage is displayed before starting initialization
            Dispatcher.Dispatch(async void () =>
            {
                try
                {
                    var startTime = DateTime.UtcNow;

                    await InitializeAsync(authData, customerProfile, storageService);

                    // Ensure LoadingPage is visible for at least 5 seconds
                    var elapsed = DateTime.UtcNow - startTime;
                    var remaining = TimeSpan.FromSeconds(5) - elapsed;
                    if (remaining > TimeSpan.Zero)
                        await Task.Delay(remaining);

                    await GoToAsync("//MainPage");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error in {Method}: {Message}", nameof(InitializeAsync), e.Message);
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
        }
    }
}
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Infrastructure.Repositories;
using Pea.Meter.Helper;
using Pea.Meter.Services;
using Pea.Meter.View;
using Pea.Meter.ViewModel;
using Syncfusion.Maui.Core.Hosting;

namespace Pea.Meter
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF1cWGhIfEx1RHxQdld5ZFRHallYTnNWUj0eQnxTdENjW31ecnRRT2BYUEZxXUleYQ==");
            
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font Awesome 7 Free-Solid-900.otf", "FontSolid");
                    fonts.AddFont("Font Awesome 7 Free-Regular-400.otf", "FontRegular");
                    fonts.AddFont("Roboto-Regular.ttf", "Roboto-Regular");
                });

            // Register services
            builder.Services.AddSingleton<IEncryptionHelper, EncryptionHelper>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<PeaAdapter>();
            builder.Services.AddSingleton<HistoricDataImportService>();
            builder.Services.AddSingleton<HistoricDataBackgroundService>();

            // Register database services
            builder.Services.AddSingleton<PeaDbContextFactory>(sp =>
            {
                // SQLite connection string - uses app data directory
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "pea.db");
                var connectionString = $"Data Source={dbPath}";
                return new PeaDbContextFactory(connectionString);
            });

            // Note: PeaDbContext and repositories should be created on-demand after user login
            // using PeaDbContextFactory.CreateDbContext(userId)
            // Don't register PeaDbContext or repositories in DI at startup since they require a userId

            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddSingleton<CustomerProfileViewModel>();
            builder.Services.AddSingleton<StatisticsViewModel>();
            builder.Services.AddSingleton<PeaServicesViewModel>();

            builder.Services.AddSingletonPopup<LoginPopup, LoginPopupViewModel>();
            
        
            // Configure Options - load synchronously from SettingsService
            builder.Services.AddSingleton<AuthDataOptions>(sp =>
            {
                var settingsService = sp.GetRequiredService<ISettingsService>();
                var authData = settingsService.LoadAuthDataAsync().GetAwaiter().GetResult();
                return new AuthDataOptions { AuthData = authData };
            });

            builder.Services.AddSingleton<ILoginHelper, LoginHelper>();
            builder.Services.AddSingleton<InfoViewModel>();
#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

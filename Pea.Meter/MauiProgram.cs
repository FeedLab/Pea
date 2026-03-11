using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Meter.Helper;
using Pea.Meter.Services;
using Pea.Meter.View;
using Pea.Meter.ViewModel;
using Pea.Meter.ViewModel.Statistics;
using Polly;
using Refit;
using Serilog;
using Syncfusion.Maui.Core.Hosting;
using MeterReadingsHourViewModel = Pea.Meter.ViewModel.Statistics.MeterReadingsHourViewModel;

namespace Pea.Meter
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF1cWGhIfEx1RHxQdld5ZFRHallYTnNWUj0eQnxTdENjW31ecnRRT2BYUEZxXUleYQ==");

            // Configure Serilog
            var logPath = Path.Combine(FileSystem.AppDataDirectory, "logs", "pea.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();

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

            // Add Serilog to the logging pipeline
            builder.Logging.AddSerilog(dispose: true);

            // Register services
            builder.Services.AddSingleton<IEncryptionHelper, EncryptionHelper>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<PeaAdapter>();
            builder.Services.AddSingleton<HistoricDataImportService>();
            builder.Services.AddSingleton<HistoricDataBackgroundService>();
            builder.Services.AddSingleton<StorageService>();

            // Register database services
            builder.Services.AddSingleton<PeaDbContextFactory>(_ =>
            {
                // SQLite connection string - uses app data directory
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "pea.db");
                var connectionString = $"Data Source={dbPath}";
                return new PeaDbContextFactory(connectionString);
            });

            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddSingleton<CustomerProfileViewModel>();
            builder.Services.AddSingleton<StatisticsViewModel>();
            builder.Services.AddSingleton<MeterReadingsHourViewModel>();
            builder.Services.AddSingleton<PeaServicesViewModel>();
            builder.Services.AddSingleton<CustomerInfoViewModel>();
            builder.Services.AddSingleton<TouVsFlatRateViewModel>();
            builder.Services.AddSingleton<MeterReadingsDailyViewModel>();
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<SolarSystemSizingViewModel>();
            
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
            
            // builder.Services.AddRefitClient<IFooApi>()
            //     .ConfigureHttpClient((sp, client) =>
            //     {
            //         client.BaseAddress = new Uri($"https://api.openweathermap.org");
            //     //    client.DefaultRequestHeaders.Add("x-api-key", ["c3d47c3c9326cf557f14aa414f0bb984"]);
            //     })
            //     .AddHttpMessageHandler(() => new ApiKeyHandler("c3d47c3c9326cf557f14aa414f0bb984"))
            //     .AddStandardResilienceHandler(options =>
            //     {
            //         // Customize retry strategy
            //         options.Retry.MaxRetryAttempts = 2;
            //         options.Retry.BackoffType = DelayBackoffType.Exponential;
            //         options.Retry.Delay = TimeSpan.FromSeconds(2);
            //         options.Retry.UseJitter = true;
            //     });
#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

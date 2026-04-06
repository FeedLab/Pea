using Akavache;
using Akavache.Sqlite3;
using Akavache.SystemTextJson;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Meter.Helpers;
using Pea.Meter.Models;
using Pea.Meter.Services;
using Pea.Meter.View;
using Pea.Meter.ViewModel;
using Pea.Meter.ViewModel.Statistics;
using Serilog;
using Splat;
using Splat.Builder;
using Syncfusion.Maui.Core.Hosting;
using MeterReadingsHourViewModel = Pea.Meter.ViewModel.Statistics.MeterReadingsHourViewModel;

namespace Pea.Meter
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JHaF5cWWdCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWXxccXZVQ2FeVEV/XUdWYEo=");

            Console.WriteLine($@"Idiom: {DeviceInfo.Idiom}");
            
            // Configure Serilog
            var memorySink = new MemoryLogSink();
            var logPath = Path.Combine(FileSystem.AppDataDirectory, "logs", "pea.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .WriteTo.Sink(memorySink)
                .CreateLogger();

            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSansRegular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSansSemibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("FontAwesome7FreeSolid900.otf", "FontSolid");
                    fonts.AddFont("FontAwesome7FreeRegular400.otf", "FontRegular");
                    fonts.AddFont("FontAwesome7BrandsRegular400.otf", "FontBrands");
                    fonts.AddFont("RobotoRegular.ttf", "RobotoRegular");
                });

            // Add Serilog to the logging pipeline
            builder.Logging.AddSerilog(dispose: true);

            AppBuilder.CreateSplatBuilder()
                .WithAkavacheCacheDatabase<SystemJsonSerializer>(b =>
                    b.WithApplicationName("PeaSmartMeter")
                        .WithSqliteProvider()
                        .WithSqliteDefaults());
            
            // Register services
            builder.Services.AddSingleton<IEncryptionHelper, EncryptionHelper>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<PeaAdapter>();
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
            builder.Services.AddSingleton<CustomerInfoViewModel>();
            builder.Services.AddSingleton<TouVsFlatRateViewModel>();
            builder.Services.AddSingleton<MeterReadingsDailyViewModel>();
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<SolarSystemSizingViewModel>();
            
            builder.Services.AddSingletonPopup<LoginPopup, LoginPopupViewModel>();
            builder.Services.AddSingleton<NewDayBackgroundTimer>();
            builder.Services.AddSingleton<DailyPeaReadingsTimer>();

            builder.Services.AddSingleton<ILoginHelper, LoginHelper>();
            builder.Services.AddSingleton<InfoViewModel>();
            builder.Services.AddSingleton(memorySink);
            builder.Services.AddSingleton<LogViewModel>();
            
            // Configure Options - load synchronously from SettingsService
            builder.Services.AddSingleton<AuthDataOptions>(sp =>
            {
                var settingsService = sp.GetRequiredService<ISettingsService>();
                var authData = Task.Run(settingsService.LoadAuthDataAsync).GetAwaiter().GetResult();
                return new AuthDataOptions { AuthData = authData };
            });
            
            builder.Services.AddSingleton<ConfigurationTariffModel>(sp =>
            {
                var model = new ConfigurationTariffModel();
                Task.Run(model.Load).GetAwaiter().GetResult();
                return model;
            });
            builder.Services.AddSingleton<ConfigurationLanguageModel>(sp =>
            {
                var model = new ConfigurationLanguageModel();
                Task.Run(model.Load).GetAwaiter().GetResult();
                return model;
            });
            builder.Services.AddSingleton<ConfigurationDataImportModel>(sp =>
            {
                var model = new ConfigurationDataImportModel();
                Task.Run(model.Load).GetAwaiter().GetResult();
                return model;
            });
            
#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            return app;
        }
    }
}

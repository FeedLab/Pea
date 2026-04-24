# Pea

A .NET MAUI cross-platform application for tracking and analyzing electricity meter readings from PEA (Provincial Electricity Authority) in Thailand. The app provides detailed statistics, cost comparisons, and solar system sizing calculations.

## Overview

Pea is a mobile application built with .NET MAUI (Multi-platform App UI) that helps users monitor their electricity consumption patterns, compare Time-of-Use (TOU) vs Flat Rate pricing, and analyze solar energy potential. It supports a Demo mode (no PEA account required) and a Live mode connected to the PEA API.

## Tech Stack

- **.NET 10.0** - Latest .NET framework
- **.NET MAUI** - Cross-platform UI framework (Android, iOS, Windows, macOS)
- **C#** - Primary programming language
- **Entity Framework Core 10.0.5** - ORM for database operations
- **SQLite** - Local database storage
- **MVVM Pattern** - Using CommunityToolkit.Mvvm
- **Akavache** - Async key-value store / cache (SQLite-backed)
- **Syncfusion** - UI components (Charts, Buttons, Expander, Picker, Sliders, TabView)
- **Refit** - REST API client
- **Serilog** - Logging framework with in-memory sink for log viewer
- **HtmlAgilityPack** - HTML parsing
- **MailKit** - Email support
- **Localization** - English and Swedish (AppResources.resx / AppResources.se.resx)

## Location for country flags
https://github.com/lipis/flag-icons/blob/main/flags/4x3/fr.svg

## Project Structure

```
Pea/
├── Pea.Meter/                  # Main MAUI application
│   ├── View/                   # XAML views and pages
│   │   ├── Statistics/         # Statistics-related views
│   │   │   ├── StatisticsView.xaml
│   │   │   ├── MeterReadingsHourView.xaml
│   │   │   ├── MeterReadingsHourChart.xaml
│   │   │   └── MeterReadingsDailyView.xaml
│   │   ├── Solar/              # Solar system views
│   │   │   └── SolarSystemSizingView.xaml
│   │   ├── Components/         # Reusable UI components
│   │   │   ├── Configuration/  # Language, tariff, data import, solar array config
│   │   │   ├── TouVsFlatRateView/  # TOU and flat rate result displays
│   │   │   ├── CustomerInfoView.xaml
│   │   │   ├── CustomerInfoPopupView.xaml
│   │   │   ├── PeaServicesView.xaml
│   │   │   ├── MeterReadingsDailyChart.xaml
│   │   │   ├── CompassDirectionComponent.xaml
│   │   │   ├── SliderComponent.xaml
│   │   │   ├── SwitchComponent.xaml
│   │   │   ├── DatePickerComponent.xaml
│   │   │   ├── LabeledBorderComponent.xaml
│   │   │   ├── ServiceBoxComponent.xaml
│   │   │   ├── ToolbarComponent.xaml
│   │   │   ├── IconButton.xaml
│   │   │   └── HomeRegisterDemoAccountWaitPopup.xaml
│   │   ├── Popup/              # Modal dialogs
│   │   │   ├── ErrorPopup.xaml
│   │   │   └── QuitPopup.xaml
│   │   ├── HomeView.xaml
│   │   ├── CustomerProfileView.xaml
│   │   ├── LoginPopup.xaml
│   │   ├── ErrorMessagePopup.xaml
│   │   ├── InfoView.xaml
│   │   ├── LogView.xaml        # In-app log viewer
│   │   ├── TouVsFlatRateView.xaml
│   │   ├── LoadingPage.xaml
│   │   └── MainPage_NotSupported.xaml
│   ├── ViewModel/              # View models (MVVM pattern)
│   │   ├── Statistics/         # Statistics view models
│   │   ├── Interface/          # ICanExecuteViewModel
│   │   └── ...
│   ├── Services/               # Business logic and API services
│   ├── Models/                 # Data models and messages
│   ├── Helper/                 # Helper classes
│   ├── Extension/              # Extension methods
│   └── Resources/              # Images, fonts, styles, localization
│       ├── Styles/             # Colors.xaml, Styles.xaml, Pea.xaml
│       ├── Fonts/              # FontAwesome 7, OpenSans, Roboto
│       └── AppResources.resx   # Localization (en, sv)
│
├── Pea.Data/                   # Data access layer
│   ├── Entities/               # Database entities
│   ├── Repositories/           # Repository pattern implementations
│   ├── Migrations/             # EF Core migrations
│   └── PeaDbContext.cs         # Database context
│
├── Pea.Infrastructure/         # Infrastructure layer
│   ├── Models/                 # Domain models
│   │   └── MeterData/          # Hierarchical meter data management
│   ├── Repositories/           # Repository interfaces
│   ├── Helpers/                # MathHelpers, WattFormatter
│   └── Extensions/             # StringExtension, WeekendAndHolidayExtension
│
├── Pea.Meter.Tests/            # Unit and integration tests (81+ tests)
│   ├── CostCompareTests.cs
│   ├── MeterDataManagerTests.cs
│   ├── MeterDataManagerIntegrationTests.cs
│   ├── MeterDataManagerFilterLevelTests.cs
│   ├── MeterDataUsageSummaryTests.cs
│   ├── PvCalculatorServiceTests.cs
│   └── MeterDataManager_TestSummary.md
│
└── Pea.DesignTime/             # Design-time support
    └── Program.cs              # EF Core design-time factory
```

## Project Dependencies

### Pea.Meter (Main Application)
**Project References:**
- `Pea.Infrastructure` - Domain models and repository interfaces
- `Pea.Data` - Data access and database context

**NuGet Packages:**
- UI Framework: `Microsoft.Maui.Controls` (10.0.51)
- MVVM: `CommunityToolkit.Maui` (14.0.1), `CommunityToolkit.Maui.Markup` (7.0.1), `CommunityToolkit.Mvvm` (8.4.2)
- Database: `Microsoft.EntityFrameworkCore` (10.0.5), `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`
- Caching: `Akavache.Sqlite3` (11.5.1), `Akavache.SystemTextJson` (11.5.1)
- HTTP: `Refit` (10.0.1), `Refit.HttpClientFactory`, `Microsoft.Extensions.Http.Resilience` (10.0.3)
- Logging: `Serilog.Extensions.Logging` (10.0.0), `Serilog.Sinks.Console` (6.1.1), `Serilog.Sinks.File` (7.0.0), `Microsoft.Extensions.Logging.Debug` (10.0.5)
- UI Components: `Syncfusion.Maui.Buttons` (33.1.45), `Syncfusion.Maui.Charts` (33.1.45), `Syncfusion.Maui.Expander` (33.1.47), `Syncfusion.Maui.Picker` (33.1.45), `Syncfusion.Maui.Sliders` (33.1.45), `Syncfusion.Maui.TabView` (33.1.45)
- Configuration: `Microsoft.Extensions.Configuration.Binder` (10.0.5), `Microsoft.Extensions.Configuration.Json` (10.0.5), `Microsoft.Extensions.Configuration.UserSecrets` (10.0.5), `Microsoft.Extensions.Hosting` (10.0.5), `Microsoft.Extensions.Options` (10.0.5)
- Utilities: `HtmlAgilityPack.NetCore` (1.5.0.1), `ObservableCollections` (3.3.4), `MailKit` (4.9.0)

### Pea.Data (Data Access Layer)
**Project References:**
- `Pea.Infrastructure` - Domain models and repository interfaces

**NuGet Packages:**
- `Microsoft.EntityFrameworkCore.Sqlite` (10.0.5) - SQLite database provider
- `Microsoft.EntityFrameworkCore.Design` (10.0.5) - Design-time tools
- `Microsoft.EntityFrameworkCore.Tools` (10.0.5) - CLI tools for migrations
- `CommunityToolkit.Maui.Markup` (7.0.1)
- `Syncfusion.Maui.Picker` (33.1.45)

### Pea.Infrastructure (Domain Layer)
**NuGet Packages:**
- `CommunityToolkit.Maui.Markup` (7.0.1)
- `CommunityToolkit.Mvvm` (8.4.2) - MVVM helpers
- `Microsoft.EntityFrameworkCore.Design` (10.0.5) - EF Core design-time support
- `Microsoft.EntityFrameworkCore.Tools` (10.0.5) - Migration tools

### Pea.Meter.Tests (Unit Tests)
**Project References:**
- `Pea.Meter` - Main application
- `Pea.Data` - Data layer
- `Pea.Infrastructure` - Domain layer

**NuGet Packages:**
- `xunit` (2.9.3) - Testing framework
- `xunit.runner.visualstudio` (3.1.5) - Visual Studio test runner
- `Microsoft.NET.Test.Sdk` (18.3.0) - Test SDK
- `FluentAssertions` (8.9.0) - Fluent assertion library
- `coverlet.collector` (8.0.1) - Code coverage collector
- `CommunityToolkit.Maui.Markup` (7.0.1)

### Dependency Graph
```
Pea.Meter (Main App)
├── Pea.Infrastructure (Domain)
└── Pea.Data (Data Access)
    └── Pea.Infrastructure (Domain)

Pea.Meter.Tests
├── Pea.Meter
├── Pea.Data
└── Pea.Infrastructure
```

## Features

### Core Features
- **Meter Reading Tracking** - Hourly and daily electricity consumption monitoring
- **Statistics & Analytics** - Comprehensive usage statistics with interactive charts
- **TOU vs Flat Rate Comparison** - Cost analysis for different pricing models
- **Solar System Sizing** - Calculate optimal solar panel system size
- **Customer Profile Management** - Manage PEA account information
- **Historic Data Import** - Import and analyze historical meter data
- **Secure Storage** - Encrypted credential storage
- **Demo Mode** - Full app experience without a real PEA account (`PeaAdapterDemo`)
- **Log Viewer** - In-app log display via `LogView` / `MemoryLogSink`
- **Localization** - English and Swedish language support
- **Caching** - Akavache SQLite-backed async key-value cache

### View Models
- `MainPageViewModel` - App startup / loading logic
- `HomeViewModel` - Dashboard and home screen
- `CustomerProfileViewModel` - User profile management
- `CustomerInfoViewModel` - Customer information display
- `LoginPopupViewModel` - Login popup authentication
- `ErrorMessagePopupViewModel` - Error display popup
- `StatisticsViewModel` - Overall statistics
- `MeterReadingsHourViewModel` - Hourly meter reading analysis
- `MeterReadingsDailyViewModel` - Daily consumption patterns
- `TouVsFlatRateViewModel` - Rate comparison calculations
- `SolarSystemSizingViewModel` - Solar panel sizing calculator
- `PeaServicesViewModel` - PEA services integration
- `InfoViewModel` - App information and settings
- `LogViewModel` - In-app log viewer

### Services
- `IPeaAdapter` / `PeaAdapterRouter` - Adapter interface and router (selects Live or Demo)
- `PeaAdapterLive` - Live PEA API integration
- `PeaAdapterDemo` - Demo mode data source (no account required)
- `AppService` - Central application service
- `SettingsService` - Application settings management
- `StorageService` - Local data storage
- `HistoricDataImportService` - Import historical meter data
- `HistoricDataBackgroundService` - Background data synchronization
- `DailyPeaReadingsTimer` - Background timer triggering daily PEA reads
- `NewDayBackgroundTimer` - Background timer for new-day cycle management
- `SolarDataService` - Solar calculations and data retrieval
- `SolarIrradianceService` - Computes solar irradiance from position/time
- `MemoryLogSink` - In-memory Serilog sink feeding the in-app log viewer

### Infrastructure (Pea.Infrastructure)
- `PvCalculatorService` - High-precision photovoltaic power estimation
  - Solar position via declination, equation of time, and hour angle
  - Panel azimuth/tilt angle modelling
  - Thailand defaults: lat `15.274053`, lon `102.622572`, solar constant `1361 W/m²`
  - System efficiency `68%` (inverter + wiring + temperature + soiling + mismatch losses)
  - Results cached in `ConcurrentDictionary`
- `MeterDataManager` hierarchy - Hierarchical aggregation: Year → Month → Day → Hour → Quarter
- `WeekendAndHolidayExtension` - Thailand-specific weekend and public holiday detection
- `WattFormatter` - Power/wattage display formatting
- `MathHelpers` - Shared math utilities

## Database

The application uses **SQLite** with Entity Framework Core for local data persistence.

### Schema

**MeterReadings** table (`MeterReadingEntity`):

| Column       | Type           | Constraints                    |
|--------------|----------------|--------------------------------|
| Id           | int            | Primary Key, Auto-increment    |
| MeterNumber  | string(50)     | Required                       |
| PeriodStart  | DateTime       | Required                       |
| PeriodEnd    | DateTime       | Required                       |
| RateA        | decimal(18,4)  | Required – Rate A consumption  |
| RateB        | decimal(18,4)  | Required – Rate B consumption  |
| RateC        | decimal(18,4)  | Required – Rate C consumption  |
| Total        | decimal(18,4)  | Required – Total consumption   |
| CreatedAt    | DateTime       | Default: `DateTime.UtcNow`     |
| UpdatedAt    | DateTime?      | Nullable                       |

**Unique index**: `(MeterNumber, PeriodStart)` — prevents duplicate readings per meter per time slot.

### Repository Methods (`MeterReadingRepository`)
- `AddRangeAsync()` — Insert multiple readings
- `AddRangeUpsertAsync()` — Insert or update (upsert)
- `GetAllMeterReadingsAsync()` — Retrieve all readings for a meter
- `HasReadingsForDateAsync()` — Check if readings exist for a date
- `GetOldestPeriodStartAsync()` — Get oldest reading timestamp
- `DeleteBeforeDateAsync()` — Delete readings older than a specified date
- `DeleteAllAsync()` — Delete all readings (meter-filtered or global)

Thread safety is ensured via `SemaphoreSlim`.

### Migrations
| Migration | Description |
|-----------|-------------|
| `20260225231645_Initial` | Initial schema |
| `20260305073031_RemoveUserId` | Removed UserId field |
| `20260407040605_AddMeterNumberToMeterReading` | Added MeterNumber field |
| `20260407041720_UpdateMeterReadingIndex` | Composite unique index on (MeterNumber, PeriodStart) |

## Tests

| Test File | Description |
|-----------|-------------|
| `CostCompareTests.cs` | Cost comparison calculations |
| `MeterDataManagerTests.cs` | 32 unit tests for MeterDataManager |
| `MeterDataManagerIntegrationTests.cs` | 27 integration tests |
| `MeterDataManagerFilterLevelTests.cs` | Hierarchical filter level tests |
| `MeterDataUsageSummaryTests.cs` | Usage summary calculations |
| `PvCalculatorServiceTests.cs` | Solar PV calculation tests |

All 81+ tests pass. See `Pea.Meter.Tests/MeterDataManager_TestSummary.md` for details.

Run tests:
```
dotnet test Pea.Meter.Tests
```

## Platform Support

- **Android** - Minimum API 29 (Android 10.0)
- **iOS** - Minimum version 15.0
- **macOS** - Catalyst version 15.0+
- **Windows** - Windows 10 version 2004 (10.0.19041.0)+

## Application Version

- **Display Version**: 1.0
- **Application Version**: 2

## Key Dependencies

| Package | Version |
|---------|---------|
| Microsoft.Maui.Controls | 10.0.51 |
| Entity Framework Core | 10.0.5 |
| Syncfusion.Maui Components | 33.1.x |
| CommunityToolkit.Maui | 14.0.1 |
| CommunityToolkit.Maui.Markup | 7.0.1 |
| CommunityToolkit.Mvvm | 8.4.2 |
| Akavache.Sqlite3 | 11.5.1 |
| Refit | 10.0.1 |
| Serilog | 10.0.0 |
| MailKit | 4.9.0 |
| ObservableCollections | 3.3.4 |
| FluentAssertions (tests) | 8.9.0 |
| xunit (tests) | 2.9.3 |

## Configuration

The app uses embedded JSON configuration files:
- `appsettings.json` - Production settings
- `appsettings.Development.json` - Development settings

Both currently contain a placeholder for encrypted PEA credentials:
```json
{
  "AuthData": {
    "EncryptedCredentials": ""
  }
}
```

User secrets (`Microsoft.Extensions.Configuration.UserSecrets`) are also supported for local development overrides.

## Logging

Serilog is configured to write logs to:
- Console output for debugging
- Rolling file logs in app data directory (7-day retention) — `{AppDataDirectory}/logs/pea.log`
- **In-memory sink** (`MemoryLogSink`) — feeds the in-app `LogView` screen for on-device log inspection

## Embedded Resources

| Resource | Description |
|----------|-------------|
| `AllMeterReadings.json.gz` | Embedded compressed historical meter data |
| `FontAwesome7BrandsRegular400.otf` | FontAwesome 7 Brands |
| `FontAwesome7FreeRegular400.otf` | FontAwesome 7 Free Regular |
| `FontAwesome7FreeSolid900.otf` | FontAwesome 7 Free Solid |
| `OpenSansRegular.ttf` | OpenSans Regular |
| `OpenSansSemibold.ttf` | OpenSans Semibold |
| `RobotoRegular.ttf` | Roboto Regular |
| `pea_amr_intro_splash.png` | Splash screen (3840×2160) |
| `pea_raw.png` | App icon |

## License

Syncfusion components are used with a registered license.

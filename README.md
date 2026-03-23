# Pea

A .NET MAUI cross-platform application for tracking and analyzing electricity meter readings from PEA (Provincial Electricity Authority) in Thailand. The app provides detailed statistics, cost comparisons, and solar system sizing calculations.

## Overview

Pea is a mobile application built with .NET MAUI (Multi-platform App UI) that helps users monitor their electricity consumption patterns, compare Time-of-Use (TOU) vs Flat Rate pricing, and analyze solar energy potential.

## Tech Stack

- **.NET 10.0** - Latest .NET framework
- **.NET MAUI** - Cross-platform UI framework (Android, iOS, Windows, macOS)
- **C#** - Primary programming language
- **Entity Framework Core 10.0.3** - ORM for database operations
- **SQLite** - Local database storage
- **MVVM Pattern** - Using CommunityToolkit.Mvvm
- **Syncfusion** - UI components (Charts, Buttons, Expander, Picker, TabView)
- **Refit** - REST API client
- **Serilog** - Logging framework
- **HtmlAgilityPack** - HTML parsing


## Location for country flags
https://github.com/lipis/flag-icons/blob/main/flags/4x3/fr.svg

## Project Structure

```
Pea/
├── Pea.Meter/              # Main MAUI application
│   ├── View/               # XAML views and pages
│   │   ├── Statistics/     # Statistics-related views
│   │   ├── Solar/          # Solar system views
│   │   └── Components/     # Reusable UI components
│   ├── ViewModel/          # View models (MVVM pattern)
│   │   └── Statistics/     # Statistics view models
│   ├── Services/           # Business logic and API services
│   ├── Models/             # Data models
│   ├── Helper/             # Helper classes and utilities
│   ├── Extension/          # Extension methods
│   └── Resources/          # Images, fonts, and assets
│
├── Pea.Data/               # Data access layer
│   ├── Entities/           # Database entities
│   ├── Repositories/       # Repository pattern implementations
│   ├── Migrations/         # EF Core migrations
│   └── PeaDbContext.cs     # Database context
│
├── Pea.Infrastructure/     # Infrastructure layer
│   ├── Models/             # Domain models
│   └── Repositories/       # Repository interfaces
│
├── Pea.Meter.Tests/        # Unit tests
│   └── CostCompareTests.cs # Cost comparison tests
│
└── Pea.DesignTime/         # Design-time support
    └── Program.cs          # EF Core design-time factory
```

## Project Dependencies

### Pea.Meter (Main Application)
**Project References:**
- `Pea.Infrastructure` - Domain models and repository interfaces
- `Pea.Data` - Data access and database context

**NuGet Packages:**
- UI Framework: `Microsoft.Maui.Controls` (10.0.41)
- MVVM: `CommunityToolkit.Maui` (14.0.0), `CommunityToolkit.Mvvm` (8.4.0)
- Database: `Microsoft.EntityFrameworkCore` (10.0.3), `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`
- HTTP: `Refit` (10.0.1), `Refit.HttpClientFactory`, `Microsoft.Extensions.Http.Resilience` (10.0.3)
- Logging: `Serilog.Extensions.Logging` (10.0.0), `Serilog.Sinks.Console` (6.1.1), `Serilog.Sinks.File` (7.0.0), `Microsoft.Extensions.Logging.Debug` (10.0.3)
- UI Components: `Syncfusion.Maui.Buttons` (32.2.5), `Syncfusion.Maui.Charts` (32.2.7), `Syncfusion.Maui.Expander` (32.2.5), `Syncfusion.Maui.Picker` (32.2.7), `Syncfusion.Maui.TabView` (32.2.5)
- Configuration: `Microsoft.Extensions.Configuration.Binder` (10.0.3), `Microsoft.Extensions.Configuration.Json` (10.0.3), `Microsoft.Extensions.Hosting` (10.0.3), `Microsoft.Extensions.Options` (10.0.3)
- Utilities: `HtmlAgilityPack.NetCore` (1.5.0.1), `ObservableCollections` (3.3.4)

### Pea.Data (Data Access Layer)
**Project References:**
- `Pea.Infrastructure` - Domain models and repository interfaces

**NuGet Packages:**
- `Microsoft.EntityFrameworkCore.Sqlite` - SQLite database provider
- `Microsoft.EntityFrameworkCore.Design` - Design-time tools
- `Microsoft.EntityFrameworkCore.Tools` - CLI tools for migrations
- `Syncfusion.Maui.Picker` (32.2.7)

### Pea.Infrastructure (Domain Layer)
**NuGet Packages:**
- `CommunityToolkit.Mvvm` (8.4.0) - MVVM helpers
- `Microsoft.EntityFrameworkCore.Design` - EF Core design-time support
- `Microsoft.EntityFrameworkCore.Tools` - Migration tools
- `Syncfusion.Maui.Picker` (32.2.7)

### Pea.Meter.Tests (Unit Tests)
**Project References:**
- `Pea.Meter` - Main application
- `Pea.Data` - Data layer
- `Pea.Infrastructure` - Domain layer

**NuGet Packages:**
- `xunit` - Testing framework
- `xunit.runner.visualstudio` - Visual Studio test runner
- `Microsoft.NET.Test.Sdk` - Test SDK
- `FluentAssertions` - Fluent assertion library
- `coverlet.collector` - Code coverage collector

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

### View Models
- `HomeViewModel` - Dashboard and home screen
- `CustomerProfileViewModel` - User profile management
- `CustomerInfoViewModel` - Customer information display
- `StatisticsViewModel` - Overall statistics
- `MeterReadingsHourViewModel` - Hourly meter reading analysis
- `MeterReadingsDailyViewModel` - Daily consumption patterns
- `TouVsFlatRateViewModel` - Rate comparison calculations
- `SolarSystemSizingViewModel` - Solar panel sizing calculator
- `PeaServicesViewModel` - PEA services integration
- `InfoViewModel` - App information and settings

### Services
- `PeaAdapter` - PEA API integration
- `SettingsService` - Application settings management
- `StorageService` - Local data storage
- `HistoricDataImportService` - Import historical meter data
- `HistoricDataBackgroundService` - Background data synchronization
- `SolarDataService` - Solar calculations and data

## Database

The application uses **SQLite** with Entity Framework Core for local data persistence:

- **MeterReadings** table stores electricity consumption data:
  - Hourly readings with period start/end timestamps
  - Rate breakdown (Rate A, Rate B, Rate C)
  - Total consumption
  - Audit timestamps (CreatedAt, UpdatedAt)

## Platform Support

- **Android** - Minimum API 29 (Android 10.0)
- **iOS** - Minimum version 15.0
- **macOS** - Catalyst version 15.0+
- **Windows** - Windows 10 version 1809 (10.0.17763.0)+

## Key Dependencies

- Microsoft.Maui.Controls (10.0.41)
- Entity Framework Core (10.0.3)
- Syncfusion.Maui Components (32.2.x)
- CommunityToolkit.Maui (14.0.0)
- CommunityToolkit.Mvvm (8.4.0)
- Refit (10.0.1)
- Serilog (10.0.0)
- ObservableCollections (3.3.4)

## Configuration

The app uses embedded JSON configuration files:
- `appsettings.json` - Production settings
- `appsettings.Development.json` - Development settings

## Logging

Serilog is configured to write logs to:
- Console output for debugging
- Rolling file logs in app data directory (7-day retention)
- Log path: `{AppDataDirectory}/logs/pea.log`

## License

Syncfusion components are used with a registered license.

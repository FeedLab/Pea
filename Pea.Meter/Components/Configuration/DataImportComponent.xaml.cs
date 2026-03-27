using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pea.Data;
using Pea.Data.Repositories;
using Pea.Meter.Services;

namespace Pea.Meter.Components.Configuration;

public partial class DataImportComponent : ContentView
{
    private readonly StorageService storageService;
    private readonly ILogger<DataImportComponent> logger;
    private readonly PeaDbContextFactory dbContextFactory;
    private readonly HistoricDataBackgroundService historicDataBackgroundService;

    public DateTime StartDate { get; set; }
    public DateTime StartTimePickerMinimumDate { get; set; }
    public DateTime StartTimePickerMaximumDate { get; set; }

    public DataImportComponent()
    {
        logger = AppService.GetRequiredService<ILogger<DataImportComponent>>();
        storageService = AppService.GetRequiredService<StorageService>();
        historicDataBackgroundService = AppService.GetRequiredService<HistoricDataBackgroundService>();
        dbContextFactory = AppService.GetRequiredService<PeaDbContextFactory>();

        StartDate = storageService.ConfigurationDataImportModel.EarliestImportedDate;
        StartTimePickerMinimumDate = DateTime.Today.Date.AddYears(-10);
        StartTimePickerMaximumDate = DateTime.Today.Date.AddDays(-1);

        InitializeComponent();
    }

    private void OnStartDateSelected(object? sender, DateTime e)
    {
        storageService.ConfigurationDataImportModel?.EarliestImportedDate = e;
    }

    private async void OnStartImportClicked(object? sender, EventArgs e)
    {
        try
        {
            historicDataBackgroundService.CancelImport();
            logger.LogInformation("Data import has been cancelled");

            using var dbContext = dbContextFactory.CreateDbContext();
            var repository = new MeterReadingRepository(dbContext);
            
            await repository.DeleteBeforeDateAsync(StartDate);
            logger.LogInformation("Data before {StartDate} has been deleted", StartDate);

            await storageService.ResetHistoricalData();
            logger.LogInformation("Historical data has been reset");
            
            historicDataBackgroundService.TriggerImport(false);
            logger.LogInformation("Data import has been triggered");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occurred while deleting data before {StartDate}", StartDate);
        }
    }
}
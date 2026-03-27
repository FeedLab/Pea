using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pea.Meter.Services;

namespace Pea.Meter.Components.Configuration;

public partial class DataImportComponent : ContentView
{
    private readonly StorageService storageService;
    private readonly ILogger<DataImportComponent> logger;
    
    public DateTime StartDate { get; set; }
    public DateTime StartTimePickerMinimumDate { get; set; }
    public DateTime StartTimePickerMaximumDate { get; set; }

    public DataImportComponent()
    {
        
        logger = AppService.GetRequiredService<ILogger<DataImportComponent>>();
        storageService = AppService.GetRequiredService<StorageService>();
        
        StartDate = storageService.ConfigurationDataImportModel.EarliestImportedDate;
        StartTimePickerMinimumDate = DateTime.Today.Date.AddYears(-10);
        StartTimePickerMaximumDate = DateTime.Today.Date.AddDays(-1);
        
        InitializeComponent();
    }

    private void OnStartDateSelected(object? sender, DateTime e)
    {
        storageService.ConfigurationDataImportModel?.EarliestImportedDate = e;
    }
}
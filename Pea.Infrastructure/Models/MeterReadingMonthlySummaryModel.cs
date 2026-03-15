﻿using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pea.Infrastructure.Models;

/// <summary>
/// Model representing a monthly summary of meter readings aggregated by peak and off-peak periods
/// </summary>
[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class MeterReadingMonthlySummary : ObservableObject
{
    /// <summary>
    /// The first day of the month for this summary
    /// </summary>
    [ObservableProperty] private DateTime date;

    /// <summary>
    /// Total kilowatt-hours used during peak period
    /// </summary>
    [ObservableProperty] private decimal kwUsedAtPeek;

    /// <summary>
    /// Total kilowatt-hours used during off-peak period
    /// </summary>
    [ObservableProperty] private decimal kwUsedAtOffPeek;

    /// <summary>
    /// Total kilowatt-hours used (peak + off-peak)
    /// </summary>
    [ObservableProperty] private decimal kwUsedTotal;

    /// <summary>
    /// Average kilowatt-hours used between 8am and 5pm
    /// </summary>
    [ObservableProperty] private decimal averageKwUsedBetween08To17Monthly;
    
    /// <summary>
    /// Average kilowatt-hours used at peak period per record
    /// </summary>
    [ObservableProperty] private decimal averageKwUsedAtPeekPerRecord;

    /// <summary>
    /// Average kilowatt-hours used at off-peak period per record
    /// </summary>
    [ObservableProperty] private decimal averageKwUsedAtOffPeekPerRecord;

    /// <summary>
    /// Average kilowatt-hours used at peak period per day
    /// </summary>
    [ObservableProperty] private decimal averageKwUsedAtPeekPerDay;

    /// <summary>
    /// Average kilowatt-hours used at off-peak period per day
    /// </summary>
    [ObservableProperty] private decimal averageKwUsedAtOffPeekPerDay;
    
    /// <summary>
    /// Total kilowatt-hours produced per month from solar cells
    /// </summary>
    [ObservableProperty] private decimal kwProducedPerMonth;
    
    
    /// <summary>
    /// Total kilowatt-hours produced (Calculated) per day from solar cells
    /// </summary>
    [ObservableProperty] private decimal calculateProducedSolarKwDaily;

    /// <summary>
    /// Total kilowatt-hours produced (Calculated) per month from solar cells
    /// </summary>
    [ObservableProperty] private decimal calculateProducedSolarKwMonthly;


    /// <summary>
    /// Battery size selected
    /// </summary>
    [ObservableProperty] private decimal batterySize;
    
    /// <summary>
    /// Solar array size selected
    /// </summary>
    [ObservableProperty] private decimal solarArraySize;
    
    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private decimal batteryKwProducedMonthly;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private decimal costSummaryTouPeek;
    
    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private decimal costSummaryTouOffPeek;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private decimal costSummaryFlatRate;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private decimal costSummaryTouTotal;
    
    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private decimal peekTouDiscounted;
    
    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private decimal offPeekTouDiscounted;
    
    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private decimal flatRateDiscounted;
    
    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private decimal peekTouSaving;
    
    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private decimal offPeekTouSaving;
    
    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private decimal flatRateSaving;
}

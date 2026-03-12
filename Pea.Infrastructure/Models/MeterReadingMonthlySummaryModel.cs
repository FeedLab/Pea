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
    [ObservableProperty] private decimal total;

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
    
}

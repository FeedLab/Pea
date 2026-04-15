using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Pea.Infrastructure.Helpers;
using Pea.Infrastructure.Models;

namespace Pea.Meter.Services;

public class PeaAdapterDemo : IPeaAdapter
{
    private readonly ILogger<PeaAdapterDemo> logger;
    private IList<PeaMeterReading> importPeriodDataAsList = [];
    private Dictionary<DateTime, List<PeaMeterReading>> importPeriodDataAsDictionary = [];

    public PeaAdapterDemo(ILogger<PeaAdapterDemo> logger)
    {
        this.logger = logger;

        Task.Run(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            logger.LogInformation("Importing period data...");

            importPeriodDataAsDictionary = GetUsageData()
                .Where(w => w.Key.Year == 2025)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            importPeriodDataAsList = importPeriodDataAsDictionary
                .SelectMany(kvp => kvp.Value)
                .ToList();

            stopwatch.Stop();
            logger.LogInformation("Importing period data completed in {Milliseconds} Milliseconds."
                , stopwatch.Elapsed.Milliseconds.ToString("G3"));
        });
    }

    public IList<PeaMeterReading> ImportPeriodDataAsList => importPeriodDataAsList;

    private string userName = string.Empty;
    private string password = string.Empty;

    public string? CustomerId { get; private set; }
    public string? CustomerCode { get; private set; }
    public string? PeaNo { get; private set; }
    public string? CustomerName { get; private set; }
    public string? PeaSite { get; private set; }
    public string? CustomerAddress { get; private set; }
    public string? CustomerPhone { get; private set; }
    public string? CustomerFax { get; private set; }
    public string? CustomerContact { get; private set; }
    public string? CustomerEmail { get; private set; }
    public string? CustomerWebsite { get; private set; }
    public string? RateType { get; private set; }
    public string? AccountType { get; private set; }
    public string? IndustrialEstate { get; private set; }
    public string? BusinessType { get; private set; }
    public string? BusinessSize { get; private set; }
    public string? MeterNumber { get; private set; }
    public string? CTVT { get; private set; }
    public string? KVA { get; private set; }
    public string? BillingCycle { get; private set; }
    public string? MeterPointId { get; private set; }

    public async Task<bool> LoginUser(string username, string password)
    {
        this.userName = username.Trim();
        this.password = password.Trim();
        var isAuthenticated = true;

        if (isAuthenticated)
        {
            await MainCustomer();
            await CustomerProfile();
            await CustomerOverviewSelect();

            return true;
        }

        return false;
    }

    public async Task<bool> ValidateCredential(string username, string password)
    {
        return await Login(username, password);
    }

    private Task<bool> Login(string username, string password)
    {
        return Task.FromResult(true);
    }

    public Task CustomerProfile()
    {
        try
        {
            PeaSite = "www.pea.co.th";
            CustomerName = "Demo User";
            CustomerAddress = "Demo address 12";
            CustomerPhone = "08-4978877";
            CustomerFax = "08-4978866";
            CustomerContact = "Bill Gate";
            CustomerEmail = "Demo.User@demo.com";
            CustomerWebsite = "DemoWebsite.com";
            RateType = "30";
            AccountType = "Agriculture ";
            IndustrialEstate = "n/A";
            BusinessType = "11140";
            BusinessSize = "Small";
            MeterNumber = "27681720";
            CTVT = "";
            KVA = "100";
            BillingCycle = "28";
            MeterPointId = "187878";

            return Task.FromResult(Task.CompletedTask);
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    public CustomerProfile GetCustomerProfileModel()
    {
        return new CustomerProfile
        {
            Personal = new PersonalInformation
            {
                CustomerName = CustomerName,
                CustomerAccount = CustomerCode,
                CustomerId = CustomerId,
                PeaSite = PeaSite
            },
            Meter = new MeterInformation
            {
                MeterNumber = MeterNumber,
                MeterPointId = MeterPointId,
                CtVtRatio = CTVT,
                Kva = KVA,
                BillingCycle = BillingCycle
            },
            Business = new BusinessInformation
            {
                RateType = RateType,
                BillingMethod = AccountType,
                BusinessType = BusinessType,
                BusinessSize = BusinessSize,
                IndustrialEstate = IndustrialEstate
            },
            Contact = new ContactInformation
            {
                Address = CustomerAddress,
                Phone = CustomerPhone,
                Fax = CustomerFax,
                ContactPerson = CustomerContact,
                Email = CustomerEmail,
                Website = CustomerWebsite
            },
            Session = new SessionInformation
            {
                Username = CustomerCode,
            }
        };
    }

    public Task MainCustomer()
    {
        return Task.CompletedTask;
    }

    public Task CustomerDashboard()
    {
        return Task.CompletedTask;
    }

    public Task CustomerShowOverview()
    {
        return Task.CompletedTask;
    }

    public Task CustomerOverviewSelect()
    {
        return Task.CompletedTask;
    }

    public Task<IList<PeaMeterReading>> GetAllReadings(DateTime startDate, int maximumDaysToRead = 365)
    {
        logger.LogInformation("Getting all readings from {0} to {1}", startDate, startDate.AddDays(-maximumDaysToRead));

        var stopwatch = Stopwatch.StartNew();

        var readings = importPeriodDataAsList
            .Select(p =>
            {
                if (p.PeriodStart.DayOfYear <= startDate.DayOfYear)
                {
                    var date = new DateTime(
                        startDate.Year,
                        p.PeriodStart.Month,
                        p.PeriodStart.Day,
                        p.PeriodStart.Hour,
                        p.PeriodStart.Minute,
                        0);

                    return new PeaMeterReading(date, p.RateA, p.RateB, p.RateC);
                }
                else
                {
                    var date = new DateTime(
                        startDate.Year - 1,
                        p.PeriodStart.Month,
                        p.PeriodStart.Day,
                        p.PeriodStart.Hour,
                        p.PeriodStart.Minute,
                        0);

                    return new PeaMeterReading(date, p.RateA, p.RateB, p.RateC);
                }
            });

        var allReadings = readings
            .Where(w => w.PeriodStart.Date < startDate.Date)
            .OrderBy(r => r.PeriodStart)
            // .TakeLast(maximumDaysToRead)
            .ToList();

        stopwatch.Stop();

        logger.LogInformation("Returning {0} readings from {1} to {2} in {Milliseconds} Milliseconds."
            , allReadings.Count, startDate, startDate.AddDays(-maximumDaysToRead),
            stopwatch.Elapsed.Milliseconds.ToString("G3"));

        return Task.FromResult<IList<PeaMeterReading>>(allReadings);
    }

    public Task<IList<PeaMeterReading>?> ShowDailyReadings(DateTime selectedDate)
    {
        var dateTransformed = new DateTime(2025, selectedDate.Month, selectedDate.Day);

        if (importPeriodDataAsDictionary.TryGetValue(dateTransformed.Date, out var periodData))
        {
            var periodDataTransformed = periodData
                .Select(p =>
                {
                    var date = new DateTime(
                        selectedDate.Year,
                        p.PeriodStart.Month,
                        p.PeriodStart.Day,
                        p.PeriodStart.Hour,
                        p.PeriodStart.Minute,
                        0);
                    return new PeaMeterReading(date, p.RateA, p.RateB, p.RateC);
                })
                .ToList();

            var today = DateTime.Today.Date;
            if (selectedDate.Month == today.Month && selectedDate.Day == today.Day)
            {
                var filtered = periodDataTransformed
                    .Where(w => w.PeriodStart.TimeOfDay < DateTime.Now.TimeOfDay)
                    .ToList();

                return Task.FromResult<IList<PeaMeterReading>?>(filtered);
            }

            return Task.FromResult<IList<PeaMeterReading>?>(periodDataTransformed);
        }

        //throw new Exception("No data found");
        logger.LogWarning("No data found for selected date: {0}", selectedDate);
        return Task.FromResult<IList<PeaMeterReading>?>(new List<PeaMeterReading>());
    }

    private static Dictionary<DateTime, List<PeaMeterReading>> GetUsageData()
    {
        var year = 2025;
        decimal[] monthlyTotalsKw =
        {
            1910, 1670, 2290, 4320, 3630, 4030, 3810, 4720, 4320, 3960, 2020, 3110,
        };

        var holidays = new HashSet<DateTime>
        {
            new(2026, 1, 1),
            new(2026, 1, 1),
            new(2026, 3, 3),
            new(2026, 4, 6),
            new(2026, 4, 13),
            new(2026, 4, 14),
            new(2026, 4, 15),
            new(2026, 5, 1),
            new(2026, 6, 3),
            new(2026, 7, 28),
            new(2026, 7, 29),
            new(2026, 7, 30),
            new(2026, 8, 12),
            new(2026, 10, 13),
            new(2026, 12, 10),
            new(2026, 12, 31),
        };

        var usageData = new List<PeaMeterReading>();

        for (var month = 1; month <= 12; month++)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var monthlyTotal = monthlyTotalsKw[month - 1];
            var dailyTotal = monthlyTotal / daysInMonth;

            for (var day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);

                var peakWeight = 0.8m;
                var offPeakWeight = 0.2m;
                var factorEnergy = 1.2m;

                var peakTotal = dailyTotal * peakWeight;
                var offPeakTotal = dailyTotal * offPeakWeight;

                var peakIntervals = 13 * 4; // 09:00–22:00
                var offPeakIntervals = 96 - peakIntervals;

                var peakPerInterval = peakTotal / peakIntervals;
                var offPeakPerInterval = offPeakTotal / offPeakIntervals;

                for (var interval = 0; interval < 96; interval++)
                {
                    var timestamp = date.AddMinutes(interval * 15);

                    decimal peak = 0, offPeak = 0, holiday = 0;

                    if (holidays.Contains(date))
                    {
                        // Example: reduce holiday usage to 50%
                        holiday = (dailyTotal * 0.5m) / 96;
                    }
                    else
                    {
                        var hour = timestamp.Hour;
                        if (hour >= 9 && hour < 22)
                            peak = peakPerInterval;
                        else
                            offPeak = offPeakPerInterval;
                    }

                    usageData.Add(new PeaMeterReading(timestamp, peak * factorEnergy, offPeak * factorEnergy, holiday * factorEnergy));
                }
            }
        }

        // Write to JSON file
        // var options = new JsonSerializerOptions { WriteIndented = true };
        // var json = JsonSerializer.Serialize(usageData, options);
        // File.WriteAllText("usageData.json", json);
        //
        // Console.WriteLine("Usage data written to usageData.json");

        return usageData.ToDictionary(r => r.PeriodStart, r => new List<PeaMeterReading> { r });
    }
}

public class PeaMeterPeriodData
{
    private readonly int periodLengthInMinutes = 15;
    private readonly int invoiceLengthInDays = 30;

    // AllMeterReadings.json


    // public static IList<PeaMeterReading> ImportPeriodDataList()
    // {
    //     // Read JSON from file
    //     var json = File.ReadAllText("AllMeterReadings.json");
    //
    //     // Deserialize into a list of strings
    //     var values = JsonSerializer.Deserialize<List<PeaMeterReading>>(json);
    //
    //     if (values == null)
    //         throw new Exception("Failed to deserialize JSON");
    //
    //     Console.WriteLine($@"Deserialized {values.Count} items");
    //
    //     return values.OrderBy(r => r.PeriodStart).ToList();
    // }

    public static Dictionary<DateTime, List<PeaMeterReading>> ImportPeriodDataDictionary()
    {
        // Read JSON from compressed embedded resource
        var assembly = typeof(PeaMeterPeriodData).Assembly;
        using var stream = assembly.GetManifestResourceStream("Pea.Meter.AllMeterReadings.json.gz")
                           ?? throw new InvalidOperationException(
                               "Embedded resource 'AllMeterReadings.json.gz' not found.");
        using var gzip = new GZipStream(stream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip);
        var json = reader.ReadToEnd();

        var values = JsonSerializer.Deserialize<Dictionary<DateTime, List<PeaMeterPeriodData>>>(json);

        if (values == null)
            throw new Exception("Failed to deserialize JSON");

        Console.WriteLine($@"Deserialized {values.Count} items");

        var valuesTransformed = values.ToDictionary(
            kvp => kvp.Key, // keep the same DateTime key
            kvp => kvp.Value
                .Select(pd => new PeaMeterReading(
                    pd.PeriodStart,
                    pd.RateA,
                    pd.RateB,
                    pd.RateC))
                .ToList());


        return new Dictionary<DateTime, List<PeaMeterReading>>(valuesTransformed);
    }

    public DateTime PeriodStart { get; set; }

    public decimal RateA { get; init; }

    public decimal RateB { get; init; }

    public decimal RateC { get; init; }
}
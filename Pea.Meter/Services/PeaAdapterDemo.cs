using System.Text.Json;
using Pea.Infrastructure.Helpers;
using Pea.Infrastructure.Models;
using Serilog.Core;

namespace Pea.Meter.Services;

public class PeaAdapterDemo : IPeaAdapter
{
    private readonly IList<PeaMeterReading> importPeriodDataAsList;
    private readonly Dictionary<DateTime, List<PeaMeterReading>> importPeriodDataAsDictionary;

    public PeaAdapterDemo()
    {
        importPeriodDataAsDictionary = PeaMeterPeriodData
            .ImportPeriodDataDictionary()
            .Where(w => w.Key.Year == 2025)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        importPeriodDataAsList = importPeriodDataAsDictionary
            .SelectMany(kvp => kvp.Value)
            .ToList();
        

    }

    public IList<PeaMeterReading> ImportPeriodDataAsList => importPeriodDataAsList;
    
    private string userName;
    private string password;

    public string? CustomerId { get; set; }
    public string? CustomerCode { get; set; }
    public string? PeaNo { get; set; }
    public string? CustomerName { get; set; }
    public string? PeaSite { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerFax { get; set; }
    public string? CustomerContact { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerWebsite { get; set; }
    public string? RateType { get; set; }
    public string? AccountType { get; set; }
    public string? IndustrialEstate { get; set; }
    public string? BusinessType { get; set; }
    public string? BusinessSize { get; set; }
    public string? MeterNumber { get; set; }
    public string? CTVT { get; set; }
    public string? KVA { get; set; }
    public string? BillingCycle { get; set; }
    public string? MeterPointId { get; set; }

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
        var dayOfYear = DateTime.Today.DayOfYear;

        var readings = importPeriodDataAsList
            .Select(p =>
            {
                if(p.PeriodStart.DayOfYear <= startDate.DayOfYear)
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
            .ToList();

        return Task.FromResult<IList<PeaMeterReading>>(allReadings);
    }

    public Task<IList<PeaMeterReading>?> ShowDailyReadings(DateTime selectedDate)
    {
        var dateTransformed = new DateTime(2025, selectedDate.Month, selectedDate.Day);
        
        if(importPeriodDataAsDictionary.TryGetValue(dateTransformed.Date, out var periodData))
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
        return Task.FromResult<IList<PeaMeterReading>?>(new List<PeaMeterReading>());
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
        // Read JSON from file
        var json = File.ReadAllText("AllMeterReadings.json");

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

    public DateTime PeriodEnd => PeriodStart.AddMinutes(periodLengthInMinutes).AddMilliseconds(-1);

    public decimal RateA { get; init; }

    public decimal RateB { get; init; }

    public decimal RateC { get; init; }

    public decimal Total => RateA + RateB + RateC;
    public decimal Peek => RateA;
    public decimal OffPeek => RateB + RateC;

    public string PeekFormatted => WattFormatter.Format(Peek * 1000);
    public string OffPeekFormatted => WattFormatter.Format(OffPeek * 1000);
    public string TotalFormatted => WattFormatter.Format(Total * 1000);
}
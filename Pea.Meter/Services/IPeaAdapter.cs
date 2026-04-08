using Pea.Infrastructure.Models;

namespace Pea.Meter.Services;

public interface IPeaAdapter
{
    string? CustomerId { get; }
    string? CustomerCode { get; }
    string? PeaNo { get; }
    string? CustomerName { get; }
    string? PeaSite { get; }
    string? CustomerAddress { get; }
    string? CustomerPhone { get; }
    string? CustomerFax { get; }
    string? CustomerContact { get; }
    string? CustomerEmail { get; }
    string? CustomerWebsite { get; }
    string? RateType { get; }
    string? AccountType { get; }
    string? IndustrialEstate { get; }
    string? BusinessType { get; }
    string? BusinessSize { get; }
    string? MeterNumber { get; }
    string? CTVT { get; }
    string? KVA { get; }
    string? BillingCycle { get; }
    string? MeterPointId { get; }
    Task<bool> LoginUser(string username, string password);
    Task<bool> ValidateCredential(string username, string password);
    Task CustomerProfile();

    /// <summary>
    /// Gets the complete customer profile as a model
    /// </summary>
    /// <returns>CustomerProfile model with all information</returns>
    CustomerProfile GetCustomerProfileModel();

    Task MainCustomer();
    Task CustomerDashboard();
    Task CustomerShowOverview();
    Task CustomerOverviewSelect();
    Task<IList<PeaMeterReading>?> ShowDailyReadings(DateTime selectedDate);

    Task<IList<PeaMeterReading>> GetAllReadings(DateTime startDate, int maximumDaysToRead = 365);

}
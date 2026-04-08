using Pea.Infrastructure.Models;

namespace Pea.Meter.Services;

public class PeaAdapterRouter(PeaAdapterLive live, PeaAdapterDemo demo) : IPeaAdapter
{
    private bool useDemo;

    public void UseDemo(bool enable) => useDemo = enable;
    public bool IsInDemoMode => useDemo;

    public string? CustomerId => useDemo ? demo.CustomerId : live.CustomerId;
    public string? CustomerCode  => useDemo ? demo.CustomerCode : live.CustomerCode;
    public string? PeaNo  => useDemo ? demo.PeaNo : live.PeaNo;
    public string? CustomerName  => useDemo ? demo.CustomerName : live.CustomerName;
    public string? PeaSite => useDemo ? demo.PeaSite : live.PeaSite;
    public string? CustomerAddress => useDemo ? demo.CustomerAddress : live.CustomerAddress;
    public string? CustomerPhone  => useDemo ? demo.CustomerPhone : live.CustomerPhone;
    public string? CustomerFax  => useDemo ? demo.CustomerFax : live.CustomerFax;
    public string? CustomerContact  => useDemo ? demo.CustomerContact : live.CustomerContact;
    public string? CustomerEmail  => useDemo ? demo.CustomerEmail : live.CustomerEmail;
    public string? CustomerWebsite  => useDemo ? demo.CustomerWebsite : live.CustomerWebsite;
    public string? RateType  => useDemo ? demo.RateType : live.RateType;
    public string? AccountType  => useDemo ? demo.AccountType : live.AccountType;
    public string? IndustrialEstate  => useDemo ? demo.IndustrialEstate : live.IndustrialEstate;
    public string? BusinessType  => useDemo ? demo.BusinessType : live.BusinessType;
    public string? BusinessSize  => useDemo ? demo.BusinessSize : live.BusinessSize;
    public string? MeterNumber => useDemo ? demo.MeterNumber : live.MeterNumber;
    public string? CTVT => useDemo ? demo.CTVT : live.CTVT;
    public string? KVA => useDemo ? demo.KVA : live.KVA;
    public string? BillingCycle => useDemo ? demo.BillingCycle : live.BillingCycle;
    public string? MeterPointId => useDemo ? demo.MeterPointId : live.MeterPointId;
    
    public Task<bool> LoginUser(string username, string password)
        => useDemo ? demo.LoginUser(username, password) : live.LoginUser(username, password);

    public Task<bool> ValidateCredential(string username, string password)
        => useDemo ? demo.LoginUser(username, password) : live.LoginUser(username, password);

    public Task CustomerProfile()
        => useDemo ? demo.CustomerProfile() : live.CustomerProfile();

    public CustomerProfile GetCustomerProfileModel()
        => useDemo ? demo.GetCustomerProfileModel() : live.GetCustomerProfileModel();

    public Task MainCustomer()
        => useDemo ? demo.MainCustomer() : live.MainCustomer();

    public Task CustomerDashboard()
        => useDemo ? demo.CustomerDashboard() : live.CustomerDashboard();

    public Task CustomerShowOverview()
        => useDemo ? demo.CustomerDashboard() : live.CustomerShowOverview();

    public Task CustomerOverviewSelect()
        => useDemo ? demo.CustomerOverviewSelect() : live.CustomerOverviewSelect();

    public Task<IList<PeaMeterReading>?> ShowDailyReadings(DateTime selectedDate)
        => useDemo ? demo.ShowDailyReadings(selectedDate) : live.ShowDailyReadings(selectedDate);

    public Task<IList<PeaMeterReading>> GetAllReadings(DateTime startDate, int maximumDaysToRead = 365)
        => useDemo ? demo.GetAllReadings(startDate, maximumDaysToRead) : live.GetAllReadings(startDate, maximumDaysToRead);
}
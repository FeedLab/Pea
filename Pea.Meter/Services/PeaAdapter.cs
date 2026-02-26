using System.Diagnostics;
using System.Globalization;
using System.Net;
using HtmlAgilityPack;
using Pea.Infrastructure.Models;

namespace Pea.Meter.Services;

/// <summary>
/// Service for interacting with PEA AMR web system
/// </summary>
public class PeaAdapter
{
    private readonly HttpClientHandler handler = new()
    {
        UseCookies = true,
        CookieContainer = new CookieContainer()
    };

    private readonly HttpClient client;
    public string? CustomerId { get; private set; }
    public string? CustomerCode { get; private set; }
    public string? PeaNo { get; private set; }

    // Customer Profile Properties
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

    public PeaAdapter()
    {
        client = new HttpClient(handler);
    }

    public async Task LoginUser(string username, string password)
    {
        await Login(username, password);
        await MainCustomer();
        await CustomerProfile();
        await CustomerOverviewSelect();
    }

    public async Task<bool> ValidateCredential(string username, string password)
    {
        return await Login(username, password);
    }

    private async Task<bool> Login(string username, string password)
    {
        // 1. GET the login page
        var loginPageResponse = await client.GetAsync("https://www.amr.pea.co.th/AMRWEB/Index.aspx");
        var loginPageHtml = await loginPageResponse.Content.ReadAsStringAsync();

        // 2. Parse hidden fields
        var doc = new HtmlDocument();
        doc.LoadHtml(loginPageHtml);

        var viewState = doc.DocumentNode
            .SelectSingleNode("//input[@id='__VIEWSTATE']")
            ?.GetAttributeValue("value", "");

        var eventValidation = doc.DocumentNode
            .SelectSingleNode("//input[@id='__EVENTVALIDATION']")
            ?.GetAttributeValue("value", "");

        var viewStateGenerator = doc.DocumentNode
            .SelectSingleNode("//input[@id='__VIEWSTATEGENERATOR']")
            ?.GetAttributeValue("value", "");

        // 3. Prepare login form data
        var values = new Dictionary<string, string>
        {
            { "txtUsername", username },
            { "txtPassword", password },
            { "__VIEWSTATE", viewState },
            { "__VIEWSTATEGENERATOR", viewStateGenerator },
            { "__EVENTVALIDATION", eventValidation },
            { "btnOK", "เข้าสู่ระบบ" }
        };

        var content = new FormUrlEncodedContent(values);

        // 4. POST credentials
        var loginResponse = await client.PostAsync("https://www.amr.pea.co.th/AMRWEB/Index.aspx", content);
        var loginResultHtml = await loginResponse.Content.ReadAsStringAsync();

        // Look for login form elements (present = failed, absent = success)
        if (loginResultHtml.Contains("txtUsername") || loginResultHtml.Contains("txtPassword"))
        {
            Console.WriteLine("Login failed - still on login page");

            return false;
        }
        else
        {
            Console.WriteLine("Login successful - redirected away from login");
        }

        var cookies = handler.CookieContainer.GetCookies(new Uri("https://www.amr.pea.co.th"));
        if (cookies["ASP.NET_SessionId"] != null || cookies[".ASPXAUTH"] != null)
        {
            Console.WriteLine("Authentication cookie found");
            return true;
        }

        return false;
    }

    public async Task CustomerProfile()
    {
        var requestUri =
            $"https://www.amr.pea.co.th/AMRWEB/CustProfile.aspx?CustCode={CustomerCode}&Custid={CustomerId}";

        var protectedResponse = await client.GetAsync(requestUri);
        var protectedHtml = await protectedResponse.Content.ReadAsStringAsync();

        if (protectedHtml.Contains("txtUsername") || protectedHtml.Contains("txtPassword"))
        {
            Console.WriteLine("Login failed - still on login page");
        }
        else
        {
            Console.WriteLine("Login successful - redirected away from login");
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(protectedHtml);

        // Parse customer profile data
        PeaSite = doc.DocumentNode.SelectSingleNode("//span[@id='lblSitename']")?.InnerText.Trim();
        CustomerName = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerName']")?.InnerText.Trim();
        CustomerAddress = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerAdd']")?.InnerText.Trim();
        CustomerPhone = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerPhone']")?.InnerText.Trim();
        CustomerFax = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerFax']")?.InnerText.Trim();
        CustomerContact = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerContact']")?.InnerText.Trim();
        CustomerEmail = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerEmail']")?.InnerText.Trim();
        CustomerWebsite = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerWeb']")?.InnerText.Trim();
        RateType = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerBussType']")?.InnerText.Trim();
        AccountType = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerAcctT']")?.InnerText.Trim();
        IndustrialEstate = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerIndust']")?.InnerText.Trim();
        BusinessType = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerTypeBuss']")?.InnerText.Trim();
        BusinessSize = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerRateType']")?.InnerText.Trim();
        MeterNumber = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerMeterNo']")?.InnerText.Trim();
        CTVT = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerCTVT']")?.InnerText.Trim();
        KVA = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerKVA']")?.InnerText.Trim();
        BillingCycle = doc.DocumentNode.SelectSingleNode("//span[@id='lblCustomerReset']")?.InnerText.Trim();

        Console.WriteLine($"Customer Profile: {CustomerName} - {PeaSite}");
    }

    /// <summary>
    /// Gets the complete customer profile as a model
    /// </summary>
    /// <returns>CustomerProfile model with all information</returns>
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
                // IpAddress and LastLoginTime would need to be extracted from HTML
            }
        };
    }

    public async Task MainCustomer()
    {
        // 5. Navigate to a protected page
        var protectedResponse = await client.GetAsync("https://www.amr.pea.co.th/AMRWEB/MainCust.aspx");
        var protectedHtml = await protectedResponse.Content.ReadAsStringAsync();

        if (protectedHtml.Contains("txtUsername") || protectedHtml.Contains("txtPassword"))
        {
            Console.WriteLine("Login failed - still on login page");
        }
        else
        {
            Console.WriteLine("Login successful - redirected away from login");
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(protectedHtml);

        var iframe = doc.DocumentNode.SelectSingleNode("//iframe[@id='frmMain']");
        var iframeSrc = iframe?.GetAttributeValue("src", "");

        var queryStart = iframeSrc.IndexOf('?');
        if (queryStart >= 0)
        {
            var queryString = iframeSrc.Substring(queryStart + 1);
            var query = System.Web.HttpUtility.ParseQueryString(queryString);

            CustomerId = query["Custid"];
            CustomerCode = query["CustCode"];
            PeaNo = query["PeaNo"];
        }

        Debug.WriteLine("Protected page content:");

        // Step 6: Inspect cookies (optional)
        foreach (Cookie cookie in handler.CookieContainer.GetCookies(new Uri("https://www.amr.pea.co.th")))
        {
            Console.WriteLine($"{cookie.Name} = {cookie.Value}");
        }
    }

    public async Task CustomerDashboard()
    {
        // 5. Navigate to a protected page
        var requestUri =
            $"https://www.amr.pea.co.th/AMRWEB/CustDashboard.aspx?CustCode={CustomerCode}&Custid={CustomerId}";

        var protectedResponse = await client.GetAsync(requestUri);
        var protectedHtml = await protectedResponse.Content.ReadAsStringAsync();

        if (protectedHtml.Contains("txtUsername") || protectedHtml.Contains("txtPassword"))
        {
            Console.WriteLine("Login failed - still on login page");
        }
        else
        {
            Console.WriteLine("Login successful - redirected away from login");
        }

        Debug.WriteLine("Protected page content:");
    }

    public async Task CustomerShowOverview()
    {
        // 5. Navigate to a protected page
        var requestUri =
            $"https://www.amr.pea.co.th/AMRWEB/frmOverviewShow.aspx?CustCode={CustomerCode}&Custid={CustomerId}&MeterNo={MeterNumber}&RepName=1&Month=02&Year=2026&Phase=All";

        var protectedResponse = await client.GetAsync(requestUri);
        var protectedHtml = await protectedResponse.Content.ReadAsStringAsync();

        if (protectedHtml.Contains("txtUsername") || protectedHtml.Contains("txtPassword"))
        {
            Console.WriteLine("Login failed - still on login page");
        }
        else
        {
            Console.WriteLine("Login successful - redirected away from login");
        }

        Debug.WriteLine("Protected page content:");
    }

    public async Task CustomerOverviewSelect()
    {
        // 5. Navigate to a protected page
        var requestUri =
            $"https://www.amr.pea.co.th/AMRWEB/frmOverviewSel.aspx?CustCode={CustomerCode}&Custid={CustomerId}";

        var protectedResponse = await client.GetAsync(requestUri);
        var protectedHtml = await protectedResponse.Content.ReadAsStringAsync();

        if (protectedHtml.Contains("txtUsername") || protectedHtml.Contains("txtPassword"))
        {
            Console.WriteLine("Login failed - still on login page");
        }
        else
        {
            Console.WriteLine("Login successful - redirected away from login");
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(protectedHtml);

        // Parse ddlMeterNo selected value
        var selectedOption = doc.DocumentNode.SelectSingleNode("//select[@id='ddlMeterNo']/option[@selected='selected']");
        if (selectedOption != null)
        {
            MeterPointId = selectedOption.GetAttributeValue("value", "");
            Console.WriteLine($"Meter No: {MeterNumber}, Meter Point ID: {MeterPointId}");
        }

        Debug.WriteLine("Protected page content:");
    }

    public async Task<IList<PeaMeterReading>> ShowDailyReadings(DateTime selectedDate)
    {
        var year = selectedDate.Year.ToString();
        var month = selectedDate.Month.ToString("00");
        var day = selectedDate.Day.ToString("00");
        
        var requestUri =
            $"https://www.amr.pea.co.th/AMRWEB/ShowDailyProfile.aspx?Overview=1&Custid={CustomerId}&CustCode={CustomerCode}&MeterPoint={MeterPointId}&SumMeter=0&RepDate={day}/{month}/{year}&GrphType=Line&DataType=2&kWh=1&kVarh=0&kW=0&kVar=0&Cur=0&Vol=0&PF=0&PD=0&kWh1=0&kVarh1=0&kW1=0&kVar1=0";

        var protectedResponse = await client.GetAsync(requestUri);
        var protectedHtml = await protectedResponse.Content.ReadAsStringAsync();

        if (protectedHtml.Contains("txtUsername") || protectedHtml.Contains("txtPassword"))
        {
            Console.WriteLine("Login failed - still on login page");
        }
        else
        {
            Console.WriteLine("Login successful - redirected away from login");
        }

        Debug.WriteLine("Protected page content:");

        var html = new HtmlDocument();
        html.LoadHtml(protectedHtml);

        // Check if HTML loaded
     //   Console.WriteLine($"HTML length: {html.DocumentNode.InnerHtml.Length}");

        // Find all divs
        // var divs = html.DocumentNode.SelectNodes("//div[@id]");
        // if (divs != null)
        // {
        //     foreach (var div in divs)
        //         Console.WriteLine($"Found div: {div.GetAttributeValue("id", "")}");
        // }

        // Find all tables
        var dataTable = html.DocumentNode.SelectSingleNode("//div[@id='divTable']//table");
        var rows = dataTable?.SelectNodes(".//tr");

        if (rows != null)
        {
            Console.WriteLine($"Year: {selectedDate.Year}, Month: {selectedDate.Month}, Day: {selectedDate.Day}");

            var listOfPeaMeterReading = new List<PeaMeterReading>();

            foreach (var row in rows.Skip(1).SkipLast(1)) // Skip header and last row
            {
                var cells = row.SelectNodes(".//td");
                if (cells.Count >= 5)
                {
                    var timestamp = cells[0].InnerText.Trim(); // "18/02/2026 00.15"
                    var rateA = string.IsNullOrEmpty(cells[1].InnerText.Trim()) ? "0" : cells[1].InnerText.Trim();
                    var rateB = string.IsNullOrEmpty(cells[2].InnerText.Trim()) ? "0" : cells[2].InnerText.Trim();
                    var rateC = string.IsNullOrEmpty(cells[3].InnerText.Trim()) ? "0" : cells[3].InnerText.Trim();
                    var total = string.IsNullOrEmpty(cells[4].InnerText.Trim()) ? "0" : cells[4].InnerText.Trim();

                    var dt = ParseAndAdjustTimestamp(timestamp);

                    // Console.WriteLine($"{timestamp}: A={rateA}, B={rateB}, C={rateC}, Total={total}");

                    var reading = new PeaMeterReading(
                        dt.AddMinutes(-15),
                        decimal.Parse(rateA, CultureInfo.InvariantCulture),
                        decimal.Parse(rateB, CultureInfo.InvariantCulture),
                        decimal.Parse(rateC, CultureInfo.InvariantCulture)
                    );

                    listOfPeaMeterReading.Add(reading);
                }
            }

            return listOfPeaMeterReading;
        }

        return new List<PeaMeterReading>();
    }

    private static DateTime ParseAndAdjustTimestamp(string timestamp)
    {
        var datetimeStr = timestamp;

        if (datetimeStr.Contains(" 24."))
        {
            // Replace hour 24 with hour 00
            datetimeStr = datetimeStr.Replace(" 24.", " 00.");
            // Parse with hour 00
            DateTime dt = DateTime.ParseExact(datetimeStr, "dd/MM/yyyy HH.mm", CultureInfo.InvariantCulture);
            // Add one day since 24:00 means midnight of next day
            return dt.AddDays(1);
        }
        else
        {
            return DateTime.ParseExact(datetimeStr, "dd/MM/yyyy HH.mm", CultureInfo.InvariantCulture);
        }
    }
}
using System.Web;
using Refit;

namespace Pea.Meter.Services;

// public interface IFooApi
// {
//     [Get("/solar/1.0/irradiance?lat={lat}&lon={lon}&date={dateIso}")]
//     Task<double> GetSolarPercentAsync(double lat, double lon, DateOnly dateTime);
//
//
//     [Get("/irradiance")]
//     Task<ApiResponse<SolarResponse>> GetIrradianceAsync(
//         [AliasAs("lat")] double latitude,
//         [AliasAs("lon")] double longitude,
//         [AliasAs("date")] string dateIso);
// }

public class ApiKeyHandler : DelegatingHandler
{
    private readonly string _apiKey;

    public ApiKeyHandler(string apiKey)
    {
        _apiKey = apiKey;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Add API key as query parameter
        var uriBuilder = new UriBuilder(request.RequestUri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["appid"] = _apiKey;   // OpenWeatherMap expects ?appid=KEY
        uriBuilder.Query = query.ToString();
        request.RequestUri = uriBuilder.Uri;

        return base.SendAsync(request, cancellationToken);

    }
}


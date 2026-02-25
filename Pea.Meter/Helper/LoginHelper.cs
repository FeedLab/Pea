using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using Pea.Meter.Services;

namespace Pea.Meter.Helper;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class AuthData(string userName, string password) : ObservableObject, IAuthData
{
    [ObservableProperty] private  string username  = userName;
    [ObservableProperty] private  string password  = password;
}

public interface ILoginHelper
{
    IAuthData? GetAuthData();
    Task<IAuthData> SaveAuthDataAsync(string userName, string password);
    Task ClearAuthDataAsync();
}

public class LoginHelper : ILoginHelper
{
    private readonly ISettingsService settingsService;
    private readonly AuthDataOptions authDataOptions;

    public LoginHelper(ISettingsService settingsService, AuthDataOptions authDataOptions)
    {
        this.settingsService = settingsService;
        this.authDataOptions = authDataOptions;
    }

    public IAuthData? GetAuthData()
    {
        return authDataOptions.AuthData;
    }

    public async Task<IAuthData> SaveAuthDataAsync(string userName, string password)
    {
        await settingsService.SaveAuthDataAsync(userName, password);
        var authData = new AuthData(userName, password);
        return authData;
    }

    public async Task ClearAuthDataAsync()
    {
        await settingsService.ClearAuthDataAsync();
    }
}

public interface IAuthData
{
    string Username { get; }
    string Password { get; }
}
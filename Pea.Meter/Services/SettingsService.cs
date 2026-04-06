using System.Reactive.Linq;
using System.Text.Json;
using Akavache;
using Pea.Meter.Helpers;
using AuthData = Pea.Meter.Helpers.AuthData;

namespace Pea.Meter.Services;

public interface ISettingsService
{
    Task<IAuthData?> LoadAuthDataAsync();
    Task SaveAuthDataAsync(string userName, string password);
    Task ClearAuthDataAsync();
}

public class SettingsService : ISettingsService
{
    private const string StoreKey = "AuthData";

    private readonly IEncryptionHelper encryptionHelper;
    private readonly IServiceProvider serviceProvider;

    private sealed record AuthDataDto(string EncryptedCredentials);

    public SettingsService(IEncryptionHelper encryptionHelper, IServiceProvider serviceProvider)
    {
        this.encryptionHelper = encryptionHelper;
        this.serviceProvider = serviceProvider;
    }

    public async Task<IAuthData?> LoadAuthDataAsync()
    {
        try
        {
            var dto = await CacheDatabase.UserAccount.GetObject<AuthDataDto>(StoreKey);
            if (string.IsNullOrEmpty(dto?.EncryptedCredentials)) return null;

            var decryptedText = encryptionHelper.Decrypt(dto.EncryptedCredentials);
            return JsonSerializer.Deserialize<AuthData>(decryptedText);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task SaveAuthDataAsync(string userName, string password)
    {
        try
        {
            var authData = new AuthData(userName, password);
            var encryptedText = encryptionHelper.Encrypt(JsonSerializer.Serialize(authData));

            await CacheDatabase.UserAccount.InsertObject(StoreKey, new AuthDataDto(encryptedText));

            var authDataOptions = serviceProvider.GetService<AuthDataOptions>();
            if (authDataOptions != null)
                authDataOptions.AuthData = authData;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save auth data", ex);
        }
    }

    public async Task ClearAuthDataAsync()
    {
        try
        {
            await CacheDatabase.UserAccount.InvalidateObject<AuthDataDto>(StoreKey);

            var authDataOptions = serviceProvider.GetService<AuthDataOptions>();
            if (authDataOptions != null)
                authDataOptions.AuthData = null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to clear auth data", ex);
        }
    }
}

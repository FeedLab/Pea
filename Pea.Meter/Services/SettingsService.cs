using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Pea.Meter.Helper;

namespace Pea.Meter.Services;

public interface ISettingsService
{
    Task<IAuthData?> LoadAuthDataAsync();
    Task SaveAuthDataAsync(string userName, string password);
    Task ClearAuthDataAsync();
}

public class SettingsService : ISettingsService
{
    private readonly IConfiguration configuration;
    private readonly IEncryptionHelper encryptionHelper;
    private readonly IServiceProvider serviceProvider;
    private readonly string settingsFilePath;
    private readonly string environment;

    public SettingsService(IEncryptionHelper encryptionHelper, IServiceProvider serviceProvider)
    {
        this.encryptionHelper = encryptionHelper;
        this.serviceProvider = serviceProvider;

        // Determine environment (check for DEBUG flag or environment variable)
#if DEBUG
        environment = "Development";
#else
        environment = "Production";
#endif

        // Get the appropriate settings file path
        var roamingAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var settingsFileName = environment == "Development" ? "appsettings.Development.json" : "appsettings.json";
        settingsFilePath = Path.Combine(roamingAppDataPath, "Pea.Meter", settingsFileName);

        // Ensure directory exists
        var directory = Path.GetDirectoryName(settingsFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Copy default settings file from embedded resources if it doesn't exist
        if (!File.Exists(settingsFilePath))
        {
            CopyDefaultSettingsFile(settingsFileName);
        }

        // Build configuration from the settings file
        configuration = new ConfigurationBuilder()
            .AddJsonFile(settingsFilePath, optional: false, reloadOnChange: false)
            .Build();
    }

    private void CopyDefaultSettingsFile(string settingsFileName)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Pea.Meter.{settingsFileName}";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var fileStream = File.Create(settingsFilePath);
                stream.CopyTo(fileStream);
            }
            else
            {
                // If embedded resource not found, create a default file
                var defaultSettings = new { AuthData = new { EncryptedCredentials = "" } };
                File.WriteAllText(settingsFilePath, JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch
        {
            // If copying fails, create a default file
            var defaultSettings = new { AuthData = new { EncryptedCredentials = "" } };
            File.WriteAllText(settingsFilePath, JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    public async Task<IAuthData?> LoadAuthDataAsync()
    {
        try
        {
            // Reload configuration to get latest values
            var currentConfig = new ConfigurationBuilder()
                .AddJsonFile(settingsFilePath, optional: false, reloadOnChange: false)
                .Build();

            var encryptedCredentials = currentConfig["AuthData:EncryptedCredentials"];

            if (string.IsNullOrEmpty(encryptedCredentials))
            {
                return null;
            }

            var decryptedText = encryptionHelper.Decrypt(encryptedCredentials);
            return JsonSerializer.Deserialize<AuthData>(decryptedText);
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
            var jsonString = JsonSerializer.Serialize(authData);
            var encryptedText = encryptionHelper.Encrypt(jsonString);

            // Read current settings
            var settingsJson = await File.ReadAllTextAsync(settingsFilePath);
            var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(settingsJson)
                          ?? new Dictionary<string, object>();

            // Update AuthData section
            var authDataSection = new Dictionary<string, string>
            {
                ["EncryptedCredentials"] = encryptedText
            };

            settings["AuthData"] = JsonSerializer.SerializeToElement(authDataSection);

            // Write back to file
            await File.WriteAllTextAsync(settingsFilePath,
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));

            // Update IOptions if available
            var authDataOptions = serviceProvider.GetService<AuthDataOptions>();
            if (authDataOptions != null)
            {
                authDataOptions.AuthData = authData;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save auth data to settings file", ex);
        }
    }

    public async Task ClearAuthDataAsync()
    {
        try
        {
            // Read current settings
            var settingsJson = await File.ReadAllTextAsync(settingsFilePath);
            var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(settingsJson)
                          ?? new Dictionary<string, object>();

            // Clear AuthData section
            var authDataSection = new Dictionary<string, string>
            {
                ["EncryptedCredentials"] = ""
            };

            settings["AuthData"] = JsonSerializer.SerializeToElement(authDataSection);

            // Write back to file
            await File.WriteAllTextAsync(settingsFilePath,
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));

            // Update IOptions if available
            var authDataOptions = serviceProvider.GetService<AuthDataOptions>();
            if (authDataOptions != null)
            {
                authDataOptions.AuthData = null;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to clear auth data from settings file", ex);
        }
    }
}

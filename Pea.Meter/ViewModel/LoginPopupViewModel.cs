using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pea.Meter.Helper;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0034:Direct field reference to [ObservableProperty] backing field")]
[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class LoginPopupViewModel(IPopupService popupService, PeaAdapter peaAdapter)
    : ObservableObject, IQueryAttributable
{
    [ObservableProperty] private AuthData? loginData;
    [ObservableProperty] private string failLoginMessage = "";
    [ObservableProperty] private bool isFailLoginMessageVisible;
 
    [RelayCommand]
    async Task Cancel()
    {
        if (LoginData is null)
        {
            throw new ArgumentNullException(nameof(LoginData), "LoginData cannot be null");
        }

        System.Diagnostics.Debug.WriteLine($"LoginPopup Cancel: Before clear - Username='{LoginData.Username}', Password='{LoginData.Password}'");
        LoginData.Username = "";
        LoginData.Password = "";
        System.Diagnostics.Debug.WriteLine($"LoginPopup Cancel: After clear - Username='{LoginData.Username}', Password='{LoginData.Password}'");

        await popupService.ClosePopupAsync(Shell.Current);
        System.Diagnostics.Debug.WriteLine("LoginPopup Cancel: Popup closed");
    }

    [RelayCommand]
    async Task Ok()
    {
        if (LoginData is null)
        {
            throw new ArgumentNullException(nameof(LoginData), "LoginData cannot be null");
        }
        
        var authResult = await peaAdapter.ValidateCredential(LoginData.Username, LoginData.Password);

        if (authResult)
        {
            await popupService.ClosePopupAsync(Shell.Current);
        }

        FailLoginMessage = "Wrong user name or password";
    }

    partial void OnFailLoginMessageChanged(string value)
    {
        IsFailLoginMessageVisible = !string.IsNullOrEmpty(FailLoginMessage);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        LoginData = (AuthData)query[nameof(AuthData)];

        if (LoginData is null)
        {
            throw new ArgumentNullException(nameof(LoginData), "LoginData cannot be null");
        }
        
        string userName = "020027734057";
        string password = "Bondegatan#16b";
        
        LoginData.Username = userName;
        LoginData.Password = password;
    }
}
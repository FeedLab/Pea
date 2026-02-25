using CommunityToolkit.Mvvm.Messaging;
using Pea.Meter.Helper;
using Pea.Infrastructure.Models;
using Pea.Meter.Models;
using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter
{
    public partial class MainPage : ContentPage
    {
        private readonly AuthDataOptions authDataOptions;
        private readonly IAuthData? authData;
        private readonly CustomerProfileViewModel customerProfile;

        public MainPage()
        {
            InitializeComponent();
            
            if (AppService.Current != null)
                BindingContext = AppService.Current.GetRequiredService<MainPageViewModel>();
            else
                throw new InvalidOperationException("AppService is not initialized");
            
            authDataOptions = AppService.Current.GetRequiredService<AuthDataOptions>();
            customerProfile = AppService.Current.GetRequiredService<CustomerProfileViewModel>();
            
            authData = authDataOptions.AuthData;
            
            TabData.IsVisible = false;
            TabCustomerProfile.IsVisible = false;
            
            WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    TabData.IsVisible = true;
                    TabCustomerProfile.IsVisible = true;
                    
                }); 
            });
            
            WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this, (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    TabData.IsVisible = false;
                    TabCustomerProfile.IsVisible = false;
                }); 
            });
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            if (authData is not null && authData.Username != "" && authData.Password != "")
            {
                await customerProfile.RefreshProfile(authData.Username, authData.Password);
                WeakReferenceMessenger.Default.Send(new UserLoggedInMessage(authData));
            }
        }
    }
}
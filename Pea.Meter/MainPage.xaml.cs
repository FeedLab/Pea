using CommunityToolkit.Mvvm.Messaging;
using Pea.Meter.Helper;
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
        private readonly StorageService storageService;
        private bool hasAppeared;

        public MainPage()
        {
            InitializeComponent();

            if (AppService.Current != null)
                BindingContext = AppService.Current.GetRequiredService<MainPageViewModel>();
            else
                throw new InvalidOperationException("AppService is not initialized");

            storageService  = AppService.Current.GetRequiredService<StorageService>();
            
            authDataOptions = AppService.Current.GetRequiredService<AuthDataOptions>();
            customerProfile = AppService.Current.GetRequiredService<CustomerProfileViewModel>();

            authData = authDataOptions.AuthData;

            TabData.IsVisible = false;
            TabCustomerProfile.IsVisible = false;
            Pea.IsVisible = true;
            TouVsFlatRate.IsVisible = false;
            Info.IsVisible = false;
            SolarSystemSizing.IsVisible = false;

            TabView.SelectedIndex = 0;

            WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    TabData.IsVisible = true;
                    TabCustomerProfile.IsVisible = true;
                    Pea.IsVisible = false;
                    Info.IsVisible = true;
                    TouVsFlatRate.IsVisible = true;
                    SolarSystemSizing.IsVisible = true;

                    TabView.SelectedIndex = 0;
                    
                    return Task.CompletedTask;
                });
            });

            WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this, (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    TabData.IsVisible = false;
                    TabCustomerProfile.IsVisible = false;
                    Pea.IsVisible = true;
                    TouVsFlatRate.IsVisible = false;
                    Info.IsVisible = false;
                    SolarSystemSizing.IsVisible = false;

                    TabView.SelectedIndex = 0;
                    
                    return Task.CompletedTask;
                });
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (hasAppeared)
                return;

            hasAppeared = true;

            if (storageService.IsAuthenticated)
            {
                WeakReferenceMessenger.Default.Send(new UserLoggedInMessage(authData));
            }
            // if (authData is not null && authData.Username != "" && authData.Password != "")
            // {
            //     await customerProfile.RefreshProfile(authData.Username, authData.Password);
            //     await storageService.Init();
            //     WeakReferenceMessenger.Default.Send(new UserLoggedInMessage(authData));
            // }
        }
    }
}
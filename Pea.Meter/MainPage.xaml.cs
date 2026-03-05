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
            Pea.IsVisible = false;
            TouVsFlatRate.IsVisible = false;
            
            WeakReferenceMessenger.Default.Register<UserLoggedInMessage>(this, (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    TabData.IsVisible = true;
                    TabCustomerProfile.IsVisible = true;
                    Pea.IsVisible = true;
                    Info.IsVisible = true;
                    TouVsFlatRate.IsVisible = true;
                });
            });

            WeakReferenceMessenger.Default.Register<UserLoggedOutMessage>(this, (r, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    TabData.IsVisible = false;
                    TabCustomerProfile.IsVisible = false;
                    Pea.IsVisible = false;
                    TouVsFlatRate.IsVisible = false;
                    Info.IsVisible = true;
                    
                    TabView.SelectedIndex = 0;
                });
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (storageService.IsAuthenticated)
            {
                WeakReferenceMessenger.Default.Send(new UserLoggedInMessage(authData));
                WeakReferenceMessenger.Default.Send(new AllAggregationsCompletedMessage());
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
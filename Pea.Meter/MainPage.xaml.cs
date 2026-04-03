using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.TabView;
using Pea.Meter.Helpers;
using Pea.Meter.Helpers;
using Pea.Meter.Models;
using Pea.Meter.Services;
using Pea.Meter.View.Solar;
using Pea.Meter.View.Statistics;
using Pea.Meter.ViewModel;
using Pea.Meter.ViewModel.Interface;
using Pea.Meter.ViewModel.Statistics;

namespace Pea.Meter
{
    public partial class MainPage : ContentPage
    {
        private readonly AuthDataOptions authDataOptions;
        private readonly IAuthData? authData;
        private readonly CustomerProfileViewModel customerProfile;
        private readonly StorageService storageService;
        private bool hasAppeared;
        private readonly ILogger<MainPage> logger;
        private ICanExecuteViewModel? oldViewModel;
        private readonly NewDayBackgroundTimer newDayBackgroundTimer;
        private readonly HistoricDataBackgroundService historicDataBackgroundService;

        public MainPage()
        {
            InitializeComponent();

            historicDataBackgroundService = AppService.GetRequiredService<HistoricDataBackgroundService>();
            newDayBackgroundTimer = AppService.GetRequiredService<NewDayBackgroundTimer>();
            BindingContext = AppService.GetRequiredService<MainPageViewModel>();
            storageService = AppService.GetRequiredService<StorageService>();
            authDataOptions = AppService.GetRequiredService<AuthDataOptions>();
            customerProfile = AppService.GetRequiredService<CustomerProfileViewModel>();
            logger = AppService.GetRequiredService<ILogger<MainPage>>();

            authData = authDataOptions.AuthData;
            StatisticsView.IsVisible = false;
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
                    StatisticsView.IsVisible = true;
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
                    StatisticsView.IsVisible = false;
                    TabCustomerProfile.IsVisible = false;
                    Pea.IsVisible = true;
                    TouVsFlatRate.IsVisible = false;
                    Info.IsVisible = false;
                    SolarSystemSizing.IsVisible = false;

                    TabView.SelectedIndex = 0;

                    return Task.CompletedTask;
                });
                
                historicDataBackgroundService.Stop();
                newDayBackgroundTimer.Stop();
            });
            
            WeakReferenceMessenger.Default.Register<DateChangedMessage>(this,
                (r, m) =>
                {
                    MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Error in {Method}: {Message}", nameof(DateChangedMessage), e.Message);
                        }

                        return Task.CompletedTask;
                    });
                });
        }

        private void TabView_OnSelectionChanged(object? sender, TabSelectionChangedEventArgs e)
        {
            oldViewModel = null;

            var selectedIndex = (int)e.NewIndex;

            if (oldViewModel != null)
            {
                oldViewModel.CanExecute(false);
            }

            if (selectedIndex == TabView.Items.IndexOf(SolarSystemSizing))
            {
                if (SolarSystemSizing.Content is SolarSystemSizingView view)
                {
                    var viewModel = AppService.GetRequiredService<SolarSystemSizingViewModel>();
                    viewModel.CanExecute(true);
                    oldViewModel = viewModel;
                }
            }
            else if (selectedIndex == TabView.Items.IndexOf(StatisticsView))
            {
                if (StatisticsView.Content is StatisticsView view)
                {
                    var viewModel = AppService.GetRequiredService<MeterReadingsDailyViewModel>();
                    viewModel.CanExecute(true);
                    oldViewModel = viewModel;
                }
            }
        }

        protected override async void OnAppearing()
        {
            try
            {
                base.OnAppearing();

                if (hasAppeared)
                    return;

                hasAppeared = true;

                if (storageService.IsAuthenticated)
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            try
                            {

                                WeakReferenceMessenger.Default.Send(new UserLoggedInMessage(authData));
                               
                                await storageService.ResetHistoricalData();
                                
                                WeakReferenceMessenger.Default.Send(new AllAggregationsCompletedMessage());
                                
                                await newDayBackgroundTimer.Start();
                                
                                historicDataBackgroundService.Start(10);
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, "Error in {Method}: {Message}", nameof(OnAppearing), e.Message);
                            }

                            return Task.CompletedTask;
                        }
                        catch (Exception exception)
                        {
                            return Task.FromException(exception);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in {Method}: {Message}", nameof(OnAppearing), e.Message);
            }
        }
    }
}
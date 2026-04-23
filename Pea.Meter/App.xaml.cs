using Serilog;

namespace Pea.Meter
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    Log.Fatal(ex, "Unhandled AppDomain exception: {Message}", ex.Message);
                }
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Log.Fatal(e.Exception, "Unobserved Task exception: {Message}", e.Exception.Message);
                e.SetObserved();
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
            // {
            //     var mainPageNotSupportedWindow = new Window(new MainPageNotSupported());
            //
            //     return mainPageNotSupportedWindow;
            // }
            
            var window = new Window(new AppShell());

            if (DeviceInfo.Platform == DevicePlatform.WinUI ||
                DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            {
                var scale = 0.65;
                window.Width = 1920 * scale;
                window.Height = 1080 * scale;
            }
              
            return window;
        }
    }
}
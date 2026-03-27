namespace Pea.Meter
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Exception ex = e.ExceptionObject as Exception;
                // Log or handle
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                // Handle async task exceptions
                e.SetObserved();
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
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
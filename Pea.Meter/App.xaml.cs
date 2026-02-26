using Microsoft.Extensions.DependencyInjection;

namespace Pea.Meter
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            if (DeviceInfo.Platform == DevicePlatform.WinUI || 
                DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            {
                window.Width = 1600;
                window.Height = 1024;
            }

            return window;
        }
    }
}
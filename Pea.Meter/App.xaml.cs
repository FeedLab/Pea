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
            var window = new Window(new AppShell())
            {
                Width = 1600,
                Height = 1024
            };
            return window;
        }
    }
}
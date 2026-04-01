using Pea.Data;
using Pea.Meter.Services;

namespace Pea.Meter
{
    public partial class LoadingPage : ContentPage
    {
        public LoadingPage()
        {
            InitializeComponent();
            
            Appearing += (sender, args) =>
            {
                // Run migrations once at startup, off the UI thread
                var factory = AppService.GetRequiredService<PeaDbContextFactory>();
                Task.Run(async () => await factory.MigrateAsync()).GetAwaiter().GetResult();
            };
        }
    }
}

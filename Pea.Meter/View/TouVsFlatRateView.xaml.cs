using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter.View;

public partial class TouVsFlatRateView : ContentView
{
    public TouVsFlatRateView()
    {
        InitializeComponent();
        
        if (AppService.Current != null)
            BindingContext = AppService.Current.GetRequiredService<TouVsFlatRateViewModel>();
        else
            throw new InvalidOperationException("AppService is not initialized");
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter.View.Solar;

public partial class SolarSystemSizingView : ContentView
{
    public SolarSystemSizingView()
    {
        InitializeComponent();
        
        if (AppService.Current != null)
            BindingContext = AppService.Current.GetRequiredService<SolarSystemSizingViewModel>();
        else
            throw new InvalidOperationException("AppService is not initialized");
    }
}
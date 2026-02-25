using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter.View;

public partial class InfoView : ContentView
{
    public InfoView()
    {
        InitializeComponent();
        
        if (AppService.Current != null)
            BindingContext = AppService.Current.GetRequiredService<InfoViewModel>();
        else
            throw new InvalidOperationException("AppService is not initialized");
    }
  

}
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
    private readonly SolarSystemSizingViewModel viewModel;

    public SolarSystemSizingView()
    {
        InitializeComponent();
        viewModel = AppService.GetRequiredService<SolarSystemSizingViewModel>();

        BindingContext = viewModel;
    }

}
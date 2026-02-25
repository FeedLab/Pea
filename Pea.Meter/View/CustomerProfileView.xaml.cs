using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter.View;

public partial class CustomerProfileView : ContentView
{
    private readonly CustomerProfileViewModel viewModel;

    public CustomerProfileView()
    {
        InitializeComponent();

        if (AppService.Current != null)
        {
            viewModel = AppService.Current.GetRequiredService<CustomerProfileViewModel>();
            BindingContext = viewModel;
        }
        else
            throw new InvalidOperationException("AppService is not initialized");
    }
    
    public async Task RefreshProfile(string user, string pwd)
    {
        await viewModel.RefreshProfile(user, pwd);
    }
}
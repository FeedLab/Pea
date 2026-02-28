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
    private readonly TouVsFlatRateViewModel viewModel;

    public TouVsFlatRateView()
    {
        InitializeComponent();
        if (AppService.Current != null)
        {
            viewModel = AppService.Current.GetRequiredService<TouVsFlatRateViewModel>();
            BindingContext = viewModel;
        }
        else
            throw new InvalidOperationException("AppService is not initialized");
    }

    private void OnStartDateTapGestureTapped(object? sender, TappedEventArgs e)
    {
#if ANDROID || IOS
        // this.StartTimePicker.Reset();
        this.StartTimePicker.IsOpen = true;
#else
        // this.StartTimePicker.Reset();
        this.StartTimePicker.IsOpen = true;
#endif
        

    }
    
    private void OnEndDateTapGestureTapped(object? sender, TappedEventArgs e)
    {
#if ANDROID || IOS
        // this.EndTimePicker.Reset();
        this.EndTimePicker.IsOpen = true;
#else
        // this.EndTimePicker.Reset();
        this.EndTimePicker.IsOpen = true;
#endif
        

    }

    private async void OnEndDatePickerOkButtonClicked(object? sender, EventArgs e)
    {
        if (EndTimePicker.SelectedDate == null)
            return;
        
        viewModel.EndDate = EndTimePicker.SelectedDate.Value;
        viewModel.StartTimePickerMaximumDate = viewModel.EndDate.AddDays(-1);
        await viewModel.CalculateCostComparisons();
    }

    private async void OnStartDatePickerOkButtonClicked(object? sender, EventArgs e)
    {
        if (StartTimePicker.SelectedDate == null)
            return;
        
        viewModel.StartDate = StartTimePicker.SelectedDate.Value;
        
        viewModel.EndTimePickerMinimumDate = viewModel.StartDate.AddDays(1);
        await viewModel.CalculateCostComparisons();
    }
}
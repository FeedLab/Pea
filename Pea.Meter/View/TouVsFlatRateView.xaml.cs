using Pea.Meter.Services;
using Pea.Meter.ViewModel;

namespace Pea.Meter.View;

public partial class TouVsFlatRateView : ContentView
{
    private readonly TouVsFlatRateViewModel viewModel;

    public TouVsFlatRateView()
    {
        InitializeComponent();

        viewModel = AppService.GetRequiredService<TouVsFlatRateViewModel>();
        BindingContext = viewModel;
    }

    private async void OnStartDateSelected(object? sender, DateTime selectedDate)
    {
        viewModel.StartDate = selectedDate;
        viewModel.EndTimePickerMinimumDate = viewModel.StartDate.AddDays(1);
        await viewModel.CalculateCostComparisons();
    }

    private async void OnEndDateSelected(object? sender, DateTime selectedDate)
    {
        viewModel.EndDate = selectedDate;
        viewModel.StartTimePickerMaximumDate = viewModel.EndDate.AddDays(-1);
        await viewModel.CalculateCostComparisons();
    }
}
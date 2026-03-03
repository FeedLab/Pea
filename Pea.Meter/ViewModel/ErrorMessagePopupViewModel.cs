using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

public partial class ErrorMessagePopupViewModel : ObservableObject, IQueryAttributable
{
    private readonly IPopupService popupService;
  
    [ObservableProperty] private string label;
    [ObservableProperty] private string errorMessage;

    public ErrorMessagePopupViewModel()
    {
        Label = "Error";
        ErrorMessage = "Something went wrong. Please try again.";
        
        popupService = AppService.GetRequiredService<IPopupService>();
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Label = query[nameof(Label)] as string ?? string.Empty;
        ErrorMessage = query[nameof(ErrorMessage)] as string ?? string.Empty;
    }
    
    [RelayCommand]
    private void Ok()
    {
        ClosePopup();
    }
    
    private void ClosePopup()
    {
        popupService.ClosePopupAsync(Shell.Current, CancellationToken.None);
    }
}
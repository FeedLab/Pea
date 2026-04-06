using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pea.Meter.Services;

namespace Pea.Meter.ViewModel;

public partial class LogViewModel : ObservableObject
{
    private readonly MemoryLogSink sink;

    [ObservableProperty] private string logContent = string.Empty;

    public LogViewModel(MemoryLogSink sink)
    {
        this.sink = sink;
    }

    [RelayCommand]
    private void Clear()
    {
        sink.Clear();
        LogContent = string.Empty;
    }

    [RelayCommand]
    private void Refresh()
    {
        LogContent = sink.GetContent();
    }
}

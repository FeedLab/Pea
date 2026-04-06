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

    [RelayCommand]
    private async Task RefreshFromFile()
    {
        var logDir = Path.Combine(FileSystem.AppDataDirectory, "logs");
        var today = DateTime.Today.ToString("yyyyMMdd");
        var logPath = Path.Combine(logDir, $"pea{today}.log");

        if (!File.Exists(logPath) && Directory.Exists(logDir))
        {
            logPath = Directory.GetFiles(logDir, "pea*.log")
                .OrderByDescending(f => f)
                .FirstOrDefault() ?? logPath;
        }

        if (File.Exists(logPath))
        {
            await using var stream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            LogContent = await reader.ReadToEndAsync();
        }
        else
        {
            LogContent = "No log file found.";
        }
    }
}

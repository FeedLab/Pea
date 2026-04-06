using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Pea.Meter.Services;

public class MemoryLogSink : ILogEventSink
{
    private readonly MessageTemplateTextFormatter formatter =
        new("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

    private readonly Queue<string> entries = new();
    private readonly object syncLock = new();
    private const int MaxEntries = 2000;

    public void Emit(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        formatter.Format(logEvent, writer);
        var line = writer.ToString().TrimEnd();

        lock (syncLock)
        {
            entries.Enqueue(line);
            while (entries.Count > MaxEntries)
                entries.Dequeue();
        }
    }

    public void Clear()
    {
        lock (syncLock)
            entries.Clear();
    }

    public string GetContent()
    {
        lock (syncLock)
            return string.Join(Environment.NewLine, entries);
    }
}

namespace Pea.Meter.View.Interface;

public interface ICloseable
{
    event EventHandler? CloseRequested;

    void CloseAction();
}

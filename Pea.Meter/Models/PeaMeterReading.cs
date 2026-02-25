namespace Pea.Meter.Models;

/// <summary>
/// Model representing a meter reading for a specific time period
/// </summary>
public class PeaMeterReading
{
    public PeaMeterReading(DateTime periodStart, decimal rateA, decimal rateB, decimal rateC)
    {
        PeriodStart = periodStart;
        RateA = rateA;
        RateB = rateB;
        RateC = rateC;
    }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd => PeriodStart.AddMinutes(15).AddMilliseconds(-1);

    public decimal RateA { get;  }

    public decimal RateB { get;  }

    public decimal RateC { get;  }

    public decimal Total => RateA + RateB + RateC;

    public override string ToString()
    {
        return $"PeriodStart: {PeriodStart:dd/MM/yyyy HH:mm}, PeriodEnd: {PeriodEnd:dd/MM/yyyy HH:mm}, RateA: {RateA}, RateB: {RateB}, RateC: {RateC}";
    }
}

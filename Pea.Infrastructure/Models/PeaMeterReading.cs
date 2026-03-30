using Pea.Infrastructure.Helpers;

namespace Pea.Infrastructure.Models;

/// <summary>
/// Model representing a meter reading for a specific time period
/// </summary>
public class PeaMeterReading
{
    private readonly int periodLength;

    public PeaMeterReading(DateTime periodStart, decimal rateA, decimal rateB, decimal rateC, int periodLength = 15)
    {
        this.periodLength = periodLength;
        PeriodStart = periodStart;
        RateA = rateA;
        RateB = rateB;
        RateC = rateC;
    }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd => PeriodStart.AddMinutes(periodLength).AddMilliseconds(-1);

    public decimal RateA { get;  }

    public decimal RateB { get;  }

    public decimal RateC { get;  }

    public decimal Total => RateA + RateB + RateC;
    public decimal Peek => RateA;
    public decimal OffPeek => RateB + RateC;

    public string PeekFormatted => WattFormatter.Format(Peek * 1000);
    public string OffPeekFormatted => WattFormatter.Format(OffPeek * 1000);
    public string TotalFormatted => WattFormatter.Format(Total * 1000);


    public override string ToString()
    {
        return $"PeriodStart: {PeriodStart:dd/MM/yyyy HH:mm}, PeriodEnd: {PeriodEnd:dd/MM/yyyy HH:mm}, RateA: {RateA}, RateB: {RateB}, RateC: {RateC}";
    }
}

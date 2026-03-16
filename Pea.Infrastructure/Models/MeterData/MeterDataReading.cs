using CommunityToolkit.Mvvm.ComponentModel;

namespace Pea.Infrastructure.Models.MeterData;

public enum CostType
{
    Holiday,
    Peek,
    OffPeek
}

public partial class MeterDataReading : ObservableObject
{
    private readonly int periodLength;
    public MeterDataReading(DateTime periodStart, decimal peekUsage, decimal offPeekUsage, decimal holidayUsage, int periodLength = 15)
    {
        this.periodLength = periodLength;
        PeriodStart = periodStart;
        PeekUsage = peekUsage;
        OffPeekUsage = offPeekUsage;
        HolidayUsage = holidayUsage;
        
        CostType = GetCostType();
    }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd => PeriodStart.AddMinutes(periodLength).AddMilliseconds(-1);

    public CostType CostType { get;  }
    
    public decimal PeekUsage { get;  }

    public decimal OffPeekUsage { get;  }

    public decimal HolidayUsage { get;  }

    public decimal Total => PeekUsage + OffPeekUsage + HolidayUsage;
    
    
    
    public CostType GetCostType()
    {
        var isWeekday = PeriodStart.DayOfWeek >= DayOfWeek.Monday &&
                        PeriodStart.DayOfWeek <= DayOfWeek.Friday;
        var isWeekend = !isWeekday;

        if (isWeekend)
        {
            return CostType.Holiday;
        }
        
        if (PeriodStart.Hour is >= 9 and < 22 && isWeekday)
        {
            return CostType.Peek;
        }

        return CostType.OffPeek;
    }
}
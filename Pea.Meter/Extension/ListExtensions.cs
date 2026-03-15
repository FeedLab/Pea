namespace Pea.Meter.Extension;

public static class ListExtensions
{
    /// <summary>
    /// Returns the closest value in the list that is strictly greater than the target.
    /// If none exist, returns default(T).
    /// </summary>
    public static T ClosestGreater<T>(this List<T> list, T target) where T : struct, IComparable<T>
    {
        // Filter values greater than target
        var greaterValues = list
            .Where(x => x.CompareTo(target) > 0)
            .ToList();

        if (!greaterValues.Any())
            return default;

        // Return the smallest of the greater values
        return greaterValues.Min();
    }
}

public static class NumberExtensions
{
    /// <summary>
    /// Rounds the number up to the nearest multiple of 5.
    /// </summary>
    public static int RoundUpToNearestFive(this int value)
    {
        if(value <= 3)
            return 3;
        
        if (value % 5 == 0)
            return value;

        return ((value / 5) + 1) * 5;
    }
    
    /// <summary>
    /// Rounds the number up to the nearest multiple of 5.
    /// </summary>
    public static decimal RoundUpToNearestFive(this decimal value)
    {
        if(value <= 3.0m)
            return 3.0m;
        
        if (value % 5 == 0)
            return value;
    
        return Math.Ceiling(value / 5.0m) * 5.0m;
    }

    /// <summary>
    /// Rounds the number up to the nearest multiple of 5.
    /// </summary>
    public static double RoundUpToNearestFive(this double value)
    {
        if(value <= 3.0)
            return 3.0;
        
        if (value % 5 == 0)
            return value;
    
        return Math.Ceiling(value / 5.0) * 5.0;
    }
}


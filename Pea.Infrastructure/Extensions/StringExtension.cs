namespace Pea.Infrastructure.Extensions;

public static class StringExtension
{
    /// <summary>
    /// Determines whether the string is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Determines whether the string is null, empty, or consists only of white-space characters.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Determines whether the string has a value (not null, empty, or whitespace).
    /// </summary>
    public static bool HasValue(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Determines whether the string is empty (zero length, but not null).
    /// </summary>
    public static bool IsEmpty(this string? value)
    {
        return value != null && value.Length == 0;
    }

    /// <summary>
    /// Trims the string and returns null if the result is empty or whitespace.
    /// </summary>
    public static string? TrimToNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    /// <summary>
    /// Trims the string and returns empty string if the value is null.
    /// </summary>
    public static string TrimOrEmpty(this string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Compares two strings ignoring case.
    /// </summary>
    public static bool EqualsIgnoreCase(this string? value, string? other)
    {
        return string.Equals(value, other, StringComparison.OrdinalIgnoreCase);
    }
}
namespace Pea.Infrastructure.Helpers;

public static class MathHelpers
{
    public static decimal ClampToZero(decimal value)
    {
        return Math.Max(0, value);
    }
}

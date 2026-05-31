namespace AutoTallerManager.Application.Common;

public static class DateTimeUtcHelper
{
    public static DateTime? AsUtcStartOfDay(DateTime? value)
    {
        if (value is null) return null;

        var d = value.Value;
        if (d.Kind == DateTimeKind.Utc)
            return d;

        return new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    public static DateTime? AsUtcEndOfDay(DateTime? value)
    {
        if (value is null) return null;

        var d = value.Value;
        if (d.Kind == DateTimeKind.Utc)
            return d;

        return new DateTime(d.Year, d.Month, d.Day, 23, 59, 59, 999, DateTimeKind.Utc);
    }
}

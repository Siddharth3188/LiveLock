using System.Globalization;
using LiveLock.Core.Models;

namespace LiveLock.Core.Formatting;

/// <summary>Pure, UI-free formatting of the clock and date. Time is a parameter so it is fully testable.</summary>
public static class ClockFormatter
{
    public static string FormatTime(DateTime now, ClockSettings s, CultureInfo culture)
    {
        if (s.Use24Hour) return now.ToString("HH:mm", culture);
        var text = now.ToString("h:mm", culture);
        if (s.ShowAmPm)
        {
            var d = culture.DateTimeFormat;
            text += " " + (now.Hour < 12 ? d.AMDesignator : d.PMDesignator);
        }
        return text.Trim();
    }

    public static string FormatDate(DateTime now, ClockSettings s, CultureInfo culture)
    {
        if (!s.ShowDate) return "";
        string? weekday = s.ShowWeekday ? now.ToString(s.AbbreviatedNames ? "ddd" : "dddd", culture) : null;
        string? month = s.ShowMonth ? now.ToString(s.AbbreviatedNames ? "MMM" : "MMMM", culture) : null;
        string? day = s.ShowDay ? now.ToString("%d", culture) : null;
        string? year = s.ShowYear ? now.ToString("yyyy", culture) : null;

        var pattern = culture.DateTimeFormat.ShortDatePattern;
        int m = pattern.IndexOf('M'), d = pattern.IndexOf('d');
        bool monthFirst = m < 0 || d < 0 || m < d;

        var parts = monthFirst ? new[] { month, day } : new[] { day, month };
        var monthDay = string.Join(" ", parts.Where(p => p is not null));

        if (year is not null)
            monthDay = monthDay.Length == 0 ? year : monthFirst && day is not null ? monthDay + ", " + year : monthDay + " " + year;

        return string.Join(", ", new[] { weekday, monthDay.Length > 0 ? monthDay : null }.Where(p => p is not null));
    }
}

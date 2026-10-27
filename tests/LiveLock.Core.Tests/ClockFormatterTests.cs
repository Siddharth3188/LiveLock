using System.Globalization;
using LiveLock.Core.Formatting;
using LiveLock.Core.Models;
using Xunit;

namespace LiveLock.Core.Tests;

public class ClockFormatterTests
{
    private static readonly DateTime Now = new(2026, 10, 27, 22, 45, 0); // a Tuesday
    private static readonly CultureInfo US = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo GB = CultureInfo.GetCultureInfo("en-GB");

    [Fact] public void Time_24Hour() => Assert.Equal("22:45", ClockFormatter.FormatTime(Now, new ClockSettings { Use24Hour = true }, US));
    [Fact] public void Time_12Hour_NoAmPm() => Assert.Equal("10:45", ClockFormatter.FormatTime(Now, new ClockSettings(), US));
    [Fact] public void Time_12Hour_WithAmPm() => Assert.Equal("10:45 PM", ClockFormatter.FormatTime(Now, new ClockSettings { ShowAmPm = true }, US));
    [Fact] public void Time_Midnight_Is12() => Assert.Equal("12:05", ClockFormatter.FormatTime(new DateTime(2026, 1, 1, 0, 5, 0), new ClockSettings(), US));

    [Fact] public void Date_Default() => Assert.Equal("Tuesday, October 27", ClockFormatter.FormatDate(Now, new ClockSettings(), US));
    [Fact] public void Date_WithYear() => Assert.Equal("Tuesday, October 27, 2026", ClockFormatter.FormatDate(Now, new ClockSettings { ShowYear = true }, US));
    [Fact] public void Date_NoWeekdayWithYear() => Assert.Equal("October 27, 2026", ClockFormatter.FormatDate(Now, new ClockSettings { ShowWeekday = false, ShowYear = true }, US));
    [Fact] public void Date_WeekdayOnly() => Assert.Equal("Tuesday", ClockFormatter.FormatDate(Now, new ClockSettings { ShowDay = false, ShowMonth = false }, US));
    [Fact] public void Date_Abbreviated_GB() => Assert.Equal("Tue, 27 Oct", ClockFormatter.FormatDate(Now, new ClockSettings { AbbreviatedNames = true }, GB));
    [Fact] public void Date_Hidden() => Assert.Equal("", ClockFormatter.FormatDate(Now, new ClockSettings { ShowDate = false }, US));
}

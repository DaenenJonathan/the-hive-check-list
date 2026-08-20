using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace TheHive.Infrastructure.Excel;

// Time cells vary by template: some are genuine Excel time values, others are plain text typed
// as "07h45" or "18u30" (NL) - a leading-hour-then-separator pattern is tried as a last resort.
public static class TimeParser
{
    private static readonly Regex HourMinute = new(@"^\s*(\d{1,2})\s*[hHuU:.]\s*(\d{0,2})", RegexOptions.Compiled);

    // Excel's serial-date baseline: a cell holding only a time-of-day (e.g. "7:30") round-trips through
    // ClosedXML as this date plus the time fraction. A cell with any other date is a real calendar date
    // (e.g. the BUILD-UP row's own DATE column) and must not be misread as a time.
    private static readonly DateTime TimeOnlyEpoch = new(1899, 12, 30);

    public static bool TryParse(XLCellValue value, out TimeSpan time)
    {
        time = default;

        if (value.IsBlank || value.IsError) return false;

        if (value.IsTimeSpan)
        {
            time = value.GetTimeSpan();
            return true;
        }

        if (value.IsDateTime)
        {
            var dateTime = value.GetDateTime();
            if (dateTime.Date != TimeOnlyEpoch) return false;

            time = dateTime.TimeOfDay;
            return true;
        }

        var text = (value.IsText ? value.GetText() : value.ToString())?.Trim();
        if (string.IsNullOrEmpty(text)) return false;

        if (TimeSpan.TryParse(text, out time)) return true;

        var match = HourMinute.Match(text);
        if (!match.Success) return false;

        var hours = int.Parse(match.Groups[1].Value);
        var minutes = match.Groups[2].Value.Length > 0 ? int.Parse(match.Groups[2].Value) : 0;
        if (hours > 23 || minutes > 59) return false;

        time = new TimeSpan(hours, minutes, 0);
        return true;
    }
}

using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace TheHive.Infrastructure.Excel;

public static class QuantityParser
{
    // Anchored to the start of the string so a stray digit elsewhere in unrelated text is never
    // mistaken for a quantity (e.g. "70 trays " -> 70, "1 box" -> 1).
    private static readonly Regex LeadingInteger = new(@"^\s*(\d+)", RegexOptions.Compiled);

    public static bool TryParse(XLCellValue value, out int qty)
    {
        qty = 0;

        if (value.IsBlank || value.IsError) return false;

        if (value.IsNumber)
        {
            qty = (int)Math.Round(value.GetNumber());
            return true;
        }

        var text = value.IsText ? value.GetText() : value.ToString();
        if (string.IsNullOrWhiteSpace(text)) return false;

        if (int.TryParse(text.Trim(), out qty)) return true;

        var match = LeadingInteger.Match(text);
        return match.Success && int.TryParse(match.Groups[1].Value, out qty);
    }
}

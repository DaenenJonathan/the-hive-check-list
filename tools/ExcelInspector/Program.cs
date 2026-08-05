using ClosedXML.Excel;

var filePath = args.Length > 0 ? args[0] : throw new Exception("Provide file path as argument");

using var wb = new XLWorkbook(filePath);

var sheet = wb.Worksheets
    .Where(w => w.Visibility == XLWorksheetVisibility.Visible)
    .FirstOrDefault(w => w.Name.Contains("Material", StringComparison.OrdinalIgnoreCase))
    ?? wb.Worksheets.First(w => w.Visibility == XLWorksheetVisibility.Visible);

Console.WriteLine($"Sheet: {sheet.Name}");

// Metadata
Console.WriteLine("\n=== METADATA ===");
Console.WriteLine($"Client:       {Get(sheet, 3, 3)}");
Console.WriteLine($"Brand:        {Get(sheet, 4, 3)}");
Console.WriteLine($"ProjectName:  {Get(sheet, 5, 3)}");
Console.WriteLine($"ActionType:   {Get(sheet, 6, 3)}");
Console.WriteLine($"EventDate:    {sheet.Cell(9, 3).Value}");
Console.WriteLine($"Account:      {Get(sheet, 12, 3)}");
Console.WriteLine($"CostCode:     {Get(sheet, 12, 10)}");
Console.WriteLine($"AddressPickUp:{Get(sheet, 13, 3)}");
Console.WriteLine($"AddressAction:{Get(sheet, 15, 3)}");

// Items
Console.WriteLine("\n=== ITEMS ===");
var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
string currentCategory = "";
int count = 0;

var footerKeywords = new[] { "WAREHOUSE", "PREPARATION :", "RETOUR :", "EXTRA INFO",
    "Upon collection", "The receipt", "The organisation", "Materials are",
    "SIGANTURE", "SIGNATURE FOR", "NAME RESPONSIBLE" };

for (int row = 20; row <= lastRow; row++)
{
    var col1 = Get(sheet, row, 1);
    var col4 = Get(sheet, row, 4);
    var col7 = sheet.Cell(row, 7).Value;
    var col8 = Get(sheet, row, 8);

    if (!string.IsNullOrWhiteSpace(col1) && footerKeywords.Any(k => col1.StartsWith(k, StringComparison.OrdinalIgnoreCase)))
        break;

    if (col4.Equals("NAME", StringComparison.OrdinalIgnoreCase) ||
        col1.StartsWith("PICTURE", StringComparison.OrdinalIgnoreCase))
        continue;

    if (!string.IsNullOrWhiteSpace(col4) && col7.IsNumber)
    {
        var qty = (int)Math.Round(col7.GetNumber());
        if (qty > 0)
        {
            Console.WriteLine($"  [{currentCategory}] {col4} x{qty}{(string.IsNullOrWhiteSpace(col8) ? "" : $" ({col8})")}");
            count++;
        }
        continue;
    }

    if (!string.IsNullOrWhiteSpace(col1) && string.IsNullOrWhiteSpace(col4))
        currentCategory = col1.Trim();
}

Console.WriteLine($"\nTotal items: {count}");

static string Get(IXLWorksheet ws, int r, int c)
{
    var v = ws.Cell(r, c).Value;
    return v.IsError || v.IsBlank ? "" : v.ToString()?.Trim() ?? "";
}

using ClosedXML.Excel;

namespace TheHive.Infrastructure.Excel;

// Classic Excel "Insert Picture" drops a floating DrawingML shape anchored near a row/column rather
// than truly living inside a cell (unlike the newer in-cell rich-data images ExcelEmbeddedImageReader
// resolves). ClosedXML reads these natively via IXLWorksheet.Pictures, so no raw OOXML parsing is
// needed here - only mapping each picture's anchor to the row it belongs to.
public static class ExcelFloatingPictureReader
{
    public static IReadOnlyDictionary<int, byte[]> ReadImagesByRow(IXLWorksheet sheet)
    {
        var imagesByRow = new Dictionary<int, byte[]>();

        foreach (var picture in sheet.Pictures)
        {
            try
            {
                var row = picture.TopLeftCell.Address.RowNumber;
                using var stream = picture.ImageStream;
                imagesByRow[row] = stream.ToArray();
            }
            catch
            {
                // A single malformed/unreadable picture must never break the import - skip it and
                // keep resolving the rest.
            }
        }

        return imagesByRow;
    }
}

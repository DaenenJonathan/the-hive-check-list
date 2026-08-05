using ClosedXML.Excel;
using FluentAssertions;
using TheHive.Infrastructure.Excel;

namespace TheHive.Infrastructure.Tests.Excel;

public class QuantityParserTests
{
    private static XLCellValue ValueFromCell(Action<IXLCell> setup)
    {
        using var workbook = new XLWorkbook();
        var cell = workbook.AddWorksheet("Sheet1").Cell(1, 1);
        setup(cell);
        return cell.Value;
    }

    [Fact]
    public void TryParse_numeric_cell_returns_the_number()
    {
        var value = ValueFromCell(c => c.Value = 5);

        QuantityParser.TryParse(value, out var qty).Should().BeTrue();
        qty.Should().Be(5);
    }

    [Theory]
    [InlineData("70", 70)]
    [InlineData("70 trays ", 70)]
    [InlineData("1 box", 1)]
    public void TryParse_extracts_leading_integer_from_text(string text, int expected)
    {
        var value = ValueFromCell(c => c.Value = text);

        QuantityParser.TryParse(value, out var qty).Should().BeTrue();
        qty.Should().Be(expected);
    }

    [Fact]
    public void TryParse_blank_cell_returns_false()
    {
        var value = ValueFromCell(_ => { });

        QuantityParser.TryParse(value, out _).Should().BeFalse();
    }

    [Fact]
    public void TryParse_text_without_a_leading_digit_returns_false()
    {
        var value = ValueFromCell(c => c.Value = "no image");

        QuantityParser.TryParse(value, out _).Should().BeFalse();
    }

    [Fact]
    public void TryParse_error_cell_returns_false()
    {
        var value = ValueFromCell(c => c.FormulaA1 = "=1/0");

        QuantityParser.TryParse(value, out _).Should().BeFalse();
    }
}

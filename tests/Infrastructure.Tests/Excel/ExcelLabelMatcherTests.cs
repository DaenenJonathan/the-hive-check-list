using FluentAssertions;
using TheHive.Infrastructure.Excel;
using TheHive.Infrastructure.Excel.Synonyms;

namespace TheHive.Infrastructure.Tests.Excel;

public class ExcelLabelMatcherTests
{
    [Theory]
    [InlineData("CLIENT : ", "CLIENT")]
    [InlineData("BRAND: ", "BRAND")]
    [InlineData("PICTURE ", "PICTURE")]
    [InlineData("  Cost   Code  :  ", "COST CODE")]
    public void Normalize_strips_trailing_punctuation_and_collapses_whitespace(string input, string expected)
    {
        ExcelLabelMatcher.Normalize(input).Should().Be(expected);
    }

    [Fact]
    public void MatchMetadataConcept_does_not_confuse_cost_code_with_account_or_address()
    {
        var normalized = ExcelLabelMatcher.Normalize("COST CODE :");

        ExcelLabelMatcher.MatchMetadataConcept(normalized).Should().Be(MetadataField.CostCode);
    }

    [Fact]
    public void MatchMetadataConcept_does_not_confuse_address_action_with_address_pick_up()
    {
        var normalized = ExcelLabelMatcher.Normalize("ADDRESS ACTION :");

        ExcelLabelMatcher.MatchMetadataConcept(normalized).Should().Be(MetadataField.AddressAction);
    }

    [Fact]
    public void MatchColumnConcepts_on_the_date_mini_header_only_matches_one_concept()
    {
        // Real trap found in the Butik template: row 8 is "NAME | DATE | START | END", a metadata
        // mini-header - not the item table header - but it does contain the literal text "NAME".
        var cells = new (int Column, string Text)[]
        {
            (2, "NAME"), (4, "DATE"), (8, "START"), (11, "END")
        };

        var matched = ExcelLabelMatcher.MatchColumnConcepts(cells);

        matched.Should().ContainKey(ItemColumn.Name);
        matched.Should().HaveCount(1);
    }

    [Fact]
    public void MatchColumnConcepts_on_the_real_item_header_row_matches_every_column()
    {
        var cells = new (int Column, string Text)[]
        {
            (2, "PICTURE "), (5, "NAME"), (8, "UNITS"), (9, "NOTES"),
            (12, "PREP BY"), (13, "# OUT"), (14, "# RETOUR")
        };

        var matched = ExcelLabelMatcher.MatchColumnConcepts(cells);

        matched.Should().HaveCount(7);
        matched[ItemColumn.Name].Should().Be(5);
        matched[ItemColumn.Units].Should().Be(8);
        matched[ItemColumn.Notes].Should().Be(9);
    }
}

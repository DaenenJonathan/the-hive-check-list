using FluentAssertions;
using TheHive.Application.Features.ExcelImports.DTOs;
using TheHive.Infrastructure.Excel;

namespace TheHive.Infrastructure.Tests.Excel;

public class ExcelChecklistParserTests
{
    private static async Task<ExcelImportPreviewDto> ParseFixtureAsync(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
        await using var stream = File.OpenRead(path);
        return await new ExcelChecklistParser().ParseAsync(stream, fileName);
    }

    [Fact]
    public async Task TelenetMixMatch_extracts_metadata_and_tolerates_a_row_with_no_quantity()
    {
        var dto = await ParseFixtureAsync("Butik_TelenetMixMatch.xlsm");

        dto.Errors.Should().BeEmpty();
        dto.Client.Should().Be("TELENET");
        dto.Brand.Should().Be("TELENET");
        dto.ProjectName.Should().Be("DUAL BRAND OPENING");
        dto.ActionType.Should().Be("Activation");
        dto.EventDate.Should().Be(new DateTime(2026, 8, 1));
        dto.Account.Should().Be("Veronique Suys");
        dto.CostCode.Should().Be("bu17001903");
        dto.AddressPickUp.Should().Be("Butik Agency, Schaarbeeklei 647, 1800 Vilvoorde");
        dto.AddressAction.Should().Contain("2300 Turnhout");
        dto.City.Should().Be("Turnhout");

        // BUILD-UP/ARRIVAL start = 07h45 (departure), BREAK-DOWN/RETOUR end = 19h45 (return) -
        // typed as plain text ("07h45") in this fixture, not real Excel time cells.
        dto.PlannedDepartureTime.Should().Be(new TimeSpan(7, 45, 0));
        dto.PlannedReturnTime.Should().Be(new TimeSpan(19, 45, 0));

        // "CTA PANEEL" has a name but no quantity in the real file - must be skipped, not crash the import.
        dto.Items.Should().NotContain(i => i.MaterialName.Contains("CTA PANEEL"));
        dto.Warnings.Should().Contain(w => w.Contains("CTA PANEEL"));

        dto.Items.Should().HaveCount(15);
        dto.Items.Should().ContainSingle(i => i.MaterialName == "MIX&MATCH stand" && i.QuantityRequested == 1 && i.Category == "MATERIALS : BIG");
        dto.Items.Should().ContainSingle(i => i.MaterialName == "PAPER ROLLS" && i.QuantityRequested == 1);
        dto.Items.Should().ContainSingle(i => i.MaterialName == "VERLENGKABEL" && i.QuantityRequested == 1 && i.Notes == "one that works!");
        dto.Items.Should().ContainSingle(i => i.MaterialName == "CAR SCENT - bingefoot" && i.QuantityRequested == 33);
        dto.Items.Should().ContainSingle(i => i.MaterialName == "TELENET TRUI" && i.QuantityRequested == 2 && i.Category == "CLOTHES");
        dto.Items.Should().ContainSingle(i => i.MaterialName == "TELENET JAS" && i.QuantityRequested == 2 && i.Category == "CLOTHES");

        // "MIX&MATCH stand" has a linked picture in the real file; extraction must resolve and
        // optimize it (the raw PNG in xl/media/ is far larger than this).
        dto.Items.Single(i => i.MaterialName == "MIX&MATCH stand").ImageData.Should().NotBeNull();
        // "VERLENGKABEL" has a blank Picture cell in the real file - no image to attach.
        dto.Items.Single(i => i.MaterialName == "VERLENGKABEL").ImageData.Should().BeNull();

        // Categories with a header row but zero items must not appear in the extracted items.
        var categories = dto.Items.Select(i => i.Category).ToHashSet();
        categories.Should().NotContain(c => c != null && c.StartsWith("CONSUMABLES"));
        categories.Should().NotContain("ICE / FRUIT / DRINKS");
        categories.Should().NotContain("MATERIALS : COCKTAIL");
        categories.Should().NotContain("MATERIALS : GLASSES");
        categories.Should().NotContain("MATERIALS : VALUABLES");
        categories.Should().NotContain("EXTRA");
    }

    [Fact]
    public async Task BasePhonebooth_extracts_metadata_with_no_postal_code_in_address()
    {
        var dto = await ParseFixtureAsync("Butik_BasePhonebooth.xlsm");

        dto.Errors.Should().BeEmpty();
        dto.Client.Should().Be("Telenet");
        dto.Brand.Should().Be("BASE");
        dto.ProjectName.Should().Be("BASE - CAMILLE YEMBE");
        dto.ActionType.Should().Be("Field");
        dto.EventDate.Should().Be(new DateTime(2026, 5, 8));
        dto.Account.Should().Be("Yana Rosseel");
        dto.CostCode.Should().Be("bu17001653");
        dto.AddressAction.Should().Be("Heliport LUIK");
        dto.City.Should().BeNull();

        dto.Items.Should().HaveCount(10);
        dto.Items.Should().ContainSingle(i => i.MaterialName == "phonebooth basis" && i.QuantityRequested == 1 && i.Category == "MATERIALS : BIG");
        dto.Items.Should().ContainSingle(i => i.MaterialName == "Branded panels FR" && i.QuantityRequested == 3);
        dto.Items.Should().ContainSingle(i => i.MaterialName == "Ipad medialife" && i.QuantityRequested == 1 && i.Notes == "in cover!" && i.Category == "MATERIALS : VALUABLES");
        dto.Items.Should().ContainSingle(i => i.MaterialName == "BASE CLOTHES" && i.QuantityRequested == 2 && i.Category == "CLOTHES");

        // Only "CLEANING SPRAY" and "CLEANING CLOTH" have a linked picture in this file.
        dto.Items.Single(i => i.MaterialName == "CLEANING SPRAY").ImageData.Should().NotBeNull();
        dto.Items.Single(i => i.MaterialName == "CLEANING CLOTH").ImageData.Should().NotBeNull();
        dto.Items.Single(i => i.MaterialName == "phonebooth basis").ImageData.Should().BeNull();
    }

    [Fact]
    public async Task Zyla_extracts_leading_integer_quantities_from_text_like_units()
    {
        var dto = await ParseFixtureAsync("Butik_Zyla.xlsm");

        dto.Errors.Should().BeEmpty();
        dto.Client.Should().Be("SPADEL");
        dto.Brand.Should().Be("ZYLA");
        dto.ProjectName.Should().Be("ZYLA DIRECT SAMPLING");
        dto.ActionType.Should().Be("Sampling");
        dto.EventDate.Should().Be(new DateTime(2026, 8, 5));
        dto.AddressAction.Should().Be("Rue Neuve Brussel");
        dto.City.Should().BeNull();

        dto.Items.Should().HaveCount(25);

        // The quantity-parsing bug fix: these came from "70 trays " text cells, not plain numbers.
        dto.Items.Should().ContainSingle(i => i.MaterialName == "ZYLA POWER BOOST" && i.QuantityRequested == 70 && i.Category == "ICE / FRUIT / DRINKS");
        dto.Items.Should().ContainSingle(i => i.MaterialName == "ZYLA FOCUS FUEL" && i.QuantityRequested == 70);
        dto.Items.Should().ContainSingle(i => i.MaterialName == "ZYLA PICK ME UP" && i.QuantityRequested == 70);

        // Plain numeric quantities in the same file must still work alongside the text-based ones.
        dto.Items.Should().ContainSingle(i => i.MaterialName == "SMALL STCIKERS" && i.QuantityRequested == 100);

        // "ZYLA POWER BOOST"'s linked picture is a ~140 KB PNG in the real file - after
        // resize/JPEG-recompression it must come back much smaller, not just be a raw passthrough.
        var powerBoostImage = dto.Items.Single(i => i.MaterialName == "ZYLA POWER BOOST").ImageData;
        powerBoostImage.Should().NotBeNull();
        powerBoostImage!.Length.Should().BeLessThan(50_000);
        // "SMALL STCIKERS" has no linked picture in the real file.
        dto.Items.Single(i => i.MaterialName == "SMALL STCIKERS").ImageData.Should().BeNull();

        var categories = dto.Items.Select(i => i.Category).ToHashSet();
        categories.Should().NotContain(c => c != null && c.StartsWith("CONSUMABLES"));
        categories.Should().NotContain("EXTRA");
    }

    [Fact]
    public async Task CanadaDry_resolves_images_pasted_as_classic_floating_pictures_not_just_in_cell_ones()
    {
        var dto = await ParseFixtureAsync("Template_CanadaDry.xlsx");

        dto.Errors.Should().BeEmpty();
        dto.Client.Should().Be("CANADA DRY");

        dto.Items.Should().HaveCount(17);

        // None of these have an in-cell (rich-data) picture in the real file - their images only
        // exist as classic floating DrawingML pictures anchored near their row, the case that used
        // to be silently dropped before ExcelFloatingPictureReader was added.
        dto.Items.Should().ContainSingle(i => i.MaterialName == "Canada Dry (15cl)" && i.QuantityRequested == 3000).Which.ImageData.Should().NotBeNull();
        dto.Items.Should().ContainSingle(i => i.MaterialName.Contains("DIABLE HBS")).Which.ImageData.Should().NotBeNull();
        dto.Items.Should().ContainSingle(i => i.MaterialName.Contains("CUTTER")).Which.ImageData.Should().NotBeNull();

        // "Celsuis Kiwi Guava" and "TOPCARD" genuinely have no picture at all (neither in-cell nor
        // floating) in the real file.
        dto.Items.Single(i => i.MaterialName == "Celsuis Kiwi Guava").ImageData.Should().BeNull();
        dto.Items.Single(i => i.MaterialName.Contains("TOPCARD")).ImageData.Should().BeNull();
    }
}

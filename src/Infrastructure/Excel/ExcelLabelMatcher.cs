using System.Text.RegularExpressions;
using TheHive.Infrastructure.Excel.Synonyms;

namespace TheHive.Infrastructure.Excel;

public static class ExcelLabelMatcher
{
    private static readonly Regex TrailingPunctuation = new(@"[:\.]+\s*$", RegexOptions.Compiled);
    private static readonly Regex MultipleSpaces = new(@"\s+", RegexOptions.Compiled);

    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var noTrailingPunctuation = TrailingPunctuation.Replace(text, string.Empty);
        var collapsed = MultipleSpaces.Replace(noTrailingPunctuation, " ").Trim();
        return collapsed.ToUpperInvariant();
    }

    public static MetadataField? MatchMetadataConcept(string normalizedText)
    {
        if (string.IsNullOrEmpty(normalizedText)) return null;

        foreach (var (field, synonyms) in ExcelFieldSynonyms.MetadataLabels)
        {
            if (synonyms.Contains(normalizedText))
                return field;
        }

        return null;
    }

    public static ItemColumn? MatchColumnConcept(string normalizedText)
    {
        if (string.IsNullOrEmpty(normalizedText)) return null;

        foreach (var (column, synonyms) in ExcelFieldSynonyms.ColumnHeaders)
        {
            if (synonyms.Contains(normalizedText))
                return column;
        }

        return null;
    }

    public static bool IsAnyKnownLabel(string normalizedText) =>
        MatchMetadataConcept(normalizedText) is not null || MatchColumnConcept(normalizedText) is not null;

    // Scores a row's cells against known column-header concepts. Returns the column index (1-based)
    // for each distinct concept matched. A row only "qualifies" as an item-table header if the caller
    // finds >= 2 distinct concepts including Name - callers must apply that rule themselves since what
    // counts as "qualifying" can differ between a strict header check and a looser diagnostic scan.
    public static Dictionary<ItemColumn, int> MatchColumnConcepts(IEnumerable<(int Column, string Text)> cells)
    {
        var result = new Dictionary<ItemColumn, int>();

        foreach (var (column, text) in cells)
        {
            var concept = MatchColumnConcept(Normalize(text));
            if (concept is { } matched && !result.ContainsKey(matched))
                result[matched] = column;
        }

        return result;
    }
}

namespace TheHive.Infrastructure.Excel.Synonyms;

// Synonyms must be the FULL normalized label text (see ExcelLabelMatcher.Normalize), matched by
// exact equality - not a substring - so a short synonym never accidentally matches a longer label
// belonging to a different concept (e.g. "ADDRESS ACTION" vs "ADDRESS PICK UP").
//
// This is the file to extend when a new agency template (Bananas, Wave Agency, ...) is observed:
// append the new label variants to the relevant concept's array, no other code should need to change.
// The EN entries are verified against real Butik template files; the FR/NL entries are best-effort
// translations, not yet verified against a real file.
public static class ExcelFieldSynonyms
{
    public static readonly IReadOnlyDictionary<MetadataField, string[]> MetadataLabels = new Dictionary<MetadataField, string[]>
    {
        [MetadataField.Client] = ["CLIENT", "KLANT"],
        [MetadataField.Brand] = ["BRAND", "MERK", "MARQUE"],
        [MetadataField.ProjectName] = ["PROJECT NAME", "PROJECTNAAM", "NOM DU PROJET"],
        [MetadataField.ActionType] = ["ACTION TYPE", "ACTIETYPE", "TYPE D'ACTION"],
        [MetadataField.EventDate] = ["EVENT / ACTION HOURS", "EVENT DATE", "ACTIE UREN", "HEURES D'ACTION"],
        [MetadataField.DepartureTime] = ["BUILD-UP / ARRIVAL", "OPBOUW / AANKOMST", "MONTAGE / ARRIVEE"],
        [MetadataField.ReturnTime] = ["BREAK-DOWN / RETOUR", "AFBOUW / RETOUR", "DEMONTAGE / RETOUR"],
        [MetadataField.Account] = ["ACCOUNT", "REKENING", "COMPTE"],
        [MetadataField.CostCode] = ["COST CODE", "KOSTENCODE", "CODE COUT"],
        [MetadataField.AddressPickUp] = ["ADDRESS PICK UP", "OPHAALADRES", "ADRESSE ENLEVEMENT"],
        [MetadataField.AddressAction] = ["ADDRESS ACTION", "ADRES ACTIE", "ADRESSE ACTION"],
    };

    public static readonly IReadOnlyDictionary<ItemColumn, string[]> ColumnHeaders = new Dictionary<ItemColumn, string[]>
    {
        [ItemColumn.Picture] = ["PICTURE", "PHOTO", "FOTO", "AFBEELDING"],
        [ItemColumn.Name] = ["NAME", "ITEM", "DESCRIPTION", "ARTICLE", "PRODUCT", "MATERIAL", "NAAM", "ARTIKEL", "PRODUIT", "NOM", "MATERIEL"],
        [ItemColumn.Units] = ["UNITS", "QTY", "QUANTITY", "AANTAL", "HOEVEELHEID", "QUANTITE", "QTE"],
        [ItemColumn.Notes] = ["NOTES", "NOTE", "REMARK", "REMARKS", "REMARQUE", "OPMERKING", "COMMENTAIRE"],
        [ItemColumn.PrepBy] = ["PREP BY", "PREPARED BY", "VOORBEREID DOOR", "PREPARE PAR"],
        [ItemColumn.QuantityOut] = ["# OUT", "OUT", "QTY OUT", "UIT"],
        [ItemColumn.QuantityRetour] = ["# RETOUR", "RETOUR", "RETURN", "TERUG"],
    };
}

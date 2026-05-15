namespace FinanzManager.Models;

/// <summary>
/// ============================================================
/// Kategorie — Gruppierung für Buchungen
/// ============================================================
/// Beispiele:
///   Einnahme-Kategorien: "Lohn", "Nebenjob", "Geschenk"
///   Ausgabe-Kategorien:  "Miete", "Lebensmittel", "Sport"
///
/// Eine Kategorie hat einen festen Typ (Einnahme oder Ausgabe).
/// Beim Erfassen einer Buchung wird zuerst der Typ gewählt und
/// danach werden nur passende Kategorien zur Auswahl angezeigt.
/// ============================================================
/// </summary>
public class Kategorie
{
    /// <summary>Primärschlüssel, von der DB automatisch vergeben.</summary>
    public int Id { get; set; }

    /// <summary>Anzeigename der Kategorie, z.B. "Lebensmittel".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Bestimmt, ob diese Kategorie für Einnahmen oder Ausgaben gilt.</summary>
    public BuchungsTyp Typ { get; set; }

    /// <summary>Hex-Farbcode für die UI-Darstellung (z.B. "#4CAF50"). Optional.</summary>
    public string? Farbe { get; set; }

    /// <summary>
    /// Navigation Property: alle Buchungen, die zu dieser Kategorie gehören.
    /// EF Core füllt diese Liste automatisch, wenn man "Include(k => k.Buchungen)"
    /// in der Abfrage verwendet. Wir initialisieren auf leere Liste, damit
    /// kein NullReferenceException auftreten kann.
    /// </summary>
    public ICollection<Buchung> Buchungen { get; set; } = new List<Buchung>();

    /// <summary>
    /// Wird z.B. in ComboBox-Anzeigen verwendet, wenn nicht explizit
    /// ein DisplayMemberPath gebunden wird.
    /// </summary>
    public override string ToString() => Name;
}

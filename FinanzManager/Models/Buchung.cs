namespace FinanzManager.Models;

/// <summary>
/// ============================================================
/// Buchung — eine einzelne Einnahme oder Ausgabe
/// ============================================================
/// Beispiel: am 15.05.2026 wurden CHF 89.50 für "Wocheneinkauf"
/// in der Kategorie "Lebensmittel" ausgegeben.
///
/// Wichtige Design-Entscheidungen:
///
/// 1. "Betrag" ist immer POSITIV gespeichert. Das Vorzeichen
///    (rein/raus) ergibt sich aus dem BuchungsTyp der Kategorie.
///    Vorteil: Das Modell ist konsistent — eine "Ausgabe von 50"
///    ist immer 50 in der DB, egal von wo gelesen.
///
/// 2. Wir nutzen "decimal", NICHT "double" oder "float".
///    Das ist im Finanzbereich Pflicht: decimal rechnet exakt
///    in Dezimalstellen, double hat Rundungsfehler
///    (0.1 + 0.2 = 0.30000000000000004).
///
/// 3. KategorieId ist der Fremdschlüssel — die direkte
///    Datenbank-Referenz. "Kategorie" ist die Navigation
///    Property, die EF Core befüllt, wenn man die Kategorie
///    miteinlädt (.Include).
/// ============================================================
/// </summary>
public class Buchung
{
    /// <summary>Primärschlüssel, von der DB automatisch vergeben.</summary>
    public int Id { get; set; }

    /// <summary>Datum der Buchung (Geschäftsdatum, nicht Erfassungsdatum).</summary>
    public DateTime Datum { get; set; } = DateTime.Today;

    /// <summary>
    /// Geldbetrag in CHF, immer positiv.
    /// Vorzeichen ergibt sich aus Kategorie.Typ.
    /// Format in DB: DECIMAL(10,2) — bis 99'999'999.99 möglich.
    /// </summary>
    public decimal Betrag { get; set; }

    /// <summary>Optionaler Freitext, z.B. "Wocheneinkauf Coop". Max 500 Zeichen.</summary>
    public string? Notiz { get; set; }

    /// <summary>Fremdschlüssel zur Kategorie.</summary>
    public int KategorieId { get; set; }

    /// <summary>
    /// Navigation Property zur Kategorie.
    /// "= null!" sagt dem Compiler: ich weiss, das ist hier null,
    /// aber EF Core füllt es zur Laufzeit beim Laden ein.
    /// (Sonst würde der Nullable-Compiler eine Warnung werfen.)
    /// </summary>
    public Kategorie Kategorie { get; set; } = null!;

    /// <summary>Audit-Feld: Wann wurde die Buchung erfasst?</summary>
    public DateTime ErstelltAm { get; set; } = DateTime.Now;
}

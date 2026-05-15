namespace FinanzManager.Data.Statistiken;

/// <summary>
/// ============================================================
/// MonatsStatistik — Auswertung eines Monats
/// ============================================================
/// Wird vom Repository gefüllt und ans Dashboard-ViewModel
/// weitergereicht. Als "record" definiert: das ist ein
/// unveränderbares Werte-Objekt mit automatischer Equals-,
/// GetHashCode- und ToString-Implementierung.
///
/// Einsatz im Dashboard: pro Monat die Summen anzeigen.
/// ============================================================
/// </summary>
public record MonatsStatistik(
    int Jahr,
    int Monat,
    decimal Einnahmen,
    decimal Ausgaben,
    int AnzahlBuchungen)
{
    /// <summary>
    /// Berechneter Saldo: positiver Wert = Plus,
    /// negativer Wert = Verlust im Monat.
    /// </summary>
    public decimal Saldo => Einnahmen - Ausgaben;

    /// <summary>Lokalisierter Monatsname, z.B. "Mai 2026".</summary>
    public string MonatBezeichnung =>
        new DateTime(Jahr, Monat, 1).ToString("MMMM yyyy",
            new System.Globalization.CultureInfo("de-CH"));
}

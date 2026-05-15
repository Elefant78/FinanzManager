namespace FinanzManager.Models;

/// <summary>
/// ============================================================
/// BuchungsTyp — Art einer Buchung
/// ============================================================
/// Eine Buchung ist entweder eine Einnahme (Geld kommt rein,
/// z.B. Lohn, Geschenk) oder eine Ausgabe (Geld geht raus,
/// z.B. Miete, Lebensmittel).
///
/// Wir verwenden ein Enum, weil:
/// - Es nur zwei sinnvolle Werte gibt (typsicher)
/// - Der Compiler uns vor Tippfehlern schützt
/// - In der Datenbank wird der Enum als int gespeichert
///   (Einnahme=1, Ausgabe=2) — kompakt und schnell.
/// ============================================================
/// </summary>
public enum BuchungsTyp
{
    Einnahme = 1,
    Ausgabe = 2
}

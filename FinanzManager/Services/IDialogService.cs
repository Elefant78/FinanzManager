using FinanzManager.Models;

namespace FinanzManager.Services;

/// <summary>
/// ============================================================
/// IDialogService — Vertrag für Dialog-Anzeigen
/// ============================================================
/// ViewModels dürfen keine Windows direkt erzeugen oder anzeigen
/// (das wäre eine Kopplung an WPF/UI, die das Testen erschwert).
/// Stattdessen rufen sie Methoden dieses Interfaces auf, und die
/// konkrete Implementierung (siehe Phase 6) kümmert sich um die
/// Window-Erzeugung.
///
/// Vorteil: das ViewModel kann mit einem Mock getestet werden,
/// und der Dialog kann später z.B. auch ein Bottom-Sheet oder
/// ein Inline-Panel sein, ohne dass die VMs angefasst werden.
/// ============================================================
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Zeigt das Buchung-Bearbeiten-Fenster.
    /// </summary>
    /// <param name="bestehende">Wenn null: neue Buchung erfassen. Sonst: die übergebene Buchung bearbeiten.</param>
    /// <returns>true, wenn der Benutzer gespeichert hat. false bei Abbrechen.</returns>
    Task<bool> ZeigeBuchungBearbeitenAsync(Buchung? bestehende = null);

    /// <summary>Zeigt eine einfache Fehler-MessageBox.</summary>
    Task ZeigeFehlerAsync(string titel, string nachricht);

    /// <summary>Zeigt eine Ja/Nein-Bestätigungsabfrage. Liefert true bei Ja.</summary>
    Task<bool> ZeigeBestaetigungAsync(string titel, string nachricht);
}

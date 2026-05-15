using FinanzManager.Models;

namespace FinanzManager.Data.Repositories;

/// <summary>
/// ============================================================
/// IKategorieRepository — Vertrag für den Kategorien-Datenzugriff
/// ============================================================
/// Das Interface beschreibt WAS gemacht werden kann, ohne zu
/// verraten WIE. Vorteile:
///  - ViewModels hängen nur am Interface, nicht an einer
///    konkreten Klasse → austauschbar (z.B. für Tests mit Mock).
///  - Klare API-Dokumentation.
///  - Folgt dem "Dependency Inversion Principle" (das D in SOLID).
/// ============================================================
/// </summary>
public interface IKategorieRepository
{
    /// <summary>Liefert alle Kategorien, sortiert nach Name.</summary>
    Task<List<Kategorie>> GetAlleAsync();

    /// <summary>Liefert nur Kategorien eines bestimmten Typs (Einnahme/Ausgabe).</summary>
    Task<List<Kategorie>> GetForTypAsync(BuchungsTyp typ);

    /// <summary>Liefert eine einzelne Kategorie oder null, falls nicht vorhanden.</summary>
    Task<Kategorie?> GetByIdAsync(int id);

    /// <summary>Fügt eine neue Kategorie hinzu. Liefert die Kategorie inkl. der von der DB vergebenen Id zurück.</summary>
    Task<Kategorie> AddAsync(Kategorie kategorie);

    /// <summary>Aktualisiert eine bestehende Kategorie (Name, Typ, Farbe).</summary>
    Task UpdateAsync(Kategorie kategorie);

    /// <summary>
    /// Löscht eine Kategorie. Schlägt fehl, wenn noch Buchungen
    /// daran hängen (DeleteBehavior.Restrict).
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>Prüft, ob eine Kategorie noch Buchungen hat (vor dem Löschen relevant).</summary>
    Task<bool> HatBuchungenAsync(int kategorieId);
}

using FinanzManager.Data.Statistiken;
using FinanzManager.Models;

namespace FinanzManager.Data.Repositories;

/// <summary>
/// ============================================================
/// IBuchungRepository — Vertrag für den Buchungs-Datenzugriff
/// ============================================================
/// Bietet alle Operationen, die ViewModels für Buchungen
/// brauchen werden: Lesen mit Filtern, CRUD, Statistik.
/// ============================================================
/// </summary>
public interface IBuchungRepository
{
    /// <summary>Alle Buchungen, neueste zuerst, inklusive Kategorie.</summary>
    Task<List<Buchung>> GetAlleAsync();

    /// <summary>Alle Buchungen eines bestimmten Monats, inklusive Kategorie.</summary>
    Task<List<Buchung>> GetForMonatAsync(int jahr, int monat);

    /// <summary>Alle Buchungen einer bestimmten Kategorie.</summary>
    Task<List<Buchung>> GetForKategorieAsync(int kategorieId);

    /// <summary>Eine einzelne Buchung anhand ihrer Id.</summary>
    Task<Buchung?> GetByIdAsync(int id);

    /// <summary>Speichert eine neue Buchung.</summary>
    Task<Buchung> AddAsync(Buchung buchung);

    /// <summary>Aktualisiert eine bestehende Buchung.</summary>
    Task UpdateAsync(Buchung buchung);

    /// <summary>Löscht eine Buchung.</summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Berechnet die Monatsstatistik (Einnahmen, Ausgaben, Saldo).
    /// Macht die Aggregation in der DB — schneller als alle
    /// Buchungen zu laden und in C# zu summieren.
    /// </summary>
    Task<MonatsStatistik> GetMonatsStatistikAsync(int jahr, int monat);
}

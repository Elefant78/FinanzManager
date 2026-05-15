using FinanzManager.Data.Statistiken;
using FinanzManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanzManager.Data.Repositories;

/// <summary>
/// ============================================================
/// BuchungRepository — Konkrete Implementierung
/// ============================================================
/// Wichtigste Lehrstücke in dieser Klasse:
///
/// 1. Include(): lädt verbundene Daten mit. Ohne Include wäre
///    "buchung.Kategorie" null. Beispiel:
///       db.Buchungen.Include(b => b.Kategorie).ToListAsync();
///
/// 2. AsNoTracking(): EF Core merkt sich normalerweise jede
///    geladene Entität, um Änderungen zu erkennen. Für reine
///    Lese-Abfragen ist das überflüssig — AsNoTracking spart
///    Speicher und macht die Abfrage schneller.
///
/// 3. Aggregation in der DB: GetMonatsStatistikAsync nutzt
///    Where + Sum direkt auf den DbSet. EF Core übersetzt das
///    in ein einziges SQL-SUM — viel schneller als alle
///    Buchungen zu laden und in C# zu summieren.
/// ============================================================
/// </summary>
public class BuchungRepository : IBuchungRepository
{
    private readonly IDbContextFactory<FinanzDbContext> _dbFactory;

    public BuchungRepository(IDbContextFactory<FinanzDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Buchung>> GetAlleAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Buchungen
            .AsNoTracking()
            .Include(b => b.Kategorie)
            .OrderByDescending(b => b.Datum)
            .ThenByDescending(b => b.Id)
            .ToListAsync();
    }

    public async Task<List<Buchung>> GetForMonatAsync(int jahr, int monat)
    {
        // Ersten und letzten Tag des Monats berechnen.
        var von = new DateTime(jahr, monat, 1);
        var bis = von.AddMonths(1); // exklusiv: erster Tag des Folgemonats

        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Buchungen
            .AsNoTracking()
            .Include(b => b.Kategorie)
            .Where(b => b.Datum >= von && b.Datum < bis)
            .OrderByDescending(b => b.Datum)
            .ThenByDescending(b => b.Id)
            .ToListAsync();
    }

    public async Task<List<Buchung>> GetForKategorieAsync(int kategorieId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Buchungen
            .AsNoTracking()
            .Include(b => b.Kategorie)
            .Where(b => b.KategorieId == kategorieId)
            .OrderByDescending(b => b.Datum)
            .ToListAsync();
    }

    public async Task<Buchung?> GetByIdAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Buchungen
            .AsNoTracking()
            .Include(b => b.Kategorie)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Buchung> AddAsync(Buchung buchung)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        // Kategorie nur per Id setzen, nicht das ganze Objekt mitgeben —
        // sonst versucht EF Core, eine "neue" Kategorie anzulegen.
        // Falls das Objekt aus dem ViewModel mit voller Kategorie kommt,
        // setzen wir die Navigation Property bewusst auf null.
        buchung.Kategorie = null!;

        db.Buchungen.Add(buchung);
        await db.SaveChangesAsync();
        return buchung;
    }

    public async Task UpdateAsync(Buchung buchung)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        buchung.Kategorie = null!; // siehe Kommentar in AddAsync
        db.Buchungen.Update(buchung);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var buchung = await db.Buchungen.FindAsync(id);
        if (buchung is null) return;

        db.Buchungen.Remove(buchung);
        await db.SaveChangesAsync();
    }

    public async Task<MonatsStatistik> GetMonatsStatistikAsync(int jahr, int monat)
    {
        var von = new DateTime(jahr, monat, 1);
        var bis = von.AddMonths(1);

        using var db = await _dbFactory.CreateDbContextAsync();

        // Wir holen pro Buchung: Betrag und Typ der Kategorie.
        // SUM und COUNT würden in zwei Queries enden, wenn wir's
        // einzeln machen — also lieber einmal alle Daten holen und
        // in C# in einer Schleife aggregieren. Bei kleinen Daten-
        // mengen (Buchungen pro Monat) ist das die simpelste Lösung.
        var monatsBuchungen = await db.Buchungen
            .AsNoTracking()
            .Where(b => b.Datum >= von && b.Datum < bis)
            .Select(b => new { b.Betrag, b.Kategorie.Typ })
            .ToListAsync();

        decimal einnahmen = monatsBuchungen
            .Where(b => b.Typ == BuchungsTyp.Einnahme)
            .Sum(b => b.Betrag);

        decimal ausgaben = monatsBuchungen
            .Where(b => b.Typ == BuchungsTyp.Ausgabe)
            .Sum(b => b.Betrag);

        return new MonatsStatistik(
            jahr,
            monat,
            einnahmen,
            ausgaben,
            monatsBuchungen.Count);
    }
}

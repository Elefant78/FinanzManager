using FinanzManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanzManager.Data.Repositories;

/// <summary>
/// ============================================================
/// KategorieRepository — Konkrete Implementierung
/// ============================================================
/// Holt sich für jede Operation einen frischen DbContext aus
/// der Factory (using-Block sorgt für korrektes Disposing).
/// Das Repository selbst hält keinen Kontext und keine
/// veränderlichen Daten — kann also problemlos als Singleton
/// im DI-Container leben.
/// ============================================================
/// </summary>
public class KategorieRepository : IKategorieRepository
{
    private readonly IDbContextFactory<FinanzDbContext> _dbFactory;

    public KategorieRepository(IDbContextFactory<FinanzDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Kategorie>> GetAlleAsync()
    {
        // Frischer Kontext pro Aufruf — verhindert veraltete Daten.
        using var db = await _dbFactory.CreateDbContextAsync();

        // AsNoTracking: wir lesen nur, also kein Change-Tracking nötig.
        // Spart Speicher und macht die Abfrage schneller.
        return await db.Kategorien
            .AsNoTracking()
            .OrderBy(k => k.Typ)
            .ThenBy(k => k.Name)
            .ToListAsync();
    }

    public async Task<List<Kategorie>> GetForTypAsync(BuchungsTyp typ)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Kategorien
            .AsNoTracking()
            .Where(k => k.Typ == typ)
            .OrderBy(k => k.Name)
            .ToListAsync();
    }

    public async Task<Kategorie?> GetByIdAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Kategorien
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<Kategorie> AddAsync(Kategorie kategorie)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        db.Kategorien.Add(kategorie);
        await db.SaveChangesAsync();
        // EF Core hat jetzt die DB-vergebene Id in das Objekt geschrieben.
        return kategorie;
    }

    public async Task UpdateAsync(Kategorie kategorie)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        db.Kategorien.Update(kategorie);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var kategorie = await db.Kategorien.FindAsync(id);
        if (kategorie is null) return;

        db.Kategorien.Remove(kategorie);
        // Wirft DbUpdateException, falls noch Buchungen daran hängen
        // (wegen OnDelete(DeleteBehavior.Restrict) im DbContext).
        await db.SaveChangesAsync();
    }

    public async Task<bool> HatBuchungenAsync(int kategorieId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Buchungen.AnyAsync(b => b.KategorieId == kategorieId);
    }
}

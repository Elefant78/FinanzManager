using System.IO;
using FinanzManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanzManager.Data;

/// <summary>
/// ============================================================
/// FinanzDbContext — Brücke zwischen C#-Klassen und SQLite-DB
/// ============================================================
/// Entity Framework Core übernimmt für uns:
///  - SQL-Generierung (wir schreiben kein SQL von Hand)
///  - Mapping zwischen Tabellen und Klassen
///  - Tracking von Änderungen
///  - Transaktionen
///
/// Nutzung:
///   using var db = factory.CreateDbContext();
///   var alleBuchungen = await db.Buchungen
///       .Include(b => b.Kategorie)
///       .Where(b => b.Datum >= startDatum)
///       .ToListAsync();
///
/// Warum ein DbContextFactory (siehe App.xaml.cs)?
/// In Web-Apps lebt ein DbContext nur für eine HTTP-Anfrage.
/// In WPF gibt es das nicht — also erstellen wir bei jeder
/// Operation einen frischen Kontext über die Factory. Das
/// vermeidet veraltete Daten und Memory-Probleme.
/// ============================================================
/// </summary>
public class FinanzDbContext : DbContext
{
    /// <summary>Die "Tabelle" der Buchungen.</summary>
    public DbSet<Buchung> Buchungen => Set<Buchung>();

    /// <summary>Die "Tabelle" der Kategorien.</summary>
    public DbSet<Kategorie> Kategorien => Set<Kategorie>();

    /// <summary>
    /// Konstruktor: bekommt die Optionen (z.B. Connection String)
    /// vom DI-Container injiziert. Selber nichts hier tun —
    /// die Konfiguration kommt von aussen (App.xaml.cs).
    /// </summary>
    public FinanzDbContext(DbContextOptions<FinanzDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Liefert den Standard-Pfad für die SQLite-Datei:
    /// %APPDATA%\FinanzManager\finanzmanager.db
    ///
    /// Vorteile dieses Pfades:
    ///  - Schreibrechte garantiert (Benutzer-Bereich)
    ///  - Wird bei Programm-Updates nicht überschrieben
    ///  - Roaming Profile-fähig (in Domänen-Umgebungen)
    /// </summary>
    public static string GetStandardDbPfad()
    {
        var appDataOrdner = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appOrdner = Path.Combine(appDataOrdner, "FinanzManager");
        Directory.CreateDirectory(appOrdner);
        return Path.Combine(appOrdner, "finanzmanager.db");
    }

    /// <summary>
    /// Wird beim Erzeugen des DB-Schemas aufgerufen.
    /// Hier definieren wir Constraints, Beziehungen und Seed-Daten
    /// per Fluent API — das ist sauberer als Attribute auf den
    /// Modell-Klassen, weil das Modell DB-unabhängig bleibt.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ----- Buchung-Konfiguration -----
        modelBuilder.Entity<Buchung>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.Datum)
                  .IsRequired();

            entity.Property(b => b.Betrag)
                  .HasColumnType("DECIMAL(10,2)")
                  .IsRequired();

            entity.Property(b => b.Notiz)
                  .HasMaxLength(500);

            entity.Property(b => b.ErstelltAm)
                  .IsRequired();

            // Beziehung Buchung -> Kategorie:
            //  - Eine Buchung gehört zu genau einer Kategorie (HasOne).
            //  - Eine Kategorie kann viele Buchungen haben (WithMany).
            //  - DeleteBehavior.Restrict: Kategorie kann NICHT gelöscht
            //    werden, solange Buchungen daran hängen — Schutz vor
            //    versehentlichem Datenverlust.
            entity.HasOne(b => b.Kategorie)
                  .WithMany(k => k.Buchungen)
                  .HasForeignKey(b => b.KategorieId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Index auf Datum: beschleunigt Filter wie "alle Buchungen im Mai"
            entity.HasIndex(b => b.Datum);
        });

        // ----- Kategorie-Konfiguration -----
        modelBuilder.Entity<Kategorie>(entity =>
        {
            entity.HasKey(k => k.Id);

            entity.Property(k => k.Name)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.Property(k => k.Farbe)
                  .HasMaxLength(9);

            entity.Property(k => k.Typ)
                  .IsRequired()
                  .HasConversion<int>(); // Enum als int speichern
        });

        // ----- Standard-Kategorien einseeden -----
        // HasData fügt diese Datensätze beim ersten Erstellen
        // der Datenbank automatisch ein. So hat der User von
        // Beginn an sinnvolle Kategorien.
        modelBuilder.Entity<Kategorie>().HasData(
            // Einnahmen
            new Kategorie { Id = 1, Name = "Lohn",         Typ = BuchungsTyp.Einnahme, Farbe = "#2E7D32" },
            new Kategorie { Id = 2, Name = "Nebenjob",     Typ = BuchungsTyp.Einnahme, Farbe = "#558B2F" },
            new Kategorie { Id = 3, Name = "Geschenk",     Typ = BuchungsTyp.Einnahme, Farbe = "#9E9D24" },
            new Kategorie { Id = 4, Name = "Sonstige Einnahme", Typ = BuchungsTyp.Einnahme, Farbe = "#827717" },
            // Ausgaben
            new Kategorie { Id = 5,  Name = "Miete",        Typ = BuchungsTyp.Ausgabe, Farbe = "#C62828" },
            new Kategorie { Id = 6,  Name = "Lebensmittel", Typ = BuchungsTyp.Ausgabe, Farbe = "#EF6C00" },
            new Kategorie { Id = 7,  Name = "Mobilität",    Typ = BuchungsTyp.Ausgabe, Farbe = "#F4511E" },
            new Kategorie { Id = 8,  Name = "Freizeit",     Typ = BuchungsTyp.Ausgabe, Farbe = "#6A1B9A" },
            new Kategorie { Id = 9,  Name = "Sport",        Typ = BuchungsTyp.Ausgabe, Farbe = "#283593" },
            new Kategorie { Id = 10, Name = "Bücher",       Typ = BuchungsTyp.Ausgabe, Farbe = "#00695C" },
            new Kategorie { Id = 11, Name = "Kleidung",     Typ = BuchungsTyp.Ausgabe, Farbe = "#AD1457" },
            new Kategorie { Id = 12, Name = "Sonstiges",    Typ = BuchungsTyp.Ausgabe, Farbe = "#455A64" }
        );

        base.OnModelCreating(modelBuilder);
    }
}

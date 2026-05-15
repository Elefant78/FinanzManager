using System.Collections.ObjectModel;
using System.Windows.Input;
using FinanzManager.Data.Repositories;
using FinanzManager.Models;
using FinanzManager.MVVM;
using FinanzManager.Services;

namespace FinanzManager.ViewModels;

/// <summary>
/// ============================================================
/// BuchungenViewModel — Liste aller Buchungen mit Filter
/// ============================================================
/// Zeigt eine sortierbare, filterbare Liste der Buchungen und
/// bietet die Aktionen "Neu", "Bearbeiten", "Löschen" an.
/// Filter:
///   - Monat (Default: aktueller Monat, "Alle" möglich)
///   - Kategorie (Default: alle)
/// ============================================================
/// </summary>
public class BuchungenViewModel : ViewModelBase
{
    private readonly IBuchungRepository _buchungRepo;
    private readonly IKategorieRepository _kategorieRepo;
    private readonly IDialogService _dialogService;

    private DateTime? _filterMonat = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    private Kategorie? _filterKategorie;
    private Buchung? _ausgewaehlteBuchung;
    private bool _laedtGerade;

    public BuchungenViewModel(
        IBuchungRepository buchungRepo,
        IKategorieRepository kategorieRepo,
        IDialogService dialogService)
    {
        _buchungRepo = buchungRepo;
        _kategorieRepo = kategorieRepo;
        _dialogService = dialogService;

        Buchungen = new ObservableCollection<Buchung>();
        VerfuegbareKategorien = new ObservableCollection<Kategorie>();

        NeueBuchungCmd      = new AsyncRelayCommand(NeueBuchungAsync);
        BearbeitenCmd       = new AsyncRelayCommand(BearbeitenAsync,  () => AusgewaehlteBuchung is not null);
        LoeschenCmd         = new AsyncRelayCommand(LoeschenAsync,    () => AusgewaehlteBuchung is not null);
        AktualisierenCmd    = new AsyncRelayCommand(LoadAsync);
        FilterZuruecksetzenCmd = new AsyncRelayCommand(FilterZuruecksetzenAsync);
    }

    /// <summary>Die aktuell angezeigten (gefilterten) Buchungen.</summary>
    public ObservableCollection<Buchung> Buchungen { get; }

    /// <summary>
    /// Kategorien für den Filter-Dropdown. Enthält "Alle" als
    /// virtueller Eintrag (null), damit der Filter zurückgesetzt
    /// werden kann.
    /// </summary>
    public ObservableCollection<Kategorie> VerfuegbareKategorien { get; }

    /// <summary>
    /// Filter: nur Buchungen dieses Monats anzeigen.
    /// null = alle Monate.
    /// </summary>
    public DateTime? FilterMonat
    {
        get => _filterMonat;
        set
        {
            if (SetProperty(ref _filterMonat, value))
                _ = LoadAsync(); // Fire-and-forget: bei Filter-Änderung neu laden
        }
    }

    /// <summary>
    /// Filter: nur Buchungen dieser Kategorie.
    /// null = alle Kategorien.
    /// </summary>
    public Kategorie? FilterKategorie
    {
        get => _filterKategorie;
        set
        {
            if (SetProperty(ref _filterKategorie, value))
                _ = LoadAsync();
        }
    }

    /// <summary>Aktuell selektierte Buchung in der Liste.</summary>
    public Buchung? AusgewaehlteBuchung
    {
        get => _ausgewaehlteBuchung;
        set => SetProperty(ref _ausgewaehlteBuchung, value);
    }

    public bool LaedtGerade
    {
        get => _laedtGerade;
        private set => SetProperty(ref _laedtGerade, value);
    }

    public ICommand NeueBuchungCmd { get; }
    public ICommand BearbeitenCmd { get; }
    public ICommand LoeschenCmd { get; }
    public ICommand AktualisierenCmd { get; }
    public ICommand FilterZuruecksetzenCmd { get; }

    /// <summary>
    /// Lädt die Buchungs-Liste passend zu den aktuellen Filtern.
    /// Wird bei Filter-Änderungen, nach Bearbeiten/Löschen und
    /// beim Aktivieren des ViewModels aufgerufen.
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            LaedtGerade = true;

            // Kategorien-Filter-Liste füllen, falls noch leer.
            if (VerfuegbareKategorien.Count == 0)
            {
                var kategorien = await _kategorieRepo.GetAlleAsync();
                VerfuegbareKategorien.Clear();
                foreach (var k in kategorien)
                    VerfuegbareKategorien.Add(k);
            }

            // Buchungen aus DB laden — je nach Filter.
            List<Buchung> buchungen;
            if (FilterMonat.HasValue)
                buchungen = await _buchungRepo.GetForMonatAsync(
                    FilterMonat.Value.Year, FilterMonat.Value.Month);
            else
                buchungen = await _buchungRepo.GetAlleAsync();

            // Kategorie-Filter in C# (kleine Datenmenge nach Monatsfilter)
            if (FilterKategorie is not null)
                buchungen = buchungen
                    .Where(b => b.KategorieId == FilterKategorie.Id)
                    .ToList();

            Buchungen.Clear();
            foreach (var b in buchungen)
                Buchungen.Add(b);
        }
        finally
        {
            LaedtGerade = false;
        }
    }

    private async Task NeueBuchungAsync()
    {
        var hatGespeichert = await _dialogService.ZeigeBuchungBearbeitenAsync(null);
        if (hatGespeichert)
            await LoadAsync();
    }

    private async Task BearbeitenAsync()
    {
        if (AusgewaehlteBuchung is null) return;

        var hatGespeichert = await _dialogService.ZeigeBuchungBearbeitenAsync(AusgewaehlteBuchung);
        if (hatGespeichert)
            await LoadAsync();
    }

    private async Task LoeschenAsync()
    {
        if (AusgewaehlteBuchung is null) return;

        var bestaetigt = await _dialogService.ZeigeBestaetigungAsync(
            "Buchung löschen?",
            $"Soll die Buchung vom {AusgewaehlteBuchung.Datum:dd.MM.yyyy} " +
            $"über CHF {AusgewaehlteBuchung.Betrag:F2} wirklich gelöscht werden?");

        if (!bestaetigt) return;

        try
        {
            await _buchungRepo.DeleteAsync(AusgewaehlteBuchung.Id);
            AusgewaehlteBuchung = null;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ZeigeFehlerAsync("Löschen fehlgeschlagen", ex.Message);
        }
    }

    private async Task FilterZuruecksetzenAsync()
    {
        // Setter umgehen, um nicht zweimal zu laden:
        _filterMonat = null;
        _filterKategorie = null;
        OnPropertyChanged(nameof(FilterMonat));
        OnPropertyChanged(nameof(FilterKategorie));
        await LoadAsync();
    }
}

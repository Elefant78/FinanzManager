using System.Collections.ObjectModel;
using System.Windows.Input;
using FinanzManager.Data.Repositories;
using FinanzManager.Models;
using FinanzManager.MVVM;

namespace FinanzManager.ViewModels;

/// <summary>
/// ============================================================
/// BuchungBearbeitenViewModel — Formular für Neu/Bearbeiten
/// ============================================================
/// Wird im Buchung-Bearbeiten-Dialog (Phase 6) als DataContext
/// gesetzt. Beim Öffnen wird LoadAsync(buchung) aufgerufen:
///  - LoadAsync(null)             → Modus "Neu", leeres Formular
///  - LoadAsync(bestehendeBuchung) → Modus "Bearbeiten"
///
/// Speichern und Abbrechen lösen das RequestClose-Event aus,
/// das vom Dialog-Window abonniert wird.
/// ============================================================
/// </summary>
public class BuchungBearbeitenViewModel : ViewModelBase
{
    private readonly IBuchungRepository _buchungRepo;
    private readonly IKategorieRepository _kategorieRepo;

    private int? _id;
    private DateTime _datum = DateTime.Today;
    private decimal _betrag;
    private string? _notiz;
    private BuchungsTyp _typ = BuchungsTyp.Ausgabe;
    private Kategorie? _ausgewaehlteKategorie;
    private bool _speichertGerade;

    public BuchungBearbeitenViewModel(
        IBuchungRepository buchungRepo,
        IKategorieRepository kategorieRepo)
    {
        _buchungRepo = buchungRepo;
        _kategorieRepo = kategorieRepo;

        VerfuegbareKategorien = new ObservableCollection<Kategorie>();

        SpeichernCmd = new AsyncRelayCommand(SpeichernAsync, () => IstFormularGueltig);
        AbbrechenCmd = new RelayCommand(() => RequestClose?.Invoke(this, false));
    }

    /// <summary>Wird gefeuert, wenn der Dialog geschlossen werden soll. bool = "wurde gespeichert?".</summary>
    public event EventHandler<bool>? RequestClose;

    /// <summary>
    /// true = neue Buchung, false = bestehende bearbeiten.
    /// Steuert den Fenster-Titel und die Beschriftung.
    /// </summary>
    public bool IstNeu => _id is null;
    public string Titel => IstNeu ? "Neue Buchung" : "Buchung bearbeiten";

    public DateTime Datum
    {
        get => _datum;
        set => SetProperty(ref _datum, value);
    }

    public decimal Betrag
    {
        get => _betrag;
        set
        {
            if (SetProperty(ref _betrag, value))
                OnPropertyChanged(nameof(IstFormularGueltig));
        }
    }

    public string? Notiz
    {
        get => _notiz;
        set => SetProperty(ref _notiz, value);
    }

    public BuchungsTyp Typ
    {
        get => _typ;
        set
        {
            if (SetProperty(ref _typ, value))
            {
                // Bei Typ-Wechsel passende Kategorien neu laden,
                // und die Auswahl zurücksetzen.
                AusgewaehlteKategorie = null;
                OnPropertyChanged(nameof(IstEinnahme));
                OnPropertyChanged(nameof(IstAusgabe));
                _ = KategorienFuerTypLadenAsync();
            }
        }
    }

    /// <summary>Helfer für RadioButton-Bindings.</summary>
    public bool IstEinnahme
    {
        get => _typ == BuchungsTyp.Einnahme;
        set { if (value) Typ = BuchungsTyp.Einnahme; }
    }

    public bool IstAusgabe
    {
        get => _typ == BuchungsTyp.Ausgabe;
        set { if (value) Typ = BuchungsTyp.Ausgabe; }
    }

    public ObservableCollection<Kategorie> VerfuegbareKategorien { get; }

    public Kategorie? AusgewaehlteKategorie
    {
        get => _ausgewaehlteKategorie;
        set
        {
            if (SetProperty(ref _ausgewaehlteKategorie, value))
                OnPropertyChanged(nameof(IstFormularGueltig));
        }
    }

    public bool SpeichertGerade
    {
        get => _speichertGerade;
        private set => SetProperty(ref _speichertGerade, value);
    }

    /// <summary>Validierung: Speichern nur erlaubt, wenn Formular OK.</summary>
    public bool IstFormularGueltig =>
        Betrag > 0 &&
        AusgewaehlteKategorie is not null;

    public ICommand SpeichernCmd { get; }
    public ICommand AbbrechenCmd { get; }

    /// <summary>
    /// Initialisiert das ViewModel.
    /// Wird vom DialogService nach DI-Resolve aufgerufen.
    /// </summary>
    public async Task LoadAsync(Buchung? bestehende)
    {
        if (bestehende is not null)
        {
            // Modus: Bearbeiten
            _id = bestehende.Id;
            _datum = bestehende.Datum;
            _betrag = bestehende.Betrag;
            _notiz = bestehende.Notiz;
            _typ = bestehende.Kategorie?.Typ ?? BuchungsTyp.Ausgabe;
        }
        else
        {
            // Modus: Neu — Defaults
            _id = null;
            _datum = DateTime.Today;
            _betrag = 0;
            _notiz = null;
            _typ = BuchungsTyp.Ausgabe;
        }

        // Beim Initial-Setzen direkt alle relevanten Property-Events feuern,
        // damit das UI sich aktualisiert.
        OnPropertyChanged(nameof(IstNeu));
        OnPropertyChanged(nameof(Titel));
        OnPropertyChanged(nameof(Datum));
        OnPropertyChanged(nameof(Betrag));
        OnPropertyChanged(nameof(Notiz));
        OnPropertyChanged(nameof(Typ));
        OnPropertyChanged(nameof(IstEinnahme));
        OnPropertyChanged(nameof(IstAusgabe));

        await KategorienFuerTypLadenAsync();

        // Falls Bearbeiten: die Kategorie der Buchung im Dropdown vorauswählen.
        if (bestehende is not null)
        {
            AusgewaehlteKategorie = VerfuegbareKategorien
                .FirstOrDefault(k => k.Id == bestehende.KategorieId);
        }

        OnPropertyChanged(nameof(IstFormularGueltig));
    }

    private async Task KategorienFuerTypLadenAsync()
    {
        var passende = await _kategorieRepo.GetForTypAsync(Typ);
        VerfuegbareKategorien.Clear();
        foreach (var k in passende)
            VerfuegbareKategorien.Add(k);
    }

    private async Task SpeichernAsync()
    {
        if (!IstFormularGueltig) return;

        try
        {
            SpeichertGerade = true;

            var buchung = new Buchung
            {
                Id = _id ?? 0,
                Datum = Datum,
                Betrag = Betrag,
                Notiz = Notiz,
                KategorieId = AusgewaehlteKategorie!.Id,
                ErstelltAm = _id is null ? DateTime.Now : DateTime.Now // bei Neu: jetzt; bei Edit: könnte original beibehalten werden, aber MVP: einfach
            };

            if (_id is null)
                await _buchungRepo.AddAsync(buchung);
            else
                await _buchungRepo.UpdateAsync(buchung);

            RequestClose?.Invoke(this, true); // true = wurde gespeichert
        }
        finally
        {
            SpeichertGerade = false;
        }
    }
}

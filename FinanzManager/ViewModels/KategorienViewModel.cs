using System.Collections.ObjectModel;
using System.Windows.Input;
using FinanzManager.Data.Repositories;
using FinanzManager.Models;
using FinanzManager.MVVM;
using FinanzManager.Services;

namespace FinanzManager.ViewModels;

/// <summary>
/// ============================================================
/// KategorienViewModel — Kategorien anzeigen und verwalten
/// ============================================================
/// MVP-Funktionalität:
///  - Alle Kategorien anzeigen, gruppiert nach Typ
///  - Neue Kategorie hinzufügen (Inline-Formular)
///  - Kategorie löschen (nur, wenn keine Buchungen daran hängen)
/// ============================================================
/// </summary>
public class KategorienViewModel : ViewModelBase
{
    private readonly IKategorieRepository _kategorieRepo;
    private readonly IDialogService _dialogService;

    private string _neuName = string.Empty;
    private BuchungsTyp _neuTyp = BuchungsTyp.Ausgabe;
    private Kategorie? _ausgewaehlteKategorie;
    private bool _laedtGerade;

    public KategorienViewModel(
        IKategorieRepository kategorieRepo,
        IDialogService dialogService)
    {
        _kategorieRepo = kategorieRepo;
        _dialogService = dialogService;

        Kategorien = new ObservableCollection<Kategorie>();

        HinzufuegenCmd = new AsyncRelayCommand(HinzufuegenAsync, () => !string.IsNullOrWhiteSpace(NeuName));
        LoeschenCmd    = new AsyncRelayCommand(LoeschenAsync,    () => AusgewaehlteKategorie is not null);
        AktualisierenCmd = new AsyncRelayCommand(LoadAsync);
    }

    public ObservableCollection<Kategorie> Kategorien { get; }

    public Kategorie? AusgewaehlteKategorie
    {
        get => _ausgewaehlteKategorie;
        set => SetProperty(ref _ausgewaehlteKategorie, value);
    }

    /// <summary>Eingabe-Feld für den Namen einer neuen Kategorie.</summary>
    public string NeuName
    {
        get => _neuName;
        set => SetProperty(ref _neuName, value);
    }

    /// <summary>Eingabe-Feld für den Typ einer neuen Kategorie.</summary>
    public BuchungsTyp NeuTyp
    {
        get => _neuTyp;
        set
        {
            if (SetProperty(ref _neuTyp, value))
            {
                OnPropertyChanged(nameof(NeuIstEinnahme));
                OnPropertyChanged(nameof(NeuIstAusgabe));
            }
        }
    }

    public bool NeuIstEinnahme
    {
        get => _neuTyp == BuchungsTyp.Einnahme;
        set { if (value) NeuTyp = BuchungsTyp.Einnahme; }
    }

    public bool NeuIstAusgabe
    {
        get => _neuTyp == BuchungsTyp.Ausgabe;
        set { if (value) NeuTyp = BuchungsTyp.Ausgabe; }
    }

    public bool LaedtGerade
    {
        get => _laedtGerade;
        private set => SetProperty(ref _laedtGerade, value);
    }

    public ICommand HinzufuegenCmd { get; }
    public ICommand LoeschenCmd { get; }
    public ICommand AktualisierenCmd { get; }

    public async Task LoadAsync()
    {
        try
        {
            LaedtGerade = true;
            var alle = await _kategorieRepo.GetAlleAsync();
            Kategorien.Clear();
            foreach (var k in alle)
                Kategorien.Add(k);
        }
        finally
        {
            LaedtGerade = false;
        }
    }

    private async Task HinzufuegenAsync()
    {
        if (string.IsNullOrWhiteSpace(NeuName)) return;

        try
        {
            var kategorie = new Kategorie
            {
                Name = NeuName.Trim(),
                Typ = NeuTyp,
                Farbe = null
            };

            await _kategorieRepo.AddAsync(kategorie);

            // Formular zurücksetzen
            NeuName = string.Empty;

            await LoadAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ZeigeFehlerAsync("Hinzufügen fehlgeschlagen", ex.Message);
        }
    }

    private async Task LoeschenAsync()
    {
        if (AusgewaehlteKategorie is null) return;

        // Schutz: nicht löschen, wenn Buchungen dranhängen.
        var hatBuchungen = await _kategorieRepo.HatBuchungenAsync(AusgewaehlteKategorie.Id);
        if (hatBuchungen)
        {
            await _dialogService.ZeigeFehlerAsync(
                "Kategorie nicht löschbar",
                $"Es existieren noch Buchungen zur Kategorie \"{AusgewaehlteKategorie.Name}\". " +
                $"Lösche oder verschiebe zuerst diese Buchungen.");
            return;
        }

        var bestaetigt = await _dialogService.ZeigeBestaetigungAsync(
            "Kategorie löschen?",
            $"Soll die Kategorie \"{AusgewaehlteKategorie.Name}\" wirklich gelöscht werden?");

        if (!bestaetigt) return;

        try
        {
            await _kategorieRepo.DeleteAsync(AusgewaehlteKategorie.Id);
            AusgewaehlteKategorie = null;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ZeigeFehlerAsync("Löschen fehlgeschlagen", ex.Message);
        }
    }
}

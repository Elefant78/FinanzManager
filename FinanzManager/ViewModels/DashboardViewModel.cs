using System.Collections.ObjectModel;
using System.Windows.Input;
using FinanzManager.Data.Repositories;
using FinanzManager.Data.Statistiken;
using FinanzManager.Models;
using FinanzManager.MVVM;

namespace FinanzManager.ViewModels;

/// <summary>
/// ============================================================
/// DashboardViewModel — Monatsübersicht
/// ============================================================
/// Zeigt die wichtigsten Zahlen für einen ausgewählten Monat:
///  - Einnahmen total
///  - Ausgaben total
///  - Saldo (Einnahmen - Ausgaben)
///  - Letzte Buchungen des Monats
///
/// Der Benutzer kann mit "voriger" / "nächster" Monat durch
/// die Historie blättern.
/// ============================================================
/// </summary>
public class DashboardViewModel : ViewModelBase
{
    private readonly IBuchungRepository _buchungRepo;

    private DateTime _aktuellerMonat = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    private MonatsStatistik? _statistik;
    private bool _laedtGerade;

    public DashboardViewModel(IBuchungRepository buchungRepo)
    {
        _buchungRepo = buchungRepo;
        LetzteBuchungen = new ObservableCollection<Buchung>();

        VorigerMonatCmd = new AsyncRelayCommand(VorigerMonatAsync);
        NaechsterMonatCmd = new AsyncRelayCommand(NaechsterMonatAsync);
        HeutigerMonatCmd  = new AsyncRelayCommand(HeutigerMonatAsync);
    }

    /// <summary>Erster Tag des aktuell angezeigten Monats.</summary>
    public DateTime AktuellerMonat
    {
        get => _aktuellerMonat;
        private set
        {
            if (SetProperty(ref _aktuellerMonat, value))
            {
                OnPropertyChanged(nameof(MonatBezeichnung));
            }
        }
    }

    /// <summary>Lokalisierter Monatsname für die Anzeige, z.B. "Mai 2026".</summary>
    public string MonatBezeichnung =>
        AktuellerMonat.ToString("MMMM yyyy",
            new System.Globalization.CultureInfo("de-CH"));

    /// <summary>Statistik-Werte des aktuellen Monats.</summary>
    public MonatsStatistik? Statistik
    {
        get => _statistik;
        private set => SetProperty(ref _statistik, value);
    }

    /// <summary>Die letzten 5 Buchungen des aktuell gewählten Monats.</summary>
    public ObservableCollection<Buchung> LetzteBuchungen { get; }

    /// <summary>Anzeige-Hilfe für die UI: läuft gerade ein Ladevorgang?</summary>
    public bool LaedtGerade
    {
        get => _laedtGerade;
        private set => SetProperty(ref _laedtGerade, value);
    }

    public ICommand VorigerMonatCmd { get; }
    public ICommand NaechsterMonatCmd { get; }
    public ICommand HeutigerMonatCmd { get; }

    /// <summary>
    /// Lädt alle Daten für den aktuell gewählten Monat aus der DB.
    /// Wird bei jedem Monatswechsel und beim Aktivieren des
    /// Dashboards aufgerufen.
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            LaedtGerade = true;

            var stats = await _buchungRepo.GetMonatsStatistikAsync(
                AktuellerMonat.Year, AktuellerMonat.Month);
            Statistik = stats;

            var buchungen = await _buchungRepo.GetForMonatAsync(
                AktuellerMonat.Year, AktuellerMonat.Month);

            LetzteBuchungen.Clear();
            foreach (var b in buchungen.Take(5))
                LetzteBuchungen.Add(b);
        }
        finally
        {
            LaedtGerade = false;
        }
    }

    private async Task VorigerMonatAsync()
    {
        AktuellerMonat = AktuellerMonat.AddMonths(-1);
        await LoadAsync();
    }

    private async Task NaechsterMonatAsync()
    {
        AktuellerMonat = AktuellerMonat.AddMonths(1);
        await LoadAsync();
    }

    private async Task HeutigerMonatAsync()
    {
        AktuellerMonat = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        await LoadAsync();
    }
}

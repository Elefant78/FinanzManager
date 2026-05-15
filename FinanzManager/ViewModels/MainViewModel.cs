using System.Windows.Input;
using FinanzManager.MVVM;

namespace FinanzManager.ViewModels;

/// <summary>
/// ============================================================
/// MainViewModel — Orchestrierung der Hauptansicht
/// ============================================================
/// Verwaltet die Navigation zwischen den drei Haupt-Sub-Views:
///   - Dashboard (Monatsübersicht)
///   - Buchungen (Liste + CRUD)
///   - Kategorien (Liste + Hinzufügen/Löschen)
///
/// Die aktuell aktive Sub-View wird in CurrentView gehalten und
/// im MainWindow per ContentControl angezeigt. Die Sidebar-
/// Buttons binden an die "Zu...Cmd"-Commands.
///
/// Die Sub-VMs werden via Constructor-Injection vom DI-Container
/// geliefert und während der Lebenszeit der App wiederverwendet
/// (Singleton). LoadAsync wird nur beim erstmaligen Aktivieren
/// und bei expliziter Aktualisierung aufgerufen.
/// ============================================================
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly DashboardViewModel _dashboardVM;
    private readonly BuchungenViewModel _buchungenVM;
    private readonly KategorienViewModel _kategorienVM;

    private ViewModelBase? _currentView;

    public MainViewModel(
        DashboardViewModel dashboardVM,
        BuchungenViewModel buchungenVM,
        KategorienViewModel kategorienVM)
    {
        _dashboardVM = dashboardVM;
        _buchungenVM = buchungenVM;
        _kategorienVM = kategorienVM;

        ZuDashboardCmd  = new AsyncRelayCommand(ZuDashboardAsync);
        ZuBuchungenCmd  = new AsyncRelayCommand(ZuBuchungenAsync);
        ZuKategorienCmd = new AsyncRelayCommand(ZuKategorienAsync);
    }

    public ViewModelBase? CurrentView
    {
        get => _currentView;
        private set
        {
            if (SetProperty(ref _currentView, value))
            {
                // Diese drei Properties hängen von CurrentView ab —
                // bei Wechsel müssen wir explizit informieren.
                OnPropertyChanged(nameof(IstDashboardAktiv));
                OnPropertyChanged(nameof(IstBuchungenAktiv));
                OnPropertyChanged(nameof(IstKategorienAktiv));
            }
        }
    }

    /// <summary>
    /// Hilfsproperties für Sidebar-Styling: zeigen, welcher
    /// Bereich gerade aktiv ist, damit der entsprechende
    /// Nav-Button hervorgehoben werden kann.
    /// </summary>
    public bool IstDashboardAktiv  => CurrentView is DashboardViewModel;
    public bool IstBuchungenAktiv  => CurrentView is BuchungenViewModel;
    public bool IstKategorienAktiv => CurrentView is KategorienViewModel;

    public ICommand ZuDashboardCmd { get; }
    public ICommand ZuBuchungenCmd { get; }
    public ICommand ZuKategorienCmd { get; }

    /// <summary>
    /// Wird beim App-Start einmalig aufgerufen.
    /// Setzt die Initial-Ansicht (Dashboard).
    /// </summary>
    public async Task InitialisierenAsync()
    {
        await ZuDashboardAsync();
    }

    private async Task ZuDashboardAsync()
    {
        CurrentView = _dashboardVM;
        await _dashboardVM.LoadAsync();
    }

    private async Task ZuBuchungenAsync()
    {
        CurrentView = _buchungenVM;
        await _buchungenVM.LoadAsync();
    }

    private async Task ZuKategorienAsync()
    {
        CurrentView = _kategorienVM;
        await _kategorienVM.LoadAsync();
    }
}

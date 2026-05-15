using System.Windows;
using FinanzManager.Models;
using FinanzManager.ViewModels;
using FinanzManager.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FinanzManager.Services;

/// <summary>
/// ============================================================
/// DialogService — vollständige Implementierung (Phase 6)
/// ============================================================
/// Öffnet das BuchungBearbeitenWindow modal und kommuniziert
/// das Ergebnis (gespeichert / abgebrochen) zurück an den Aufrufer.
///
/// Pattern für die Window-Lebenszeit:
///  1. ViewModel und Window aus dem DI-Container holen
///     (beide als Transient registriert → frische Instanz pro Aufruf)
///  2. ViewModel.LoadAsync(bestehende) aufrufen
///  3. Window.DataContext = ViewModel
///  4. Auf das RequestClose-Event des VM hören —
///     wenn ausgelöst: Window schliessen und Ergebnis merken
///  5. ShowDialog() (blockiert bis das Window geschlossen ist)
///  6. Event-Handler abmelden, Ergebnis zurückgeben
///
/// Owner = MainWindow: das Dialog-Fenster wird über dem
/// Hauptfenster zentriert und ist modal nur dazu (nicht zu
/// anderen Apps).
/// ============================================================
/// </summary>
public class DialogService : IDialogService
{
    private readonly IServiceProvider _services;

    public DialogService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<bool> ZeigeBuchungBearbeitenAsync(Buchung? bestehende = null)
    {
        // Frisches VM und Window aus dem DI-Container.
        var vm = _services.GetRequiredService<BuchungBearbeitenViewModel>();
        await vm.LoadAsync(bestehende);

        var window = _services.GetRequiredService<BuchungBearbeitenWindow>();
        window.DataContext = vm;
        window.Owner = Application.Current.MainWindow;

        // Event-Handler zum Schliessen registrieren.
        bool hatGespeichert = false;
        void Handler(object? sender, bool gespeichert)
        {
            hatGespeichert = gespeichert;
            window.Close();
        }

        vm.RequestClose += Handler;
        try
        {
            // ShowDialog() blockiert, bis das Fenster geschlossen wird.
            window.ShowDialog();
        }
        finally
        {
            vm.RequestClose -= Handler;
        }

        return hatGespeichert;
    }

    public Task ZeigeFehlerAsync(string titel, string nachricht)
    {
        MessageBox.Show(
            Application.Current.MainWindow,
            nachricht, titel,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        return Task.CompletedTask;
    }

    public Task<bool> ZeigeBestaetigungAsync(string titel, string nachricht)
    {
        var result = MessageBox.Show(
            Application.Current.MainWindow,
            nachricht, titel,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return Task.FromResult(result == MessageBoxResult.Yes);
    }
}

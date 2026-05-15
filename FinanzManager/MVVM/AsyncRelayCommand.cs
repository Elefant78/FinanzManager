using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinanzManager.MVVM;

/// <summary>
/// ============================================================
/// AsyncRelayCommand — Variante für asynchrone Operationen
/// ============================================================
/// Datenbank-Aufrufe sind asynchron (await). Das normale
/// RelayCommand würde async-Lambdas zwar akzeptieren, aber die
/// Tasks würden ohne Awaiten "ins Leere laufen" — Fehler kämen
/// nicht beim Caller an und CanExecute würde schon true liefern,
/// während die Operation noch läuft.
///
/// AsyncRelayCommand löst beides:
///   - Wartet auf den Task
///   - Setzt während der Ausführung CanExecute auf false
///     (schützt vor Doppelklick-Fehlern)
///   - Triggert CanExecuteChanged sauber
///
/// Verwendung im ViewModel:
///   SpeichernCmd = new AsyncRelayCommand(
///       executeAsync: SpeichernAsync,
///       canExecute: () => IstFormularGueltig);
/// ============================================================
/// </summary>
public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _executeAsync;
    private readonly Func<bool>? _canExecute;
    private bool _laeuftGerade;

    public AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
    {
        _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (_laeuftGerade) return false;          // während laufender Ausführung blockieren
        return _canExecute?.Invoke() ?? true;
    }

    public async void Execute(object? parameter)
    {
        // "async void" ist hier ok, weil ICommand.Execute void zurückgeben muss.
        // Das ist eine der wenigen Stellen, wo async void erlaubt ist.
        if (!CanExecute(parameter)) return;

        try
        {
            _laeuftGerade = true;
            RaiseCanExecuteChanged();
            await _executeAsync();
        }
        finally
        {
            _laeuftGerade = false;
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public void RaiseCanExecuteChanged()
        => CommandManager.InvalidateRequerySuggested();
}

using System;
using System.Windows.Input;

namespace FinanzManager.MVVM;

/// <summary>
/// ============================================================
/// RelayCommand — Implementierung von ICommand
/// ============================================================
/// In WPF werden Buttons (und ähnliche Steuerelemente) nicht
/// per Click-Event-Handler aus dem Code-Behind verdrahtet,
/// sondern via Command-Property an ein ICommand im ViewModel
/// gebunden:
///
///   <Button Content="Speichern" Command="{Binding SpeichernCmd}" />
///
/// ICommand braucht zwei Methoden:
///  - Execute(): was passiert beim Klick?
///  - CanExecute(): darf der Button geklickt werden?
///
/// Diese Klasse "leitet" (deshalb der Name "Relay") den Aufruf
/// einfach an Lambdas oder Methoden weiter, die man im
/// Konstruktor übergibt.
///
/// Beispiel im ViewModel:
///   SpeichernCmd = new RelayCommand(
///       execute: () => Speichern(),
///       canExecute: () => IstFormularGueltig);
///
/// CanExecuteChanged: WPF muss informiert werden, wenn sich das
/// CanExecute-Ergebnis ändert (z.B. wenn ein Formular gültig
/// wird). CommandManager.RequerySuggested fragt automatisch
/// alle Commands neu, wenn sich der Fokus oder die Tastatur-
/// Eingabe ändert — gut genug für 95% der Fälle.
/// ============================================================
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
        => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter)
        => _execute();

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    /// <summary>
    /// Manuell auslösen, falls man WPF zwingen will, CanExecute
    /// jetzt sofort neu zu prüfen (z.B. nach einer asynchronen
    /// Aktion, die den Zustand verändert hat).
    /// </summary>
    public void RaiseCanExecuteChanged()
        => CommandManager.InvalidateRequerySuggested();
}

/// <summary>
/// ============================================================
/// RelayCommand&lt;T&gt; — Variante mit Parameter
/// ============================================================
/// Manchmal will man einen Wert an den Command übergeben, z.B.
/// eine Buchung, die gelöscht werden soll:
///
///   <Button Command="{Binding LoeschenCmd}"
///           CommandParameter="{Binding}" />
///
/// Im ViewModel:
///   LoeschenCmd = new RelayCommand&lt;Buchung&gt;(b => Loeschen(b));
/// ============================================================
/// </summary>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Predicate<T?>? _canExecute;

    public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute is null) return true;
        // Cast je nach Wert sicher durchführen
        if (parameter is null && default(T) is not null) return false;
        return _canExecute((T?)parameter);
    }

    public void Execute(object? parameter)
        => _execute((T?)parameter);

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}

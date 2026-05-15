using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FinanzManager.MVVM;

/// <summary>
/// ============================================================
/// ViewModelBase — Basisklasse für alle ViewModels
/// ============================================================
/// Stellt INotifyPropertyChanged bereit, das zentrale Interface
/// von WPF-Data-Binding. Wenn ein ViewModel eine Property ändert,
/// muss es das UI darüber informieren — sonst aktualisiert sich
/// die Anzeige nicht. INotifyPropertyChanged ist genau dafür da:
/// Es wirft das PropertyChanged-Event mit dem Property-Namen,
/// und WPF bindet die UI dann neu.
///
/// Statt das in jedem ViewModel selbst zu implementieren, erbt
/// jedes ViewModel von dieser Basisklasse und nutzt die zwei
/// Helfer-Methoden:
///
///  - OnPropertyChanged()       : Event manuell auslösen
///  - SetProperty(ref _x, v)    : Feld setzen + Event auslösen
///                                + true zurückgeben, wenn der
///                                  Wert sich tatsächlich
///                                  geändert hat (nützlich für
///                                  Logik in Settern).
///
/// CallerMemberName: ein Compiler-Trick. Wenn man die Methode
/// ohne propertyName aufruft, fügt der Compiler automatisch den
/// Namen der aufrufenden Property/Methode ein. Spart Tipparbeit
/// und vermeidet Tippfehler.
/// ============================================================
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// Wird ausgelöst, wenn sich eine Property ändert.
    /// WPF abonniert dieses Event automatisch beim Binding.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Wirft das PropertyChanged-Event manuell. Nützlich, wenn
    /// eine berechnete Property von einem anderen Wert abhängt:
    ///
    ///   public string Vollname => $"{Vorname} {Nachname}";
    ///
    /// Beim Setzen von Vorname:
    ///   OnPropertyChanged(nameof(Vollname));
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Standard-Setter für Properties. Vergleicht alten und neuen
    /// Wert; wenn unterschiedlich, setzt das Feld und feuert das
    /// Event. Liefert true bei Änderung, false sonst.
    ///
    /// Verwendung im ViewModel:
    ///   private string _name = "";
    ///   public string Name
    ///   {
    ///       get => _name;
    ///       set => SetProperty(ref _name, value);
    ///   }
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

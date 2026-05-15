using System.Windows;
using FinanzManager.Data;
using FinanzManager.Data.Repositories;
using FinanzManager.Services;
using FinanzManager.ViewModels;
using FinanzManager.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FinanzManager;

/// <summary>
/// ============================================================
/// App.xaml.cs — Einstiegspunkt der Anwendung
/// ============================================================
/// Phase 5: alle ViewModels und Services sind im DI-Container
/// registriert. Beim Start wird das MainWindow mit dem
/// MainViewModel als DataContext angezeigt.
/// ============================================================
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // ----- Datenbank -----
                services.AddDbContextFactory<FinanzDbContext>(options =>
                {
                    var dbPfad = FinanzDbContext.GetStandardDbPfad();
                    options.UseSqlite($"Data Source={dbPfad}");
                });

                // ----- Repositories -----
                services.AddSingleton<IKategorieRepository, KategorieRepository>();
                services.AddSingleton<IBuchungRepository, BuchungRepository>();

                // ----- Services -----
                services.AddSingleton<IDialogService, DialogService>();

                // ----- ViewModels -----
                // Sub-VMs als Singleton: einmal erstellt, von MainViewModel
                // wiederverwendet. Halten Status (z.B. aktueller Monat im
                // Dashboard) zwischen Navigation.
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<BuchungenViewModel>();
                services.AddSingleton<KategorienViewModel>();
                services.AddSingleton<MainViewModel>();

                // BuchungBearbeitenViewModel als Transient: pro Dialog-Öffnen
                // ein frisches ViewModel mit neuem Zustand.
                services.AddTransient<BuchungBearbeitenViewModel>();

                // ----- Windows -----
                services.AddSingleton<MainWindow>();
                // Dialog-Window als Transient: jedes Mal ein frisches
                // Fenster, weil ein einmal geschlossenes Window in WPF
                // nicht erneut geöffnet werden kann.
                services.AddTransient<BuchungBearbeitenWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // ----- Datenbank initialisieren -----
        var dbFactory = _host.Services.GetRequiredService<IDbContextFactory<FinanzDbContext>>();
        using (var db = await dbFactory.CreateDbContextAsync())
        {
            await db.Database.EnsureCreatedAsync();
        }

        // ----- MainViewModel initialisieren -----
        var mainVM = _host.Services.GetRequiredService<MainViewModel>();
        await mainVM.InitialisierenAsync();

        // ----- MainWindow zeigen mit MainVM als DataContext -----
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = mainVM;
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}

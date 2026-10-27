using LiveLock.App.Services;
using LiveLock.Core.Logging;
using Microsoft.UI.Xaml;

namespace LiveLock.App;

public partial class App : Application
{
    public static AppState State { get; private set; } = null!;
    public static MainWindow MainWindow { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) =>
        {
            AppLogger.Error("Unhandled exception", e.Exception);
            e.Handled = true; // one bad media file must not take the whole app down
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        State = AppState.Create();
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}

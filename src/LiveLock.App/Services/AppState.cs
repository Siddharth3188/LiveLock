using LiveLock.Core.Configuration;
using LiveLock.Core.Logging;
using LiveLock.Core.Models;
using Microsoft.UI.Dispatching;

namespace LiveLock.App.Services;

/// <summary>Owns the single AppSettings instance and saves it (debounced) so slider drags don't hammer the disk.</summary>
public sealed class AppState
{
    private readonly ISettingsStore _store;
    private readonly DispatcherQueueTimer _saveTimer;

    public AppSettings Settings { get; }

    private AppState(ISettingsStore store, AppSettings settings)
    {
        _store = store;
        Settings = settings;
        _saveTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _saveTimer.Interval = TimeSpan.FromMilliseconds(400);
        _saveTimer.IsRepeating = false;
        _saveTimer.Tick += (_, _) => SaveNow();
    }

    public static AppState Create()
    {
        var store = new JsonSettingsStore(JsonSettingsStore.DefaultPath());
        return new AppState(store, store.Load());
    }

    public void RequestSave() { _saveTimer.Stop(); _saveTimer.Start(); }

    public void SaveNow()
    {
        try { _store.Save(Settings); }
        catch (Exception ex) { AppLogger.Error("Failed to save settings", ex); }
    }

    public WallpaperItem? SelectedWallpaper =>
        Settings.SelectedWallpaperId is { } id ? Settings.Library.FirstOrDefault(i => i.Id == id) : null;
}

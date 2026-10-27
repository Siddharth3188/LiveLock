using System.Text.Json;
using System.Text.Json.Serialization;
using LiveLock.Core.Logging;
using LiveLock.Core.Models;

namespace LiveLock.Core.Configuration;

public interface ISettingsStore
{
    AppSettings Load();
    void Save(AppSettings settings);
}

public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _path;
    private readonly object _gate = new();

    public JsonSettingsStore(string path) => _path = path;

    public static string DefaultPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LiveLock", "settings.json");

    public AppSettings Load()
    {
        lock (_gate)
        {
            if (!File.Exists(_path)) return new AppSettings();
            try
            {
                var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path), Options) ?? new AppSettings();
                if (settings.Version < AppSettings.CurrentVersion) settings.Version = AppSettings.CurrentVersion; // future migrations go here
                settings.Validate();
                return settings;
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                AppLogger.Error("Settings file unreadable; starting with defaults.", ex);
                try { File.Move(_path, _path + $".corrupt-{DateTime.UtcNow:yyyyMMddHHmmss}", overwrite: true); } catch { }
                return new AppSettings();
            }
        }
    }

    public void Save(AppSettings settings)
    {
        lock (_gate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var tmp = _path + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(settings, Options));
            File.Move(tmp, _path, overwrite: true); // replace, so a crash never leaves a half-written file
        }
    }
}

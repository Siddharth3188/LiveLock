namespace LiveLock.Core.Logging;

/// <summary>Minimal file logger. Callers log file names only, never full user paths or file contents.</summary>
public static class AppLogger
{
    private static readonly object Gate = new();
    public static string LogPath { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LiveLock", "logs", "livelock.log");

    public static void Info(string message) => Write("INFO", message, null);
    public static void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

    private static void Write(string level, string message, Exception? ex)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                var tail = ex is null ? "" : $" :: {ex.GetType().Name}: {ex.Message}";
                File.AppendAllText(LogPath, $"{DateTime.Now:O} [{level}] {message}{tail}{Environment.NewLine}");
            }
        }
        catch { /* logging must never crash the app */ }
    }
}

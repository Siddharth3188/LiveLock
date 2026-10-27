using LiveLock.Core.Models;

namespace LiveLock.Core.Media;

public enum ImportStatus { Added, Duplicate, Unsupported, NotFound }

public sealed record ImportResult(ImportStatus Status, WallpaperItem? Item, string Message);

/// <summary>Adds references to files. Never copies, moves, overwrites or deletes the user's file.</summary>
public static class LibraryImporter
{
    public static ImportResult Import(IList<WallpaperItem> library, string path)
    {
        var name = Path.GetFileName(path);
        if (!File.Exists(path)) return new(ImportStatus.NotFound, null, $"“{name}” could not be found.");

        var kind = MediaTypes.Classify(path);
        if (kind == MediaKind.Unknown)
            return new(ImportStatus.Unsupported, null, $"“{name}” is not a supported type. Supported: {string.Join(", ", MediaTypes.SupportedExtensions)}");

        var full = Path.GetFullPath(path);
        var existing = library.FirstOrDefault(i => string.Equals(i.Path, full, StringComparison.OrdinalIgnoreCase));
        if (existing is not null) return new(ImportStatus.Duplicate, existing, $"“{name}” is already in your library.");

        var item = new WallpaperItem { Path = full, DisplayName = Path.GetFileNameWithoutExtension(full), Kind = kind };
        library.Add(item);
        return new(ImportStatus.Added, item, $"Added “{name}”.");
    }
}

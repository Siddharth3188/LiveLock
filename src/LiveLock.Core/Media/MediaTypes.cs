using LiveLock.Core.Models;

namespace LiveLock.Core.Media;

/// <summary>Single place that decides which file types LiveLock accepts. Add new formats here.</summary>
public static class MediaTypes
{
    private static readonly Dictionary<string, MediaKind> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        [".mp4"] = MediaKind.Video, [".m4v"] = MediaKind.Video, [".mov"] = MediaKind.Video,
        [".wmv"] = MediaKind.Video, [".avi"] = MediaKind.Video, [".mkv"] = MediaKind.Video, [".webm"] = MediaKind.Video,
        [".gif"] = MediaKind.AnimatedImage,
        [".png"] = MediaKind.Image, [".jpg"] = MediaKind.Image, [".jpeg"] = MediaKind.Image,
        [".webp"] = MediaKind.Image, [".bmp"] = MediaKind.Image,
    };

    public static IReadOnlyCollection<string> SupportedExtensions => Map.Keys;

    public static MediaKind Classify(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return MediaKind.Unknown;
        return Map.TryGetValue(Path.GetExtension(path), out var kind) ? kind : MediaKind.Unknown;
    }

    public static bool IsSupported(string? path) => Classify(path) != MediaKind.Unknown;

    public static string Describe(MediaKind kind) => kind switch
    {
        MediaKind.Video => "Video",
        MediaKind.AnimatedImage => "Animated GIF",
        MediaKind.Image => "Image",
        _ => "Unsupported",
    };
}

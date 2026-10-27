using System.Text.Json.Serialization;

namespace LiveLock.Core.Models;

/// <summary>A reference to a user's media file. The original file is never copied, modified or deleted.</summary>
public sealed class WallpaperItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Path { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public MediaKind Kind { get; set; }
    public DateTime AddedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedUtc { get; set; }

    [JsonIgnore] public bool IsMissing => string.IsNullOrEmpty(Path) || !File.Exists(Path);
    [JsonIgnore] public string FileName => System.IO.Path.GetFileName(Path);
}

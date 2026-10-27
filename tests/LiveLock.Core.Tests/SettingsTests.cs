using LiveLock.Core.Configuration;
using LiveLock.Core.Media;
using LiveLock.Core.Models;
using Xunit;

namespace LiveLock.Core.Tests;

public class SettingsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "livelock-tests-" + Guid.NewGuid().ToString("N"));
    public SettingsTests() => Directory.CreateDirectory(_dir);
    public void Dispose() { try { Directory.Delete(_dir, true); } catch { } }

    [Fact]
    public void Defaults_AreSane()
    {
        var s = new AppSettings();
        Assert.True(s.Clock.ShowClock);
        Assert.True(s.Playback.Loop);
        Assert.True(s.Playback.Muted);
        Assert.Equal(AppSettings.CurrentVersion, s.Version);
    }

    [Fact]
    public void RoundTrip_PreservesValues()
    {
        var store = new JsonSettingsStore(Path.Combine(_dir, "s.json"));
        var s = new AppSettings();
        s.Clock.Use24Hour = true; s.Clock.ColorHex = "#39F5FF"; s.Clock.Alignment = ClockAlignment.Right;
        s.Library.Add(new WallpaperItem { Path = @"C:\x\a.mp4", DisplayName = "a", Kind = MediaKind.Video });
        store.Save(s);
        var loaded = store.Load();
        Assert.True(loaded.Clock.Use24Hour);
        Assert.Equal("#39F5FF", loaded.Clock.ColorHex);
        Assert.Equal(ClockAlignment.Right, loaded.Clock.Alignment);
        Assert.Single(loaded.Library);
        Assert.Equal(MediaKind.Video, loaded.Library[0].Kind);
    }

    [Fact]
    public void CorruptFile_FallsBackToDefaults_AndKeepsBackup()
    {
        var path = Path.Combine(_dir, "s.json");
        File.WriteAllText(path, "{ not json");
        var loaded = new JsonSettingsStore(path).Load();
        Assert.True(loaded.Clock.ShowClock);
        Assert.Contains(Directory.GetFiles(_dir), f => f.Contains(".corrupt-"));
    }

    [Fact]
    public void Validate_ClampsOutOfRange()
    {
        var s = new AppSettings();
        s.Clock.FontSize = 9999; s.Clock.VerticalPosition = -3; s.Clock.ColorHex = "nope"; s.Appearance.Dimming = 5;
        s.Validate();
        Assert.Equal(400, s.Clock.FontSize);
        Assert.Equal(0, s.Clock.VerticalPosition);
        Assert.Equal("#FFFFFF", s.Clock.ColorHex);
        Assert.Equal(0.9, s.Appearance.Dimming);
    }

    [Fact]
    public void Preset_ChangesValues_AndRecordsName()
    {
        var s = new AppSettings();
        Assert.True(Presets.Apply(s, "Neon futuristic"));
        Assert.True(s.Clock.Use24Hour);
        Assert.Equal("Neon futuristic", s.SelectedPreset);
        Assert.False(Presets.Apply(s, "does not exist"));
    }

    [Fact]
    public void Preset_DoesNotTouchLibrary()
    {
        var s = new AppSettings();
        s.Library.Add(new WallpaperItem { Path = "x.mp4" });
        Presets.Apply(s, "Cinematic");
        Assert.Single(s.Library);
    }
}

public class MediaTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "livelock-media-" + Guid.NewGuid().ToString("N"));
    public MediaTests() => Directory.CreateDirectory(_dir);
    public void Dispose() { try { Directory.Delete(_dir, true); } catch { } }

    [Theory]
    [InlineData("a.MP4", MediaKind.Video)] [InlineData("a.gif", MediaKind.AnimatedImage)]
    [InlineData("a.JPEG", MediaKind.Image)] [InlineData("a.webp", MediaKind.Image)]
    [InlineData("a.txt", MediaKind.Unknown)] [InlineData("", MediaKind.Unknown)]
    public void Classify(string path, MediaKind expected) => Assert.Equal(expected, MediaTypes.Classify(path));

    [Fact]
    public void Import_AddsThenDetectsDuplicate_AndNeverModifiesFile()
    {
        var f = Path.Combine(_dir, "clip.mp4");
        File.WriteAllText(f, "data");
        var lib = new List<WallpaperItem>();
        Assert.Equal(ImportStatus.Added, LibraryImporter.Import(lib, f).Status);
        Assert.Equal(ImportStatus.Duplicate, LibraryImporter.Import(lib, f).Status);
        Assert.Single(lib);
        Assert.Equal("data", File.ReadAllText(f));
    }

    [Fact] public void Import_Unsupported() => Assert.Equal(ImportStatus.Unsupported, ImportFile("notes.txt"));
    [Fact] public void Import_Missing() => Assert.Equal(ImportStatus.NotFound, LibraryImporter.Import(new List<WallpaperItem>(), Path.Combine(_dir, "nope.mp4")).Status);

    [Fact]
    public void MissingFile_IsDetected()
    {
        var f = Path.Combine(_dir, "gone.mp4");
        File.WriteAllText(f, "x");
        var lib = new List<WallpaperItem>();
        var item = LibraryImporter.Import(lib, f).Item!;
        Assert.False(item.IsMissing);
        File.Delete(f);
        Assert.True(item.IsMissing);
    }

    private ImportStatus ImportFile(string name)
    {
        var f = Path.Combine(_dir, name);
        File.WriteAllText(f, "x");
        return LibraryImporter.Import(new List<WallpaperItem>(), f).Status;
    }
}

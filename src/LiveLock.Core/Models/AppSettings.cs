using LiveLock.Core.Formatting;

namespace LiveLock.Core.Models;

public enum ClockAlignment { Left, Center, Right }
public enum ScalingMode { Cover, Contain, Stretch }

public sealed class ClockSettings
{
    public bool ShowClock { get; set; } = true;
    public bool Use24Hour { get; set; } = false;
    public bool ShowAmPm { get; set; } = false;
    public string FontFamily { get; set; } = "Segoe UI Variable Display";
    public double FontSize { get; set; } = 140;
    public int FontWeight { get; set; } = 600;
    public string ColorHex { get; set; } = "#FFFFFF";
    public ClockAlignment Alignment { get; set; } = ClockAlignment.Center;
    /// <summary>0 = top edge, 1 = bottom edge.</summary>
    public double VerticalPosition { get; set; } = 0.2;
    public double Opacity { get; set; } = 0.95;

    public bool ShowDate { get; set; } = true;
    public bool ShowWeekday { get; set; } = true;
    public bool ShowDay { get; set; } = true;
    public bool ShowMonth { get; set; } = true;
    public bool ShowYear { get; set; } = false;
    public bool AbbreviatedNames { get; set; } = false;
}

public sealed class AppearanceSettings
{
    public bool ShowOverlay { get; set; } = true;
    /// <summary>0..0.9 black dimming over the whole wallpaper.</summary>
    public double Dimming { get; set; } = 0.15;
    /// <summary>0..1 strength of the top gradient behind the clock.</summary>
    public double GradientIntensity { get; set; } = 0.35;
    public ScalingMode Scaling { get; set; } = ScalingMode.Cover;
}

public sealed class PlaybackSettings
{
    public bool Loop { get; set; } = true;
    public bool Muted { get; set; } = true;
    public bool AutoPlay { get; set; } = true;
}

public sealed class AppSettings
{
    public const int CurrentVersion = 1;

    public int Version { get; set; } = CurrentVersion;
    public ClockSettings Clock { get; set; } = new();
    public AppearanceSettings Appearance { get; set; } = new();
    public PlaybackSettings Playback { get; set; } = new();
    public string? SelectedPreset { get; set; } = "macOS minimal";
    public Guid? SelectedWallpaperId { get; set; }
    public List<WallpaperItem> Library { get; set; } = new();

    /// <summary>Clamps out-of-range values so a hand-edited or corrupt file cannot break the UI.</summary>
    public void Validate()
    {
        Clock ??= new(); Appearance ??= new(); Playback ??= new(); Library ??= new();
        Clock.FontSize = Math.Clamp(Clock.FontSize, 24, 400);
        Clock.FontWeight = Math.Clamp(Clock.FontWeight, 100, 900);
        Clock.VerticalPosition = Math.Clamp(Clock.VerticalPosition, 0, 1);
        Clock.Opacity = Math.Clamp(Clock.Opacity, 0.1, 1);
        if (string.IsNullOrWhiteSpace(Clock.FontFamily)) Clock.FontFamily = "Segoe UI";
        if (!ColorParser.TryParse(Clock.ColorHex, out _)) Clock.ColorHex = "#FFFFFF";
        Appearance.Dimming = Math.Clamp(Appearance.Dimming, 0, 0.9);
        Appearance.GradientIntensity = Math.Clamp(Appearance.GradientIntensity, 0, 1);
        Library.RemoveAll(i => i is null || string.IsNullOrWhiteSpace(i.Path));
    }
}

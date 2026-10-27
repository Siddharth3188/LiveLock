using LiveLock.Core.Models;

namespace LiveLock.Core.Configuration;

public sealed record Preset(string Name, Action<AppSettings> Apply);

/// <summary>Presets only change configuration values; they never duplicate rendering code.</summary>
public static class Presets
{
    public static IReadOnlyList<Preset> BuiltIn { get; } = new[]
    {
        new Preset("macOS minimal", s =>
        {
            var c = s.Clock; c.FontFamily = "Segoe UI Variable Display"; c.FontSize = 140; c.FontWeight = 600; c.ColorHex = "#FFFFFF";
            c.Alignment = ClockAlignment.Center; c.VerticalPosition = 0.2; c.Opacity = 0.95; c.Use24Hour = false; c.ShowAmPm = false;
            c.ShowDate = true; c.ShowWeekday = true; c.ShowDay = true; c.ShowMonth = true; c.ShowYear = false;
            s.Appearance.Dimming = 0.15; s.Appearance.GradientIntensity = 0.35;
        }),
        new Preset("Cinematic", s =>
        {
            var c = s.Clock; c.FontFamily = "Georgia"; c.FontSize = 120; c.FontWeight = 300; c.ColorHex = "#F5E9D0";
            c.Alignment = ClockAlignment.Left; c.VerticalPosition = 0.75; c.Opacity = 0.9;
            s.Appearance.Dimming = 0.3; s.Appearance.GradientIntensity = 0.6;
        }),
        new Preset("Minimal dark", s =>
        {
            var c = s.Clock; c.FontFamily = "Segoe UI"; c.FontSize = 90; c.FontWeight = 300; c.ColorHex = "#D0D0D0";
            c.Alignment = ClockAlignment.Center; c.VerticalPosition = 0.5; c.Opacity = 0.85;
            s.Appearance.Dimming = 0.55; s.Appearance.GradientIntensity = 0;
        }),
        new Preset("Nature", s =>
        {
            var c = s.Clock; c.FontFamily = "Segoe UI Variable Display"; c.FontSize = 130; c.FontWeight = 400; c.ColorHex = "#F2FFF0";
            c.Alignment = ClockAlignment.Right; c.VerticalPosition = 0.15; c.Opacity = 0.95;
            s.Appearance.Dimming = 0.1; s.Appearance.GradientIntensity = 0.25;
        }),
        new Preset("Neon futuristic", s =>
        {
            var c = s.Clock; c.FontFamily = "Bahnschrift"; c.FontSize = 150; c.FontWeight = 700; c.ColorHex = "#39F5FF"; c.Use24Hour = true;
            c.Alignment = ClockAlignment.Center; c.VerticalPosition = 0.3; c.Opacity = 1;
            s.Appearance.Dimming = 0.4; s.Appearance.GradientIntensity = 0.5;
        }),
    };

    public static Preset? Find(string? name) => BuiltIn.FirstOrDefault(p => p.Name == name);

    public static bool Apply(AppSettings settings, string name)
    {
        var p = Find(name);
        if (p is null) return false;
        p.Apply(settings);
        settings.SelectedPreset = name;
        settings.Validate();
        return true;
    }
}

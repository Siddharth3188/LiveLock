using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveLock.App.Services;
using LiveLock.Core.Configuration;
using LiveLock.Core.Formatting;
using LiveLock.Core.Models;

namespace LiveLock.App.ViewModels;

/// <summary>
/// Editable view of AppSettings. Every change is written straight back to the settings model and a debounced save is requested.
/// It never touches the wallpaper player, so changing clock settings cannot restart playback.
/// </summary>
public sealed partial class StudioViewModel : ObservableObject
{
    private readonly AppState _state;
    private bool _loading;

    public StudioViewModel(AppState state)
    {
        _state = state;
        // A saved font that isn't in the curated list must still be selectable, or the ComboBox would write null back.
        var current = state.Settings.Clock.FontFamily;
        if (!string.IsNullOrWhiteSpace(current) && !FontChoices.Contains(current)) FontChoices.Insert(0, current);
        Reload();
    }

    public IReadOnlyList<string> PresetNames { get; } = Presets.BuiltIn.Select(p => p.Name).ToList();
    public List<string> FontChoices { get; } = new() { "Segoe UI Variable Display", "Segoe UI", "Bahnschrift", "Georgia", "Consolas", "Cascadia Mono", "Arial" };

    // Clock
    [ObservableProperty] private bool _showClock;
    [ObservableProperty] private bool _use24Hour;
    [ObservableProperty] private bool _showAmPm;
    [ObservableProperty] private string _fontFamily = "";
    [ObservableProperty] private double _fontSize;
    [ObservableProperty] private double _fontWeight;
    [ObservableProperty] private string _colorHex = "#FFFFFF";
    [ObservableProperty] private int _alignmentIndex;
    [ObservableProperty] private double _verticalPosition;   // 0..100
    [ObservableProperty] private double _clockOpacity;       // 10..100
    // Date
    [ObservableProperty] private bool _showDate;
    [ObservableProperty] private bool _showWeekday;
    [ObservableProperty] private bool _showDay;
    [ObservableProperty] private bool _showMonth;
    [ObservableProperty] private bool _showYear;
    [ObservableProperty] private bool _abbreviatedNames;
    // Appearance
    [ObservableProperty] private bool _showOverlay;
    [ObservableProperty] private double _dimming;            // 0..90
    [ObservableProperty] private double _gradient;           // 0..100
    [ObservableProperty] private int _scalingIndex;
    // Playback
    [ObservableProperty] private bool _loop;
    [ObservableProperty] private bool _muted;
    // Presets / live text
    [ObservableProperty] private string? _presetName;
    [ObservableProperty] private string _clockText = "";
    [ObservableProperty] private string _dateText = "";

    public void Reload()
    {
        _loading = true;
        var s = _state.Settings; var c = s.Clock;
        ShowClock = c.ShowClock; Use24Hour = c.Use24Hour; ShowAmPm = c.ShowAmPm; FontFamily = c.FontFamily;
        FontSize = c.FontSize; FontWeight = c.FontWeight; ColorHex = c.ColorHex; AlignmentIndex = (int)c.Alignment;
        VerticalPosition = c.VerticalPosition * 100; ClockOpacity = c.Opacity * 100;
        ShowDate = c.ShowDate; ShowWeekday = c.ShowWeekday; ShowDay = c.ShowDay; ShowMonth = c.ShowMonth; ShowYear = c.ShowYear; AbbreviatedNames = c.AbbreviatedNames;
        ShowOverlay = s.Appearance.ShowOverlay; Dimming = s.Appearance.Dimming * 100; Gradient = s.Appearance.GradientIntensity * 100; ScalingIndex = (int)s.Appearance.Scaling;
        Loop = s.Playback.Loop; Muted = s.Playback.Muted; PresetName = s.SelectedPreset;
        _loading = false;
        UpdateTime(DateTime.Now);
    }

    public void ApplyPreset(string name)
    {
        if (!Presets.Apply(_state.Settings, name)) return;
        Reload();
        _state.RequestSave();
    }

    public void ResetToDefaults()
    {
        var d = new AppSettings();
        var s = _state.Settings;
        s.Clock = d.Clock; s.Appearance = d.Appearance; s.Playback = d.Playback; s.SelectedPreset = d.SelectedPreset;
        Reload();
        _state.RequestSave();
    }

    public void UpdateTime(DateTime now)
    {
        var culture = CultureInfo.CurrentCulture;
        ClockText = ClockFormatter.FormatTime(now, _state.Settings.Clock, culture);
        DateText = ClockFormatter.FormatDate(now, _state.Settings.Clock, culture);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (_loading || e.PropertyName is nameof(ClockText) or nameof(DateText) or nameof(PresetName)) return;
        // Controls can briefly push null/-1 (e.g. an empty ComboBox selection). Ignore those instead of corrupting settings.
        if (string.IsNullOrWhiteSpace(FontFamily) || AlignmentIndex is < 0 or > 2 || ScalingIndex is < 0 or > 2) return;
        WriteBack();
        UpdateTime(DateTime.Now); // format-affecting settings refresh the text immediately
        _state.RequestSave();
    }

    private void WriteBack()
    {
        var s = _state.Settings; var c = s.Clock;
        c.ShowClock = ShowClock; c.Use24Hour = Use24Hour; c.ShowAmPm = ShowAmPm; c.FontFamily = FontFamily;
        c.FontSize = FontSize; c.FontWeight = (int)FontWeight; c.ColorHex = ColorHex; c.Alignment = (ClockAlignment)AlignmentIndex;
        c.VerticalPosition = VerticalPosition / 100; c.Opacity = ClockOpacity / 100;
        c.ShowDate = ShowDate; c.ShowWeekday = ShowWeekday; c.ShowDay = ShowDay; c.ShowMonth = ShowMonth; c.ShowYear = ShowYear; c.AbbreviatedNames = AbbreviatedNames;
        s.Appearance.ShowOverlay = ShowOverlay; s.Appearance.Dimming = Dimming / 100; s.Appearance.GradientIntensity = Gradient / 100; s.Appearance.Scaling = (ScalingMode)ScalingIndex;
        s.Playback.Loop = Loop; s.Playback.Muted = Muted;
        s.SelectedPreset = null; // manual tweak: no longer exactly a preset (ApplyPreset sets it again)
    }
}

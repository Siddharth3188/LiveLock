using System.ComponentModel;
using LiveLock.App.ViewModels;
using LiveLock.Core.Formatting;
using LiveLock.Core.Logging;
using LiveLock.Core.Models;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace LiveLock.App.Views;

public sealed partial class StudioPage : Page
{
    public StudioViewModel VM { get; }
    private DispatcherQueueTimer? _clockTimer;
    private MediaPlayer? _player;
    private bool _syncingUi;

    public StudioPage()
    {
        VM = new StudioViewModel(App.State);
        InitializeComponent();
        PresetBox.ItemsSource = VM.PresetNames;
        VM.PropertyChanged += VM_PropertyChanged;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        _syncingUi = true;
        PresetBox.SelectedItem = VM.PresetName;
        var (a, r, g, b) = ColorParser.TryParse(VM.ColorHex, out var c) ? c : ((byte)255, (byte)255, (byte)255, (byte)255);
        ClockColorPicker.Color = Windows.UI.Color.FromArgb(a, r, g, b);
        _syncingUi = false;

        LoadWallpaper(App.State.SelectedWallpaper);
        ApplyOverlay();
        ApplyPlaybackAndScaling();

        // Event-driven clock: wake once per minute, aligned to the minute boundary. No per-second polling.
        _clockTimer = DispatcherQueue.CreateTimer();
        _clockTimer.IsRepeating = false;
        _clockTimer.Tick += (_, _) => TickClock();
        TickClock();
        App.MainWindow.Activated += MainWindow_Activated; // refresh immediately after resume / re-activation
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        App.MainWindow.Activated -= MainWindow_Activated;
        VM.PropertyChanged -= VM_PropertyChanged;
        _clockTimer?.Stop(); _clockTimer = null;
        ReleasePlayer();
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args) => TickClock();

    private void TickClock()
    {
        var now = DateTime.Now;
        VM.UpdateTime(now);
        var next = now.AddMinutes(1).AddSeconds(-now.Second).AddMilliseconds(-now.Millisecond + 50) - now;
        if (_clockTimer is not null) { _clockTimer.Interval = next; _clockTimer.Start(); }
    }

    // ---- Wallpaper layer: only touched when the wallpaper changes ----
    private void LoadWallpaper(WallpaperItem? item)
    {
        ReleasePlayer();
        ImageLayer.Visibility = VideoLayer.Visibility = Visibility.Collapsed;
        DemoBackground.Visibility = Visibility.Visible;
        WallpaperLabel.Text = "Demo background (locally generated). Pick a wallpaper in the Library.";
        PlayPauseButton.Visibility = Visibility.Collapsed;
        if (item is null) return;
        if (item.IsMissing) { WallpaperLabel.Text = $"“{item.FileName}” is missing. Locate it in the Library."; return; }

        try
        {
            if (item.Kind == MediaKind.Video)
            {
                _player = new MediaPlayer
                {
                    Source = MediaSource.CreateFromUri(new Uri(item.Path)),
                    IsLoopingEnabled = App.State.Settings.Playback.Loop,
                    IsMuted = App.State.Settings.Playback.Muted,
                    AutoPlay = App.State.Settings.Playback.AutoPlay,
                };
                _player.MediaFailed += (_, e) => DispatcherQueue.TryEnqueue(() =>
                {
                    AppLogger.Error($"Playback failed for {item.FileName}: {e.Error}");
                    WallpaperLabel.Text = $"Couldn't play “{item.FileName}”. The file may be damaged or use an unsupported codec.";
                    VideoLayer.Visibility = Visibility.Collapsed; DemoBackground.Visibility = Visibility.Visible;
                });
                VideoLayer.SetMediaPlayer(_player);
                VideoLayer.Visibility = Visibility.Visible;
                DemoBackground.Visibility = Visibility.Collapsed;
                PlayPauseButton.Visibility = Visibility.Visible;
                PlayPauseButton.Content = App.State.Settings.Playback.AutoPlay ? "Pause" : "Play";
            }
            else
            {
                // BitmapImage animates GIFs natively; DecodePixelWidth is intentionally not set so quality is preserved.
                ImageLayer.Source = new BitmapImage(new Uri(item.Path));
                ImageLayer.Visibility = Visibility.Visible;
                DemoBackground.Visibility = Visibility.Collapsed;
            }
            WallpaperLabel.Text = item.FileName;
        }
        catch (Exception ex)
        {
            AppLogger.Error($"Could not load {item.FileName}", ex);
            WallpaperLabel.Text = $"Couldn't open “{item.FileName}”.";
        }
    }

    private void ReleasePlayer()
    {
        if (_player is null) return;
        VideoLayer.SetMediaPlayer(null);
        _player.Pause();
        _player.Source = null;
        _player.Dispose();
        _player = null;
    }

    private void PlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (_player is null) return;
        if (_player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing) { _player.Pause(); PlayPauseButton.Content = "Play"; }
        else { _player.Play(); PlayPauseButton.Content = "Pause"; }
    }

    // ---- Settings -> visuals. None of this touches the MediaPlayer's source or position. ----
    private void VM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ApplyOverlay();
        ApplyPlaybackAndScaling();
    }

    private void ApplyPlaybackAndScaling()
    {
        if (_player is not null) { _player.IsLoopingEnabled = VM.Loop; _player.IsMuted = VM.Muted; }
        var stretch = (ScalingMode)VM.ScalingIndex switch { ScalingMode.Contain => Stretch.Uniform, ScalingMode.Stretch => Stretch.Fill, _ => Stretch.UniformToFill };
        ImageLayer.Stretch = stretch;
        VideoLayer.Stretch = stretch;
    }

    private void ApplyOverlay()
    {
        OverlayLayer.Visibility = VM.ShowOverlay ? Visibility.Visible : Visibility.Collapsed;
        DimLayer.Opacity = VM.Dimming / 100;
        GradientLayer.Opacity = VM.Gradient / 100;
        ClockPanel.Visibility = VM.ShowClock ? Visibility.Visible : Visibility.Collapsed;
        ClockPanel.Opacity = VM.ClockOpacity / 100;

        ColorParser.TryParse(VM.ColorHex, out var c);
        var brush = new SolidColorBrush(Windows.UI.Color.FromArgb(c.A, c.R, c.G, c.B));
        var family = new FontFamily(string.IsNullOrWhiteSpace(VM.FontFamily) ? "Segoe UI" : VM.FontFamily);
        var weight = new Windows.UI.Text.FontWeight { Weight = (ushort)Math.Clamp((int)VM.FontWeight, 100, 900) };
        var (h, t) = (ClockAlignment)VM.AlignmentIndex switch
        {
            ClockAlignment.Left => (HorizontalAlignment.Left, TextAlignment.Left),
            ClockAlignment.Right => (HorizontalAlignment.Right, TextAlignment.Right),
            _ => (HorizontalAlignment.Center, TextAlignment.Center),
        };

        ClockPanel.HorizontalAlignment = h;
        TimeBlock.Text = VM.ClockText; TimeBlock.FontSize = VM.FontSize; TimeBlock.FontFamily = family; TimeBlock.FontWeight = weight; TimeBlock.Foreground = brush; TimeBlock.TextAlignment = t;
        DateBlock.Text = VM.DateText; DateBlock.FontSize = Math.Max(16, VM.FontSize * 0.22); DateBlock.FontFamily = family; DateBlock.FontWeight = weight; DateBlock.Foreground = brush; DateBlock.TextAlignment = t;
        DateBlock.Visibility = string.IsNullOrEmpty(VM.DateText) ? Visibility.Collapsed : Visibility.Visible;
        PositionClock();
    }

    private void PositionClock()
    {
        var free = Math.Max(0, PreviewGrid.ActualHeight - ClockPanel.ActualHeight);
        ClockPanel.Margin = new Thickness(32, VM.VerticalPosition / 100 * free, 32, 0);
    }

    private void Preview_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        PreviewClip.Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
        PositionClock();
    }

    private void ClockPanel_SizeChanged(object sender, SizeChangedEventArgs e) => PositionClock();

    private void PresetBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingUi || PresetBox.SelectedItem is not string name) return;
        VM.ApplyPreset(name);
        SyncColorPicker();
    }

    private void ClockColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        if (_syncingUi) return;
        VM.ColorHex = ColorParser.ToHex(args.NewColor.R, args.NewColor.G, args.NewColor.B);
    }

    private void SyncColorPicker()
    {
        _syncingUi = true;
        if (ColorParser.TryParse(VM.ColorHex, out var c)) ClockColorPicker.Color = Windows.UI.Color.FromArgb(c.A, c.R, c.G, c.B);
        _syncingUi = false;
    }

    private async void Reset_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot, Title = "Reset to defaults?",
            Content = "Clock, date, appearance and playback settings return to their defaults. Your library is not affected.",
            PrimaryButtonText = "Reset", CloseButtonText = "Cancel", DefaultButton = ContentDialogButton.Close,
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        VM.ResetToDefaults();
        _syncingUi = true; PresetBox.SelectedItem = VM.PresetName; _syncingUi = false;
        SyncColorPicker();
    }
}

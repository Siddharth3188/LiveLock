using CommunityToolkit.Mvvm.ComponentModel;
using LiveLock.Core.Logging;
using LiveLock.Core.Media;
using LiveLock.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace LiveLock.App.ViewModels;

public sealed partial class LibraryItemViewModel : ObservableObject
{
    public WallpaperItem Item { get; }

    [ObservableProperty] private BitmapImage? _thumbnail;
    [ObservableProperty] private bool _isMissing;
    [ObservableProperty] private bool _isSelected;

    public LibraryItemViewModel(WallpaperItem item)
    {
        Item = item;
        _isMissing = item.IsMissing;
    }

    public string Name => Item.DisplayName;
    public string TypeText => $"{MediaTypes.Describe(Item.Kind)} · {Item.FileName}";
    public Visibility MissingVisibility => IsMissing ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SelectedVisibility => IsSelected ? Visibility.Visible : Visibility.Collapsed;

    partial void OnIsMissingChanged(bool value) => OnPropertyChanged(nameof(MissingVisibility));
    partial void OnIsSelectedChanged(bool value) => OnPropertyChanged(nameof(SelectedVisibility));

    public void Refresh()
    {
        OnPropertyChanged(string.Empty);
        IsMissing = Item.IsMissing;
    }

    /// <summary>Uses the Windows shell thumbnail cache, so large videos are never fully decoded for a 320px tile.</summary>
    public async Task LoadThumbnailAsync()
    {
        if (Item.IsMissing) return;
        try
        {
            var file = await StorageFile.GetFileFromPathAsync(Item.Path);
            var mode = Item.Kind == MediaKind.Video ? ThumbnailMode.VideosView : ThumbnailMode.PicturesView;
            using var thumb = await file.GetThumbnailAsync(mode, 320, ThumbnailOptions.UseCurrentScale);
            if (thumb is null || thumb.Size == 0) return;
            var bmp = new BitmapImage();
            await bmp.SetSourceAsync(thumb);
            Thumbnail = bmp;
        }
        catch (Exception ex)
        {
            AppLogger.Error($"Thumbnail failed for {Item.FileName}", ex);
        }
    }
}

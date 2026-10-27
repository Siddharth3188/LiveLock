using System.Collections.ObjectModel;
using LiveLock.App.ViewModels;
using LiveLock.Core.Logging;
using LiveLock.Core.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace LiveLock.App.Views;

public sealed partial class LibraryPage : Page
{
    private readonly ObservableCollection<LibraryItemViewModel> _items = new();
    public LibraryPage()
    {
        InitializeComponent();
        LibraryGrid.ItemsSource = _items;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _items.Clear();
        foreach (var w in App.State.Settings.Library)
        {
            var vm = new LibraryItemViewModel(w) { IsSelected = w.Id == App.State.Settings.SelectedWallpaperId };
            _items.Add(vm);
            _ = vm.LoadThumbnailAsync();
        }
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        var empty = _items.Count == 0;
        EmptyState.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;
        LibraryGrid.Visibility = empty ? Visibility.Collapsed : Visibility.Visible;
    }

    private void Show(string message, InfoBarSeverity severity)
    {
        Status.Message = message; Status.Severity = severity; Status.IsOpen = true;
    }

    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, App.MainWindow.Hwnd);
        foreach (var ext in MediaTypes.SupportedExtensions) picker.FileTypeFilter.Add(ext);
        var files = await picker.PickMultipleFilesAsync();
        if (files is { Count: > 0 }) ImportPaths(files.Select(f => f.Path));
    }

    private void ImportPaths(IEnumerable<string> paths)
    {
        int added = 0; string? problem = null;
        foreach (var path in paths)
        {
            var result = LibraryImporter.Import(App.State.Settings.Library, path);
            if (result.Status == ImportStatus.Added && result.Item is not null)
            {
                added++;
                var vm = new LibraryItemViewModel(result.Item);
                _items.Add(vm);
                _ = vm.LoadThumbnailAsync();
            }
            else problem = result.Message;
        }
        if (added > 0) { App.State.SaveNow(); Show($"Added {added} item(s).", InfoBarSeverity.Success); }
        if (problem is not null) Show(problem, InfoBarSeverity.Warning);
        UpdateEmptyState();
    }

    private void Page_DragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Link; // reference only, never move/copy
            e.DragUIOverride.Caption = "Add to LiveLock library";
        }
    }

    private async void Page_Drop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;
        var items = await e.DataView.GetStorageItemsAsync();
        ImportPaths(items.OfType<StorageFile>().Select(f => f.Path));
    }

    private void LibraryGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not LibraryItemViewModel vm) return;
        vm.Refresh();
        if (vm.IsMissing) { Show($"“{vm.Item.FileName}” is missing. Use the locate button to point LiveLock at it again.", InfoBarSeverity.Warning); return; }
        foreach (var i in _items) i.IsSelected = false;
        vm.IsSelected = true;
        vm.Item.LastUsedUtc = DateTime.UtcNow;
        App.State.Settings.SelectedWallpaperId = vm.Item.Id;
        App.State.SaveNow();
        App.MainWindow.NavigateTo("studio");
    }

    private async void Remove_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not LibraryItemViewModel vm) return;
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot, Title = "Remove from library?",
            Content = $"“{vm.Name}” will be removed from LiveLock. The original file will not be deleted.",
            PrimaryButtonText = "Remove", CloseButtonText = "Cancel", DefaultButton = ContentDialogButton.Close,
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        App.State.Settings.Library.Remove(vm.Item);
        if (App.State.Settings.SelectedWallpaperId == vm.Item.Id) App.State.Settings.SelectedWallpaperId = null;
        _items.Remove(vm);
        App.State.SaveNow();
        UpdateEmptyState();
    }

    private async void Locate_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not LibraryItemViewModel vm) return;
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, App.MainWindow.Hwnd);
        foreach (var ext in MediaTypes.SupportedExtensions) picker.FileTypeFilter.Add(ext);
        var file = await picker.PickSingleFileAsync();
        if (file is null) return;
        if (!MediaTypes.IsSupported(file.Path)) { Show("That file type isn't supported.", InfoBarSeverity.Warning); return; }
        vm.Item.Path = file.Path;
        vm.Item.Kind = MediaTypes.Classify(file.Path);
        vm.Refresh();
        _ = vm.LoadThumbnailAsync();
        App.State.SaveNow();
        AppLogger.Info($"Relocated {file.Name}");
    }
}

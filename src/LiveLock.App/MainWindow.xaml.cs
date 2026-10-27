using LiveLock.App.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace LiveLock.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "LiveLock — Bring your screen to life";
        Closed += (_, _) => App.State.SaveNow();
        Nav.SelectedItem = Nav.MenuItems[0];
    }

    public IntPtr Hwnd => WindowNative.GetWindowHandle(this);

    public void NavigateTo(string tag)
    {
        foreach (var item in Nav.MenuItems.OfType<NavigationViewItem>())
            if ((string)item.Tag == tag) { Nav.SelectedItem = item; return; }
    }

    private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var tag = (args.SelectedItem as NavigationViewItem)?.Tag as string;
        ContentFrame.Navigate(tag == "library" ? typeof(LibraryPage) : typeof(StudioPage));
    }
}

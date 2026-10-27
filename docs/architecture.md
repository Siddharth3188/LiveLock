# Architecture

- `LiveLock.Core` (net8.0, no Windows dependency): models, `ClockFormatter`, `Presets`, `JsonSettingsStore`, `MediaTypes`, `LibraryImporter`, `AppLogger`. Fully unit-tested.
- `LiveLock.App` (WinUI 3, unpackaged): `AppState` (single settings instance, debounced saves), view models, Library and Studio pages.
- Wallpaper and overlay are sibling layers in `StudioPage`. Settings only change overlay properties or `MediaPlayer.IsLoopingEnabled/IsMuted`; the media source is set only in `LoadWallpaper`.
- The clock wakes once per minute (aligned to the boundary) and on window activation.
- A separate `LiveLock.Media` project was not created yet; it would be empty. Dependency injection is deferred; `App.State` is a simple singleton.

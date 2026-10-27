# LiveLock

**Bring your screen to life.**

LiveLock is an open-source Windows personalization project exploring cinematic wallpapers, beautiful lock-screen layouts, and a customizable clock experience. macOS is design inspiration only; no Apple assets are used.

## Current status (honest)

Phase 1 source code is written. **It has not yet been compiled or run**; it was authored in a Linux environment without the .NET SDK or Windows. Expect first-build fixes. CI (`.github/workflows/ci.yml`) will be the first real build.

**Important:** the Studio preview is an in-app window, *not* the Windows lock screen. Applying images to the real lock screen and any full-motion lock screen are **not implemented**; see [docs/native-lock-screen.md](docs/native-lock-screen.md).

## Features (Phase 1, unverified on Windows)
- Wallpaper library: import MP4/video, GIF, PNG, JPG, WebP (codec-dependent) via picker or drag-and-drop; references only, originals untouched; missing-file detection with "locate".
- Studio: wallpaper layer + independent clock/date overlay, live customization, five presets, dimming/gradient, scaling modes, loop/mute, reset.
- Settings persisted to `%LOCALAPPDATA%\LiveLock\settings.json` (versioned, atomic write).

## Not yet built
Dashboard, Appearance and Settings pages, borderless fullscreen preview, custom presets, lock-screen image apply, video frame extraction, startup option, reduced-animation preference, screenshots/GIFs.

## Stack
C#, .NET 8, Windows App SDK 1.6 (WinUI 3, unpackaged), CommunityToolkit.Mvvm, xUnit.

## Build, run, test
See [docs/development.md](docs/development.md). Tests for `LiveLock.Core` run on any OS with .NET 8; the app requires Windows.

## Roadmap
Verify build on Windows → fullscreen preview → remaining pages → static lock-screen apply + frame extraction → packaging → experimental research.

## License
MIT is included as a proposal; confirm before publishing.

## Acknowledgements
Windows App SDK, CommunityToolkit.Mvvm, xUnit (see package references).

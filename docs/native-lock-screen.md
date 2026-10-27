# Native lock screen: what is and isn't planned

LiveLock separates three things, and only the first exists today.

| Capability | Status |
|---|---|
| A. Studio / fullscreen-style preview (this app's window) | Implemented in the Studio page (windowed). Borderless fullscreen is not implemented yet. |
| B. Apply a static image to the real Windows lock screen | **Not implemented.** Needs research and testing on real Windows; candidate: `Windows.System.UserProfile.LockScreen.SetImageFileAsync` (a documented WinRT API; its behavior and requirements for unpackaged apps must be verified before use). |
| C. Continuous video on the real lock screen | **Unverified / experimental.** No documented API is known to us. Nothing here claims it works. |

A desktop window does not remain visible once Windows locks, so the preview is not the lock screen.

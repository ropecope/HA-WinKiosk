# Required components

- Windows 10 x64 build 19041 or newer is the current installer minimum (application targets net8.0-windows10.0.19041.0).
- .NET 8, WPF, OpenCvSharp and native libraries ship with the self-contained x64 publish; no separate .NET SDK, Python or OpenCV installation.
- WebView2 Evergreen Runtime: detect HKLM32/HKCU registry version as documented by Microsoft, and check the SDK at app startup. Setup offers the bundled verified Microsoft bootstrapper only if absent. Internet is needed unless the runtime already exists or the operator installs the official standalone offline runtime. No Edge full browser installation requested.
- Build downloads the official bootstrapper from Microsoft and verifies Authenticode before packaging. App first startup and Check components offer installation on consent, keeping kiosk lockdown off if missing.
- Camera, touchscreen, Wi-Fi and graphics drivers are OS/hardware dependencies, not installed automatically. MQTT and camera are optional; no service or system policy changes for prerequisites.
- Reference: https://learn.microsoft.com/microsoft-edge/webview2/concepts/distribution
- Acceptance: clean Windows VM/device without WebView2, runtime present, no internet, user decline, failure and retry; existing HA and app settings preserved. Build success alone does not validate these cases.

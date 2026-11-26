# GrowBox Panel preview acceptance

## Installation
Use only a successful run for the growbox branch under GitHub Actions. Download GrowBox-Panel-preview-installer. It installs alongside upstream HA-WinKiosk, with separate settings and autostart identity. It is unsigned and intended for testing, not production. WebView2 Evergreen Runtime is still required. No automatic updates.

## First launch
Select a box profile, enter the actual HA dashboard URL, optionally apply the profile device name. MQTT is optional; enter the broker and credentials only locally. Keep the gear visible and leave power commands disabled until verified. Save. To use software rendering, toggle Disable GPU acceleration and restart the app. Configuration is under %APPDATA%\GrowBox-Panel. Diagnostics: panel.log.

## Required hands-on tests
- First launch without physical keyboard; change default URL, save, close and reopen, verify configuration.
- Missing WebView2: settings and exit remain reachable.
- With kiosk lockdown on/off, exit from settings via touch. Verify Windows taskbar restored. Double-launch and verify existing kiosk unaffected.
- PIN entry, cancel, reset and MQTT settings request (if enabled). Verify local authentication.
- Disconnect Wi-Fi, stop/restart HA, restart broker: offline status, reload, reconnect and telemetry; verify actual dashboard live values after recovery.
- Ensure MQTT retained shutdown/restart commands do not execute on reconnect using an isolated test broker, not a production broker.
- Verify monitor state/sleep/wake and charger/unplug on the specific tablet. Do not enable sleep or shutdown until separately tested.
- Measure app and WebView2 combined memory/CPU after 10 minutes and after extended running (2 GB Fusion5). Test graphics acceleration both ways.
- Check portrait/landscape and display scaling; ensure header controls and settings exit reachable.
- Test motion wake on the selected front camera: choose named camera, select motion mode, set 1 minute for testing, walk toward the panel, verify display wakes, touch extends timeout, then verify display off while Windows and Wi-Fi remain active. Cover the camera to test response; verify rear camera selection is not mistaken for front (OpenCV index ordering may differ). Unplug/disable camera while screen is off and verify fail-safe wake. Verify no MJPEG stream is listening in motion mode, and disabling motion releases the power override. Test on AC power; camera use may increase battery drain.
- Check original HA-WinKiosk settings and installer remain untouched. Avoid running both kiosk apps at once.
- Confirm OS Start menu behavior; this app is not an OS security boundary.

Record device, OS, WebView2 version, build commit, observed result and any log. No production release before hardware acceptance.

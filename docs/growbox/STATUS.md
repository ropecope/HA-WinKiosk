# HMI_Panel

Baseline upstream v3.30.2, commit 11820437b2459b87d02692082c86563cf10cec4c. Original main remains clean. Product target: touch-only Windows companion for GrowBox, MycoBox and GreenBox, with existing HA dashboards. Keep general HA compatibility and GPL-3.0.

## Current state and handover (2026-11-27)

This section lets another agent continue without the chat history. Older sections below are the chronological change log. Legend: **built+CI** = compiled and passed the automated CI checks; **HW?** = not verified on the real tablet.

### Repository and locations
- GitHub: `https://github.com/ropecope/HA-WinKiosk`. Branch `growbox` = HMI_Panel development; branch `main` = untouched mirror of upstream kattcrazy/HA-WinKiosk (do not commit there). `growbox` is 24 commits ahead of `main`.
- Last confirmed code commit: `e342b04` (2026-11-27 00:45 UTC) "Fix both stale portable EXE paths after HMI_Panel rename". Documentation-only commits may follow it.
- No local working copy of the source exists on the owner's workstation; work from a fresh clone of `growbox`. Local artifact: `%USERPROFILE%\Desktop\HMI_Panel-exe.zip` (113,272,597 bytes) containing `HMI_Panel.exe` (285,636,376 bytes). Its size matches exactly the CI artifact `HMI_Panel-exe` of run 45918598932 for `e342b04`, so it is that build.
- Settings and WebView2 profile: `%APPDATA%\GrowBox-Panel` (legacy name kept on purpose), log `panel.log` there. Single-instance mutex `GrowBox-Panel-SingleInstance`.

### Build and artifacts
- CI workflow `.github/workflows/growbox-ci.yml` ("HMI_Panel Windows CI"): build, `tests/GrowBox.Smoke` regression smoke tests, x64 self-contained publish, camera settings WPF UI regression, portable directory artifact and single-file EXE artifact.
- `e342b04`: CI **success**. Artifacts `HMI_Panel-x64-portable` (multi-file directory, recommended fallback on 2 GB tablets) and `HMI_Panel-exe` (single file). Both expire 2026-12-04; re-run CI or keep a copy if needed later.
- `d7aa6b5`: CI failed in "Publish portable single EXE" only because the smoke test still used the old executable name; fixed in `e342b04`.
- No installer is produced in this phase (owner decision); installer sources and `release-installer.yml` remain for later. No release has been published. Builds are unsigned previews.
- No local build was made on the workstation. Startup speed, RAM and CPU have not been measured on the device.

### Feature status
| Area | Status |
|---|---|
| Rename to HMI_Panel (UI, EXE, metadata; default MQTT device `hmi-panel-{machine}`) | built+CI, HW? |
| Hardening: touch exit, offline panel and retry, HA reachability 15 s, MQTT without retained command replay, atomic YAML with backup | built+CI, HW? |
| WebView2 prerequisite check with verified Microsoft bootstrapper (setup path only; portable builds omit bootstrapper) | built+CI, clean-VM test HW? |
| Dark theme readability corrections | built+CI, HW? |
| Camera settings section always visible, 15 s enumeration timeout, manual refresh, persisted selection, serialized apply, slider range fix | built+CI (WPF UI regression without camera), HW? |
| **Motion Wake** (`Mode = motion`): OpenCV frame difference, 2 samples/s, 160×120, hold 1-60 min (default 3), sensitivity 1-100 (default 50), keeps Windows awake while monitoring, wakes display if frames stop | built+CI, **HW?** |
| **Face Presence** (`a768d1c`): optional Windows `FaceDetector`, exact Windows camera ID, ~1 check/s, 320×240 requested, no recording or recognition; design in [CAMERA.md](CAMERA.md) | built+CI, **HW?**; driver compatibility unknown |
| MJPEG camera export (upstream) | unchanged |

### Known problems and risks
- **Camera:** camera settings could be hidden under Audio when Windows enumeration failed; commits `23b3da4`, `8222099`, `f16338b` address this in UI code and CI, but two-camera discovery on the Fusion5, privacy/driver access and the OpenCV index vs Windows device order (front vs rear) are unverified. Face Presence may not work with every camera driver, darkness, face coverings or rotated frames.
- **Motion Wake:** motion is not presence; stationary people can time out, fans or light changes can trigger. Display sleep/wake commands are untested on the device.
- **Single EXE startup** is slow on first run (native extraction, antivirus, WebView2 init); not measured. Use the portable directory if it is too slow.
- PIN stored as Base64; MQTT password in YAML; broker must stay inside the trusted LAN.
- Upstream issue #3 (Start menu/swipe) is an OS limitation; #5 shell replacement deferred. Kiosk mode is not a security boundary.
- [TESTING.md](TESTING.md) still describes an installer flow in its first section; for this phase use the portable artifacts (corrected note added there).

### Test results
- Automated: CI smoke and WPF UI regression pass for `e342b04` (no camera, display or touch hardware involved).
- Hardware: **none performed** for any item above. Acceptance list in [TESTING.md](TESTING.md).

### Unfinished, suggested order
1. Owner runs `HMI_Panel.exe` (or the portable directory) on the Fusion5: startup, HA login, settings persistence, touch exit; record device, OS, WebView2 version, commit and `panel.log`.
2. Camera on the device: enumeration of both cameras, select front camera, Motion Wake with 1-minute hold, then Face Presence; verify display off/on and fail-safe wake when the camera disappears.
3. Measure RAM/CPU after 10 min and longer run; decide single EXE vs portable directory.
4. Network/HA/MQTT outage tests with an isolated broker.
5. Only after acceptance: installer, signing, GrowBox optional theme (deferred), upstream sync per the workflow below. No new features without owner approval.

## Review and plan (2026-11-26)
1. Stabilize touch exit, startup, missing WebView2, settings persistence, MQTT command handling and reconnection.
2. Isolate update source and application identity before distributing a fork installer.
3. Add Windows CI, regression tests, diagnostics and lightweight defaults; measure actual memory use on Fusion5.
4. Add configurable box profiles, Serbian UI, branding, reconnect/offline indication; avoid duplicating HA logic.
5. Produce tested Windows installer and release only after checks. Upstream issues #3 (Start menu / swipe) and #5 (shell replacement) require OS-level testing; #17 is a feature request, not a defect. Upstream #10/#11/#15 are closed but need regression testing.

## Upstream workflow
Fetch upstream main; fast-forward our main only; create integration branch from growbox; merge upstream/main; review changes, test and merge into growbox. Keep product releases independent. No force push to main, no direct upstream modifications or automatic installation.

## Verification
Initial review covers central WPF, configuration, MQTT, update and installer paths. Not a complete runtime audit. No Windows hardware test has been performed. All changes must be evaluated on the actual tablet before production.

## Implemented in the first hardening pass
- Separate application settings folder, installer ID, autostart name and single-instance mutex from upstream; manual updates only.
- First-run setup does not lock the Windows shell; recovery settings and touch exit remain available. Failed navigation hides stale browser content and shows a native offline panel with periodic retry.
- Host:port normalization, valid HTTP(S) address required, same-origin remote navigation and browser gesture/microphone restrictions.
- No retained MQTT command replay; remote settings respects PIN, one initial/reconnect loop, conservative defaults.
- Atomic YAML replacement with previous file backup and recovery on invalid YAML.
- Windows CI builds/publishes preview binary and installer; smoke tests cover URL rules and settings defaults.

## Open / acceptance tests
- Actual Fusion5 touch behavior, settings gear, native keyboard, WebView2 renderer/browser crash, HA outage and reconnect, MQTT broker restart and device power/sleep. No hardware test yet.
- Upstream issue #3 remains an OS shell limitation; #5 shell replacement deferred (not appropriate for an untested touch-only device).
- PIN storage still Base64; MQTT passwords in YAML and transport security need a deliberate design. Do not expose the broker outside the trusted LAN.
- Full UI localization, resource measurements, independent watchdog process and branded icon remain planned. Profile selection and optional WebView2 software rendering are implemented; profile-specific dashboard URLs must be entered by the operator. Changes to rendering require restart.
- No production release published. CI artifacts are previews; install only on a test device after green build.

## Second hardening pass
- Fixed duplicate instance mutex ownership and taskbar restoration. Wrapped header actions for small displays and added touch keyboard access to settings and PIN dialog.
- Added optional GPU acceleration disable and bounded local panel.log (256 KiB plus one rotated log). Box profiles and profile-generated names were removed at the user's request. No actual server addresses assumed.
- HA HTTP reachability checks every 15 s for the configured HA URL; two consecutive failures show offline. This does not prove live WebSocket or sensor freshness. Self-signed HTTPS override skips the native HTTP probe, retaining browser navigation/network checks.
- MQTT standard telemetry 60 s, optional last-active telemetry 15 s; GPU WMI checks skipped on settings normalization when GPU sensor disabled. Commands ignored if config cannot be loaded.
- Smoke tests expanded for profile and rendering round-trip. Runtime hardware acceptance remains outstanding.

## Prerequisite installation UX
- Missing WebView2: visible progress while preparing, downloading/installing and verifying. The signed Microsoft bootstrapper remains silent. No simulated percentages. Successful verification is required before continuing setup. App check displays status without the short update-popup timer. Actual clean Windows installation still requires hardware/VM validation.

## Local camera motion wake
- Opt-in camera mode motion, mutually exclusive with MJPEG. Explicit camera selection, 1–60 minute hold (default 3), sensitivity 1–100 (default 50).
- Local frame differences at 2 samples/s, requested 320×240 capture, downsampled to 160×120. No JPEG encoding, image storage, streaming or face recognition in this mode.
- Motion, input and open settings extend the display timeout. Windows execution state keeps the system awake while monitoring, and is released on disable/exit. If frames stop, the panel releases the override and attempts to wake any display it switched off.
- Motion is not proof of human presence; stationary people may time out, fans/light changes may cause false triggers. Camera ID-to-OpenCV-index mapping and wake/sleep behavior must be verified on the two-camera Fusion5. No production acceptance yet.

## Readability correction
- Fixed nearly white section text on pale blue dark-mode backgrounds. Explicit dark surfaces, contrasting borders, dark text on blue action buttons, consistent drop-down selected/hover states, checkbox text, PIN error colors and Segoe UI. Theme choice and functionality unchanged. Verify live on Fusion5 at native scaling, including dropdowns, PIN and both themes. No Home Assistant dashboard theme changes.

## Owner feedback: simple HA login
- Removed panel profiles and automatic profile naming from UI, model and YAML conversion. Unknown legacy YAML properties are ignored by the existing deserializer. Existing URL, device name and credentials are not reset. Existing HA reachability check now uses the configured URL without a profile gate.
- Display retains the original Auto/Light/Dark selector. Dark text inputs, section headers and selected options use neutral gray rather than blue. No new theme selector. Only requested corrections; no new functions without owner approval.

## Portable EXE
- Separate self-contained single-file x64 artifact with all bundle contents extracted by .NET into its local cache. No installer needed for updates; use the same EXE path, close before replacing, retain existing AppData settings and WebView2 profile. Microsoft bootstrapper embedded for first use. CI tests native OpenCV Mat allocation and embedded resource from an otherwise empty folder. Original multi-file portable ZIP and installer remain available because upstream reported native DLL issues with single-file builds; real camera and WebView2 testing remains required. Initial extraction consumes temporary disk space; no claim of lower RAM usage.

## Portable startup feedback
- Owner reports single-file startup is slow and already has WebView2. Both portable artifacts now omit the WebView2 bootstrapper. Classic setup still handles prerequisites. Single EXE no longer requests extraction of all managed content, only required native dependencies; no compression. CI exercises both the OpenCV native Mat and WebView2 loader from an empty directory. First-run native extraction, antivirus scanning, and WebView initialization may still affect startup. Multi-file portable ZIP needs no bundle extraction and is the recommended fallback on 2 GB tablets. Actual speed not measured on Fusion5; no HA or Windows system changes.

## Camera visibility fix
- Camera is a separate visible settings section rather than hidden under Audio if Windows enumeration fails. Device enumeration permits up to 15 s, reports timeout/error/no devices, and has a manual refresh button. Mode and configured camera ID are not reset when enumeration fails. Runtime camera errors are visible in settings; camera off does not need enumeration to start kiosk. Existing local detection algorithms and HA unchanged. Verify two-camera discovery, driver/privacy access and wake on real Fusion5.

- Follow-up review: camera list is disabled while enumerating, persisted camera is populated before asynchronous enumeration, and camera application requests are serialized. OpenCV opening/configuration runs on a worker, enumeration has a 15 s timeout, failed reads are paced. CI now exercises the actual WPF camera controls in all four modes without hardware. This is a UI regression check, not a real camera or display wake test. No new user features.

- Fixed slider range initialization: WPF Slider and SettingsSliderRow had different defaults, so setting Maximum to the wrapper's default 100 could skip the change callback. Explicit range initialization and synchronized value label. UI regression covers real slider minimum, maximum, and value/label synchronization for sensitivity, hold time and FPS. Hardware touch dragging remains unverified.

## HMI_Panel portable preview
- Product renamed to HMI_Panel in UI, executable artifact and product metadata; fresh default MQTT device name is hmi-panel-{machine}. Existing device names are unchanged. Internal source namespaces, branch, mutex, installer sources and legacy AppData directory remain for compatibility during tablet testing. No data migration, no HA login reset. CI builds portable single EXE and portable directory only, no Setup and no bundled WebView2 bootstrapper. Includes previous camera visibility and slider corrections. GrowBox theme deferred as agreed.

- CI rename follow-up: corrected both remaining old executable paths in the portable smoke test and artifact upload. The preceding run compiled successfully and passed regression/WPF tests; single-EXE packaging failed only because the test referenced the old filename.

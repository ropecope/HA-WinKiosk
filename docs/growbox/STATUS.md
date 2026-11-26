# GrowBox Panel

Baseline upstream v3.30.2, commit 11820437b2459b87d02692082c86563cf10cec4c. Original main remains clean. Product target: touch-only Windows companion for GrowBox, MycoBox and GreenBox, with existing HA dashboards. Keep general HA compatibility and GPL-3.0.

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
- Added saved box profile, explicit profile device-name generation, optional GPU acceleration disable, and bounded local panel.log (256 KiB plus one rotated log). No actual server addresses assumed.
- HA HTTP reachability checks every 15 s for non-custom profiles; two consecutive failures show offline. This does not prove live WebSocket or sensor freshness. Self-signed HTTPS override skips the native HTTP probe, retaining browser navigation/network checks.
- MQTT standard telemetry 60 s, optional last-active telemetry 15 s; GPU WMI checks skipped on settings normalization when GPU sensor disabled. Commands ignored if config cannot be loaded.
- Smoke tests expanded for profile and rendering round-trip. Runtime hardware acceptance remains outstanding.

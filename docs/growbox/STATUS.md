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

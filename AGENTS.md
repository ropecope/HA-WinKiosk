# GrowBox Panel development rules

- `main` mirrors upstream kattcrazy/HA-WinKiosk. Never commit GrowBox changes there.
- Develop only on `growbox` or temporary branches based on it. Preserve GPL-3.0, upstream notices and history.
- Merge upstream into a temporary integration branch, review conflicts and run Windows build + regression tests before merging to growbox. Do not automatically deploy upstream installers.
- Keep changes small and upstream-friendly; isolate GrowBox-specific behavior. Generic fixes may be proposed upstream separately with owner approval.
- No modifications to live Home Assistant, automations, PLCs or deployed devices. Never commit real credentials, URLs with tokens or personal data.
- UI must be operable with touch only, including recovery and exit. Do not treat kiosk mode as an OS security boundary. HA owns all process control.
- Test on Windows 10 x64 / 2 GB RAM and modern Windows; verify startup, missing runtime, network outage, MQTT, exit and restart. Do not claim hardware tests without evidence.
- Record changes and limitations in docs/growbox/STATUS.md. Prefer conservative defaults and explicit opt-in for remote system commands.

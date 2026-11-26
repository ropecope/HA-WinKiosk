# Camera design

Reference: AreYouThere (https://github.com/nathanatgit/AreYouThere, commit eca73d198536219adaab6512f58dc20f52d13589), MIT, copyright 2026 HGY. Its CameraCatalog and PresenceService informed the camera position labels, exact Windows camera selection, shared read-only capture, and native face detection. License retained in third-party/AreYouThere-MIT.txt. GrowBox implementation is adapted to the existing WPF and camera coordinator, not a new app.

SageKiosk (https://github.com/Sage222/SageKiosk, commit 1f66e03305bf9bc7b72bc4e37391d123e5c4db0a) has a comparable frame-difference approach. No license was visible, so no code was copied.

Modes: Off (default), MJPEG (upstream), Motion (existing OpenCV frame differences, low resolution), Face presence (optional Windows FaceDetector, exact device ID, CPU memory, 320x240 requested reader output, ~1 check/s). One mode only. No AI runtime, new package, cloud, recording or recognition. If native capture fails, screen remains available; user may explicitly choose motion mode. OpenCV camera index order can differ from Windows list, so verify actual front camera. Native face detection does not work reliably with all cameras, face coverings, darkness or rotated frames. Verify driver compatibility, CPU/RAM, wake behavior and timeout on actual Fusion5; no claim of performance without measurement.

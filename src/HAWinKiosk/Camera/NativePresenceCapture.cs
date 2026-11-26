using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using HAWinKiosk.Mqtt;

namespace HAWinKiosk.Camera;

/// <summary>
/// Optional local face presence using the Windows inbox detector and an exact device ID.
/// No recognition, network, image persistence, WinUI dependency or model download.
/// </summary>
internal sealed class NativePresenceCapture : IDisposable
{
    private MediaCapture? _capture;
    private MediaFrameReader? _reader;
    private FaceDetector? _detector;
    private int _busy;
    private int _generation;
    private long _lastFrame;
    private long _lastPresence;
    private long _lastAttempt;
    private int _warmup;
    private int _sensitivity;
    public long LastFrameTicks => Interlocked.Read(ref _lastFrame);
    public long LastPresenceTicks => Interlocked.Read(ref _lastPresence);

    public async Task StartAsync(string deviceId, int sensitivity)
    {
        Stop();
        var generation = Volatile.Read(ref _generation);
        _sensitivity = Math.Clamp(sensitivity, 1, 100);
        if (!FaceDetector.IsSupported) throw new NotSupportedException("Windows face detection is unavailable.");
        var capture = new MediaCapture();
        try
        {
            await capture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                VideoDeviceId = deviceId,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                SharingMode = MediaCaptureSharingMode.SharedReadOnly
            });
            if (generation != Volatile.Read(ref _generation)) throw new OperationCanceledException();
            var source = capture.FrameSources.Values.FirstOrDefault(s => s.Info.SourceKind == MediaFrameSourceKind.Color)
                ?? throw new InvalidOperationException("No color camera source.");
            _detector = await FaceDetector.CreateAsync();
            if (generation != Volatile.Read(ref _generation)) throw new OperationCanceledException();
            // Request small frames from the reader instead of processing full camera resolution.
            var reader = await capture.CreateFrameReaderAsync(source, MediaEncodingSubtypes.Bgra8,
                new BitmapSize { Width = 320, Height = 240 });
            if (generation != Volatile.Read(ref _generation)) { reader.Dispose(); throw new OperationCanceledException(); }
            _capture = capture;
            _reader = reader;
            reader.AcquisitionMode = MediaFrameReaderAcquisitionMode.Realtime;
            reader.FrameArrived += OnFrame;
            var result = await reader.StartAsync();
            if (result != MediaFrameReaderStartStatus.Success) throw new InvalidOperationException("Camera reader failed to start.");
        }
        catch
        {
            if (generation == Volatile.Read(ref _generation)) Stop();
            capture.Dispose();
            throw;
        }
    }

    private async void OnFrame(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
    {
        if (!ReferenceEquals(sender, _reader)) return;
        var now = Environment.TickCount64;
        if (now - Interlocked.Read(ref _lastAttempt) < 1000 || Interlocked.Exchange(ref _busy, 1) != 0) return;
        var generation = Volatile.Read(ref _generation);
        Interlocked.Exchange(ref _lastAttempt, now);
        try
        {
            var detector = _detector;
            using var frame = sender.TryAcquireLatestFrame();
            using var bitmap = frame?.VideoMediaFrame?.SoftwareBitmap;
            if (bitmap == null || detector == null) return;
            if (++_warmup <= 2) return;
            using var gray = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Gray8);
            var faces = await detector.DetectFacesAsync(gray);
            if (generation != Volatile.Read(ref _generation)) return;
            Interlocked.Exchange(ref _lastFrame, Environment.TickCount64);
            var minimum = 0.045 - 0.04 * _sensitivity / 100.0;
            var area = (double)gray.PixelWidth * gray.PixelHeight;
            if (area > 0 && faces.Any(f => (double)f.FaceBox.Width * f.FaceBox.Height / area >= minimum))
                Interlocked.Exchange(ref _lastPresence, Environment.TickCount64);
        }
        catch (Exception ex)
        {
            if (generation == Volatile.Read(ref _generation)) PanelLog.Write("Native presence frame error", ex);
        }
        finally { Volatile.Write(ref _busy, 0); }
    }

    public void Stop()
    {
        Interlocked.Increment(ref _generation);
        var reader = _reader;
        _reader = null;
        if (reader != null)
        {
            reader.FrameArrived -= OnFrame;
            try { reader.Dispose(); } catch { }
        }
        try { _capture?.Dispose(); } catch { }
        _capture = null;
        _detector = null;
        _warmup = 0;
        Interlocked.Exchange(ref _lastFrame, 0);
        Interlocked.Exchange(ref _lastPresence, 0);
        Interlocked.Exchange(ref _lastAttempt, 0);
    }

    public void Dispose() => Stop();
}

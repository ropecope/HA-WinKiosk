using System.IO;
using HAWinKiosk.Mqtt;
using HAWinKiosk.Mqtt.Models;

namespace HAWinKiosk.Camera;

/// <summary>Starts/stops capture and routes frames to local MJPEG HTTP.</summary>
public sealed class CameraStreamCoordinator : IDisposable
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GrowBox-Panel",
        "camera.log");

    private readonly CameraCaptureService _capture = new();
    private readonly MjpegHttpServer _mjpeg = new();
    private MqttClientService? _mqtt;
    private string _mode = "off";
    private int _fps = 5;
    private int _port = 8081;
    private string? _deviceId;
    private bool _disposed;
    private int _motionSensitivity = 50;
    public bool MotionActive => _mode == "motion";
    public long LastFrameTicks => _capture.LastFrameTicks;
    public long LastMotionTicks => _capture.LastMotionTicks;

    public CameraStreamCoordinator()
    {
        _capture.FrameCaptured += OnFrame;
    }

    /// <summary>Detach MQTT during broker reconnect.</summary>
    public void SetMqttClient(MqttClientService? mqtt) => _mqtt = mqtt;

    public async Task ApplyAsync(AppSettings settings, MqttClientService? mqtt)
    {
        _mqtt = mqtt;
        var mode = (settings.Sensors.CameraStream.Mode ?? "off").Trim().ToLowerInvariant();
        if (mode is not ("mjpeg" or "motion"))
            mode = "off";
        var fps = Math.Clamp(settings.Sensors.CameraStream.Fps, 1, 15);
        var port = Math.Clamp(settings.Sensors.CameraStream.Port <= 0 ? 8081 : settings.Sensors.CameraStream.Port, 1, 65535);
        var deviceId = string.IsNullOrWhiteSpace(settings.Kiosk.CameraDeviceId)
            ? null
            : settings.Kiosk.CameraDeviceId.Trim();

        if (mode == "motion")
        {
            var sensitivity = Math.Clamp(settings.Sensors.CameraStream.MotionSensitivity, 1, 100);
            if (mode == _mode && sensitivity == _motionSensitivity && string.Equals(deviceId, _deviceId, StringComparison.Ordinal)) return;
            StopRuntime();
            _mode = "off";
            _deviceId = deviceId;
            _motionSensitivity = sensitivity;
            try
            {
                if (string.IsNullOrWhiteSpace(deviceId)) throw new InvalidOperationException("Select a specific camera for motion wake.");
                await _capture.StartAsync(deviceId, 2, localMotion: true, sensitivity: sensitivity);
                _mode = "motion";
            }
            catch { StopRuntime(); throw; }
            await ClearMqttCameraDiscoveryAsync();
            return;
        }

        if (mode == _mode && mode == "off")
        {
            await ClearMqttCameraDiscoveryAsync();
            return;
        }

        // Settings save reconnects MQTT but often leaves camera options unchanged - keep capture running.
        if (mode == "mjpeg"
            && mode == _mode
            && fps == _fps
            && port == _port
            && string.Equals(deviceId, _deviceId, StringComparison.Ordinal))
        {
            await ClearMqttCameraDiscoveryAsync();
            return;
        }

        // FPS-only change: restart capture pacing without touching MQTT discovery.
        if (mode == "mjpeg"
            && mode == _mode
            && fps != _fps
            && port == _port
            && string.Equals(deviceId, _deviceId, StringComparison.Ordinal))
        {
            _fps = fps;
            try
            {
                await _capture.StartAsync(deviceId, fps);
            }
            catch (Exception)
            {
                StopRuntime();
                _mode = "off";
                await ClearMqttCameraDiscoveryAsync();
                throw;
            }

            return;
        }

        StopRuntime();
        _mode = mode;
        _fps = fps;
        _port = port;
        _deviceId = deviceId;

        if (mode != "mjpeg")
        {
            await ClearMqttCameraDiscoveryAsync();
            return;
        }

        try
        {
            _mjpeg.Start(port);
            await _capture.StartAsync(deviceId, fps);
            await ClearMqttCameraDiscoveryAsync();
        }
        catch (Exception)
        {
            StopRuntime();
            _mode = "off";
            await ClearMqttCameraDiscoveryAsync();
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _capture.FrameCaptured -= OnFrame;
        StopRuntime();
        _capture.Dispose();
        _mjpeg.Dispose();
    }

    private void StopRuntime()
    {
        _capture.Stop();
        _mjpeg.Stop();
    }

    private void OnFrame(byte[] jpeg)
    {
        if (_mode == "mjpeg")
            _mjpeg.UpdateFrame(jpeg);
    }

    private async Task ClearMqttCameraDiscoveryAsync()
    {
        if (_mqtt == null) return;
        await _mqtt.ClearCameraDiscoveryAsync();
    }

    private static void Log(string message)
    {
        try
        {
            var dir = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.AppendAllText(LogPath, $"{DateTime.Now:O} {message}{Environment.NewLine}");
        }
        catch
        {
            // never break streaming for logging
        }
    }
}

using System.Windows;
using HAWinKiosk.Mqtt;
using HAWinKiosk.Mqtt.Models;

namespace HAWinKiosk;

public partial class App : System.Windows.Application
{
    private const string SingleInstanceMutexName = "GrowBox-Panel-SingleInstance";
    private Mutex? _singleInstanceMutex;
    private bool _ownsMutex;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        if (e.Args.Contains("--check-camera-ui"))
        {
            try
            {
                ThemePalette.ApplyAppWide(true);
                var window = new KioskWindow(true);
                Shutdown(window.CheckCameraSettingsUi() ? 0 : 4);
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "growbox-camera-ui-test.log"), ex.ToString());
                Shutdown(5);
            }
            return;
        }
        // CI verifies the actual standalone bundle in an otherwise empty directory.
        if (e.Args.Contains("--check-portable"))
        {
            try
            {
                using var mat = new OpenCvSharp.Mat(8, 8, OpenCvSharp.MatType.CV_8UC1);
                // Exercise the WebView2 native loader as well as OpenCV. A missing
                // installed runtime is acceptable on CI; missing loader DLL is not.
                try { _ = Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString(); }
                catch (Microsoft.Web.WebView2.Core.WebView2RuntimeNotFoundException) { }
                Shutdown(!mat.Empty() && typeof(App).Assembly.GetManifestResourceInfo("GrowBox.WebView2Bootstrapper.exe") == null ? 0 : 2);
            }
            catch { Shutdown(3); }
            return;
        }
        _singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out var createdNew);
        if (!createdNew)
        {
            Shutdown(0);
            return;
        }
        _ownsMutex = true;
        KioskWindow.RestoreTaskbar();
        DispatcherUnhandledException += (_, args) => { KioskWindow.RestoreTaskbar(); PanelLog.Write("Unhandled UI error", args.Exception); };
        AppDomain.CurrentDomain.UnhandledException += (_, _) => KioskWindow.RestoreTaskbar();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => KioskWindow.RestoreTaskbar();
        PanelLog.Write("Panel startup");

        var previewWebViewError = e.Args.Any(a =>
            string.Equals(a, "--preview-webview-error", StringComparison.OrdinalIgnoreCase));

        var appSettings = SettingsManager.Load();
        ThemePalette.ApplyAppWide(UiThemeHelper.ResolveEffectiveDark(appSettings.Kiosk.UiTheme));
        var showSettingsFirst = !WebView2Prerequisites.IsInstalled() || !SettingsManager.SettingsExists || SettingsManager.LastLoadFailed || string.IsNullOrWhiteSpace(appSettings.Kiosk.Url?.Trim());

        var kiosk = new KioskWindow(showSettingsFirst, previewWebViewError);
        kiosk.Show();
        // GrowBox releases are installed explicitly; never run the upstream updater.
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_ownsMutex)
        {
            KioskWindow.RestoreTaskbar();
            _singleInstanceMutex?.ReleaseMutex();
        }
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}

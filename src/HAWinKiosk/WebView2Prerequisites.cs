using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;
using HAWinKiosk.Mqtt;

namespace HAWinKiosk;

internal static class WebView2Prerequisites
{
    private const string RuntimeKey = @"SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}";
    public const string DownloadPage = "https://developer.microsoft.com/microsoft-edge/webview2/#download-section";

    private static bool HasRegisteredRuntime(RegistryHive hive, RegistryView view)
    {
        try
        {
            using var root = RegistryKey.OpenBaseKey(hive, view);
            using var key = root.OpenSubKey(RuntimeKey);
            return Version.TryParse(key?.GetValue("pv") as string, out var version) && version > new Version(0, 0, 0, 0);
        }
        catch { return false; }
    }

    public static bool IsInstalled()
    {
        if (!HasRegisteredRuntime(RegistryHive.LocalMachine, RegistryView.Registry32)
            && !HasRegisteredRuntime(RegistryHive.CurrentUser, RegistryView.Default)) return false;
        try { return !string.IsNullOrWhiteSpace(CoreWebView2Environment.GetAvailableBrowserVersionString()); }
        catch { return false; }
    }

    public static async Task<bool> InstallAsync(Action<string>? report = null)
    {
        report?.Invoke("Checking required WebView2 Runtime...");
        if (IsInstalled()) return true;
        var path = Path.Combine(AppContext.BaseDirectory, "MicrosoftEdgeWebview2Setup.exe");
        string? temp = null;
        try
        {
            if (!File.Exists(path))
            {
                using var bundled = typeof(WebView2Prerequisites).Assembly.GetManifestResourceStream("GrowBox.WebView2Bootstrapper.exe");
                if (bundled == null)
                {
                    report?.Invoke("Opening the official Microsoft download page...");
                    OpenDownloadPage();
                    return false;
                }
                temp = Path.Combine(Path.GetTempPath(), "GrowBox-WebView2-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(temp);
                path = Path.Combine(temp, "MicrosoftEdgeWebview2Setup.exe");
                using (var output = File.Create(path)) await bundled.CopyToAsync(output);
            }
            // The included bootstrapper is verified against Microsoft's Authenticode signature at build time.
            report?.Invoke("Downloading and installing WebView2. Internet access may be required...");
            using var process = Process.Start(new ProcessStartInfo(path, "/silent /install") { UseShellExecute = true });
            if (process == null) return false;
            await process.WaitForExitAsync();
            report?.Invoke("Verifying the installed runtime...");
            PanelLog.Write("WebView2 bootstrapper finished");
            return IsInstalled();
        }
        finally
        {
            if (temp != null)
                try { Directory.Delete(temp, recursive: true); } catch { }
        }
    }

    public static void OpenDownloadPage() =>
        Process.Start(new ProcessStartInfo(DownloadPage) { UseShellExecute = true });
}

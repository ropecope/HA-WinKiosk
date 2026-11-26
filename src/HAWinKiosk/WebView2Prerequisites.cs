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
        if (!File.Exists(path))
        {
            report?.Invoke("Opening the official Microsoft download page...");
            OpenDownloadPage();
            return false;
        }
        // Bundled bootstrapper is validated against Microsoft's Authenticode signature at build time.
        report?.Invoke("Downloading and installing WebView2. Internet access may be required...");
        using var process = Process.Start(new ProcessStartInfo(path, "/silent /install") { UseShellExecute = true });
        if (process == null) return false;
        await process.WaitForExitAsync();
        report?.Invoke("Verifying the installed runtime...");
        PanelLog.Write("WebView2 bootstrapper finished");
        return IsInstalled();
    }

    public static void OpenDownloadPage() =>
        Process.Start(new ProcessStartInfo(DownloadPage) { UseShellExecute = true });
}

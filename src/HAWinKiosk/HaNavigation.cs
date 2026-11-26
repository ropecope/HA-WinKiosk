using Microsoft.Web.WebView2.Core;
using HAWinKiosk.Mqtt;

namespace HAWinKiosk;

/// <summary>Browser Mod-style in-app navigation for Home Assistant SPA routes.</summary>
public static class HaNavigation
{
    public static async Task NavigateHaPathAsync(
        CoreWebView2? webView,
        string kioskBaseUrl,
        string path,
        Action<string> fullNavigate,
        CancellationToken cancellationToken = default)
    {
        if (webView == null || string.IsNullOrWhiteSpace(path))
            return;

        var target = KioskUrlPolicy.ResolvePath(kioskBaseUrl, path);
        if (target == null) return;
        var haPath = new Uri(target).PathAndQuery + new Uri(target).Fragment;
        if (!KioskUrlPolicy.IsSameOrigin(kioskBaseUrl, webView.Source))
        {
            fullNavigate(target);
            return;
        }
        var escaped = System.Text.Json.JsonSerializer.Serialize(haPath);
        // Match Home Assistant frontend navigate(): pushState then location-changed with bubbles/composed/detail.
        var script =
            "(function(){try{var path=" + escaped + ";"
            + "history.pushState(null,\"\",path);"
            + "var ev=new Event(\"location-changed\",{bubbles:true,composed:true});"
            + "ev.detail={replace:false};"
            + "window.dispatchEvent(ev);"
            + "return window.location.pathname===path||window.location.pathname===path.replace(/\\/$/,'');"
            + "}catch(e){return false;}})()";

        try
        {
            var result = await webView.ExecuteScriptAsync(script);
            if (string.Equals(result, "true", StringComparison.OrdinalIgnoreCase))
                return;
        }
        catch
        {
            // fall through to full navigation
        }

        cancellationToken.ThrowIfCancellationRequested();
        fullNavigate(target);
    }

    internal static string BuildFullUrl(string kioskBaseUrl, string haPath)
    {
        if (!Uri.TryCreate(kioskBaseUrl, UriKind.Absolute, out var baseUri))
            return haPath;

        var relative = haPath.StartsWith('/') ? haPath : "/" + haPath;
        return Uri.TryCreate(baseUri, relative, out var combined)
            ? combined.ToString()
            : haPath;
    }

    internal static string NormalizeNavigatePayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return "";

        var trimmed = payload.Trim();
        if (trimmed.Length >= 2
            && ((trimmed.StartsWith('"') && trimmed.EndsWith('"'))
                || (trimmed.StartsWith('\'') && trimmed.EndsWith('\''))))
        {
            trimmed = trimmed[1..^1].Trim();
        }

        return trimmed;
    }
}

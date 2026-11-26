namespace HAWinKiosk.Mqtt;

/// <summary>Only HTTP(S) origins are valid kiosk endpoints; MQTT paths stay on the configured origin.</summary>
public static class KioskUrlPolicy
{
    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "about:blank";
        var input = raw.Trim();
        if (TryHttp(input, out var direct)) return direct!.AbsoluteUri;
        if (!input.Contains("://") && !input.StartsWith('/') && !input.Contains('\\')
            && TryHttp("http://" + input, out var withScheme)) return withScheme!.AbsoluteUri;
        return "about:blank";
    }

    private static bool TryHttp(string value, out Uri? uri) =>
        Uri.TryCreate(value, UriKind.Absolute, out uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        && !string.IsNullOrWhiteSpace(uri.Host) && string.IsNullOrEmpty(uri.UserInfo);

    public static bool IsSameOrigin(string? configured, string? candidate)
    {
        return TryHttp(Normalize(configured), out var origin)
            && TryHttp(candidate ?? "", out var target)
            && string.Equals(origin!.Scheme, target!.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(origin.IdnHost, target.IdnHost, StringComparison.OrdinalIgnoreCase)
            && origin.Port == target.Port;
    }

    public static string? ResolvePath(string? configured, string? payload)
    {
        if (!TryHttp(Normalize(configured), out var origin) || string.IsNullOrWhiteSpace(payload)) return null;
        var path = payload.Trim();
        if (path.StartsWith("//") || path.Contains('\\')) return null;
        if (TryHttp(path, out var absolute))
            return IsSameOrigin(configured, absolute!.AbsoluteUri) ? absolute.AbsoluteUri : null;
        if (path.Contains("://") || path.Contains(':') && !path.StartsWith('/')) return null;
        if (!path.StartsWith('/')) path = "/" + path;
        return Uri.TryCreate(origin, path, out var target) && IsSameOrigin(configured, target.AbsoluteUri)
            ? target.AbsoluteUri : null;
    }
}

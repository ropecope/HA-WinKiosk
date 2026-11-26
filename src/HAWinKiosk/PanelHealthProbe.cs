using System.Net;
using System.Net.Http;
using HAWinKiosk.Mqtt;

namespace HAWinKiosk;

internal static class PanelHealthProbe
{
    private static readonly HttpClient Client = new(new HttpClientHandler { AllowAutoRedirect = false })
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    // For a deliberately trusted self-signed HTTPS endpoint, use the browser's navigation
    // and network-status recovery rather than weakening TLS verification in a second client.
    public static async Task<bool> IsReachableAsync(string? configured, bool ignoreCertificateErrors)
    {
        if (!Uri.TryCreate(KioskUrlPolicy.Normalize(configured), UriKind.Absolute, out var origin)) return false;
        if (ignoreCertificateErrors && origin.Scheme == Uri.UriSchemeHttps) return true;
        try
        {
            var target = new Uri(origin, "/api/");
            using var response = await Client.GetAsync(target, HttpCompletionOption.ResponseHeadersRead);
            return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Unauthorized
                || response.StatusCode == HttpStatusCode.Forbidden;
        }
        catch (HttpRequestException) { return false; }
        catch (TaskCanceledException) { return false; }
    }
}

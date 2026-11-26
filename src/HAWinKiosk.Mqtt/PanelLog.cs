using System.IO;

namespace HAWinKiosk.Mqtt;

/// <summary>Small local diagnostic log; never records URLs, payloads, credentials or exception messages.</summary>
public static class PanelLog
{
    private static readonly object Gate = new();
    public static string PathName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GrowBox-Panel", "panel.log");
    public static void Write(string category, Exception? error = null)
    {
        lock (Gate)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PathName)!);
                if (File.Exists(PathName) && new FileInfo(PathName).Length > 256 * 1024)
                    File.Move(PathName, PathName + ".old", overwrite: true);
                File.AppendAllText(PathName, $"{DateTimeOffset.Now:O} {category} {error?.GetType().Name}{Environment.NewLine}");
            }
            catch { /* Diagnostics never block the kiosk. */ }
        }
    }
}

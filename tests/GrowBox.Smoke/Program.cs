using HAWinKiosk.Mqtt;
using HAWinKiosk.Mqtt.Models;

static void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException(name);
    Console.WriteLine("PASS " + name);
}
Check(KioskUrlPolicy.Normalize("homeassistant.local:8123") == "http://homeassistant.local:8123/", "host and port");
Check(KioskUrlPolicy.Normalize("https://ha.example/dashboard") == "https://ha.example/dashboard", "HTTPS");
Check(KioskUrlPolicy.Normalize("file:///C:/Windows") == "about:blank", "file blocked");
Check(KioskUrlPolicy.Normalize("http://user:password@ha.example") == "about:blank", "credentials rejected");
Check(KioskUrlPolicy.ResolvePath("http://ha.local:8123", "/growbox/status") == "http://ha.local:8123/growbox/status", "local path");
Check(KioskUrlPolicy.ResolvePath("http://ha.local:8123", "//outside.example") == null, "external relative URL blocked");
Check(KioskUrlPolicy.ResolvePath("http://ha.local:8123", "https://outside.example") == null, "external absolute URL blocked");
Check(KioskUrlPolicy.ResolvePath("http://ha.local:8123", "http://ha.local:8124/status") == null, "different port blocked");
var defaults = new AppSettings();
Check(defaults.Mqtt.Host == "", "MQTT opt-in");
Check(!defaults.Sensors.Enabled.Contains("last_active"), "no high-frequency default sensor");
Check(!defaults.Commands.Enabled.Contains("shutdown"), "no default remote shutdown");
var roundTrip = SettingsYamlConversion.ToAppSettings(SettingsYamlConversion.FromAppSettings(defaults));
Check(roundTrip.Mqtt.Host == defaults.Mqtt.Host && roundTrip.Mqtt.DeviceName == defaults.Mqtt.DeviceName, "settings conversion");

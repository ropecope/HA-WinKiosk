using System;
using System.Windows;
using System.Windows.Media;

namespace HAWinKiosk;

/// <summary>App UI colours with explicit readable light and dark surfaces.</summary>
public static class ThemePalette
{
    // Accents
    public const byte AccentBoldR = 0x59, AccentBoldG = 0x93, AccentBoldB = 0xF6;       // #5993F6
    public const byte AccentMediumR = 0x87, AccentMediumG = 0xBB, AccentMediumB = 0xFF; // #87BBFF
    public const byte AccentLightR = 0xC1, AccentLightG = 0xDE, AccentLightB = 0xFF;     // #C1DEFF

    // Surface endpoints (#171B21 = darkest surface, #F2F4F9 = lightest surface)
    private static readonly System.Windows.Media.Color SurfaceDarkest = Rgb(0x17, 0x1B, 0x21); // #171B21
    private static readonly System.Windows.Media.Color SurfaceLightest = Rgb(0xF2, 0xF4, 0xF9); // #F2F4F9

    // Light surfaces (canonical mid steps)
    private static readonly System.Windows.Media.Color LightShade1 = Rgb(0xF4, 0xF6, 0xF8); // #F4F6F8
    private static readonly System.Windows.Media.Color LightShade2 = Rgb(0xD0, 0xD8, 0xE4); // #D0D8E4

    public static System.Windows.Media.Color Accent => Rgb(AccentBoldR, AccentBoldG, AccentBoldB);
    public static System.Windows.Media.Color AccentMedium => Rgb(AccentMediumR, AccentMediumG, AccentMediumB);
    public static System.Windows.Media.Color AccentPale => Rgb(AccentLightR, AccentLightG, AccentLightB);

    public static string GestureTickLightStroke => "#F2F4F9";
    public static string GestureTickDarkStroke => "#171B21";

    /// <summary>CSS radial-gradient for gesture-ack glow in the WebView overlay.</summary>
    public static string GestureGlowGradient =>
        $"radial-gradient(circle closest-side, rgba({AccentBoldR},{AccentBoldG},{AccentBoldB},0.58) 0%, "
        + $"rgba({AccentBoldR},{AccentBoldG},{AccentBoldB},0.28) 38%, rgba({AccentBoldR},{AccentBoldG},{AccentBoldB},0.09) 58%, transparent 76%)";

    public static void Apply(ResourceDictionary resources, bool dark)
    {
        static SolidColorBrush B(System.Windows.Media.Color c) => new(c);
        void Set(string key, System.Windows.Media.Color color) => resources[key] = B(color);

        // Light: panel -> card -> input uses shade 2 -> shade 1 -> lightest surface (#F2F4F9).
        // Dark: explicit panel, card and input surfaces with contrasting borders.
        var darkPanel = SurfaceDarkest;
        var darkSurface = Rgb(0x24, 0x2B, 0x34);
        var darkInput = Rgb(0x30, 0x30, 0x30);

        var panel = dark ? darkPanel : LightShade2;
        var surface = dark ? darkSurface : LightShade1;
        var border = dark ? Rgb(0x5F, 0x70, 0x82) : LightShade2;
        var fg = dark ? SurfaceLightest : SurfaceDarkest;
        var fgMuted = dark ? LightShade2 : SurfaceDarkest;
        var fgSub = fgMuted;
        var input = dark ? darkInput : SurfaceLightest;
        var inputBorder = dark ? Rgb(0x7B, 0x8B, 0x9D) : Rgb(0x78, 0x87, 0x99);
        var inputFg = dark ? SurfaceLightest : SurfaceDarkest;
        var disabledTrack = dark ? darkPanel : LightShade2;
        var disabledThumb = AccentPale;
        var thumbOff = SurfaceLightest;
        var breakingChangesBg = dark ? Lerp(darkSurface, Accent, 0.22) : AccentPale;
        var breakingChangesFg = dark ? SurfaceLightest : SurfaceDarkest;
        var breakingChangesIcon = dark ? AccentMedium : Rgb(0x20, 0x51, 0xA2);

        Set("Theme.Accent", Accent);
        Set("Theme.Accent.Light", AccentMedium);
        Set("Theme.Accent.Pale", AccentPale);
        Set("Theme.Accent.Fg", SurfaceDarkest);
        Set("Theme.Section.Bg", dark ? Rgb(0x35, 0x35, 0x35) : AccentPale);
        Set("Theme.Section.HoverBg", dark ? Rgb(0x45, 0x45, 0x45) : AccentMedium);
        Set("Theme.Section.Fg", fg);
        Set("Theme.Selection.Bg", dark ? Rgb(0x50, 0x50, 0x50) : AccentPale);
        Set("Theme.Selection.Fg", fg);
        Set("Theme.Error.Fg", dark ? Rgb(0xFF, 0xA8, 0xA8) : Rgb(0x9B, 0x18, 0x18));

        Set("Theme.Kiosk.Bg", panel);
        Set("Theme.Kiosk.SettingsButtonBg", Accent);
        Set("Theme.Settings.PanelBg", panel);
        Set("Theme.Settings.CardBg", surface);
        Set("Theme.Settings.CardBorder", border);
        Set("Theme.Settings.HeaderBg", surface);
        Set("Theme.Settings.HeaderBorder", border);
        Set("Theme.Settings.Fg", fg);
        Set("Theme.Settings.FgMuted", fgMuted);
        Set("Theme.Settings.FgSub", fgSub);
        Set("Theme.Settings.InputBg", input);
        Set("Theme.Settings.InputBorder", inputBorder);
        Set("Theme.Settings.InputFg", inputFg);
        Set("Theme.Button.SecondaryBg", input);
        Set("Theme.Button.SecondaryFg", inputFg);
        Set("Theme.Button.SecondaryBorder", inputBorder);
        Set("Theme.Toggle.TrackOff", dark ? Rgb(0x58, 0x68, 0x79) : Rgb(0x78, 0x87, 0x99));
        Set("Theme.Toggle.ThumbOff", thumbOff);
        Set("Theme.Toggle.TrackOn", Accent);
        Set("Theme.Toggle.ThumbOn", SurfaceDarkest);
        Set("Theme.Toggle.DisabledTrack", disabledTrack);
        Set("Theme.Toggle.DisabledThumb", disabledThumb);
        Set("Theme.Pin.CardBg", surface);
        Set("Theme.Pin.CardBorder", border);
        Set("Theme.Pin.Fg", fg);
        Set("Theme.Pin.FgMuted", fgMuted);
        Set("Theme.SecretInput.EyeMuted", fgMuted);
        Set("Theme.SecretInput.EyeActive", dark ? AccentMedium : Rgb(0x20, 0x51, 0xA2));
        Set("Theme.BreakingChanges.Bg", breakingChangesBg);
        Set("Theme.BreakingChanges.Border", Accent);
        Set("Theme.BreakingChanges.Fg", breakingChangesFg);
        Set("Theme.BreakingChanges.Icon", breakingChangesIcon);
    }

    public static void ApplyAppWide(bool dark) => Apply(System.Windows.Application.Current.Resources, dark);

    private static System.Windows.Media.Color Lerp(System.Windows.Media.Color from, System.Windows.Media.Color to, double t)
    {
        t = Math.Clamp(t, 0, 1);
        return Rgb(
            (byte)Math.Round(from.R + (to.R - from.R) * t),
            (byte)Math.Round(from.G + (to.G - from.G) * t),
            (byte)Math.Round(from.B + (to.B - from.B) * t));
    }

    private static System.Windows.Media.Color Rgb(byte r, byte g, byte b) => System.Windows.Media.Color.FromRgb(r, g, b);
}

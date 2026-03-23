using Godot;

namespace GameTest;

public static class GameUi
{
    private static void ApplyTextFont(Control control, bool bold)
    {
        control.AddThemeFontOverride("font", GameAssets.GetUiFont(bold));
    }

    private static void ApplyRichTextFont(RichTextLabel body, bool bold)
    {
        var normalFont = GameAssets.GetUiFont(false);
        var boldFont = GameAssets.GetUiFont(true);

        body.AddThemeFontOverride("normal_font", bold ? boldFont : normalFont);
        body.AddThemeFontOverride("bold_font", boldFont);
        body.AddThemeFontOverride("italics_font", normalFont);
        body.AddThemeFontOverride("bold_italics_font", boldFont);
        body.AddThemeFontOverride("mono_font", normalFont);
    }

    public static void StyleHudPanel(PanelContainer panel)
    {
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0f, 0f, 0f, 0.86f),
            BorderColor = new Color("6d5b42"),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4,
            ContentMarginLeft = 16,
            ContentMarginTop = 10,
            ContentMarginRight = 16,
            ContentMarginBottom = 10
        });
    }

    public static void StyleOverlayPanel(PanelContainer panel)
    {
        panel.AddThemeStyleboxOverride("panel", GameAssets.CreateUiStyleBox(UiChrome.DarkPanel));
    }

    public static void StyleButton(Button button)
    {
        ApplyTextFont(button, bold: false);
        button.AddThemeStyleboxOverride("normal", GameAssets.CreateUiStyleBox(UiChrome.Button));
        button.AddThemeStyleboxOverride("hover", GameAssets.CreateUiStyleBox(UiChrome.ButtonHover));
        button.AddThemeStyleboxOverride("pressed", GameAssets.CreateUiStyleBox(UiChrome.ButtonPressed));
        button.AddThemeStyleboxOverride("focus", GameAssets.CreateUiStyleBox(UiChrome.ButtonHover));
        button.AddThemeColorOverride("font_color", new Color("fff7da"));
        button.AddThemeColorOverride("font_hover_color", new Color("ffffff"));
        button.AddThemeColorOverride("font_pressed_color", new Color("fff3c4"));
        button.AddThemeColorOverride("font_focus_color", new Color("ffffff"));
        button.AddThemeColorOverride("font_disabled_color", new Color("b9b0a0"));
        button.AddThemeColorOverride("font_outline_color", new Color("1b140f"));
        button.AddThemeConstantOverride("outline_size", 2);
        button.AddThemeFontSizeOverride("font_size", 16);
        button.CustomMinimumSize = new Vector2(184, 50);
        button.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
    }

    public static void StyleHeader(Label label)
    {
        ApplyTextFont(label, bold: false);
        label.AddThemeColorOverride("font_color", new Color("f7f0df"));
        label.AddThemeFontSizeOverride("font_size", 20);
    }

    public static void StyleLogoPrimary(Label label)
    {
        ApplyTextFont(label, bold: true);
        label.AddThemeColorOverride("font_color", new Color("fff0b3"));
        label.AddThemeColorOverride("font_outline_color", new Color("2b1920"));
        label.AddThemeConstantOverride("outline_size", 6);
        label.AddThemeFontSizeOverride("font_size", 52);
        label.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public static void StyleLogoSecondary(Label label)
    {
        ApplyTextFont(label, bold: true);
        label.AddThemeColorOverride("font_color", new Color("9fe5ff"));
        label.AddThemeColorOverride("font_outline_color", new Color("162537"));
        label.AddThemeConstantOverride("outline_size", 6);
        label.AddThemeFontSizeOverride("font_size", 42);
        label.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public static void StylePixelCaption(Label label)
    {
        ApplyTextFont(label, bold: true);
        label.AddThemeColorOverride("font_color", new Color("f6d27b"));
        label.AddThemeColorOverride("font_outline_color", new Color("231813"));
        label.AddThemeConstantOverride("outline_size", 2);
        label.AddThemeFontSizeOverride("font_size", 14);
        label.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public static void StyleAccent(Label label)
    {
        ApplyTextFont(label, bold: false);
        label.AddThemeColorOverride("font_color", new Color("f6d27b"));
        label.AddThemeFontSizeOverride("font_size", 14);
    }

    public static void StyleHudKey(Label label)
    {
        ApplyTextFont(label, bold: true);
        label.AddThemeColorOverride("font_color", new Color("f6d27b"));
        label.AddThemeColorOverride("font_outline_color", new Color("231813"));
        label.AddThemeConstantOverride("outline_size", 2);
        label.AddThemeFontSizeOverride("font_size", 12);
        label.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public static void StyleValue(Label label)
    {
        ApplyTextFont(label, bold: false);
        label.AddThemeColorOverride("font_color", new Color("ffffff"));
        label.AddThemeFontSizeOverride("font_size", 16);
    }

    public static void StyleHudValue(Label label)
    {
        ApplyTextFont(label, bold: true);
        label.AddThemeColorOverride("font_color", new Color("ffffff"));
        label.AddThemeColorOverride("font_outline_color", new Color("1a2230"));
        label.AddThemeConstantOverride("outline_size", 2);
        label.AddThemeFontSizeOverride("font_size", 16);
        label.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public static void StyleBody(RichTextLabel body)
    {
        ApplyRichTextFont(body, bold: false);
        body.AddThemeColorOverride("default_color", new Color("edf5ff"));
        body.AddThemeFontSizeOverride("normal_font_size", 16);
        body.AddThemeFontSizeOverride("bold_font_size", 16);
        body.AddThemeFontSizeOverride("italics_font_size", 16);
        body.AddThemeFontSizeOverride("bold_italics_font_size", 16);
        body.AddThemeFontSizeOverride("mono_font_size", 16);
    }
}

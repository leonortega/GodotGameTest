using Godot;

namespace GameTest;

public static class GameUi
{
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
        button.AddThemeConstantOverride("outline_size", 3);
        button.AddThemeFontSizeOverride("font_size", 18);
        button.CustomMinimumSize = new Vector2(168, 46);
        button.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
    }

    public static void StyleHeader(Label label)
    {
        label.AddThemeColorOverride("font_color", new Color("f7f0df"));
    }

    public static void StyleLogoPrimary(Label label)
    {
        label.AddThemeColorOverride("font_color", new Color("fff0b3"));
        label.AddThemeColorOverride("font_outline_color", new Color("2b1920"));
        label.AddThemeConstantOverride("outline_size", 8);
        label.AddThemeFontSizeOverride("font_size", 38);
        label.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public static void StyleLogoSecondary(Label label)
    {
        label.AddThemeColorOverride("font_color", new Color("9fe5ff"));
        label.AddThemeColorOverride("font_outline_color", new Color("162537"));
        label.AddThemeConstantOverride("outline_size", 8);
        label.AddThemeFontSizeOverride("font_size", 30);
        label.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public static void StylePixelCaption(Label label)
    {
        label.AddThemeColorOverride("font_color", new Color("f6d27b"));
        label.AddThemeColorOverride("font_outline_color", new Color("231813"));
        label.AddThemeConstantOverride("outline_size", 4);
        label.AddThemeFontSizeOverride("font_size", 15);
        label.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public static void StyleAccent(Label label)
    {
        label.AddThemeColorOverride("font_color", new Color("f6d27b"));
    }

    public static void StyleHudKey(Label label)
    {
        label.AddThemeColorOverride("font_color", new Color("f6d27b"));
        label.AddThemeColorOverride("font_outline_color", new Color("231813"));
        label.AddThemeConstantOverride("outline_size", 3);
        label.AddThemeFontSizeOverride("font_size", 14);
        label.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public static void StyleValue(Label label)
    {
        label.AddThemeColorOverride("font_color", new Color("ffffff"));
    }

    public static void StyleHudValue(Label label)
    {
        label.AddThemeColorOverride("font_color", new Color("ffffff"));
        label.AddThemeColorOverride("font_outline_color", new Color("1a2230"));
        label.AddThemeConstantOverride("outline_size", 4);
        label.AddThemeFontSizeOverride("font_size", 18);
        label.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public static void StyleBody(RichTextLabel body)
    {
        body.AddThemeColorOverride("default_color", new Color("edf5ff"));
    }
}

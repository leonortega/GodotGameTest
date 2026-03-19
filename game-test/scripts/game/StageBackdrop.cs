using Godot;

namespace GameTest;

public partial class StageBackdrop : Node2D
{
    private StageTheme _theme;
    private Rect2 _bounds;

    public void Configure(StageTheme theme, Rect2 bounds)
    {
        _theme = theme;
        _bounds = bounds;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_bounds == default)
        {
            return;
        }

        var size = _bounds.Size;
        DrawRect(new Rect2(Vector2.Zero, size), GetSkyColor(_theme), true);

        DrawStrip(GameAssets.GetBackdropClouds(_theme), 22f, 210f, new Color(1f, 1f, 1f, 0.95f), 1.25f);

        if (_theme == StageTheme.Grassland || _theme == StageTheme.Treetop)
        {
            DrawStrip(GameAssets.GetBackdropMid(_theme), size.Y - 250f, 230f, Colors.White, 1.35f);
        }
        else
        {
            DrawStrip(GameAssets.GetBackdropMid(_theme), size.Y - 220f, 190f, new Color(1f, 1f, 1f, 0.92f), 1.15f);
        }
    }

    private void DrawStrip(Texture2D texture, float y, float height, Color modulate, float widthScale)
    {
        var safeY = Mathf.Clamp(y, 0f, _bounds.Size.Y);
        var stripHeight = Mathf.Min(height, _bounds.Size.Y - safeY);
        if (stripHeight <= 0f)
        {
            return;
        }

        var segmentWidth = texture.GetWidth() / (float)texture.GetHeight() * stripHeight * widthScale;
        for (var x = -segmentWidth * 0.25f; x < _bounds.Size.X + segmentWidth; x += segmentWidth - 6f)
        {
            DrawTextureRect(texture, new Rect2(x, safeY, segmentWidth, stripHeight), false, modulate);
        }
    }

    private static Color GetSkyColor(StageTheme theme) => theme switch
    {
        StageTheme.Cave => new Color("b8bfd3"),
        StageTheme.Treetop => new Color("bfe0ff"),
        StageTheme.Fortress => new Color("cfc8d8"),
        _ => new Color("b8dcff")
    };
}

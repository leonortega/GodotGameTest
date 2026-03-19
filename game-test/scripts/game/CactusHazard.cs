using Godot;

namespace GameTest;

public partial class CactusHazard : StaticBody2D
{
    private static readonly Vector2 VisualSize = new(38f, 54f);
    private static readonly Vector2 CollisionSize = new(34f, 40f);
    private static readonly Vector2 DamageSize = new(42f, 46f);

    private CollisionShape2D _collision = null!;
    private Sprite2D _sprite = null!;

    private static float GroundBottomOffset => VisualSize.Y * 0.5f;
    private static float CollisionCenterY => GroundBottomOffset - (CollisionSize.Y * 0.5f);
    private static float DamageCenterY => GroundBottomOffset - (DamageSize.Y * 0.5f) - 4f;

    public float GroundContactOffset => GroundBottomOffset;
    public Rect2 HurtBox => new(
        GlobalPosition + new Vector2(-DamageSize.X * 0.5f, DamageCenterY - DamageSize.Y * 0.5f),
        DamageSize);

    public override void _Ready()
    {
        CollisionLayer = 1;
        CollisionMask = 0;

        _collision = new CollisionShape2D
        {
            Shape = new RectangleShape2D
            {
                Size = CollisionSize
            },
            Position = new Vector2(0f, CollisionCenterY)
        };

        _sprite = new Sprite2D();
        AddChild(_sprite);
        AddChild(_collision);
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        GameAssets.ApplyFittedSprite(_sprite, GameAssets.GetCactusTexture(), new Vector2(48f, 68f), GroundBottomOffset);
    }

    public void Deactivate()
    {
        Visible = false;
        CollisionLayer = 0;
        CollisionMask = 0;
        _collision.Disabled = true;
    }
}

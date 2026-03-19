using Godot;

namespace GameTest;

public partial class CactusHazard : StaticBody2D
{
    private readonly Vector2 _size = new(38, 54);
    private CollisionShape2D _collision = null!;
    private Sprite2D _sprite = null!;

    public float GroundContactOffset => _size.Y * 0.5f;
    public Rect2 HurtBox => new(GlobalPosition + new Vector2(-18f, -52f), new Vector2(36f, 52f));

    public override void _Ready()
    {
        CollisionLayer = 1;
        CollisionMask = 0;

        _collision = new CollisionShape2D
        {
            Shape = new RectangleShape2D
            {
                Size = _size
            }
        };

        _sprite = new Sprite2D();
        AddChild(_sprite);
        AddChild(_collision);
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        GameAssets.ApplyFittedSprite(_sprite, GameAssets.GetCactusTexture(), new Vector2(48f, 68f), _size.Y * 0.5f);
    }

    public void Deactivate()
    {
        Visible = false;
        CollisionLayer = 0;
        CollisionMask = 0;
        _collision.Disabled = true;
    }
}

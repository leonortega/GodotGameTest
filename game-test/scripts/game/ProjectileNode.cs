using Godot;

namespace GameTest;

public partial class ProjectileNode : Node2D
{
    private const float Speed = 620f;
    private readonly Vector2 _size = new(18, 12);
    private Sprite2D _sprite = null!;
    private Vector2 _direction = Vector2.Right;
    private float _lifetime = 1.5f;

    public bool IsExpired { get; private set; }

    public Rect2 HitBox => new(GlobalPosition - _size * 0.5f, _size);

    public override void _Ready()
    {
        _sprite = new Sprite2D();
        AddChild(_sprite);
        UpdateVisual();
    }

    public void Configure(Vector2 startPosition, int facing)
    {
        GlobalPosition = startPosition;
        _direction = facing >= 0 ? Vector2.Right : Vector2.Left;
        UpdateVisual();
    }

    public void Advance(double delta)
    {
        if (IsExpired)
        {
            return;
        }

        GlobalPosition += _direction * Speed * (float)delta;
        UpdateVisual();
        _lifetime -= (float)delta;
        if (_lifetime <= 0f)
        {
            Expire();
        }
    }

    public void Expire()
    {
        if (IsExpired)
        {
            return;
        }

        IsExpired = true;
        QueueFree();
    }

    private void UpdateVisual()
    {
        if (_sprite is null)
        {
            return;
        }

        GameAssets.ApplyFittedSprite(_sprite, GameAssets.GetProjectileTexture(), new Vector2(24, 20), 0f, true);
        _sprite.FlipH = _direction.X < 0f;
    }
}

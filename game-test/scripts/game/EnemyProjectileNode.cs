using Godot;

namespace GameTest;

public partial class EnemyProjectileNode : Node2D
{
    private const float Speed = 300f;
    private readonly Vector2 _size = new(16, 16);
    private Sprite2D _sprite = null!;
    private Vector2 _direction = Vector2.Left;
    private float _lifetime = 2.4f;

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
        _lifetime = 2.4f;
        IsExpired = false;
        UpdateVisual();
    }

    public void Advance(double delta)
    {
        if (IsExpired)
        {
            return;
        }

        GlobalPosition += _direction * Speed * (float)delta;
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

        GameAssets.ApplyFittedSprite(_sprite, GameAssets.GetEnemyProjectileTexture(), new Vector2(20f, 20f), 0f, true);
        _sprite.Modulate = new Color("ff8f66");
        _sprite.FlipH = _direction.X < 0f;
    }
}

using Godot;

namespace GameTest;

public partial class PickupNode : Node2D
{
    private readonly Vector2 _size = new(26, 26);
    private Sprite2D _sprite = null!;
    private float _bobTimer;
    private Vector2 _basePosition;

    [Export]
    public PickupType AuthoredPickupType { get; set; } = PickupType.Coin;

    public PickupType PickupType { get; private set; }
    public bool Collected { get; private set; }
    public Rect2 HitBox => new(GlobalPosition - _size * 0.5f, _size);

    public override void _Ready()
    {
        _sprite = new Sprite2D();
        AddChild(_sprite);
        _basePosition = Position;
        UpdateVisual();
    }

    public void Configure(PickupType pickupType, Vector2 worldPosition)
    {
        PickupType = pickupType;
        GlobalPosition = worldPosition;
        _basePosition = Position;
        UpdateVisual();
    }

    public override void _Process(double delta)
    {
        if (Collected)
        {
            return;
        }

        _bobTimer += (float)delta;
        Position = new Vector2(_basePosition.X, _basePosition.Y + Mathf.Sin(_bobTimer * 3f) * 4f);
        UpdateVisual();
    }

    public void Collect()
    {
        Collected = true;
        QueueFree();
    }

    private void UpdateVisual()
    {
        var texture = GameAssets.GetPickupTexture(PickupType, Mathf.PosMod(Mathf.FloorToInt(_bobTimer * 6f), 2) == 1);
        GameAssets.ApplyFittedSprite(_sprite, texture, _size, _size.Y * 0.5f);
    }
}

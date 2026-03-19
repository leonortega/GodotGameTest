using Godot;

namespace GameTest;

public partial class MysteryBlock : StaticBody2D
{
    private readonly Vector2 _size = new(40, 40);
    private CollisionShape2D _collision = null!;
    private Sprite2D _sprite = null!;

    [Export]
    public PickupType Reward { get; set; } = PickupType.Coin;

    public bool Activated { get; private set; }
    public Rect2 HitBox => new(GlobalPosition - _size * 0.5f, _size);

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

    public void Configure(PickupType reward, Vector2 position)
    {
        Reward = reward;
        GlobalPosition = position;
        Activated = false;
        UpdateVisual();
    }

    public bool Activate(WorldRoot world)
    {
        if (Activated)
        {
            return false;
        }

        Activated = true;
        UpdateVisual();

        if (Reward == PickupType.Coin)
        {
            GameSession.Instance.AddCoin();
            GameSession.Instance.AddScore(50);
            AudioDirector.Instance.PlaySfx("coin");
        }
        else
        {
            world.SpawnPickup(Reward, GlobalPosition + new Vector2(0, -34));
            AudioDirector.Instance.PlaySfx(Reward == PickupType.ExtraLife ? "extra_life" : "powerup");
        }

        return true;
    }

    private void UpdateVisual()
    {
        GameAssets.ApplyFittedSprite(_sprite, GameAssets.GetBlockTexture(Reward, Activated), _size, _size.Y * 0.5f);
    }
}

using System.Collections.Generic;
using Godot;

namespace GameTest;

public partial class FallingBlock : AnimatableBody2D
{
    private const float BlockWidth = 32f;
    private const float CollisionHeight = 24f;
    private const float RideAreaHeight = 22f;
    private const float SpriteScale = 0.5f;
    private const float SupportTopOffset = CollisionHeight * 0.5f;

    private readonly HashSet<PlayerController> _riders = [];

    private CollisionShape2D _collisionShape = null!;
    private Area2D _rideArea = null!;
    private CollisionShape2D _rideAreaCollision = null!;
    private Sprite2D _sprite = null!;
    private float _contactTime;
    private float _fallVelocity;

    [Export]
    public float TriggerDelaySeconds { get; set; } = 0.5f;

    [Export]
    public float FallGravity { get; set; } = 1600f;

    [Export]
    public bool UseParentStageTheme { get; set; } = true;

    [Export]
    public StageTheme ThemeOverride { get; set; } = StageTheme.Grassland;

    public bool SimulationActive { get; set; } = true;
    public bool IsFalling { get; private set; }
    public float SupportTopY => GlobalPosition.Y - SupportTopOffset;
    public float SupportLeftX => GlobalPosition.X - BlockWidth * 0.5f;
    public float SupportRightX => GlobalPosition.X + BlockWidth * 0.5f;

    public override void _Ready()
    {
        CollisionLayer = 1;
        CollisionMask = 0;
        SyncToPhysics = true;

        _collisionShape = new CollisionShape2D
        {
            Shape = new RectangleShape2D
            {
                Size = new Vector2(BlockWidth, CollisionHeight)
            }
        };
        AddChild(_collisionShape);

        _rideArea = new Area2D
        {
            CollisionLayer = 0,
            CollisionMask = 2,
            Monitoring = true,
            Monitorable = false
        };
        _rideArea.BodyEntered += OnRideBodyEntered;
        _rideArea.BodyExited += OnRideBodyExited;
        AddChild(_rideArea);

        _rideAreaCollision = new CollisionShape2D
        {
            Shape = new RectangleShape2D
            {
                Size = new Vector2(BlockWidth - 4f, RideAreaHeight)
            },
            Position = new Vector2(0f, -SupportTopOffset - RideAreaHeight * 0.5f + 8f)
        };
        _rideArea.AddChild(_rideAreaCollision);

        _sprite = new Sprite2D
        {
            Centered = true,
            TextureFilter = TextureFilterEnum.Nearest,
            Scale = Vector2.One * SpriteScale
        };
        AddChild(_sprite);

        ApplyThemeTexture();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!SimulationActive)
        {
            return;
        }

        if (!IsFalling)
        {
            _contactTime = HasStandingRider() ? _contactTime + (float)delta : 0f;
            if (_contactTime >= TriggerDelaySeconds)
            {
                IsFalling = true;
                _fallVelocity = 0f;
            }

            return;
        }

        _fallVelocity += FallGravity * (float)delta;
        GlobalPosition += new Vector2(0f, _fallVelocity * (float)delta);
    }

    public override void _ExitTree()
    {
        if (_rideArea is not null)
        {
            _rideArea.BodyEntered -= OnRideBodyEntered;
            _rideArea.BodyExited -= OnRideBodyExited;
        }
    }

    private void ApplyThemeTexture()
    {
        _sprite.Texture = GameAssets.GetFallingBlockTexture(ResolveTheme());
    }

    private StageTheme ResolveTheme()
    {
        if (!UseParentStageTheme)
        {
            return ThemeOverride;
        }

        for (Node? node = GetParent(); node is not null; node = node.GetParent())
        {
            if (node is StageScene stage)
            {
                return stage.Theme;
            }
        }

        return ThemeOverride;
    }

    private bool HasStandingRider()
    {
        foreach (var rider in _riders.ToArray())
        {
            if (!IsInstanceValid(rider))
            {
                _riders.Remove(rider);
                continue;
            }

            if (IsStandingOnBlock(rider))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsStandingOnBlock(PlayerController player)
    {
        var hitBox = player.HitBox;
        var topY = GlobalPosition.Y - SupportTopOffset;
        if (hitBox.End.Y < topY - 10f || hitBox.End.Y > topY + 12f)
        {
            return false;
        }

        var blockLeft = GlobalPosition.X - BlockWidth * 0.5f + 4f;
        var blockRight = GlobalPosition.X + BlockWidth * 0.5f - 4f;
        return hitBox.End.X > blockLeft && hitBox.Position.X < blockRight;
    }

    private void OnRideBodyEntered(Node body)
    {
        if (body is PlayerController player)
        {
            _riders.Add(player);
        }
    }

    private void OnRideBodyExited(Node body)
    {
        if (body is PlayerController player)
        {
            _riders.Remove(player);
        }
    }
}
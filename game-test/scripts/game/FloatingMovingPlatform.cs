using Godot;

namespace GameTest;

public partial class FloatingMovingPlatform : AnimatableBody2D
{
    private const float TilePixels = 32f;
    private const float CollisionHeight = 24f;

    private CollisionShape2D _collision = null!;
    private RectangleShape2D _collisionShape = null!;
    private Node2D _visualRoot = null!;
    private Vector2 _origin;
    private float _direction = 1f;

    [Export]
    public int WidthTiles { get; set; } = 4;

    [Export]
    public float PatrolDistance { get; set; } = 96f;

    [Export]
    public float MoveSpeed { get; set; } = 84f;

    [Export]
    public bool UseParentStageTheme { get; set; } = true;

    [Export]
    public StageTheme ThemeOverride { get; set; } = StageTheme.Grassland;

    public bool SimulationActive { get; set; } = true;
    public float SupportTopY => GlobalPosition.Y - CollisionHeight * 0.5f;
    public float SupportLeftX => GlobalPosition.X - Mathf.Max(1, WidthTiles) * TilePixels * 0.5f;
    public float SupportRightX => GlobalPosition.X + Mathf.Max(1, WidthTiles) * TilePixels * 0.5f;

    public override void _Ready()
    {
        CollisionLayer = 1;
        CollisionMask = 0;
        SyncToPhysics = true;

        _collisionShape = new RectangleShape2D();
        _collision = new CollisionShape2D
        {
            Shape = _collisionShape
        };

        _visualRoot = new Node2D();

        AddChild(_visualRoot);
        AddChild(_collision);

        _origin = GlobalPosition;
        RebuildPlatform();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!SimulationActive)
        {
            return;
        }

        var motionX = _direction * MoveSpeed * (float)delta;
        var nextOffset = GlobalPosition.X + motionX - _origin.X;
        if (Mathf.Abs(nextOffset) > PatrolDistance)
        {
            var clampedX = Mathf.Clamp(_origin.X + nextOffset, _origin.X - PatrolDistance, _origin.X + PatrolDistance);
            motionX = clampedX - GlobalPosition.X;
            _direction *= -1f;
        }

        var motion = new Vector2(motionX, 0f);
        GlobalPosition += motion;
    }

    private void RebuildPlatform()
    {
        var widthPixels = Mathf.Max(1, WidthTiles) * TilePixels;
        _collisionShape.Size = new Vector2(widthPixels, CollisionHeight);

        foreach (Node child in _visualRoot.GetChildren())
        {
            child.QueueFree();
        }

        var theme = ResolveTheme();
        var halfWidth = widthPixels * 0.5f;
        for (var index = 0; index < Mathf.Max(1, WidthTiles); index++)
        {
            var kind = WidthTiles == 1
                ? TerrainVisualKind.PlatformMiddle
                : index == 0
                    ? TerrainVisualKind.PlatformLeft
                    : index == WidthTiles - 1
                        ? TerrainVisualKind.PlatformRight
                        : TerrainVisualKind.PlatformMiddle;

            var sprite = new Sprite2D
            {
                Texture = GameAssets.GetTerrainTexture(theme, kind),
                Centered = true,
                TextureFilter = TextureFilterEnum.Nearest,
                Scale = Vector2.One * 0.5f,
                Position = new Vector2(-halfWidth + TilePixels * 0.5f + index * TilePixels, 0f)
            };
            _visualRoot.AddChild(sprite);
        }
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
}
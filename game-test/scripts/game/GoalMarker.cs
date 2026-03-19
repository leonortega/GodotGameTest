using Godot;

namespace GameTest;

public partial class GoalMarker : Node2D
{
    private Sprite2D _flag = null!;
    private float _animationTime;

    public Rect2 HitBox => new(GlobalPosition + new Vector2(-26, -54), new Vector2(52, 58));

    public override void _Ready()
    {
        _flag = new Sprite2D();
        AddChild(_flag);
        UpdateFlag(true);
    }

    public override void _Process(double delta)
    {
        _animationTime += (float)delta;
        UpdateFlag();
    }

    public void Configure(Vector2 position)
    {
        GlobalPosition = position;
        UpdateFlag(true);
    }

    private void UpdateFlag(bool forceRefresh = false)
    {
        if (!forceRefresh && _flag is null)
        {
            return;
        }

        var waving = Mathf.PosMod(Mathf.FloorToInt(_animationTime * 5f), 2) == 1;
        GameAssets.ApplyFittedSprite(_flag, GameAssets.GetGoalFlagTexture(waving), new Vector2(44, 44), -2f);
        _flag.Position = new Vector2(0f, _flag.Position.Y);
    }
}

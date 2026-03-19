using Godot;

namespace GameTest;

public partial class PlayerController : CharacterBody2D
{
    private const float Gravity = 1850f;
    private const float WalkSpeed = 250f;
    private const float RunSpeed = 380f;
    private const float JumpVelocity = -640f;
    private const float Acceleration = 1600f;
    private const float Friction = 1800f;
    private const float BounceVelocity = -360f;
    private const float GroundProbeDepth = 42f;
    private const float GroundSnapAllowance = 24f;

    private readonly Vector2 _size = new(34, 46);
    private CollisionShape2D _collision = null!;
    private RectangleShape2D _shape = null!;
    private Sprite2D _sprite = null!;
    private float _invulnerabilityTime;
    private float _fireCooldown;
    private float _animationTime;
    private int _remainingAirJumps = 1;

    public bool SimulationActive { get; set; }
    public int Facing { get; private set; } = 1;
    public Rect2 HitBox => new(GlobalPosition - _size * 0.5f, _size);
    public bool IsInvulnerable => _invulnerabilityTime > 0f;

    public event Action<Vector2, int>? FireRequested;
    public event Action<MysteryBlock>? BlockHit;

    public override void _Ready()
    {
        CollisionLayer = 2;
        CollisionMask = 1;
        FloorSnapLength = 18f;
        FloorMaxAngle = Mathf.DegToRad(50f);

        _shape = new RectangleShape2D
        {
            Size = _size
        };

        _collision = new CollisionShape2D
        {
            Shape = _shape
        };

        _sprite = new Sprite2D();
        AddChild(_sprite);
        AddChild(_collision);
        UpdateVisual(true);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!SimulationActive)
        {
            return;
        }

        var moveDirection = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        var running = Input.IsActionPressed("action");
        var targetSpeed = moveDirection * (running ? RunSpeed : WalkSpeed);

        if (IsOnFloor())
        {
            _remainingAirJumps = 1;
        }

        if (Mathf.Abs(moveDirection) > 0.01f)
        {
            Velocity = new Vector2(Mathf.MoveToward(Velocity.X, targetSpeed, Acceleration * (float)delta), Velocity.Y);
            Facing = moveDirection > 0 ? 1 : -1;
        }
        else
        {
            Velocity = new Vector2(Mathf.MoveToward(Velocity.X, 0f, Friction * (float)delta), Velocity.Y);
        }

        if (Input.IsActionJustPressed("jump"))
        {
            if (IsOnFloor())
            {
                Velocity = new Vector2(Velocity.X, JumpVelocity);
                AudioDirector.Instance.PlaySfx("jump");
            }
            else if (_remainingAirJumps > 0)
            {
                _remainingAirJumps--;
                Velocity = new Vector2(Velocity.X, JumpVelocity);
                AudioDirector.Instance.PlaySfx("jump");
            }
        }

        var wasMovingUp = Velocity.Y < 0f;
        Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * (float)delta);
        MoveAndSlide();
        SnapToGround();

        if (GameSession.Instance.CurrentForm == PlayerForm.Enhanced && Input.IsActionJustPressed("action") && _fireCooldown <= 0f)
        {
            _fireCooldown = 0.28f;
            FireRequested?.Invoke(GlobalPosition + new Vector2(Facing * 26f, -6f), Facing);
            AudioDirector.Instance.PlaySfx("fire");
        }

        if (wasMovingUp)
        {
            for (var i = 0; i < GetSlideCollisionCount(); i++)
            {
                var collision = GetSlideCollision(i);
                if (collision.GetCollider() is MysteryBlock block && collision.GetNormal().Y > 0.6f)
                {
                    BlockHit?.Invoke(block);
                }
            }
        }

        _animationTime += (float)delta;
        _fireCooldown = Mathf.Max(0f, _fireCooldown - (float)delta);
        _invulnerabilityTime = Mathf.Max(0f, _invulnerabilityTime - (float)delta);
        UpdateVisual();
    }

    public void Respawn(Vector2 spawnPoint)
    {
        GlobalPosition = spawnPoint;
        Velocity = Vector2.Zero;
        _invulnerabilityTime = 0f;
        Facing = 1;
        _animationTime = 0f;
        _remainingAirJumps = 1;
        UpdateVisual(true);
    }

    public void BounceFromStomp()
    {
        Velocity = new Vector2(Velocity.X, BounceVelocity);
    }

    public DamageResult ApplyDamage()
    {
        if (IsInvulnerable)
        {
            return DamageResult.Ignored;
        }

        var result = GameSession.Instance.ApplyDamage();
        _invulnerabilityTime = 1.15f;
        AudioDirector.Instance.PlaySfx("damage");
        UpdateVisual(true);
        return result;
    }

    private void UpdateVisual(bool forceRefresh = false)
    {
        var frames = GameAssets.GetPlayerFrames(GameSession.Instance.CurrentForm);
        var texture = _invulnerabilityTime > 0.82f ? frames.Hit : frames.Idle;

        if (_invulnerabilityTime <= 0.82f)
        {
            if (!IsOnFloor())
            {
                texture = frames.Jump;
            }
            else if (Input.IsActionPressed("move_down") && Mathf.Abs(Velocity.X) < 32f)
            {
                texture = frames.Duck;
            }
            else if (Mathf.Abs(Velocity.X) > 36f)
            {
                texture = Mathf.PosMod(Mathf.FloorToInt(_animationTime * 10f), 2) == 0 ? frames.WalkA : frames.WalkB;
            }
        }

        GameAssets.ApplyFittedSprite(_sprite, texture, new Vector2(_size.X + 10f, _size.Y + 12f), _size.Y * 0.5f, true);
        _sprite.FlipH = Facing < 0;

        if (_invulnerabilityTime > 0f && !forceRefresh)
        {
            _sprite.Visible = Mathf.FloorToInt(_invulnerabilityTime * 16f) % 2 != 0;
            return;
        }

        _sprite.Visible = true;
    }

    private void SnapToGround()
    {
        if (Velocity.Y < 0f)
        {
            return;
        }

        var query = PhysicsRayQueryParameters2D.Create(
            GlobalPosition + new Vector2(0f, -_size.Y * 0.5f + 4f),
            GlobalPosition + new Vector2(0f, _size.Y * 0.5f + GroundProbeDepth));
        query.CollisionMask = 1;
        query.Exclude = [GetRid()];

        var result = GetWorld2D().DirectSpaceState.IntersectRay(query);
        if (result.Count == 0)
        {
            return;
        }

        var normal = result["normal"].AsVector2();
        if (normal.Y < 0.55f)
        {
            return;
        }

        var supportY = result["position"].AsVector2().Y;
        var desiredY = supportY - _size.Y * 0.5f;
        if (desiredY < GlobalPosition.Y - GroundSnapAllowance || desiredY > GlobalPosition.Y + GroundProbeDepth)
        {
            return;
        }

        GlobalPosition = new Vector2(GlobalPosition.X, desiredY);
        if (Velocity.Y > 0f)
        {
            Velocity = new Vector2(Velocity.X, 0f);
        }
    }
}

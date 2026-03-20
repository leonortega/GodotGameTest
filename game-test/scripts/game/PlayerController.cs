using Godot;

namespace GameTest;

public partial class PlayerController : CharacterBody2D
{
    private const float CoyoteTimeSeconds = 0.1f;
    private const float JumpBufferSeconds = 0.15f;
    private const float JumpReleaseVelocityMultiplier = 0.5f;
    private const float LandingFeedbackThreshold = 280f;
    private const float HeavyImpactThreshold = 620f;
    private const float Gravity = 1850f;
    private const float FallGravityMultiplier = 1.25f;
    private const float MaxFallSpeed = 900f;
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
    private Tween? _feedbackTween;
    private float _invulnerabilityTime;
    private float _fireCooldown;
    private float _animationTime;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _relativeVisualVelocityX;
    private int _remainingAirJumps = 1;
    private Vector2 _feedbackScale = Vector2.One;

    public bool SimulationActive { get; set; }
    public int Facing { get; private set; } = 1;
    public Rect2 HitBox => new(GlobalPosition - _size * 0.5f, _size);
    public bool IsInvulnerable => _invulnerabilityTime > 0f;

    [Signal]
    public delegate void FireRequestedEventHandler(Vector2 origin, int facing);

    [Signal]
    public delegate void BlockHitEventHandler(MysteryBlock block);

    [Signal]
    public delegate void ImpactFeedbackRequestedEventHandler(float shakeAmount);

    [Signal]
    public delegate void DustBurstRequestedEventHandler(Vector2 worldPosition, int facing, float strength);

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
        var relativeVelocityX = Velocity.X;
        var deltaSeconds = (float)delta;
        var onFloorAtFrameStart = IsOnFloor();

        if (onFloorAtFrameStart)
        {
            _coyoteTimer = CoyoteTimeSeconds;
            _remainingAirJumps = 1;
        }
        else
        {
            _coyoteTimer = Mathf.Max(0f, _coyoteTimer - deltaSeconds);
        }

        if (Input.IsActionJustPressed("jump"))
        {
            _jumpBufferTimer = JumpBufferSeconds;
        }
        else
        {
            _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - deltaSeconds);
        }

        if (Mathf.Abs(moveDirection) > 0.01f)
        {
            relativeVelocityX = Mathf.MoveToward(relativeVelocityX, targetSpeed, Acceleration * deltaSeconds);
            Facing = moveDirection > 0 ? 1 : -1;
        }
        else
        {
            relativeVelocityX = Mathf.MoveToward(relativeVelocityX, 0f, Friction * deltaSeconds);
        }

        _relativeVisualVelocityX = relativeVelocityX;
        Velocity = new Vector2(relativeVelocityX, Velocity.Y);

        if (_jumpBufferTimer > 0f)
        {
            if (onFloorAtFrameStart || _coyoteTimer > 0f)
            {
                Velocity = new Vector2(Velocity.X, JumpVelocity);
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
                AudioDirector.Instance.PlaySfx("jump");
            }
            else if (_remainingAirJumps > 0)
            {
                _remainingAirJumps--;
                Velocity = new Vector2(Velocity.X, JumpVelocity);
                _jumpBufferTimer = 0f;
                AudioDirector.Instance.PlaySfx("jump");
            }
        }

        if (Input.IsActionJustReleased("jump") && Velocity.Y < 0f)
        {
            Velocity = new Vector2(Velocity.X, Velocity.Y * JumpReleaseVelocityMultiplier);
        }

        var wasMovingUp = Velocity.Y < 0f;
        if (!IsOnFloor() || Velocity.Y < 0f)
        {
            var gravityScale = Velocity.Y > 0f ? FallGravityMultiplier : 1f;
            var nextVerticalVelocity = Velocity.Y + Gravity * gravityScale * deltaSeconds;
            Velocity = new Vector2(Velocity.X, Mathf.Min(nextVerticalVelocity, MaxFallSpeed));
        }
        else if (Velocity.Y > 0f)
        {
            Velocity = new Vector2(Velocity.X, 0f);
        }

        var impactVelocityY = Velocity.Y;

        MoveAndSlide();
        if (IsOnCeiling() && Velocity.Y < 0f)
        {
            Velocity = new Vector2(Velocity.X, 0f);
        }

        var snappedToGround = SnapToGround();
        var groundedAfterMove = IsOnFloor() || snappedToGround;
        if (!onFloorAtFrameStart && groundedAfterMove && impactVelocityY > LandingFeedbackThreshold)
        {
            var impactStrength = Mathf.Clamp((impactVelocityY - LandingFeedbackThreshold) / (MaxFallSpeed - LandingFeedbackThreshold), 0f, 1f);
            PlayFeedbackSquash(new Vector2(1f + impactStrength * 0.22f, 1f - impactStrength * 0.18f));
            EmitSignal(SignalName.DustBurstRequested, GetFootPosition(), Facing, 0.75f + impactStrength * 0.65f);

            if (impactVelocityY >= HeavyImpactThreshold)
            {
                EmitSignal(SignalName.ImpactFeedbackRequested, 1.8f + impactStrength * 2.8f);
            }
        }

        if (GameSession.Instance.CurrentForm == PlayerForm.Enhanced && Input.IsActionJustPressed("action") && _fireCooldown <= 0f)
        {
            _fireCooldown = 0.28f;
            EmitSignal(SignalName.FireRequested, GlobalPosition + new Vector2(Facing * 26f, -6f), Facing);
            AudioDirector.Instance.PlaySfx("fire");
        }

        if (wasMovingUp)
        {
            for (var i = 0; i < GetSlideCollisionCount(); i++)
            {
                var collision = GetSlideCollision(i);
                if (collision.GetCollider() is MysteryBlock block && collision.GetNormal().Y > 0.6f)
                {
                    EmitSignal(SignalName.BlockHit, block);
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
        _coyoteTimer = 0f;
        _jumpBufferTimer = 0f;
        _relativeVisualVelocityX = 0f;
        _remainingAirJumps = 1;
        _feedbackTween?.Kill();
        _feedbackTween = null;
        _feedbackScale = Vector2.One;
        UpdateVisual(true);
    }

    public void BounceFromStomp()
    {
        Velocity = new Vector2(Velocity.X, BounceVelocity);
        PlayFeedbackSquash(new Vector2(0.9f, 1.14f));
        EmitSignal(SignalName.ImpactFeedbackRequested, 2.2f);
        EmitSignal(SignalName.DustBurstRequested, GetFootPosition(), Facing, 0.95f);
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
            else if (Input.IsActionPressed("move_down") && Mathf.Abs(_relativeVisualVelocityX) < 32f)
            {
                texture = frames.Duck;
            }
            else if (Mathf.Abs(_relativeVisualVelocityX) > 36f)
            {
                texture = Mathf.PosMod(Mathf.FloorToInt(_animationTime * 10f), 2) == 0 ? frames.WalkA : frames.WalkB;
            }
        }

        GameAssets.ApplyFittedSprite(_sprite, texture, new Vector2(_size.X + 10f, _size.Y + 12f), _size.Y * 0.5f, true);
        ApplyFeedbackTransform(_size.Y * 0.5f);
        _sprite.FlipH = Facing < 0;

        if (_invulnerabilityTime > 0f && !forceRefresh)
        {
            _sprite.Visible = Mathf.FloorToInt(_invulnerabilityTime * 16f) % 2 != 0;
            return;
        }

        _sprite.Visible = true;
    }

    private void ApplyFeedbackTransform(float bottomY)
    {
        if (_sprite.Texture is null)
        {
            return;
        }

        var textureSize = _sprite.Texture.GetSize();
        var scaled = new Vector2(_sprite.Scale.X * _feedbackScale.X, _sprite.Scale.Y * _feedbackScale.Y);
        _sprite.Scale = scaled;
        _sprite.Position = new Vector2(_sprite.Position.X, bottomY - textureSize.Y * scaled.Y * 0.5f);
    }

    private void PlayFeedbackSquash(Vector2 peakScale)
    {
        _feedbackTween?.Kill();
        _feedbackTween = CreateTween();
        _feedbackTween.SetTrans(Tween.TransitionType.Back);
        _feedbackTween.SetEase(Tween.EaseType.Out);
        _feedbackTween.TweenProperty(this, nameof(FeedbackScale), peakScale, 0.07f);
        _feedbackTween.TweenProperty(this, nameof(FeedbackScale), Vector2.One, 0.12f);
    }

    public Vector2 FeedbackScale
    {
        get => _feedbackScale;
        set => _feedbackScale = value;
    }

    private Vector2 GetFootPosition()
    {
        return GlobalPosition + new Vector2(0f, _size.Y * 0.5f - 2f);
    }

    private bool SnapToGround()
    {
        if (Velocity.Y < 0f)
        {
            return false;
        }

        var query = PhysicsRayQueryParameters2D.Create(
            GlobalPosition + new Vector2(0f, -_size.Y * 0.5f + 4f),
            GlobalPosition + new Vector2(0f, _size.Y * 0.5f + GroundProbeDepth));
        query.CollisionMask = 1;
        query.Exclude = [GetRid()];

        var result = GetWorld2D().DirectSpaceState.IntersectRay(query);
        if (result.Count == 0)
        {
            return false;
        }

        var normal = result["normal"].AsVector2();
        if (normal.Y < 0.55f)
        {
            return false;
        }

        var supportY = result["position"].AsVector2().Y;
        var desiredY = supportY - _size.Y * 0.5f;
        if (desiredY < GlobalPosition.Y - GroundSnapAllowance || desiredY > GlobalPosition.Y + GroundProbeDepth)
        {
            return false;
        }

        GlobalPosition = new Vector2(GlobalPosition.X, desiredY);
        if (Velocity.Y > 0f)
        {
            Velocity = new Vector2(Velocity.X, 0f);
        }

        return true;
    }
}

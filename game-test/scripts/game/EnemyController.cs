using System;
using Godot;

namespace GameTest;

public partial class EnemyController : CharacterBody2D
{
    private const float Gravity = 1700f;
    private const float DefeatGravity = 1500f;
    private const float GroundProbeDepth = 56f;
    private const float GroundSnapAllowance = 40f;
    private const float MaximumSupportedDrop = 38f;
    private const float MaximumSupportedRise = 56f;

    private CollisionShape2D _collision = null!;
    private RectangleShape2D _shape = null!;
    private Sprite2D _sprite = null!;
    private Vector2 _size = new(34, 34);
    private int _direction = -1;
    private float _speed = 90f;
    private Vector2 _spawnPoint;
    private float _flightTime;
    private float _patrolDistance;
    private float _animationTime;
    private float _shotCooldown;
    private float _turnLockTime;
    private bool _isDefeating;

    [Export]
    public EnemyKind AuthoredKind { get; set; } = EnemyKind.Ground;

    [Export]
    public float AuthoredPatrolDistance { get; set; }

    public EnemyKind Kind { get; private set; }
    public bool IsAlive { get; private set; } = true;
    public bool SimulationActive { get; set; }
    public Func<float, float, float, float?>? SupportTopProvider { get; set; }
    public bool IsDefeating => _isDefeating;
    public Rect2 HitBox => new(GlobalPosition - _size * 0.5f, _size);
    public static Vector2 GetCollisionSizeForKind(EnemyKind kind) => kind switch
    {
        EnemyKind.Armored => new Vector2(42, 32),
        EnemyKind.ProtectedHead => new Vector2(44, 38),
        EnemyKind.Flying => new Vector2(38, 30),
        EnemyKind.Shooter => new Vector2(34, 30),
        _ => new Vector2(34, 34)
    };

    public event Action<Vector2, int>? ShootRequested;

    public override void _Ready()
    {
        CollisionLayer = 4;
        CollisionMask = 1;
        FloorSnapLength = 8f;
        FloorMaxAngle = Mathf.DegToRad(50f);

        _shape = new RectangleShape2D();
        _collision = new CollisionShape2D { Shape = _shape };
        _sprite = new Sprite2D();
        AddChild(_sprite);
        AddChild(_collision);
    }

    public void Configure(EnemySpawn spawn)
    {
        Kind = spawn.Kind;
        _spawnPoint = spawn.Position;
        _patrolDistance = Mathf.Max(140f, spawn.PatrolDistance);
        GlobalPosition = spawn.Position;
        Visible = true;
        IsAlive = true;
        _isDefeating = false;
        _direction = -1;
        _flightTime = 0f;
        _animationTime = 0f;
        _shotCooldown = 0.75f;
        _turnLockTime = 0f;
        Velocity = Vector2.Zero;
        Rotation = 0f;
        _size = GetCollisionSizeForKind(Kind);

        switch (Kind)
        {
            case EnemyKind.Armored:
                _speed = 68f;
                break;
            case EnemyKind.ProtectedHead:
                _speed = 82f;
                break;
            case EnemyKind.Flying:
                _speed = 1f;
                break;
            case EnemyKind.Shooter:
                _speed = 0f;
                _direction = 1;
                _shotCooldown = 1.1f;
                break;
            default:
                _speed = 92f;
                break;
        }

        _shape.Size = _size;
        UpdateVisual(true);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!SimulationActive || !IsAlive)
        {
            if (SimulationActive && _isDefeating)
            {
                AdvanceDefeatAnimation((float)delta);
            }
            return;
        }

        _animationTime += (float)delta;
        _turnLockTime = Mathf.Max(0f, _turnLockTime - (float)delta);
        if (Kind == EnemyKind.Flying)
        {
            _flightTime += (float)delta;
            GlobalPosition = _spawnPoint + new Vector2(Mathf.Sin(_flightTime * 1.15f) * _patrolDistance, Mathf.Sin(_flightTime * 2.5f) * 24f);
            UpdateVisual();
            return;
        }

        if (Kind == EnemyKind.Shooter)
        {
            _shotCooldown -= (float)delta;
            Velocity = new Vector2(0f, Velocity.Y + Gravity * (float)delta);
            MoveAndSlide();
            SnapToGround();

            if (_shotCooldown <= 0f)
            {
                _shotCooldown = 1.7f;
                ShootRequested?.Invoke(GlobalPosition + new Vector2(_direction * 20f, -8f), _direction);
            }

            UpdateVisual();
            return;
        }

        if (_turnLockTime <= 0f && ShouldReverseAtPatrolLimit())
        {
            ReverseDirection();
        }

        var stepDistance = _direction * _speed * (float)delta;
        if (_turnLockTime <= 0f && !TryMoveAcrossTerrain(stepDistance))
        {
            ReverseDirection();
        }

        Velocity = new Vector2(_direction * _speed, 0f);
        SnapToGround();

        UpdateVisual();
    }

    public bool CanBeStomped() => Kind == EnemyKind.Ground;

    public ProjectileHitResult TakeProjectileHit()
    {
        if (!IsAlive)
        {
            return ProjectileHitResult.Ignored;
        }

        if (Kind == EnemyKind.Armored)
        {
            return ProjectileHitResult.Reflected;
        }

        Defeat();
        return ProjectileHitResult.Defeated;
    }

    public void ReverseDirection()
    {
        _direction *= -1;
        _turnLockTime = 0.18f;
    }

    public void FaceToward(float targetX)
    {
        _direction = targetX >= GlobalPosition.X ? 1 : -1;
    }

    public void Defeat()
    {
        if (_isDefeating || !IsAlive)
        {
            return;
        }

        IsAlive = false;
        _isDefeating = true;
        CollisionLayer = 0;
        CollisionMask = 0;
        _collision.Disabled = true;
        Velocity = new Vector2(_direction * 90f, -260f);
        _sprite.FlipV = true;
        UpdateVisual(true);
    }

    private void UpdateVisual(bool forceRefresh = false)
    {
        if (!forceRefresh && !Visible)
        {
            return;
        }

        var frames = GameAssets.GetEnemyFrames(Kind);
        Texture2D texture;
        if (Kind == EnemyKind.Flying)
        {
            texture = Mathf.PosMod(Mathf.FloorToInt(_animationTime * 8f), 3) switch
            {
                1 => frames.WalkA,
                2 => frames.WalkB,
                _ => frames.Idle
            };
        }
        else if (!IsOnFloor() && Kind == EnemyKind.Armored)
        {
            texture = frames.Alternate;
        }
        else if (Kind == EnemyKind.Shooter)
        {
            texture = _shotCooldown < 0.35f
                ? (Mathf.PosMod(Mathf.FloorToInt(_animationTime * 14f), 2) == 0 ? frames.WalkA : frames.WalkB)
                : frames.Idle;
        }
        else if (Mathf.Abs(Velocity.X) > 8f)
        {
            texture = Mathf.PosMod(Mathf.FloorToInt(_animationTime * 7f), 2) == 0 ? frames.WalkA : frames.WalkB;
        }
        else
        {
            texture = frames.Idle;
        }

        GameAssets.ApplyFittedSprite(_sprite, texture, _size + new Vector2(8f, 8f), _size.Y * 0.5f, true);
        _sprite.FlipH = _direction > 0;
        _sprite.FlipV = _isDefeating;
    }

    private bool TryMoveAcrossTerrain(float stepDistance)
    {
        var nextX = GlobalPosition.X + stepDistance;
        var bodyHalfWidth = _size.X * 0.5f - 4f;
        var frontProbeX = nextX + _direction * (_size.X * 0.3f);
        if (!TryGetGroundSupportY(frontProbeX, 2f, out var supportY))
        {
            return false;
        }

        var feetY = GlobalPosition.Y + _size.Y * 0.5f;
        var deltaY = supportY - feetY;
        if (deltaY > MaximumSupportedDrop || deltaY < -MaximumSupportedRise)
        {
            return false;
        }

        if (!TryGetGroundSupportY(nextX, bodyHalfWidth, out var bodySupportY))
        {
            bodySupportY = supportY;
        }

        GlobalPosition = new Vector2(nextX, bodySupportY - _size.Y * 0.5f);
        return true;
    }

    private bool ShouldReverseAtPatrolLimit()
    {
        if (Kind is EnemyKind.Flying or EnemyKind.Shooter)
        {
            return false;
        }

        var deltaFromSpawn = GlobalPosition.X - _spawnPoint.X;
        return deltaFromSpawn * _direction >= _patrolDistance;
    }

    private void AdvanceDefeatAnimation(float delta)
    {
        _animationTime += delta;
        Velocity = new Vector2(Velocity.X, Velocity.Y + DefeatGravity * delta);
        GlobalPosition += Velocity * delta;
        Rotation += _direction * 3.2f * delta;

        if (GlobalPosition.Y > GetViewportRect().Size.Y + 160f && GlobalPosition.Y > _spawnPoint.Y + 180f)
        {
            QueueFree();
        }
    }

    private void SnapToGround()
    {
        if (_isDefeating || Kind == EnemyKind.Flying)
        {
            return;
        }

        var halfWidth = _size.X * 0.5f - 4f;
        if (TryGetGroundSupportY(GlobalPosition.X, halfWidth, out var supportY))
        {
            var desiredY = supportY - _size.Y * 0.5f;
            if (desiredY >= GlobalPosition.Y - GroundSnapAllowance && desiredY <= GlobalPosition.Y + GroundProbeDepth)
            {
                GlobalPosition = new Vector2(GlobalPosition.X, desiredY);
                if (Velocity.Y > 0f)
                {
                    Velocity = new Vector2(Velocity.X, 0f);
                }

                return;
            }
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

        var fallbackSupportY = result["position"].AsVector2().Y;
        var fallbackDesiredY = fallbackSupportY - _size.Y * 0.5f;
        if (fallbackDesiredY < GlobalPosition.Y - GroundSnapAllowance || fallbackDesiredY > GlobalPosition.Y + GroundProbeDepth)
        {
            return;
        }

        GlobalPosition = new Vector2(GlobalPosition.X, fallbackDesiredY);
        if (Velocity.Y > 0f)
        {
            Velocity = new Vector2(Velocity.X, 0f);
        }
    }

    private bool TryGetGroundSupportY(float centerX, float halfWidth, out float supportY)
    {
        supportY = 0f;

        if (SupportTopProvider is not null)
        {
            var providedSupport = SupportTopProvider(centerX, halfWidth, GlobalPosition.Y + _size.Y * 0.5f);
            if (providedSupport.HasValue)
            {
                supportY = providedSupport.Value;
                return true;
            }
        }

        var query = PhysicsRayQueryParameters2D.Create(
            new Vector2(centerX, GlobalPosition.Y - _size.Y * 0.5f),
            new Vector2(centerX, GlobalPosition.Y + _size.Y * 0.5f + GroundProbeDepth));
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

        supportY = result["position"].AsVector2().Y;
        return true;
    }
}

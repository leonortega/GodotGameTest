using Godot;

namespace GameTest;

public partial class WorldRoot : Node2D
{
    private const float CameraVerticalOffset = 90f;
    private const float CameraLookAheadDistance = 96f;
    private const float CameraLookAheadResponsiveness = 5.5f;
    private const float CameraCatchUpResponsiveness = 7.5f;
    private const float CameraShakeDecay = 14f;

    private static readonly PackedScene PlayerScene = GD.Load<PackedScene>("res://scenes/actors/Player.tscn");
    private static readonly PackedScene GroundEnemyScene = GD.Load<PackedScene>("res://scenes/actors/GroundEnemy.tscn");
    private static readonly PackedScene ArmoredEnemyScene = GD.Load<PackedScene>("res://scenes/actors/ArmoredEnemy.tscn");
    private static readonly PackedScene FlyingEnemyScene = GD.Load<PackedScene>("res://scenes/actors/FlyingEnemy.tscn");
    private static readonly PackedScene ProtectedHeadEnemyScene = GD.Load<PackedScene>("res://scenes/actors/ProtectedHeadEnemy.tscn");
    private static readonly PackedScene ShooterEnemyScene = GD.Load<PackedScene>("res://scenes/actors/ShooterEnemy.tscn");

    private readonly List<Rect2> _solidRects = [];
    private readonly List<EnemyController> _enemies = [];
    private readonly List<PickupNode> _pickups = [];
    private readonly List<MysteryBlock> _blocks = [];
    private readonly List<ProjectileNode> _projectiles = [];
    private readonly List<EnemyProjectileNode> _enemyProjectiles = [];
    private readonly List<CactusHazard> _hazards = [];
    private readonly RandomNumberGenerator _cameraShakeRng = new();

    private Node2D _stageRoot = null!;
    private Node2D _entityRoot = null!;
    private StageScene _stage = null!;
    private PlayerController _player = null!;
    private Camera2D _camera = null!;
    private GoalMarker _goal = null!;
    private double _timerAccumulator;
    private int _stageTimeRemaining;
    private bool _transitionLocked;
    private float _cameraLookAheadX;
    private float _cameraShakeAmount;

    public bool SimulationActive { get; private set; }

    [Signal]
    public delegate void StageCompletedEventHandler();

    [Signal]
    public delegate void StageRestartRequestedEventHandler();

    [Signal]
    public delegate void GameOverEventHandler();

    public override void _Ready()
    {
        _cameraShakeRng.Randomize();
        _stageRoot = new Node2D { Name = "StageRoot" };
        _entityRoot = new Node2D { Name = "EntityRoot" };

        AddChild(_stageRoot);
        AddChild(_entityRoot);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!SimulationActive || _stage is null || _transitionLocked)
        {
            return;
        }

        _timerAccumulator += delta;
        while (_timerAccumulator >= 1.0)
        {
            _timerAccumulator -= 1.0;
            _stageTimeRemaining = Mathf.Max(0, _stageTimeRemaining - 1);
            GameSession.Instance.SetTimer(_stageTimeRemaining);

            if (_stageTimeRemaining <= 0)
            {
                HandleStageFailure();
                return;
            }
        }

        UpdateCameraOffset((float)delta);
        UpdateEnemySimulation();
        AdvanceProjectiles(delta);
        if (AdvanceEnemyProjectiles(delta))
        {
            return;
        }
        HandlePickups();
        if (HandleHazards())
        {
            return;
        }
        HandleEnemies();
        HandleGoal();

        if (_player.GlobalPosition.Y > _stage.WorldBounds.End.Y + 120f)
        {
            HandleStageFailure();
        }
    }

    public void LoadStage(string stageId)
    {
        _stage = StageCatalog.InstantiateStage(stageId);
        _transitionLocked = false;
        _timerAccumulator = 0;
        _stageTimeRemaining = _stage.TimerSeconds;
        _solidRects.Clear();
        _enemies.Clear();
        _pickups.Clear();
        _blocks.Clear();
        _projectiles.Clear();
        _enemyProjectiles.Clear();
        _hazards.Clear();

        ClearChildren(_stageRoot);
        ClearChildren(_entityRoot);

        _stageRoot.AddChild(_stage);
        _solidRects.AddRange(_stage.GetSolidRects());

        foreach (var block in _stage.GetBlocks())
        {
            block.Configure(block.Reward, block.GlobalPosition);
            _blocks.Add(block);
        }

        PopulateEnemiesForDifficulty();
        foreach (var enemy in _enemies)
        {
            enemy.SupportTopProvider = (centerX, halfWidth, objectBottomY) => _stage.GetEnemySupportTop(centerX, halfWidth, objectBottomY, 40f);
            enemy.ShootRequested += (position, facing) => SpawnEnemyProjectile(position, facing);
        }

        foreach (var pickup in _stage.GetPickups())
        {
            pickup.Configure(pickup.AuthoredPickupType, pickup.GlobalPosition);
            _pickups.Add(pickup);
        }

        foreach (var hazard in _stage.GetHazards())
        {
            _hazards.Add(hazard);
        }

        _goal = _stage.GetGoal();

        _player = PlayerScene.Instantiate<PlayerController>();
        _entityRoot.AddChild(_player);
        _player.Respawn(_stage.PlayerSpawnPosition);
        _player.BlockHit += HandleBlockHit;
        _player.FireRequested += SpawnProjectile;
        _player.ImpactFeedbackRequested += OnImpactFeedbackRequested;
        _player.DustBurstRequested += OnDustBurstRequested;

        _camera = new Camera2D
        {
            Enabled = true,
            PositionSmoothingEnabled = false,
            LimitSmoothed = true
        };
        _entityRoot.AddChild(_camera);
        _camera.MakeCurrent();
        _cameraLookAheadX = 0f;
        _cameraShakeAmount = 0f;
        UpdateCameraOffset(0f, true);

        GameSession.Instance.SetTimer(_stageTimeRemaining);
        AudioDirector.Instance.PlayMusicForTheme(_stage.Theme);
        SetSimulationActive(true);
    }

    public void SpawnPickup(PickupType pickupType, Vector2 position)
    {
        var pickup = new PickupNode();
        _stage.AddChild(pickup);
        pickup.Configure(pickupType, position);
        _pickups.Add(pickup);
    }

    public void SetSimulationActive(bool isActive)
    {
        SimulationActive = isActive;

        if (_player is not null)
        {
            _player.SimulationActive = isActive;
        }

        if (_stage is not null)
        {
            foreach (var platform in _stage.GetMovingPlatforms())
            {
                platform.SimulationActive = isActive;
            }

            foreach (var block in _stage.GetFallingBlocks())
            {
                block.SimulationActive = isActive;
            }
        }

        UpdateEnemySimulation();
    }

    private void HandlePickups()
    {
        foreach (var pickup in _pickups.ToArray())
        {
            if (!IsInstanceValid(pickup) || pickup.Collected)
            {
                _pickups.Remove(pickup);
                continue;
            }

            if (!_player.HitBox.Intersects(pickup.HitBox))
            {
                continue;
            }

            switch (pickup.PickupType)
            {
                case PickupType.Coin:
                    GameSession.Instance.AddCoin();
                    AudioDirector.Instance.PlaySfx("coin");
                    break;
                case PickupType.Growth:
                    var gainedGrowthPower = GameSession.Instance.CurrentForm == PlayerForm.Small;
                    if (GameSession.Instance.CurrentForm == PlayerForm.Small)
                    {
                        GameSession.Instance.SetForm(PlayerForm.Powered);
                    }
                    else
                    {
                        GameSession.Instance.AddScore(200);
                    }

                    AudioDirector.Instance.PlaySfx("powerup");
                    if (gainedGrowthPower)
                    {
                        AudioDirector.Instance.PlaySfx("power_gain_happy");
                    }
                    break;
                case PickupType.Flame:
                    var previousForm = GameSession.Instance.CurrentForm;
                    if (GameSession.Instance.CurrentForm == PlayerForm.Small)
                    {
                        GameSession.Instance.SetForm(PlayerForm.Powered);
                    }
                    else
                    {
                        GameSession.Instance.SetForm(PlayerForm.Enhanced);
                    }

                    AudioDirector.Instance.PlaySfx("powerup");
                    if (GameSession.Instance.CurrentForm != previousForm)
                    {
                        AudioDirector.Instance.PlaySfx("power_gain_happy");
                    }
                    break;
                case PickupType.ExtraLife:
                    GameSession.Instance.AddLife();
                    GameSession.Instance.AddScore(500);
                    AudioDirector.Instance.PlaySfx("extra_life");
                    break;
            }

            pickup.Collect();
            _pickups.Remove(pickup);
        }
    }

    private void HandleEnemies()
    {
        foreach (var enemy in _enemies.ToArray())
        {
            if (!IsInstanceValid(enemy))
            {
                _enemies.Remove(enemy);
                continue;
            }

            if (!enemy.IsAlive)
            {
                continue;
            }

            if (!_player.HitBox.Intersects(enemy.HitBox))
            {
                continue;
            }

            var stompWindow = _player.Velocity.Y > 80f && _player.HitBox.End.Y <= enemy.HitBox.Position.Y + 20f;
            if (stompWindow && enemy.CanBeStomped())
            {
                enemy.Defeat();
                _player.BounceFromStomp();
                GameSession.Instance.AddScore(200);
                AudioDirector.Instance.PlaySfx("stomp");
                continue;
            }

            var damageResult = _player.ApplyDamage();
            if (damageResult == DamageResult.GameOver)
            {
                TriggerGameOver();
                return;
            }

            if (damageResult == DamageResult.LifeLost)
            {
                HandleContactLifeLoss();
                return;
            }
        }
    }

    private void HandleGoal()
    {
        if (_goal is null || !_player.HitBox.Intersects(_goal.HitBox))
        {
            return;
        }

        _transitionLocked = true;
        SetSimulationActive(false);
        GameSession.Instance.AddScore(_stageTimeRemaining * 10);
        AudioDirector.Instance.StopMusic();
        AudioDirector.Instance.PlaySfx("goal_happy");
        EmitSignal(SignalName.StageCompleted);
    }

    private void HandleBlockHit(MysteryBlock block)
    {
        if (block.Activate(this))
        {
            AudioDirector.Instance.PlaySfx("block");
        }
    }

    private void SpawnProjectile(Vector2 position, int facing)
    {
        if (_projectiles.Count(projectile => IsInstanceValid(projectile) && !projectile.IsExpired) >= 2)
        {
            return;
        }

        var projectile = new ProjectileNode();
        _entityRoot.AddChild(projectile);
        projectile.Configure(position, facing);
        _projectiles.Add(projectile);
    }

    private void SpawnEnemyProjectile(Vector2 position, int facing, bool playSfx = true)
    {
        if (_enemyProjectiles.Count(projectile => IsInstanceValid(projectile) && !projectile.IsExpired) >= 4)
        {
            return;
        }

        var projectile = new EnemyProjectileNode();
        _entityRoot.AddChild(projectile);
        projectile.Configure(position, facing);
        _enemyProjectiles.Add(projectile);
        if (playSfx)
        {
            AudioDirector.Instance.PlaySfx("fire");
        }
    }

    private void AdvanceProjectiles(double delta)
    {
        foreach (var projectile in _projectiles.ToArray())
        {
            if (!IsInstanceValid(projectile) || projectile.IsExpired)
            {
                _projectiles.Remove(projectile);
                continue;
            }

            projectile.Advance(delta);

            if (projectile.GlobalPosition.X < _stage.WorldBounds.Position.X - 40 || projectile.GlobalPosition.X > _stage.WorldBounds.End.X + 40)
            {
                projectile.Expire();
                _projectiles.Remove(projectile);
                continue;
            }

            if (_solidRects.Any(rect => rect.Intersects(projectile.HitBox)) || _blocks.Any(block => IsInstanceValid(block) && block.HitBox.Intersects(projectile.HitBox)))
            {
                projectile.Expire();
                _projectiles.Remove(projectile);
                continue;
            }

            foreach (var enemy in _enemies.ToArray())
            {
                if (!IsInstanceValid(enemy))
                {
                    _enemies.Remove(enemy);
                    continue;
                }

                if (!enemy.IsAlive)
                {
                    continue;
                }

                if (!enemy.HitBox.Intersects(projectile.HitBox))
                {
                    continue;
                }

                var hitResult = enemy.TakeProjectileHit();
                if (hitResult == ProjectileHitResult.Defeated)
                {
                    GameSession.Instance.AddScore(250);
                    AudioDirector.Instance.PlaySfx("enemy_down");
                }
                else if (hitResult == ProjectileHitResult.Reflected)
                {
                    var reflectedFacing = -projectile.Facing;
                    SpawnEnemyProjectile(enemy.GlobalPosition + new Vector2(reflectedFacing * 20f, -8f), reflectedFacing, false);
                    AudioDirector.Instance.PlaySfx("block");
                }

                projectile.Expire();
                _projectiles.Remove(projectile);
                break;
            }
        }
    }

    private bool AdvanceEnemyProjectiles(double delta)
    {
        foreach (var projectile in _enemyProjectiles.ToArray())
        {
            if (!IsInstanceValid(projectile) || projectile.IsExpired)
            {
                _enemyProjectiles.Remove(projectile);
                continue;
            }

            projectile.Advance(delta);

            if (projectile.GlobalPosition.X < _stage.WorldBounds.Position.X - 40f || projectile.GlobalPosition.X > _stage.WorldBounds.End.X + 40f)
            {
                projectile.Expire();
                _enemyProjectiles.Remove(projectile);
                continue;
            }

            if (_solidRects.Any(rect => rect.Intersects(projectile.HitBox)) || _blocks.Any(block => IsInstanceValid(block) && block.HitBox.Intersects(projectile.HitBox)))
            {
                projectile.Expire();
                _enemyProjectiles.Remove(projectile);
                continue;
            }

            if (!_player.HitBox.Intersects(projectile.HitBox))
            {
                continue;
            }

            projectile.Expire();
            _enemyProjectiles.Remove(projectile);

            var damageResult = _player.ApplyDamage();
            if (damageResult == DamageResult.GameOver)
            {
                TriggerGameOver();
                return true;
            }

            if (damageResult == DamageResult.LifeLost)
            {
                HandleContactLifeLoss();
                return true;
            }
        }

        return false;
    }

    private bool HandleHazards()
    {
        foreach (var hazard in _hazards)
        {
            if (!IsInstanceValid(hazard) || !_player.HitBox.Intersects(hazard.HurtBox))
            {
                continue;
            }

            var damageResult = _player.ApplyDamage();
            if (damageResult == DamageResult.GameOver)
            {
                TriggerGameOver();
                return true;
            }

            if (damageResult == DamageResult.LifeLost)
            {
                HandleContactLifeLoss();
                return true;
            }
        }

        return false;
    }

    private async void HandleStageFailure()
    {
        if (_transitionLocked)
        {
            return;
        }

        _transitionLocked = true;
        SetSimulationActive(false);

        var gameOver = GameSession.Instance.LoseLife();
        AudioDirector.Instance.PlaySfx("life_lost");

        await ToSignal(GetTree().CreateTimer(0.8), SceneTreeTimer.SignalName.Timeout);

        if (gameOver)
        {
            TriggerGameOver();
            return;
        }

        EmitSignal(SignalName.StageRestartRequested);
    }

    private async void HandleContactLifeLoss()
    {
        if (_transitionLocked)
        {
            return;
        }

        _transitionLocked = true;
        SetSimulationActive(false);
        AudioDirector.Instance.PlaySfx("life_lost");

        await ToSignal(GetTree().CreateTimer(0.6), SceneTreeTimer.SignalName.Timeout);

        EmitSignal(SignalName.StageRestartRequested);
    }

    private void TriggerGameOver()
    {
        SetSimulationActive(false);
        EmitSignal(SignalName.GameOver);
    }

    private void UpdateCameraOffset(float deltaSeconds, bool snap = false)
    {
        if (_camera is null || _player is null)
        {
            return;
        }

        var viewportSize = GetViewportRect().Size * _camera.Zoom;
        var halfViewport = viewportSize * 0.5f;
        var bounds = _stage.WorldBounds;

        var minCenterX = bounds.Position.X + halfViewport.X;
        var maxCenterX = bounds.End.X - halfViewport.X;
        var minCenterY = bounds.Position.Y + halfViewport.Y;
        var maxCenterY = bounds.End.Y - halfViewport.Y;

        if (minCenterX > maxCenterX)
        {
            minCenterX = maxCenterX = bounds.GetCenter().X;
        }

        if (minCenterY > maxCenterY)
        {
            minCenterY = maxCenterY = bounds.GetCenter().Y;
        }

        var lookAheadTargetX = 0f;
        if (Mathf.Abs(_player.Velocity.X) > 24f)
        {
            lookAheadTargetX = Mathf.Sign(_player.Velocity.X) * CameraLookAheadDistance;
        }
        else if (_player.Facing != 0)
        {
            lookAheadTargetX = _player.Facing * (CameraLookAheadDistance * 0.35f);
        }

        if (snap || deltaSeconds <= 0f)
        {
            _cameraLookAheadX = lookAheadTargetX;
        }
        else
        {
            var lookAheadLerpWeight = 1f - Mathf.Exp(-CameraLookAheadResponsiveness * deltaSeconds);
            _cameraLookAheadX = Mathf.Lerp(_cameraLookAheadX, lookAheadTargetX, lookAheadLerpWeight);
        }

        var desiredCenter = _player.GlobalPosition + new Vector2(_cameraLookAheadX, CameraVerticalOffset);
        var clampedCenter = new Vector2(
            Mathf.Clamp(desiredCenter.X, minCenterX, maxCenterX),
            Mathf.Clamp(desiredCenter.Y, minCenterY, maxCenterY));

        if (snap || deltaSeconds <= 0f)
        {
            _camera.GlobalPosition = clampedCenter;
            _camera.Offset = Vector2.Zero;
            return;
        }

        var cameraLerpWeight = 1f - Mathf.Exp(-CameraCatchUpResponsiveness * deltaSeconds);
        _camera.GlobalPosition = _camera.GlobalPosition.Lerp(clampedCenter, cameraLerpWeight);

        if (_cameraShakeAmount > 0f)
        {
            _camera.Offset = new Vector2(
                _cameraShakeRng.RandfRange(-_cameraShakeAmount, _cameraShakeAmount),
                _cameraShakeRng.RandfRange(-_cameraShakeAmount * 0.75f, _cameraShakeAmount * 0.75f));
            _cameraShakeAmount = Mathf.Max(0f, _cameraShakeAmount - CameraShakeDecay * deltaSeconds);
        }
        else
        {
            _camera.Offset = Vector2.Zero;
        }
    }

    private void OnImpactFeedbackRequested(float shakeAmount)
    {
        _cameraShakeAmount = Mathf.Max(_cameraShakeAmount, shakeAmount);
    }

    private void OnDustBurstRequested(Vector2 worldPosition, int facing, float strength)
    {
        var dustPuff = new DustPuff();
        _entityRoot.AddChild(dustPuff);
        dustPuff.Configure(worldPosition, facing, strength);
    }

    private void PopulateEnemiesForDifficulty()
    {
        var authoredEnemies = _stage.GetEnemies().ToList();
        var difficulty = GameSession.Instance.CurrentDifficulty;

        for (var index = 0; index < authoredEnemies.Count; index++)
        {
            var enemy = authoredEnemies[index];
            if (difficulty == DifficultyLevel.Easy && authoredEnemies.Count > 2 && index % 3 == 2)
            {
                enemy.QueueFree();
                continue;
            }

            enemy.Configure(new EnemySpawn(enemy.AuthoredKind, enemy.GlobalPosition, enemy.AuthoredPatrolDistance));
            _enemies.Add(enemy);
        }

        if (difficulty != DifficultyLevel.Hard)
        {
            return;
        }

        var bonusSources = authoredEnemies
            .Where(enemy => IsInstanceValid(enemy) && enemy.AuthoredKind != EnemyKind.Flying)
            .Take(2)
            .ToArray();

        for (var index = 0; index < bonusSources.Length; index++)
        {
            var source = bonusSources[index];
            var clone = InstantiateEnemyScene(source.AuthoredKind);
            clone.Name = $"{source.Name}_Hard_{index + 1}";
            clone.AuthoredPatrolDistance = source.AuthoredPatrolDistance;

            source.GetParent().AddChild(clone);
            var spawnPosition = source.AuthoredKind == EnemyKind.Flying
                ? source.GlobalPosition
                : FindHardCloneSpawnPosition(source, index);
            clone.Configure(new EnemySpawn(source.AuthoredKind, spawnPosition, Mathf.Max(140f, source.AuthoredPatrolDistance)));
            _enemies.Add(clone);
        }
    }

    private Vector2 FindHardCloneSpawnPosition(EnemyController source, int cloneIndex)
    {
        var preferredOffset = cloneIndex % 2 == 0 ? 96f : -96f;
        var collisionSize = EnemyController.GetCollisionSizeForKind(source.AuthoredKind);
        var supportHalfWidth = collisionSize.X * 0.5f - 4f;
        var minX = _stage.WorldBounds.Position.X + 64f;
        var maxX = _stage.WorldBounds.End.X - 64f;
        var candidateOffsets = new[]
        {
            preferredOffset,
            -preferredOffset,
            preferredOffset * 1.5f,
            -preferredOffset * 1.5f,
            0f
        };

        foreach (var offset in candidateOffsets)
        {
            var candidateX = Mathf.Clamp(source.GlobalPosition.X + offset, minX, maxX);
            var supportTop = _stage.GetEnemySupportTop(candidateX, supportHalfWidth, source.GlobalPosition.Y + collisionSize.Y * 0.5f, 40f);
            if (!supportTop.HasValue)
            {
                continue;
            }

            return new Vector2(candidateX, supportTop.Value - collisionSize.Y * 0.5f);
        }

        return source.GlobalPosition;
    }

    private static EnemyController InstantiateEnemyScene(EnemyKind kind)
    {
        var scene = kind switch
        {
            EnemyKind.Armored => ArmoredEnemyScene,
            EnemyKind.Flying => FlyingEnemyScene,
            EnemyKind.ProtectedHead => ProtectedHeadEnemyScene,
            EnemyKind.Shooter => ShooterEnemyScene,
            _ => GroundEnemyScene
        };

        return scene.Instantiate<EnemyController>();
    }

    private void UpdateEnemySimulation()
    {
        if (_enemies.Count == 0)
        {
            return;
        }

        if (!SimulationActive || _camera is null || _player is null)
        {
            foreach (var enemy in _enemies)
            {
                if (IsInstanceValid(enemy))
                {
                    enemy.SimulationActive = false;
                }
            }

            return;
        }

        var activeCap = GameSession.Instance.GetMaxActiveEnemiesOnScreen();
        var viewportSize = GetViewportRect().Size * _camera.Zoom;
        var expandedView = new Rect2(_camera.GlobalPosition - viewportSize * 0.5f, viewportSize).GrowIndividual(220f, 120f, 220f, 120f);

        var activeEnemies = _enemies
            .Where(enemy => IsInstanceValid(enemy) && enemy.IsAlive && expandedView.Intersects(enemy.HitBox))
            .OrderBy(enemy => enemy.GlobalPosition.DistanceSquaredTo(_player.GlobalPosition))
            .Take(activeCap)
            .ToHashSet();

        foreach (var enemy in _enemies)
        {
            if (!IsInstanceValid(enemy))
            {
                continue;
            }

            if (enemy.Kind == EnemyKind.Shooter && enemy.IsAlive)
            {
                enemy.FaceToward(_player.GlobalPosition.X);
            }

            enemy.SimulationActive = enemy.IsDefeating || activeEnemies.Contains(enemy);
        }
    }

    private static void ClearChildren(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            child.QueueFree();
        }
    }
}

using Godot;

namespace GameTest;

public partial class WorldRoot : Node2D
{
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

    private Node2D _stageRoot = null!;
    private Node2D _entityRoot = null!;
    private StageScene _stage = null!;
    private PlayerController _player = null!;
    private Camera2D _camera = null!;
    private GoalMarker _goal = null!;
    private double _timerAccumulator;
    private int _stageTimeRemaining;
    private bool _transitionLocked;

    public bool SimulationActive { get; private set; }

    public event Action? StageCompleted;
    public event Action? StageRestartRequested;
    public event Action? GameOver;

    public override void _Ready()
    {
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

        UpdateCameraOffset();
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

        _camera = new Camera2D
        {
            Enabled = true,
            PositionSmoothingEnabled = false
        };
        _entityRoot.AddChild(_camera);
        _camera.MakeCurrent();
        UpdateCameraOffset();

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
                    if (GameSession.Instance.CurrentForm == PlayerForm.Small)
                    {
                        GameSession.Instance.SetForm(PlayerForm.Powered);
                    }
                    else
                    {
                        GameSession.Instance.AddScore(200);
                    }

                    AudioDirector.Instance.PlaySfx("powerup");
                    break;
                case PickupType.Flame:
                    if (GameSession.Instance.CurrentForm == PlayerForm.Small)
                    {
                        GameSession.Instance.SetForm(PlayerForm.Powered);
                    }
                    else
                    {
                        GameSession.Instance.SetForm(PlayerForm.Enhanced);
                    }

                    AudioDirector.Instance.PlaySfx("powerup");
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
        AudioDirector.Instance.PlaySfx("clear");
        StageCompleted?.Invoke();
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

        StageRestartRequested?.Invoke();
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

        StageRestartRequested?.Invoke();
    }

    private void TriggerGameOver()
    {
        SetSimulationActive(false);
        GameOver?.Invoke();
    }

    private void UpdateCameraOffset()
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

        var desiredCenter = _player.GlobalPosition + new Vector2(0f, 90f);
        var clampedCenter = new Vector2(
            Mathf.Clamp(desiredCenter.X, minCenterX, maxCenterX),
            Mathf.Clamp(desiredCenter.Y, minCenterY, maxCenterY));

        _camera.GlobalPosition = clampedCenter;
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
            var offsetX = index % 2 == 0 ? 96f : -96f;
            var desiredPosition = new Vector2(
                Mathf.Clamp(source.GlobalPosition.X + offsetX, _stage.WorldBounds.Position.X + 64f, _stage.WorldBounds.End.X - 64f),
                source.GlobalPosition.Y);
            var spawnPosition = source.AuthoredKind == EnemyKind.Flying
                ? desiredPosition
                : _stage.SnapEnemyPosition(desiredPosition, source.AuthoredKind);
            clone.Configure(new EnemySpawn(source.AuthoredKind, spawnPosition, Mathf.Max(140f, source.AuthoredPatrolDistance)));
            _enemies.Add(clone);
        }
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

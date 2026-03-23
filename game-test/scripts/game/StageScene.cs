using System.Linq;
using Godot;

namespace GameTest;

public partial class StageScene : Node2D
{
    private const int TilePixels = 32;
    private const float BlockHalfSize = 20f;
    private const float MinimumBlockClearance = 52f;
    private const float MaximumReachablePlatformRise = 160f;
    private const float GoalSupportHalfWidth = 16f;
    private const float HazardSupportHalfWidth = 18f;
    private const float SupportSnapTolerance = 56f;

    [Export]
    public string StageId { get; set; } = string.Empty;

    [Export]
    public StageTheme Theme { get; set; } = StageTheme.Grassland;

    [Export]
    public int TimerSeconds { get; set; } = 300;

    [Export]
    public Rect2 WorldBounds { get; set; } = new(0, 0, 1280, 720);

    [Export]
    public Godot.Collections.Array<Rect2I> SolidRectTiles { get; set; } = [];

    [Export]
    public Godot.Collections.Array<Vector4I> SlopeSegments { get; set; } = [];

    private StageBackdrop _backdrop = null!;
    private TileMapLayer _terrainLayer = null!;
    private TileMapLayer _decorLayer = null!;
    private Node2D _tileVisualRoot = null!;
    private Node? _terrainHillsRoot;
    private Marker2D _playerSpawn = null!;
    private Node2D _collisionRoot = null!;
    private Node _pickupsRoot = null!;
    private Node _blocksRoot = null!;
    private Node _enemiesRoot = null!;
    private Node? _dynamicsRoot;
    private Node? _hazardsRoot;
    private GoalMarker _goal = null!;

    public Vector2 PlayerSpawnPosition
    {
        get
        {
            EnsureReferences();
            return _playerSpawn.GlobalPosition;
        }
    }

    public override void _Ready()
    {
        EnsureReferences();
        _backdrop.Configure(Theme, WorldBounds);
        PrepareTerrainPresentation();
        RebuildCollisionBodies();
        NormalizeEnemyPlacement();
        NormalizeBlockPlacement();
        NormalizeHazardPlacement();
        NormalizeGoalPlacement();
    }

    public IEnumerable<Rect2> GetSolidRects()
    {
        EnsureReferences();
        foreach (var rect in SolidRectTiles)
        {
            yield return new Rect2(rect.Position * TilePixels, rect.Size * TilePixels);
        }

        foreach (var slope in SlopeSegments)
        {
            if (!TryGetSlopeInfo(slope, out var info))
            {
                continue;
            }

            yield return info.Bounds;
        }

        foreach (var hill in GetAuthoredHills())
        {
            foreach (var rect in hill.GetSolidRects())
            {
                yield return rect;
            }
        }
    }

    public IEnumerable<PickupNode> GetPickups()
    {
        EnsureReferences();
        foreach (Node child in _pickupsRoot.GetChildren())
        {
            if (child is PickupNode pickup)
            {
                yield return pickup;
            }
        }
    }

    public IEnumerable<MysteryBlock> GetBlocks()
    {
        EnsureReferences();
        foreach (Node child in _blocksRoot.GetChildren())
        {
            if (child is MysteryBlock block)
            {
                yield return block;
            }
        }
    }

    public IEnumerable<EnemyController> GetEnemies()
    {
        EnsureReferences();
        foreach (Node child in _enemiesRoot.GetChildren())
        {
            if (child is EnemyController enemy)
            {
                yield return enemy;
            }
        }
    }

    public IEnumerable<CactusHazard> GetHazards()
    {
        EnsureReferences();
        if (_hazardsRoot is null)
        {
            yield break;
        }

        foreach (Node child in _hazardsRoot.GetChildren())
        {
            if (child is CactusHazard hazard)
            {
                yield return hazard;
            }
        }
    }

    public IEnumerable<FloatingMovingPlatform> GetMovingPlatforms()
    {
        EnsureReferences();
        if (_dynamicsRoot is null)
        {
            yield break;
        }

        foreach (Node child in _dynamicsRoot.GetChildren())
        {
            if (child is FloatingMovingPlatform platform)
            {
                yield return platform;
            }
        }
    }

    public IEnumerable<FallingBlock> GetFallingBlocks()
    {
        EnsureReferences();
        if (_dynamicsRoot is null)
        {
            yield break;
        }

        foreach (Node child in _dynamicsRoot.GetChildren())
        {
            if (child is FallingBlock block)
            {
                yield return block;
            }
        }
    }

    public GoalMarker GetGoal()
    {
        EnsureReferences();
        return _goal;
    }

    public Vector2 SnapEnemyPosition(Vector2 desiredPosition, EnemyKind kind)
    {
        var collisionSize = EnemyController.GetCollisionSizeForKind(kind);
        var supportHalfWidth = collisionSize.X * 0.5f - 4f;
        var supportTop = FindNearestSupportTop(
            desiredPosition.X - supportHalfWidth,
            desiredPosition.X + supportHalfWidth,
            desiredPosition.Y + collisionSize.Y * 0.5f,
            includeMovingPlatforms: false,
            upwardTolerance: SupportSnapTolerance);

        if (supportTop is null)
        {
            return desiredPosition;
        }

        return new Vector2(desiredPosition.X, supportTop.Value - collisionSize.Y * 0.5f);
    }

    public float? GetEnemySupportTop(float centerX, float halfWidth, float objectBottomY, float upwardTolerance = 0f)
    {
        return FindNearestSupportTop(
            centerX - halfWidth,
            centerX + halfWidth,
            objectBottomY,
            includeMovingPlatforms: false,
            upwardTolerance: upwardTolerance);
    }

    private void EnsureReferences()
    {
        if (_terrainLayer is not null)
        {
            return;
        }

        _backdrop = GetNode<StageBackdrop>("Backdrop");
        _terrainLayer = GetNode<TileMapLayer>("TerrainLayer");
        _decorLayer = GetNode<TileMapLayer>("DecorLayer");
        _tileVisualRoot = GetNodeOrNull<Node2D>("TileVisualRoot") ?? new Node2D { Name = "TileVisualRoot" };
        _terrainHillsRoot = GetNodeOrNull("TerrainHills");
        _playerSpawn = GetNode<Marker2D>("Markers/PlayerSpawn");
        _collisionRoot = GetNodeOrNull<Node2D>("CollisionRoot") ?? new Node2D { Name = "CollisionRoot" };
        _pickupsRoot = GetNode("Gameplay/Pickups");
        _blocksRoot = GetNode("Gameplay/Blocks");
        _enemiesRoot = GetNode("Gameplay/Enemies");
        _dynamicsRoot = GetNodeOrNull("Gameplay/Dynamics");
        _hazardsRoot = GetNodeOrNull("Gameplay/Hazards");
        _goal = GetNode<GoalMarker>("Gameplay/Goal");

        if (_collisionRoot.GetParent() is null)
        {
            AddChild(_collisionRoot);
        }

        if (_tileVisualRoot.GetParent() is null)
        {
            AddChild(_tileVisualRoot);
            MoveChild(_tileVisualRoot, _decorLayer.GetIndex() + 1);
        }
    }

    private void PrepareTerrainPresentation()
    {
        _terrainLayer.Clear();
        _decorLayer.Clear();
        _terrainLayer.Visible = false;
        _decorLayer.Visible = false;
        ClearGeneratedVisuals();
        BuildTerrainVisuals();
        BuildFullSlopeOverlays();
    }

    private void ClearGeneratedVisuals()
    {
        foreach (Node child in _tileVisualRoot.GetChildren())
        {
            _tileVisualRoot.RemoveChild(child);
            child.Free();
        }

        var rootChildren = GetChildren().Cast<Node>().ToArray();
        foreach (var child in rootChildren)
        {
            if (child == _tileVisualRoot)
            {
                continue;
            }

            if (child.Name.ToString().StartsWith("FullSlope_") || child.IsInGroup("full_slope"))
            {
                RemoveChild(child);
                child.Free();
            }
        }
    }

    private void BuildTerrainVisuals()
    {
        foreach (Node child in _tileVisualRoot.GetChildren())
        {
            child.QueueFree();
        }

        var occupied = new HashSet<Vector2I>();
        foreach (var rect in SolidRectTiles)
        {
            for (var x = rect.Position.X; x < rect.End.X; x++)
            {
                for (var y = rect.Position.Y; y < rect.End.Y; y++)
                {
                    occupied.Add(new Vector2I(x, y));
                }
            }
        }

        var authoredHillCells = CollectAuthoredHillCells();
        var connectedOccupied = new HashSet<Vector2I>(occupied);
        connectedOccupied.UnionWith(authoredHillCells);

        var slopeCells = CollectSlopeCells();
        var fullSlopePairs = CollectFullSlopePairs();
        var fullSlopeOverlayCells = CollectFullSlopeOverlayCells(fullSlopePairs);
        foreach (var cell in occupied)
        {
            if (fullSlopeOverlayCells.Contains(cell))
            {
                continue;
            }

            var sprite = new Sprite2D
            {
                Texture = GameAssets.GetTerrainTexture(Theme, ResolveTerrainVisual(connectedOccupied, slopeCells, cell)),
                Centered = true,
                TextureFilter = TextureFilterEnum.Nearest,
                Position = cell * TilePixels + new Vector2(TilePixels * 0.5f, TilePixels * 0.5f),
                Scale = Vector2.One * 0.5f
            };
            _tileVisualRoot.AddChild(sprite);
        }

        BuildSlopeVisuals(fullSlopePairs);
        BuildElevatedSurfaceOverlays(occupied, fullSlopeOverlayCells);
    }

    private HashSet<Vector2I> CollectAuthoredHillCells()
    {
        var occupied = new HashSet<Vector2I>();
        foreach (var hill in GetAuthoredHills())
        {
            foreach (var cell in hill.GetOccupiedCells())
            {
                occupied.Add(cell);
            }
        }

        return occupied;
    }

    private void BuildSlopeVisuals(IReadOnlyList<FullSlopePair> fullSlopePairs)
    {
        var pairedSlopeCells = new HashSet<Vector2I>();
        foreach (var pair in fullSlopePairs)
        {
            foreach (var cell in pair.Left.Cells)
            {
                pairedSlopeCells.Add(cell);
            }

            foreach (var cell in pair.Right.Cells)
            {
                pairedSlopeCells.Add(cell);
            }
        }

        foreach (var slope in SlopeSegments)
        {
            if (!TryGetSlopeInfo(slope, out var info))
            {
                continue;
            }

            if (info.Cells.All(pairedSlopeCells.Contains))
            {
                continue;
            }

            foreach (var cell in info.Cells)
            {
                for (var fillY = cell.Y + 1; fillY < info.BaseTopTileY; fillY++)
                {
                    AddTerrainSprite(
                        new Vector2I(cell.X, fillY),
                        GameAssets.GetTerrainTexture(Theme, TerrainVisualKind.Center));
                }
            }

            AddSlopeRampRun(info.Cells, info.Direction < 0);
        }
    }

    private void BuildFullSlopeOverlays()
    {
        var fullSlopePairs = CollectFullSlopePairs();
        for (var index = 0; index < fullSlopePairs.Count; index++)
        {
            AddFullSlopeOverlay(index + 1, fullSlopePairs[index].Left, fullSlopePairs[index].Right);
        }
    }

    private void AddFullSlopeOverlay(int index, SlopeInfo leftSlope, SlopeInfo rightSlope)
    {
        var topY = leftSlope.Cells.Min(cell => cell.Y);
        var plateauStartX = leftSlope.Cells.Max(cell => cell.X) + 1;
        var plateauEndX = rightSlope.Cells.Min(cell => cell.X) - 1;
        var isPrimaryStageHill = StageId == "1-1" && index == 1;
        var origin = new Vector2(
            leftSlope.Cells[0].X * TilePixels + TilePixels * 0.5f,
            topY * TilePixels + TilePixels * 0.5f);

        var root = new Node2D
        {
            Name = $"FullSlope_{index:00}",
            Position = origin
        };
        root.AddToGroup("full_slope", true);
        AddOwnedChild(this, root);

        AddOverlayTopRun(root, plateauStartX, plateauEndX, topY, origin, isPrimaryStageHill);

        var leftByY = leftSlope.Cells.ToDictionary(cell => cell.Y, cell => cell.X);
        var rightByY = rightSlope.Cells.ToDictionary(cell => cell.Y, cell => cell.X);
        for (var y = topY + 1; y < leftSlope.BaseTopTileY; y++)
        {
            if (!leftByY.TryGetValue(y, out var leftFillX) || !rightByY.TryGetValue(y, out var rightFillX))
            {
                continue;
            }

            for (var x = leftFillX; x <= rightFillX; x++)
            {
                var texture = GameAssets.GetTerrainTexture(Theme, TerrainVisualKind.Center);

                AddOverlaySprite(
                    root,
                    $"Fill_{x}_{y}",
                    new Vector2(
                        x * TilePixels + TilePixels * 0.5f - origin.X,
                        y * TilePixels + TilePixels * 0.5f - origin.Y),
                    texture);
            }
        }

        AddOverlaySlopeRun(root, leftSlope.Cells, mirrorHorizontally: true);
        AddOverlaySlopeRun(root, rightSlope.Cells, mirrorHorizontally: false);
    }

    private void BuildElevatedSurfaceOverlays(HashSet<Vector2I> occupied, HashSet<Vector2I> fullSlopeOverlayCells)
    {
        var slopeCells = CollectSlopeCells();
        var topCells = occupied
            .Where(cell => !occupied.Contains(cell + Vector2I.Up) && !slopeCells.Contains(cell) && !fullSlopeOverlayCells.Contains(cell))
            .ToList();
        if (topCells.Count == 0)
        {
            return;
        }

        var groundTopY = topCells.Max(cell => cell.Y);
        var elevatedTopCells = topCells
            .Where(cell => cell.Y < groundTopY)
            .OrderBy(cell => cell.Y)
            .ThenBy(cell => cell.X)
            .ToList();

        foreach (var group in elevatedTopCells.GroupBy(cell => cell.Y))
        {
            var run = new List<Vector2I>();
            Vector2I? previous = null;
            foreach (var cell in group)
            {
                if (previous is not null && cell.X != previous.Value.X + 1)
                {
                    AddHorizontalSurfaceRun(run, slopeCells);
                    run.Clear();
                }

                run.Add(cell);
                previous = cell;
            }

            AddHorizontalSurfaceRun(run, slopeCells);
        }
    }

    private void RebuildCollisionBodies()
    {
        foreach (Node child in _collisionRoot.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var rect in SolidRectTiles)
        {
            var pixelRect = new Rect2(rect.Position * TilePixels, rect.Size * TilePixels);
            var body = new StaticBody2D
            {
                CollisionLayer = 1,
                CollisionMask = 0,
                Position = pixelRect.Position + pixelRect.Size * 0.5f
            };

            var collision = new CollisionShape2D
            {
                Shape = new RectangleShape2D
                {
                    Size = pixelRect.Size
                }
            };

            body.AddChild(collision);
            _collisionRoot.AddChild(body);
        }

        foreach (var slope in SlopeSegments)
        {
            if (!TryGetSlopeInfo(slope, out var info))
            {
                continue;
            }

            var body = new StaticBody2D
            {
                CollisionLayer = 1,
                CollisionMask = 0
            };

            var collision = new CollisionPolygon2D
            {
                Polygon = info.CollisionPolygon
            };

            body.AddChild(collision);
            _collisionRoot.AddChild(body);
        }
    }

    private void NormalizeBlockPlacement()
    {
        foreach (var block in GetBlocks())
        {
            var supportTop = FindNearestSupportTop(
                block.Position.X - BlockHalfSize + 4f,
                block.Position.X + BlockHalfSize - 4f,
                block.Position.Y + BlockHalfSize);
            if (supportTop is null)
            {
                continue;
            }

            var blockBottom = block.Position.Y + BlockHalfSize;
            var clearance = supportTop.Value - blockBottom;
            if (clearance >= MinimumBlockClearance)
            {
                continue;
            }

            block.Position = new Vector2(
                block.Position.X,
                supportTop.Value - MinimumBlockClearance - BlockHalfSize);
        }
    }

    private void NormalizeEnemyPlacement()
    {
        foreach (var enemy in GetEnemies())
        {
            if (enemy.AuthoredKind == EnemyKind.Flying)
            {
                continue;
            }
            enemy.Position = SnapEnemyPosition(enemy.Position, enemy.AuthoredKind);
        }
    }

    private void NormalizeHazardPlacement()
    {
        foreach (var hazard in GetHazards())
        {
            var supportTop = FindNearestSupportTop(
                hazard.Position.X - HazardSupportHalfWidth,
                hazard.Position.X + HazardSupportHalfWidth,
                hazard.Position.Y,
                upwardTolerance: SupportSnapTolerance);

            if (supportTop is null)
            {
                hazard.Deactivate();
                continue;
            }

            hazard.Position = new Vector2(
                hazard.Position.X,
                supportTop.Value - hazard.GroundContactOffset);
        }
    }

    private void NormalizeReachablePlatforms()
    {
        if (SolidRectTiles.Count == 0)
        {
            return;
        }

        var adjustedRects = new Godot.Collections.Array<Rect2I>(SolidRectTiles);
        for (var index = 0; index < adjustedRects.Count; index++)
        {
            var rect = adjustedRects[index];
            if (rect.Size.Y != 1)
            {
                continue;
            }

            var pixelRect = new Rect2(rect.Position * TilePixels, rect.Size * TilePixels);
            var supportTop = FindNearestSupportTop(
                pixelRect.Position.X + 4f,
                pixelRect.End.X - 4f,
                pixelRect.End.Y,
                rect);
            if (supportTop is null)
            {
                continue;
            }

            var platformRise = supportTop.Value - pixelRect.Position.Y;
            if (platformRise <= MaximumReachablePlatformRise)
            {
                continue;
            }

            var desiredTop = supportTop.Value - MaximumReachablePlatformRise;
            adjustedRects[index] = new Rect2I(
                rect.Position.X,
                Mathf.Max(0, Mathf.RoundToInt(desiredTop / TilePixels)),
                rect.Size.X,
                rect.Size.Y);
        }

        SolidRectTiles = adjustedRects;
    }

    private void NormalizeGoalPlacement()
    {
        var supportTop = FindNearestSupportTop(
            _goal.Position.X - GoalSupportHalfWidth,
            _goal.Position.X + GoalSupportHalfWidth,
            _goal.Position.Y,
            upwardTolerance: SupportSnapTolerance);
        if (supportTop is null)
        {
            return;
        }

        _goal.Position = new Vector2(_goal.Position.X, supportTop.Value);
    }

    private float? FindNearestSupportTop(float overlapMinX, float overlapMaxX, float objectBottomY, Rect2I? excludeRect = null, bool includeMovingPlatforms = true, float upwardTolerance = 0f)
    {
        float? nearestTop = null;
        var sampleX = (overlapMinX + overlapMaxX) * 0.5f;

        foreach (var rect in SolidRectTiles)
        {
            if (excludeRect.HasValue && rect == excludeRect.Value)
            {
                continue;
            }

            var pixelRect = new Rect2(rect.Position * TilePixels, rect.Size * TilePixels);
            if (pixelRect.End.X <= overlapMinX || pixelRect.Position.X >= overlapMaxX)
            {
                continue;
            }

            if (pixelRect.Position.Y < objectBottomY - upwardTolerance)
            {
                continue;
            }

            if (nearestTop is null || pixelRect.Position.Y < nearestTop.Value)
            {
                nearestTop = pixelRect.Position.Y;
            }
        }

        foreach (var slope in SlopeSegments)
        {
            if (!TryGetSlopeInfo(slope, out var info))
            {
                continue;
            }

            if (info.Bounds.End.X <= overlapMinX || info.Bounds.Position.X >= overlapMaxX)
            {
                continue;
            }

            var clampedX = Mathf.Clamp(sampleX, info.Bounds.Position.X, info.Bounds.End.X);
            var surfaceY = GetSlopeSurfaceY(info, clampedX);
            if (surfaceY < objectBottomY - upwardTolerance)
            {
                continue;
            }

            if (nearestTop is null || surfaceY < nearestTop.Value)
            {
                nearestTop = surfaceY;
            }
        }

        foreach (var hill in GetAuthoredHills())
        {
            if (!hill.TryGetSupportTop(overlapMinX, overlapMaxX, objectBottomY, upwardTolerance, out var surfaceY))
            {
                continue;
            }

            if (nearestTop is null || surfaceY < nearestTop.Value)
            {
                nearestTop = surfaceY;
            }
        }

        if (!includeMovingPlatforms)
        {
            return nearestTop;
        }

        foreach (var platform in GetMovingPlatforms())
        {
            if (platform.SupportRightX <= overlapMinX || platform.SupportLeftX >= overlapMaxX)
            {
                continue;
            }

            if (platform.SupportTopY < objectBottomY - upwardTolerance)
            {
                continue;
            }

            if (nearestTop is null || platform.SupportTopY < nearestTop.Value)
            {
                nearestTop = platform.SupportTopY;
            }
        }

        return nearestTop;
    }

    private IEnumerable<GrassHill> GetAuthoredHills()
    {
        if (_terrainHillsRoot is null)
        {
            yield break;
        }

        foreach (Node child in _terrainHillsRoot.GetChildren())
        {
            if (child is GrassHill hill)
            {
                yield return hill;
            }
        }
    }

    private static float GetSlopeSurfaceY(SlopeInfo info, float sampleX)
    {
        var normalized = Mathf.InverseLerp(info.Bounds.Position.X, info.Bounds.End.X, sampleX);
        var topY = info.Bounds.Position.Y;
        var baseY = info.Bounds.Position.Y + info.Bounds.Size.Y;
        if (info.Direction < 0)
        {
            return Mathf.Lerp(baseY, topY, normalized);
        }

        return Mathf.Lerp(topY, baseY, normalized);
    }

    private void AddTerrainSprite(Vector2I cell, Texture2D texture, bool flipHorizontally = false)
    {
        var sprite = new Sprite2D
        {
            Texture = texture,
            Centered = true,
            TextureFilter = TextureFilterEnum.Nearest,
            Position = cell * TilePixels + new Vector2(TilePixels * 0.5f, TilePixels * 0.5f),
            Scale = Vector2.One * 0.5f,
            FlipH = flipHorizontally
        };

        _tileVisualRoot.AddChild(sprite);
    }

    private void AddRampSprite(Vector2I cell, RampVisualKind kind, bool flipHorizontally = false)
    {
        AddTerrainSprite(cell, GameAssets.GetRampTexture(Theme, kind), flipHorizontally);
    }

    private void AddSlopeRampRun(IReadOnlyList<Vector2I> cells, bool mirrorHorizontally)
    {
        if (cells.Count == 4)
        {
            if (mirrorHorizontally)
            {
                AddRampSprite(cells[0], RampVisualKind.ShortB, true);
                AddRampSprite(cells[2], RampVisualKind.ShortA, true);
            }
            else
            {
                AddRampSprite(cells[0], RampVisualKind.ShortA, false);
                AddRampSprite(cells[2], RampVisualKind.ShortB, false);
            }

            return;
        }

        if (cells.Count == 6)
        {
            if (mirrorHorizontally)
            {
                AddRampSprite(cells[0], RampVisualKind.LongC, true);
                AddRampSprite(cells[2], RampVisualKind.LongB, true);
                AddRampSprite(cells[4], RampVisualKind.LongA, true);
            }
            else
            {
                AddRampSprite(cells[0], RampVisualKind.LongA, false);
                AddRampSprite(cells[2], RampVisualKind.LongB, false);
                AddRampSprite(cells[4], RampVisualKind.LongC, false);
            }

            return;
        }

        var index = 0;
        while (index < cells.Count)
        {
            var remaining = cells.Count - index;
            if (remaining == 2)
            {
                if (mirrorHorizontally)
                {
                    AddRampSprite(cells[index], RampVisualKind.ShortB, true);
                    AddRampSprite(cells[index + 1], RampVisualKind.ShortA, true);
                }
                else
                {
                    AddRampSprite(cells[index], RampVisualKind.ShortA, false);
                    AddRampSprite(cells[index + 1], RampVisualKind.ShortB, false);
                }
                index += 2;
                continue;
            }

            if (remaining >= 3)
            {
                if (mirrorHorizontally)
                {
                    AddRampSprite(cells[index], RampVisualKind.LongC, true);
                    AddRampSprite(cells[index + 1], RampVisualKind.LongB, true);
                    AddRampSprite(cells[index + 2], RampVisualKind.LongA, true);
                }
                else
                {
                    AddRampSprite(cells[index], RampVisualKind.LongA, false);
                    AddRampSprite(cells[index + 1], RampVisualKind.LongB, false);
                    AddRampSprite(cells[index + 2], RampVisualKind.LongC, false);
                }
                index += 3;
                continue;
            }

            break;
        }
    }

    private void AddOverlaySlopeRun(Node2D parent, IReadOnlyList<Vector2I> cells, bool mirrorHorizontally)
    {
        var parentOrigin = parent.Position;

        if (cells.Count == 4)
        {
            if (mirrorHorizontally)
            {
                AddOverlayRampSprite(parent, "LeftRampB", cells[0], RampVisualKind.ShortB, parentOrigin, true);
                AddOverlayRampSprite(parent, "LeftRampA", cells[2], RampVisualKind.ShortA, parentOrigin, true);
            }
            else
            {
                AddOverlayRampSprite(parent, "RightRampA", cells[0], RampVisualKind.ShortA, parentOrigin, false);
                AddOverlayRampSprite(parent, "RightRampB", cells[2], RampVisualKind.ShortB, parentOrigin, false);
            }

            return;
        }

        if (cells.Count == 6)
        {
            if (mirrorHorizontally)
            {
                AddOverlayRampSprite(parent, "LeftRampC", cells[0], RampVisualKind.LongC, parentOrigin, true);
                AddOverlayRampSprite(parent, "LeftRampB", cells[2], RampVisualKind.LongB, parentOrigin, true);
                AddOverlayRampSprite(parent, "LeftRampA", cells[4], RampVisualKind.LongA, parentOrigin, true);
            }
            else
            {
                AddOverlayRampSprite(parent, "RightRampA", cells[0], RampVisualKind.LongA, parentOrigin, false);
                AddOverlayRampSprite(parent, "RightRampB", cells[2], RampVisualKind.LongB, parentOrigin, false);
                AddOverlayRampSprite(parent, "RightRampC", cells[4], RampVisualKind.LongC, parentOrigin, false);
            }

            return;
        }

        var index = 0;
        while (index < cells.Count)
        {
            var remaining = cells.Count - index;
            if (remaining == 2)
            {
                if (mirrorHorizontally)
                {
                    AddOverlayRampSprite(parent, "LeftRampB", cells[index], RampVisualKind.ShortB, parentOrigin, true);
                    AddOverlayRampSprite(parent, "LeftRampA", cells[index + 1], RampVisualKind.ShortA, parentOrigin, true);
                }
                else
                {
                    AddOverlayRampSprite(parent, "RightRampA", cells[index], RampVisualKind.ShortA, parentOrigin, false);
                    AddOverlayRampSprite(parent, "RightRampB", cells[index + 1], RampVisualKind.ShortB, parentOrigin, false);
                }

                index += 2;
                continue;
            }

            if (remaining >= 3)
            {
                if (mirrorHorizontally)
                {
                    AddOverlayRampSprite(parent, "LeftRampC", cells[index], RampVisualKind.LongC, parentOrigin, true);
                    AddOverlayRampSprite(parent, "LeftRampB", cells[index + 1], RampVisualKind.LongB, parentOrigin, true);
                    AddOverlayRampSprite(parent, "LeftRampA", cells[index + 2], RampVisualKind.LongA, parentOrigin, true);
                }
                else
                {
                    AddOverlayRampSprite(parent, "RightRampA", cells[index], RampVisualKind.LongA, parentOrigin, false);
                    AddOverlayRampSprite(parent, "RightRampB", cells[index + 1], RampVisualKind.LongB, parentOrigin, false);
                    AddOverlayRampSprite(parent, "RightRampC", cells[index + 2], RampVisualKind.LongC, parentOrigin, false);
                }

                index += 3;
                continue;
            }

            break;
        }
    }

    private void AddOverlayRampSprite(Node2D parent, string namePrefix, Vector2I cell, RampVisualKind kind, Vector2 parentOrigin, bool flipHorizontally)
    {
        AddOverlaySprite(
            parent,
            $"{namePrefix}_{cell.X}_{cell.Y}",
            new Vector2(
                cell.X * TilePixels + TilePixels * 0.5f - parentOrigin.X,
                cell.Y * TilePixels + TilePixels * 0.5f - parentOrigin.Y),
            GameAssets.GetRampTexture(Theme, kind),
            flipHorizontally);
    }

    private void AddOverlaySprite(Node2D parent, string name, Vector2 localPosition, Texture2D texture, bool flipHorizontally = false)
    {
        var sprite = new Sprite2D
        {
            Name = name,
            Texture = texture,
            Centered = true,
            TextureFilter = TextureFilterEnum.Nearest,
            Position = localPosition,
            Scale = Vector2.One * 0.5f,
            FlipH = flipHorizontally
        };
        sprite.AddToGroup("full_slope", true);
        AddOwnedChild(parent, sprite);
    }

    private void AddOverlayTopRun(Node2D parent, int startX, int endX, int topY, Vector2 origin, bool usePlatformCaps = false)
    {
        for (var x = startX; x <= endX; x++)
        {
            var kind = usePlatformCaps
                ? x == startX
                    ? TerrainVisualKind.PlatformLeft
                    : x == endX
                        ? TerrainVisualKind.PlatformRight
                        : TerrainVisualKind.PlatformMiddle
                : TerrainVisualKind.Top;

            AddOverlaySprite(
                parent,
                $"Top_{x}",
                new Vector2(
                    x * TilePixels + TilePixels * 0.5f - origin.X,
                    topY * TilePixels + TilePixels * 0.5f - origin.Y),
                GameAssets.GetTerrainTexture(Theme, kind));
        }
    }

    private void AddHorizontalSurfaceRun(IReadOnlyList<Vector2I> cells, HashSet<Vector2I> slopeCells)
    {
        if (cells.Count == 0)
        {
            return;
        }

        var joinsSlopeOnLeft = TouchesSlope(cells[0], Vector2I.Left, slopeCells);
        var joinsSlopeOnRight = TouchesSlope(cells[^1], Vector2I.Right, slopeCells);
        if (joinsSlopeOnLeft || joinsSlopeOnRight)
        {
            foreach (var cell in cells)
            {
                AddTerrainSprite(cell, GameAssets.GetTerrainTexture(Theme, TerrainVisualKind.Top));
            }
            return;
        }

        if (cells.Count == 1)
        {
            AddTerrainSprite(cells[0], GameAssets.GetTerrainTexture(Theme, TerrainVisualKind.PlatformMiddle));
            return;
        }

        for (var index = 0; index < cells.Count; index++)
        {
            var kind = index == 0
                ? joinsSlopeOnLeft
                    ? TerrainVisualKind.PlatformMiddle
                    : TerrainVisualKind.PlatformLeft
                : index == cells.Count - 1
                    ? joinsSlopeOnRight
                        ? TerrainVisualKind.PlatformMiddle
                        : TerrainVisualKind.PlatformRight
                    : TerrainVisualKind.PlatformMiddle;
            AddTerrainSprite(cells[index], GameAssets.GetTerrainTexture(Theme, kind));
        }
    }

    private static bool TouchesSlope(Vector2I cell, Vector2I direction, HashSet<Vector2I> slopeCells)
    {
        return slopeCells.Contains(cell + direction)
            || slopeCells.Contains(cell + direction + Vector2I.Up)
            || slopeCells.Contains(cell + direction + Vector2I.Down);
    }

    private void AddOwnedChild(Node parent, Node child)
    {
        parent.AddChild(child);
        child.Owner = this;
    }

    private static bool TryGetSlopeInfo(Vector4I definition, out SlopeInfo info)
    {
        info = default;
        if (definition.Z < 2 || definition.W == 0)
        {
            return false;
        }

        var direction = definition.W > 0 ? 1 : -1;
        var cells = new List<Vector2I>(definition.Z);
        for (var index = 0; index < definition.Z; index++)
        {
            cells.Add(new Vector2I(definition.X + index, definition.Y + index * direction));
        }

        var topTileY = cells.Min(cell => cell.Y);
        var baseTopTileY = cells.Max(cell => cell.Y) + 1;
        var leftX = definition.X * TilePixels;
        var rightX = (definition.X + definition.Z) * TilePixels;
        var baseY = baseTopTileY * TilePixels;
        var topY = topTileY * TilePixels;

        Vector2[] polygon;
        if (direction < 0)
        {
            polygon =
            [
                new Vector2(leftX, baseY),
                new Vector2(rightX, topY),
                new Vector2(rightX, baseY)
            ];
        }
        else
        {
            polygon =
            [
                new Vector2(leftX, topY),
                new Vector2(rightX, baseY),
                new Vector2(leftX, baseY)
            ];
        }

        info = new SlopeInfo(
            cells,
            direction,
            baseTopTileY,
            new Rect2(leftX, topY, definition.Z * TilePixels, baseY - topY),
            polygon);
        return true;
    }

    private HashSet<Vector2I> CollectSlopeCells()
    {
        var slopeCells = new HashSet<Vector2I>();
        foreach (var slope in SlopeSegments)
        {
            if (!TryGetSlopeInfo(slope, out var info))
            {
                continue;
            }

            foreach (var cell in info.Cells)
            {
                slopeCells.Add(cell);
            }
        }

        return slopeCells;
    }

    private List<FullSlopePair> CollectFullSlopePairs()
    {
        var slopeInfos = SlopeSegments
            .Select(segment => TryGetSlopeInfo(segment, out var info) ? info : (SlopeInfo?)null)
            .Where(info => info.HasValue)
            .Select(info => info!.Value)
            .ToList();
        if (slopeInfos.Count < 2)
        {
            return [];
        }

        var leftSlopes = slopeInfos.Where(info => info.Direction < 0).ToList();
        var rightSlopes = slopeInfos.Where(info => info.Direction > 0).ToList();
        var pairs = new List<FullSlopePair>(Mathf.Min(leftSlopes.Count, rightSlopes.Count));
        for (var index = 0; index < leftSlopes.Count && index < rightSlopes.Count; index++)
        {
            pairs.Add(new FullSlopePair(leftSlopes[index], rightSlopes[index]));
        }

        return pairs;
    }

    private static HashSet<Vector2I> CollectFullSlopeOverlayCells(IReadOnlyList<FullSlopePair> fullSlopePairs)
    {
        var overlayCells = new HashSet<Vector2I>();
        foreach (var pair in fullSlopePairs)
        {
            foreach (var cell in pair.Left.Cells)
            {
                overlayCells.Add(cell);
            }

            foreach (var cell in pair.Right.Cells)
            {
                overlayCells.Add(cell);
            }

            var topY = pair.Left.Cells.Min(cell => cell.Y);
            var plateauStartX = pair.Left.Cells.Max(cell => cell.X) + 1;
            var plateauEndX = pair.Right.Cells.Min(cell => cell.X) - 1;
            for (var x = plateauStartX; x <= plateauEndX; x++)
            {
                overlayCells.Add(new Vector2I(x, topY));
            }

            var leftByY = pair.Left.Cells.ToDictionary(cell => cell.Y, cell => cell.X);
            var rightByY = pair.Right.Cells.ToDictionary(cell => cell.Y, cell => cell.X);
            for (var y = topY + 1; y < pair.Left.BaseTopTileY; y++)
            {
                if (!leftByY.TryGetValue(y, out var leftFillX) || !rightByY.TryGetValue(y, out var rightFillX))
                {
                    continue;
                }

                for (var x = leftFillX; x <= rightFillX; x++)
                {
                    overlayCells.Add(new Vector2I(x, y));
                }
            }
        }

        return overlayCells;
    }

    private static TerrainVisualKind ResolveTerrainVisual(HashSet<Vector2I> occupied, HashSet<Vector2I> slopeCells, Vector2I cell)
    {
        var left = occupied.Contains(cell + Vector2I.Left) || slopeCells.Contains(cell + Vector2I.Left);
        var right = occupied.Contains(cell + Vector2I.Right) || slopeCells.Contains(cell + Vector2I.Right);
        var up = occupied.Contains(cell + Vector2I.Up);
        var down = occupied.Contains(cell + Vector2I.Down);

        if (!up && !down)
        {
            return !left && right
                ? TerrainVisualKind.PlatformLeft
                : left && !right
                    ? TerrainVisualKind.PlatformRight
                    : TerrainVisualKind.PlatformMiddle;
        }

        if (!up)
        {
            return !left ? TerrainVisualKind.TopLeft : !right ? TerrainVisualKind.TopRight : TerrainVisualKind.Top;
        }

        if (!down)
        {
            return !left ? TerrainVisualKind.BottomLeft : !right ? TerrainVisualKind.BottomRight : TerrainVisualKind.Bottom;
        }

        return !left ? TerrainVisualKind.Left : !right ? TerrainVisualKind.Right : TerrainVisualKind.Center;
    }

    private readonly record struct SlopeInfo(
        IReadOnlyList<Vector2I> Cells,
        int Direction,
        int BaseTopTileY,
        Rect2 Bounds,
        Vector2[] CollisionPolygon);

    private readonly record struct FullSlopePair(
        SlopeInfo Left,
        SlopeInfo Right);
}

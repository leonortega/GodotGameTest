using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GameTest;

public partial class GrassHill : Node2D
{
    private const float TilePixels = 32f;

    private CollisionPolygon2D _collisionPolygon = null!;

    public override void _Ready()
    {
        _collisionPolygon = GetNode<CollisionPolygon2D>("CollisionBody/CollisionPolygon");
    }

    public Rect2 GetBounds()
    {
        var localBounds = GetLocalBounds();
        return new Rect2(
            GlobalPosition + localBounds.Position,
            localBounds.Size);
    }

    public IEnumerable<Rect2> GetSolidRects()
    {
        var groupedRuns = GetOccupiedCells()
            .GroupBy(cell => cell.Y)
            .OrderBy(group => group.Key);

        foreach (var row in groupedRuns)
        {
            var orderedColumns = row.Select(cell => cell.X).OrderBy(x => x).ToArray();
            if (orderedColumns.Length == 0)
            {
                continue;
            }

            var runStart = orderedColumns[0];
            var previous = orderedColumns[0];
            for (var index = 1; index < orderedColumns.Length; index++)
            {
                var current = orderedColumns[index];
                if (current == previous + 1)
                {
                    previous = current;
                    continue;
                }

                yield return CreateWorldRect(runStart, previous, row.Key);
                runStart = current;
                previous = current;
            }

            yield return CreateWorldRect(runStart, previous, row.Key);
        }
    }

    public IEnumerable<Vector2I> GetOccupiedCells()
    {
        var localBounds = GetLocalBounds();
        var startColumn = Mathf.FloorToInt(localBounds.Position.X / TilePixels);
        var endColumn = Mathf.CeilToInt(localBounds.End.X / TilePixels) - 1;
        var bottomRow = Mathf.CeilToInt(localBounds.End.Y / TilePixels) - 1;
        var worldOriginCell = new Vector2I(
            Mathf.RoundToInt(GlobalPosition.X / TilePixels),
            Mathf.RoundToInt(GlobalPosition.Y / TilePixels));

        for (var column = startColumn; column <= endColumn; column++)
        {
            var localSampleX = column * TilePixels + TilePixels * 0.5f;
            if (!TryGetLocalSurfaceY(localSampleX, out var localSurfaceY))
            {
                continue;
            }

            var topRow = Mathf.FloorToInt(localSurfaceY / TilePixels);
            for (var row = topRow; row <= bottomRow; row++)
            {
                yield return new Vector2I(worldOriginCell.X + column, worldOriginCell.Y + row);
            }
        }
    }

    public bool TryGetSupportTop(float overlapMinX, float overlapMaxX, float objectBottomY, float upwardTolerance, out float supportTop)
    {
        supportTop = 0f;
        var bounds = GetBounds();
        if (bounds.End.X <= overlapMinX || bounds.Position.X >= overlapMaxX)
        {
            return false;
        }

        var sampleX = (overlapMinX + overlapMaxX) * 0.5f;
        var localX = sampleX - GlobalPosition.X;
        if (!TryGetLocalSurfaceY(localX, out var localSurfaceY))
        {
            return false;
        }

        supportTop = GlobalPosition.Y + localSurfaceY;
        return supportTop >= objectBottomY - upwardTolerance;
    }

    private Rect2 GetLocalBounds()
    {
        var polygon = GetCollisionPolygon();
        var minX = polygon.Min(point => point.X);
        var minY = polygon.Min(point => point.Y);
        var maxX = polygon.Max(point => point.X);
        var maxY = polygon.Max(point => point.Y);
        return new Rect2(minX, minY, maxX - minX, maxY - minY);
    }

    private bool TryGetLocalSurfaceY(float localX, out float localSurfaceY)
    {
        localSurfaceY = 0f;
        var polygon = GetCollisionPolygon();
        if (polygon.Length < 3)
        {
            return false;
        }

        var localBounds = GetLocalBounds();
        if (localX < localBounds.Position.X || localX > localBounds.End.X)
        {
            return false;
        }

        var intersections = new List<float>();
        for (var index = 0; index < polygon.Length; index++)
        {
            var start = polygon[index];
            var end = polygon[(index + 1) % polygon.Length];
            var minX = Mathf.Min(start.X, end.X);
            var maxX = Mathf.Max(start.X, end.X);
            if (localX < minX || localX > maxX)
            {
                continue;
            }

            if (Mathf.IsEqualApprox(start.X, end.X))
            {
                if (Mathf.IsEqualApprox(localX, start.X))
                {
                    intersections.Add(Mathf.Min(start.Y, end.Y));
                }

                continue;
            }

            var ratio = (localX - start.X) / (end.X - start.X);
            if (ratio < 0f || ratio > 1f)
            {
                continue;
            }

            intersections.Add(Mathf.Lerp(start.Y, end.Y, ratio));
        }

        if (intersections.Count == 0)
        {
            return false;
        }

        localSurfaceY = intersections.Min();
        return true;
    }

    private Vector2[] GetCollisionPolygon()
    {
        if (_collisionPolygon is null)
        {
            _collisionPolygon = GetNode<CollisionPolygon2D>("CollisionBody/CollisionPolygon");
        }

        return _collisionPolygon.Polygon;
    }

    private static Rect2 CreateWorldRect(int startColumn, int endColumn, int row)
    {
        return new Rect2(
            startColumn * TilePixels,
            row * TilePixels,
            (endColumn - startColumn + 1) * TilePixels,
            TilePixels);
    }
}
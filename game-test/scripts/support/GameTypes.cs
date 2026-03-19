using Godot;

namespace GameTest;

public enum PlayerForm
{
    Small,
    Powered,
    Enhanced
}

public enum PickupType
{
    Coin,
    Growth,
    Flame,
    ExtraLife
}

public enum EnemyKind
{
    Ground,
    Armored,
    Flying,
    ProtectedHead,
    Shooter
}

public enum StageTheme
{
    Grassland,
    Cave,
    Treetop,
    Fortress
}

public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard
}

public enum DamageResult
{
    Ignored,
    Downgraded,
    LifeLost,
    GameOver
}

public enum ProjectileHitResult
{
    Ignored,
    Defeated,
    Reflected
}

public sealed record EnemySpawn(EnemyKind Kind, Vector2 Position, float PatrolDistance = 0f);

public sealed class SaveSlotData
{
    public string? HighestClearedStage { get; set; }
    public int BestScore { get; set; }
    public float MusicVolumeDb { get; set; } = -4f;
    public float SfxVolumeDb { get; set; } = -2f;
    public string Difficulty { get; set; } = DifficultyLevel.Normal.ToString();
}

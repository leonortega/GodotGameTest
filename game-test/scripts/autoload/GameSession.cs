using System.Text.Json;
using Godot;
using FileAccess = Godot.FileAccess;

namespace GameTest;

public partial class GameSession : Node
{
    public static GameSession Instance { get; private set; } = null!;

    public const int StartingLives = 3;
    public const int ExtraLifeCoinThreshold = 100;

    private readonly string[] _stageOrder = ["1-1", "1-2", "1-3", "1-4"];
    private SaveSlotData _saveData = new();

    public bool HasActiveRun { get; private set; }
    public int CurrentStageIndex { get; private set; }
    public string CurrentStageId => _stageOrder[Mathf.Clamp(CurrentStageIndex, 0, _stageOrder.Length - 1)];
    public int Lives { get; private set; } = StartingLives;
    public int Coins { get; private set; }
    public int Score { get; private set; }
    public int TimeRemaining { get; private set; }
    public PlayerForm CurrentForm { get; private set; } = PlayerForm.Small;
    public int HighestClearedStageIndex { get; private set; } = -1;
    public int BestScore => _saveData.BestScore;
    public float MusicVolumeDb => _saveData.MusicVolumeDb;
    public float SfxVolumeDb => _saveData.SfxVolumeDb;
    public DifficultyLevel CurrentDifficulty { get; private set; } = DifficultyLevel.Normal;

    public override void _Ready()
    {
        Instance = this;
        LoadProgress();
    }

    public void StartNewRun()
    {
        HasActiveRun = true;
        CurrentStageIndex = 0;
        Lives = StartingLives;
        Coins = 0;
        Score = 0;
        TimeRemaining = 0;
        CurrentForm = PlayerForm.Small;
    }

    public void SetCurrentStageIndex(int stageIndex)
    {
        CurrentStageIndex = Mathf.Clamp(stageIndex, 0, _stageOrder.Length - 1);
    }

    public void CycleDifficulty()
    {
        CurrentDifficulty = CurrentDifficulty switch
        {
            DifficultyLevel.Easy => DifficultyLevel.Normal,
            DifficultyLevel.Normal => DifficultyLevel.Hard,
            _ => DifficultyLevel.Easy
        };

        _saveData.Difficulty = CurrentDifficulty.ToString();
        SaveProgress();
    }

    public int GetMaxActiveEnemiesOnScreen() => CurrentDifficulty switch
    {
        DifficultyLevel.Easy => 3,
        DifficultyLevel.Hard => 7,
        _ => 5
    };

    public bool AdvanceToNextStage()
    {
        if (CurrentStageIndex >= _stageOrder.Length - 1)
        {
            return false;
        }

        CurrentStageIndex++;
        return true;
    }

    public void SetTimer(int seconds)
    {
        TimeRemaining = Mathf.Max(0, seconds);
    }

    public void AddScore(int amount)
    {
        Score = Mathf.Max(0, Score + amount);
    }

    public void AddCoin()
    {
        Coins++;
        AddScore(100);

        if (Coins % ExtraLifeCoinThreshold == 0)
        {
            AddLife();
        }
    }

    public void AddLife()
    {
        Lives++;
    }

    public void SetForm(PlayerForm form)
    {
        CurrentForm = form;
    }

    public DamageResult ApplyDamage()
    {
        switch (CurrentForm)
        {
            case PlayerForm.Enhanced:
                CurrentForm = PlayerForm.Powered;
                return DamageResult.Downgraded;
            case PlayerForm.Powered:
                CurrentForm = PlayerForm.Small;
                return DamageResult.Downgraded;
            default:
                return LoseLife() ? DamageResult.GameOver : DamageResult.LifeLost;
        }
    }

    public bool LoseLife()
    {
        Lives = Mathf.Max(0, Lives - 1);
        CurrentForm = PlayerForm.Small;
        return Lives <= 0;
    }

    public void MarkStageCleared()
    {
        HighestClearedStageIndex = Mathf.Max(HighestClearedStageIndex, CurrentStageIndex);
        _saveData.HighestClearedStage = _stageOrder[HighestClearedStageIndex];
        _saveData.BestScore = Mathf.Max(_saveData.BestScore, Score);
        SaveProgress();
    }

    public string GetDisplayStageId() => _stageOrder[Mathf.Clamp(CurrentStageIndex, 0, _stageOrder.Length - 1)];

    private string GetSavePath() => ProjectSettings.GlobalizePath("user://super_pixel_quest_save.json");

    public void LoadProgress()
    {
        if (!FileAccess.FileExists("user://super_pixel_quest_save.json"))
        {
            HighestClearedStageIndex = -1;
            _saveData = new SaveSlotData();
            return;
        }

        try
        {
            using var file = FileAccess.Open("user://super_pixel_quest_save.json", FileAccess.ModeFlags.Read);
            var json = file.GetAsText();
            _saveData = JsonSerializer.Deserialize<SaveSlotData>(json) ?? new SaveSlotData();
            if (!Enum.TryParse<DifficultyLevel>(_saveData.Difficulty, true, out var parsedDifficulty))
            {
                parsedDifficulty = DifficultyLevel.Normal;
            }

            CurrentDifficulty = parsedDifficulty;

            HighestClearedStageIndex = Array.IndexOf(_stageOrder, _saveData.HighestClearedStage);
            if (HighestClearedStageIndex < 0)
            {
                HighestClearedStageIndex = -1;
            }
        }
        catch (Exception)
        {
            _saveData = new SaveSlotData();
            HighestClearedStageIndex = -1;
            CurrentDifficulty = DifficultyLevel.Normal;
        }
    }

    public void SaveProgress()
    {
        try
        {
            var absolutePath = GetSavePath();
            var directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var file = FileAccess.Open("user://super_pixel_quest_save.json", FileAccess.ModeFlags.Write);
            file.StoreString(JsonSerializer.Serialize(_saveData, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
        catch (Exception exception)
        {
            GD.PrintErr($"Failed to save progress: {exception.Message}");
        }
    }
}

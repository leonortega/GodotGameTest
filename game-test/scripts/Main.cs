using Godot;

namespace GameTest;

public partial class Main : Node
{
	private const double TransitionCardDurationSeconds = 3.0;
	private const int TitleStageCount = 4;

	private WorldRoot _world = null!;
	private HudLayer _hud = null!;
	private OverlayLayer _overlay = null!;
	private ulong _transitionVersion;
	private int _selectedTitleStageIndex;

	public override void _Ready()
	{
		EnsureInputMap();

		_world = new WorldRoot();
		_world.Name = "WorldRoot";
		_world.StageCompleted += OnStageCompleted;
		_world.StageRestartRequested += OnStageRestartRequested;
		_world.GameOver += OnGameOver;
		AddChild(_world);

		_hud = new HudLayer();
		AddChild(_hud);

		_overlay = new OverlayLayer();
		AddChild(_overlay);

		ShowTitleOverlay();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("pause") && GameSession.Instance.HasActiveRun)
		{
			if (_overlay.Visible)
			{
				if (_world.SimulationActive || !_overlay.AllowsPauseResume)
				{
					return;
				}

				ResumeGame();
			}
			else if (_world.SimulationActive)
			{
				PauseGame();
			}
		}
	}

	private void ShowTitleOverlay()
	{
		_transitionVersion++;
		_world.SetSimulationActive(false);
		AudioDirector.Instance.StopMusic();

		_overlay.ShowTitleScreen(
			"Run, jump, stomp, power up, and survive four short stages built in Godot 4 .NET.\n\n[b]A / D[/b] or arrows move, [b]W / Up / Space[/b] jumps, [b]Shift[/b] attacks when enhanced, [b]Esc[/b] pauses.",
			$"Difficulty: {GameSession.Instance.CurrentDifficulty}",
			$"Level: 1-{_selectedTitleStageIndex + 1}",
			StartNewGame,
			ShowControlsOverlay,
			CycleDifficulty,
			CycleTitleStage);
	}

	private void CycleDifficulty()
	{
		AudioDirector.Instance.PlayUi("menu_open");
		GameSession.Instance.CycleDifficulty();
		ShowTitleOverlay();
	}

	private void ShowControlsOverlay()
	{
		_overlay.ShowOverlay(
			"Controls",
			"How To Play",
			"[b]Move[/b]: A / D or Left / Right\n[b]Jump[/b]: W / Up / Space\n[b]Double Jump[/b]: Press jump again in mid-air\n[b]Attack[/b]: Shift when enhanced\n[b]Pause[/b]: Esc",
			"Back",
			ShowTitleOverlay);
	}

	private void StartNewGame()
	{
		GameSession.Instance.StartNewRun();
		GameSession.Instance.SetCurrentStageIndex(_selectedTitleStageIndex);
		BeginStageEntryTransition();
	}

	private void CycleTitleStage()
	{
		AudioDirector.Instance.PlayUi("menu_open");
		_selectedTitleStageIndex = (_selectedTitleStageIndex + 1) % TitleStageCount;
		ShowTitleOverlay();
	}

	private void BeginStageEntryTransition()
	{
		RunStageEntryTransitionAsync(++_transitionVersion);
	}

	private async void RunStageEntryTransitionAsync(ulong version)
	{
		_world.SetSimulationActive(false);
		AudioDirector.Instance.SetPaused(false);
		AudioDirector.Instance.StopMusic();
		_overlay.ShowStageIntro(GameSession.Instance.CurrentForm, GameSession.Instance.Lives, GameSession.Instance.GetDisplayStageId());

		await ToSignal(GetTree().CreateTimer(TransitionCardDurationSeconds), SceneTreeTimer.SignalName.Timeout);

		if (version != _transitionVersion || !IsInsideTree())
		{
			return;
		}

		_world.LoadStage(GameSession.Instance.CurrentStageId);
		_overlay.HideOverlay();
	}

	private void PauseGame()
	{
		_world.SetSimulationActive(false);
		AudioDirector.Instance.SetPaused(true);
		AudioDirector.Instance.PlayUi("pause");
		_overlay.ShowOverlay(
			"Paused",
			$"Stage {GameSession.Instance.GetDisplayStageId()}",
			"Simulation is suspended. Timer, enemies, and movement are frozen.",
			"Resume",
			ResumeGame,
			"Title",
			ShowTitleOverlay,
			true);
	}

	private void ResumeGame()
	{
		_overlay.HideOverlay();
		AudioDirector.Instance.SetPaused(false);
		AudioDirector.Instance.PlayUi("resume");
		_world.SetSimulationActive(true);
	}

	private void OnStageCompleted()
	{
		var clearedStageId = GameSession.Instance.GetDisplayStageId();
		GameSession.Instance.MarkStageCleared();
		_world.SetSimulationActive(false);
		AudioDirector.Instance.SetPaused(false);

		if (GameSession.Instance.AdvanceToNextStage())
		{
			RunStageClearSequenceAsync(++_transitionVersion, clearedStageId);
			return;
		}

		_transitionVersion++;
		_overlay.ShowOverlay(
			"World Clear",
			"You Cleared The MVP World",
			$"Final score: [b]{GameSession.Instance.Score}[/b]\nHighest clear saved: [b]{GameSession.Instance.HighestClearedStageIndex + 1} stages[/b]",
			"Play Again",
			StartNewGame,
			"Title",
			ShowTitleOverlay);
	}

	private async void RunStageClearSequenceAsync(ulong version, string clearedStageId)
	{
		_overlay.ShowStageSummary(
			clearedStageId,
			GameSession.Instance.CurrentForm,
			GameSession.Instance.Score,
			GameSession.Instance.Coins,
			GameSession.Instance.Lives,
			GameSession.Instance.TimeRemaining);

		await ToSignal(GetTree().CreateTimer(TransitionCardDurationSeconds), SceneTreeTimer.SignalName.Timeout);

		if (version != _transitionVersion || !IsInsideTree())
		{
			return;
		}

		_overlay.ShowStageIntro(GameSession.Instance.CurrentForm, GameSession.Instance.Lives, GameSession.Instance.GetDisplayStageId());

		await ToSignal(GetTree().CreateTimer(TransitionCardDurationSeconds), SceneTreeTimer.SignalName.Timeout);

		if (version != _transitionVersion || !IsInsideTree())
		{
			return;
		}

		_world.LoadStage(GameSession.Instance.CurrentStageId);
		_overlay.HideOverlay();
	}

	private void OnStageRestartRequested()
	{
		BeginStageEntryTransition();
	}

	private void OnGameOver()
	{
		_transitionVersion++;
		_world.SetSimulationActive(false);
		AudioDirector.Instance.StopMusic();
		_overlay.ShowOverlay(
			"Game Over",
			"Run Ended",
			$"Best Score: [b]{GameSession.Instance.BestScore}[/b]\nTry the world again from stage 1-1.",
			"Restart",
			StartNewGame,
			"Title",
			ShowTitleOverlay);
	}

	private void EnsureInputMap()
	{
		SetAction("move_left", new InputEventKey { Keycode = Key.A }, new InputEventKey { Keycode = Key.Left });
		SetAction("move_right", new InputEventKey { Keycode = Key.D }, new InputEventKey { Keycode = Key.Right });
		SetAction("move_down", new InputEventKey { Keycode = Key.S }, new InputEventKey { Keycode = Key.Down });
		SetAction("jump", new InputEventKey { Keycode = Key.W }, new InputEventKey { Keycode = Key.Up }, new InputEventKey { Keycode = Key.Space }, new InputEventKey { Keycode = Key.K });
		SetAction("action", new InputEventKey { Keycode = Key.Shift }, new InputEventKey { Keycode = Key.J });
		SetAction("pause", new InputEventKey { Keycode = Key.Escape }, new InputEventKey { Keycode = Key.P });
	}

	private static void SetAction(string actionName, params InputEvent[] inputEvents)
	{
		if (!InputMap.HasAction(actionName))
		{
			InputMap.AddAction(actionName);
		}

		foreach (var existingEvent in InputMap.ActionGetEvents(actionName))
		{
			InputMap.ActionEraseEvent(actionName, existingEvent);
		}

		foreach (var inputEvent in inputEvents)
		{
			InputMap.ActionAddEvent(actionName, inputEvent);
		}
	}
}

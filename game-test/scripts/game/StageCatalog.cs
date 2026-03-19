using Godot;

namespace GameTest;

public static class StageCatalog
{
	public static StageScene InstantiateStage(string stageId)
	{
		var path = stageId switch
		{
			"1-1" => "res://scenes/levels/Stage_1_1.tscn",
			"1-2" => "res://scenes/levels/Stage_1_2.tscn",
			"1-3" => "res://scenes/levels/Stage_1_3.tscn",
			"1-4" => "res://scenes/levels/Stage_1_4.tscn",
			_ => "res://scenes/levels/Stage_1_1.tscn"
		};

		var scene = GD.Load<PackedScene>(path);
		if (scene is null)
		{
			throw new InvalidOperationException($"Unable to load stage scene at '{path}'.");
		}

		return scene.Instantiate<StageScene>();
	}
}

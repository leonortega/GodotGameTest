using Godot;

namespace GameTest;

public partial class HudLayer : CanvasLayer
{
    private readonly Label _scoreValue = new();
    private readonly Label _coinsValue = new();
    private readonly Label _stageValue = new();
    private readonly Label _livesValue = new();
    private readonly Label _timeValue = new();
    private readonly Label _formValue = new();

    public override void _Ready()
    {
        Layer = 5;

        var root = new MarginContainer
        {
            AnchorRight = 1,
            OffsetLeft = 16,
            OffsetTop = 16,
            OffsetRight = -16,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        var panel = new PanelContainer
        {
            AnchorRight = 1,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        GameUi.StyleHudPanel(panel);

        var bar = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center
        };
        bar.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        bar.AddThemeConstantOverride("separation", 12);

        bar.AddChild(BuildItem("Score", _scoreValue));
        bar.AddChild(BuildItem("Coins", _coinsValue));
        bar.AddChild(BuildItem("Stage", _stageValue));
        bar.AddChild(BuildItem("Lives", _livesValue));
        bar.AddChild(BuildItem("Time", _timeValue));
        bar.AddChild(BuildItem("Form", _formValue));

        panel.AddChild(bar);
        root.AddChild(panel);
        AddChild(root);

        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        if (GameSession.Instance is null)
        {
            return;
        }

        _scoreValue.Text = GameSession.Instance.Score.ToString("000000");
        _coinsValue.Text = GameSession.Instance.Coins.ToString("00");
        _stageValue.Text = GameSession.Instance.GetDisplayStageId();
        _livesValue.Text = GameSession.Instance.Lives.ToString();
        _timeValue.Text = GameSession.Instance.TimeRemaining.ToString("000");
        _formValue.Text = GameSession.Instance.CurrentForm.ToString();
    }

    private static VBoxContainer BuildItem(string label, Label value)
    {
        var box = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(96, 0),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        box.AddThemeConstantOverride("separation", 4);

        var header = new Label
        {
            Text = label.ToUpperInvariant(),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        GameUi.StyleHudKey(header);

        value.Text = "---";
        value.HorizontalAlignment = HorizontalAlignment.Center;
        GameUi.StyleHudValue(value);

        box.AddChild(header);
        box.AddChild(value);
        return box;
    }
}

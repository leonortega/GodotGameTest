using Godot;

namespace GameTest;

public partial class OverlayLayer : CanvasLayer
{
    private static readonly EnemyKind[] TitleEnemyChoices =
    [
        EnemyKind.Ground,
        EnemyKind.Armored,
        EnemyKind.ProtectedHead
    ];

    private readonly ColorRect _backdrop = new();

    private readonly CenterContainer _titleCenter = new();
    private readonly PanelContainer _titlePanel = new();
    private readonly Label _logoPrimary = new();
    private readonly Label _logoSecondary = new();
    private readonly Label _logoCaption = new();
    private readonly Control _titleAnimationArea = new();
    private readonly TextureRect _titleRunner = new();
    private readonly TextureRect _titleChaser = new();
    private readonly RichTextLabel _titleBody = new();
    private readonly Button _titleStartButton = new();
    private readonly Button _titleControlsButton = new();
    private readonly Button _titleDifficultyButton = new();

    private readonly CenterContainer _menuCenter = new();
    private readonly PanelContainer _panel = new();
    private readonly Label _kicker = new();
    private readonly Label _title = new();
    private readonly RichTextLabel _body = new();
    private readonly Button _primaryButton = new();
    private readonly Button _secondaryButton = new();

    private readonly CenterContainer _timedCenter = new();
    private readonly PanelContainer _timedPanel = new();
    private readonly Label _timedKicker = new();
    private readonly TextureRect _timedPortrait = new();
    private readonly Label _timedTitle = new();
    private readonly RichTextLabel _timedBody = new();

    private Action? _primaryAction;
    private Action? _secondaryAction;
    private Action? _tertiaryAction;
    private float _titleAnimationTime;
    private EnemyKind _titleEnemyKind = EnemyKind.ProtectedHead;

    public bool AllowsPauseResume { get; private set; }

    public override void _Ready()
    {
        Layer = 10;
        ProcessMode = ProcessModeEnum.Always;

        _backdrop.Color = new Color(0.02f, 0.04f, 0.06f, 0.74f);
        _backdrop.AnchorRight = 1;
        _backdrop.AnchorBottom = 1;
        AddChild(_backdrop);

        BuildTitleScreen();
        BuildGenericOverlay();
        BuildTimedCard();

        Visible = false;
    }

    public override void _Process(double delta)
    {
        if (!_titleCenter.Visible)
        {
            return;
        }

        _titleAnimationTime += (float)delta;
        UpdateTitleAnimation();
    }

    public void ShowTitleScreen(string body, string difficultyText, Action startAction, Action controlsAction, Action difficultyAction)
    {
        _backdrop.Color = new Color(0.03f, 0.04f, 0.08f, 0.98f);
        _logoPrimary.Text = "SUPER";
        _logoSecondary.Text = "PIXEL QUEST";
        _logoCaption.Text = "RUN. STOMP. SURVIVE.";
        _titleBody.Text = body;
        _titleDifficultyButton.Text = difficultyText;

        _primaryAction = startAction;
        _secondaryAction = controlsAction;
        _tertiaryAction = difficultyAction;
        AllowsPauseResume = false;
        _titleAnimationTime = 0f;
        _titleEnemyKind = TitleEnemyChoices[GD.RandRange(0, TitleEnemyChoices.Length - 1)];

        _titleCenter.Visible = true;
        _menuCenter.Visible = false;
        _timedCenter.Visible = false;
        Visible = true;
        UpdateTitleAnimation();
    }

    public void ShowOverlay(
        string kicker,
        string title,
        string body,
        string primaryText,
        Action primaryAction,
        string? secondaryText = null,
        Action? secondaryAction = null,
        bool allowPauseResume = false)
    {
        _backdrop.Color = new Color(0.02f, 0.04f, 0.06f, 0.74f);
        _kicker.Text = kicker;
        _title.Text = title;
        _body.Text = body;

        _primaryButton.Text = primaryText;
        _primaryButton.Visible = true;
        _secondaryButton.Visible = !string.IsNullOrWhiteSpace(secondaryText);
        _secondaryButton.Text = secondaryText ?? string.Empty;

        _primaryAction = primaryAction;
        _secondaryAction = secondaryAction;
        _tertiaryAction = null;
        AllowsPauseResume = allowPauseResume;

        _titleCenter.Visible = false;
        _menuCenter.Visible = true;
        _timedCenter.Visible = false;
        Visible = true;
    }

    public void ShowStageIntro(PlayerForm form, int lives, string stageId)
    {
        var body = $"[center]Lives [b]x {lives}[/b]\nForm [b]{form}[/b][/center]";
        ShowTimedCard(new Color(0f, 0f, 0f, 1f), $"Stage {stageId}", GameAssets.GetPlayerFrames(form).Idle, "Get Ready", body);
    }

    public void ShowStageSummary(string stageId, PlayerForm form, int score, int coins, int lives, int timeRemaining)
    {
        var body = $"[center]Score [b]{score:000000}[/b]\nCoins [b]{coins:00}[/b]\nLives [b]{lives}[/b]\nTime [b]{timeRemaining:000}[/b]\nForm [b]{form}[/b][/center]";
        ShowTimedCard(new Color(0f, 0f, 0f, 0.92f), $"Stage {stageId} Clear", GameAssets.GetPlayerFrames(form).Idle, "Run Summary", body);
    }

    public void HideOverlay()
    {
        Visible = false;
        AllowsPauseResume = false;
        _primaryAction = null;
        _secondaryAction = null;
        _tertiaryAction = null;
        _titleCenter.Visible = false;
        _menuCenter.Visible = false;
        _timedCenter.Visible = false;
    }

    private void BuildTitleScreen()
    {
        _titleCenter.AnchorRight = 1;
        _titleCenter.AnchorBottom = 1;

        _titlePanel.CustomMinimumSize = new Vector2(840, 0);
        GameUi.StyleOverlayPanel(_titlePanel);

        var card = new VBoxContainer();
        card.AddThemeConstantOverride("separation", 14);

        GameUi.StyleLogoPrimary(_logoPrimary);
        GameUi.StyleLogoSecondary(_logoSecondary);
        GameUi.StylePixelCaption(_logoCaption);

        _titleAnimationArea.CustomMinimumSize = new Vector2(760, 156);
        _titleAnimationArea.MouseFilter = Control.MouseFilterEnum.Ignore;
        _titleAnimationArea.ClipContents = true;

        ConfigureTitleSprite(_titleRunner, new Vector2(116f, 116f));
        ConfigureTitleSprite(_titleChaser, new Vector2(108f, 108f));
        _titleAnimationArea.AddChild(_titleRunner);
        _titleAnimationArea.AddChild(_titleChaser);

        _titleBody.BbcodeEnabled = true;
        _titleBody.FitContent = true;
        _titleBody.ScrollActive = false;
        _titleBody.CustomMinimumSize = new Vector2(720, 0);
        GameUi.StyleBody(_titleBody);

        var actions = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center
        };
        actions.AddThemeConstantOverride("separation", 12);

        GameUi.StyleButton(_titleStartButton);
        GameUi.StyleButton(_titleControlsButton);
        GameUi.StyleButton(_titleDifficultyButton);
        _titleDifficultyButton.CustomMinimumSize = new Vector2(220, 46);
        _titleStartButton.Text = "Start Game";
        _titleControlsButton.Text = "Controls";

        _titleStartButton.Pressed += () => _primaryAction?.Invoke();
        _titleControlsButton.Pressed += () => _secondaryAction?.Invoke();
        _titleDifficultyButton.Pressed += () => _tertiaryAction?.Invoke();

        actions.AddChild(_titleStartButton);
        actions.AddChild(_titleControlsButton);
        actions.AddChild(_titleDifficultyButton);

        card.AddChild(_logoPrimary);
        card.AddChild(_logoSecondary);
        card.AddChild(_logoCaption);
        card.AddChild(_titleAnimationArea);
        card.AddChild(_titleBody);
        card.AddChild(actions);
        _titlePanel.AddChild(card);
        _titleCenter.AddChild(_titlePanel);
        AddChild(_titleCenter);
    }

    private void BuildGenericOverlay()
    {
        _menuCenter.AnchorRight = 1;
        _menuCenter.AnchorBottom = 1;

        _panel.CustomMinimumSize = new Vector2(560, 0);
        GameUi.StyleOverlayPanel(_panel);

        var card = new VBoxContainer();
        card.AddThemeConstantOverride("separation", 12);

        GameUi.StyleAccent(_kicker);
        GameUi.StyleHeader(_title);
        _title.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _body.BbcodeEnabled = true;
        _body.FitContent = true;
        _body.CustomMinimumSize = new Vector2(520, 0);
        _body.ScrollActive = false;
        GameUi.StyleBody(_body);

        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", 12);

        GameUi.StyleButton(_primaryButton);
        GameUi.StyleButton(_secondaryButton);
        _primaryButton.Pressed += () => _primaryAction?.Invoke();
        _secondaryButton.Pressed += () => _secondaryAction?.Invoke();

        actions.AddChild(_primaryButton);
        actions.AddChild(_secondaryButton);

        card.AddChild(_kicker);
        card.AddChild(_title);
        card.AddChild(_body);
        card.AddChild(actions);
        _panel.AddChild(card);
        _menuCenter.AddChild(_panel);
        AddChild(_menuCenter);
    }

    private void BuildTimedCard()
    {
        _timedCenter.AnchorRight = 1;
        _timedCenter.AnchorBottom = 1;

        _timedPanel.CustomMinimumSize = new Vector2(360, 0);
        GameUi.StyleOverlayPanel(_timedPanel);

        var timedCard = new VBoxContainer();
        timedCard.AddThemeConstantOverride("separation", 10);

        _timedKicker.HorizontalAlignment = HorizontalAlignment.Center;
        GameUi.StyleAccent(_timedKicker);

        var portraitCenter = new CenterContainer
        {
            CustomMinimumSize = new Vector2(0, 96)
        };
        _timedPortrait.CustomMinimumSize = new Vector2(84, 96);
        _timedPortrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _timedPortrait.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        portraitCenter.AddChild(_timedPortrait);

        _timedTitle.HorizontalAlignment = HorizontalAlignment.Center;
        GameUi.StyleHeader(_timedTitle);

        _timedBody.BbcodeEnabled = true;
        _timedBody.FitContent = true;
        _timedBody.ScrollActive = false;
        _timedBody.CustomMinimumSize = new Vector2(280, 0);
        GameUi.StyleBody(_timedBody);

        timedCard.AddChild(_timedKicker);
        timedCard.AddChild(portraitCenter);
        timedCard.AddChild(_timedTitle);
        timedCard.AddChild(_timedBody);
        _timedPanel.AddChild(timedCard);
        _timedCenter.AddChild(_timedPanel);
        AddChild(_timedCenter);
    }

    private static void ConfigureTitleSprite(TextureRect sprite, Vector2 size)
    {
        sprite.CustomMinimumSize = size;
        sprite.Size = size;
        sprite.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        sprite.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        sprite.MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    private void UpdateTitleAnimation()
    {
        var areaSize = _titleAnimationArea.Size;
        if (areaSize.X <= 0f || areaSize.Y <= 0f)
        {
            areaSize = _titleAnimationArea.CustomMinimumSize;
        }

        var playerFrames = GameAssets.GetPlayerFrames(PlayerForm.Enhanced);
        _titleRunner.Texture = Mathf.PosMod(Mathf.FloorToInt(_titleAnimationTime * 12f), 2) == 0
            ? playerFrames.WalkA
            : playerFrames.WalkB;

        var enemyFrames = GameAssets.GetEnemyFrames(_titleEnemyKind);
        _titleChaser.Texture = Mathf.PosMod(Mathf.FloorToInt(_titleAnimationTime * 10f), 2) == 0
            ? enemyFrames.WalkA
            : enemyFrames.WalkB;

        var cycleWidth = areaSize.X + 300f;
        var runnerX = areaSize.X - Mathf.PosMod(_titleAnimationTime * 190f, cycleWidth);
        var runnerY = areaSize.Y - 114f;
        _titleRunner.Position = new Vector2(runnerX, runnerY);
        _titleRunner.FlipH = true;

        var chaserX = runnerX + 148f;
        var chaserY = areaSize.Y - 102f;
        _titleChaser.Position = new Vector2(chaserX, chaserY);
        _titleChaser.FlipH = true;
    }

    private void ShowTimedCard(Color backdropColor, string kicker, Texture2D portrait, string title, string body)
    {
        _backdrop.Color = backdropColor;
        _timedKicker.Text = kicker;
        _timedKicker.Visible = !string.IsNullOrWhiteSpace(kicker);
        _timedPortrait.Texture = portrait;
        _timedTitle.Text = title;
        _timedTitle.Visible = !string.IsNullOrWhiteSpace(title);
        _timedBody.Text = body;
        _timedBody.Visible = !string.IsNullOrWhiteSpace(body);

        _primaryAction = null;
        _secondaryAction = null;
        _tertiaryAction = null;
        AllowsPauseResume = false;
        _titleCenter.Visible = false;
        _menuCenter.Visible = false;
        _timedCenter.Visible = true;
        Visible = true;
    }
}

using Godot;

namespace GameTest;

public partial class OverlayLayer : CanvasLayer
{
    private const float TitleLogoIntroDuration = 0.55f;
    private static readonly Vector2 LogoSourceSize = new(430f, 368f);

    private static readonly EnemyKind[] TitleEnemyChoices =
    [
        EnemyKind.Ground,
        EnemyKind.Armored,
        EnemyKind.ProtectedHead
    ];

    private readonly ColorRect _backdrop = new();

    private readonly CenterContainer _titleCenter = new();
    private readonly PanelContainer _titlePanel = new();
    private readonly Control _titleLogoStage = new();
    private readonly TextureRect _titleLogo = new();
    private readonly Control _titleAnimationArea = new();
    private readonly TextureRect _titleRunner = new();
    private readonly TextureRect _titleChaser = new();
    private readonly RichTextLabel _titleBody = new();
    private readonly Button _titleStartButton = new();
    private readonly Button _titleControlsButton = new();
    private readonly Button _titleDifficultyButton = new();
    private readonly Button _titleLevelButton = new();

    private readonly CenterContainer _menuCenter = new();
    private readonly PanelContainer _panel = new();
    private readonly TextureRect _menuLogo = new();
    private readonly Label _kicker = new();
    private readonly Label _title = new();
    private readonly RichTextLabel _body = new();
    private readonly Button _primaryButton = new();
    private readonly Button _secondaryButton = new();

    private readonly CenterContainer _timedCenter = new();
    private readonly PanelContainer _timedPanel = new();
    private readonly TextureRect _timedLogo = new();
    private readonly Label _timedKicker = new();
    private readonly TextureRect _timedPortrait = new();
    private readonly Label _timedTitle = new();
    private readonly RichTextLabel _timedBody = new();

    private Action? _primaryAction;
    private Action? _secondaryAction;
    private Action? _tertiaryAction;
    private Action? _quaternaryAction;
    private float _titleAnimationTime;
    private EnemyKind _titleEnemyKind = EnemyKind.ProtectedHead;
    private bool _useCompactMenuLogo;

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

        if (GetViewport() is { } viewport)
        {
            viewport.SizeChanged += OnViewportSizeChanged;
        }

        RefreshResponsiveLayout();

        Visible = false;
    }

    public override void _ExitTree()
    {
        if (GetViewport() is { } viewport)
        {
            viewport.SizeChanged -= OnViewportSizeChanged;
        }
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

    public void ShowTitleScreen(string body, string difficultyText, string levelText, Action startAction, Action controlsAction, Action difficultyAction, Action levelAction)
    {
        RefreshResponsiveLayout();
        _backdrop.Color = new Color(0.03f, 0.04f, 0.08f, 0.98f);
        _titleLogo.Texture = GameAssets.GetTitleLogo();
        _titleBody.Text = body;
        _titleDifficultyButton.Text = difficultyText;
        _titleLevelButton.Text = levelText;

        _primaryAction = startAction;
        _secondaryAction = controlsAction;
        _tertiaryAction = difficultyAction;
        _quaternaryAction = levelAction;
        AllowsPauseResume = false;
        _titleAnimationTime = 0f;
        _titleEnemyKind = TitleEnemyChoices[GD.RandRange(0, TitleEnemyChoices.Length - 1)];
        _titleLogo.Modulate = new Color(1f, 1f, 1f, 0f);
        _titleLogo.Scale = Vector2.One * 0.9f;

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
        bool allowPauseResume = false,
        bool showLogo = false,
        bool compactLogo = false)
    {
        _useCompactMenuLogo = compactLogo;
        RefreshResponsiveLayout();
        _backdrop.Color = new Color(0.02f, 0.04f, 0.06f, 0.74f);
        _menuLogo.Texture = showLogo ? GameAssets.GetTitleLogo() : null;
        _menuLogo.Visible = showLogo;
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
        RefreshResponsiveLayout();
        var body = $"[center]Lives [b]x {lives}[/b]\nForm [b]{form}[/b][/center]";
        ShowTimedCard(new Color(0f, 0f, 0f, 1f), $"Stage {stageId}", GameAssets.GetPlayerFrames(form).Idle, "Get Ready", body, showLogo: false);
    }

    public void ShowStageSummary(string stageId, PlayerForm form, int score, int coins, int lives, int timeRemaining)
    {
        RefreshResponsiveLayout();
        var body = $"[center]Score [b]{score:000000}[/b]\nCoins [b]{coins:00}[/b]\nLives [b]{lives}[/b]\nTime [b]{timeRemaining:000}[/b]\nForm [b]{form}[/b][/center]";
        ShowTimedCard(new Color(0f, 0f, 0f, 0.92f), $"Stage {stageId} Clear", GameAssets.GetPlayerFrames(form).Idle, "Run Summary", body, showLogo: false);
    }

    public void HideOverlay()
    {
        Visible = false;
        AllowsPauseResume = false;
        _primaryAction = null;
        _secondaryAction = null;
        _tertiaryAction = null;
        _quaternaryAction = null;
        _titleCenter.Visible = false;
        _menuCenter.Visible = false;
        _timedCenter.Visible = false;
    }

    private void BuildTitleScreen()
    {
        _titleCenter.AnchorRight = 1;
        _titleCenter.AnchorBottom = 1;

        _titlePanel.CustomMinimumSize = new Vector2(960, 0);
        GameUi.StyleOverlayPanel(_titlePanel);

        var card = new VBoxContainer();
        card.AddThemeConstantOverride("separation", 18);

        _titleLogoStage.CustomMinimumSize = new Vector2(460, 360);
        _titleLogoStage.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        _titleLogoStage.MouseFilter = Control.MouseFilterEnum.Ignore;

        _titleLogo.CustomMinimumSize = new Vector2(430, 368);
        _titleLogo.Size = new Vector2(430, 368);
        _titleLogo.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        _titleLogo.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _titleLogo.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        _titleLogo.MouseFilter = Control.MouseFilterEnum.Ignore;
        _titleLogo.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
        _titleLogo.Texture = GameAssets.GetTitleLogo();
        _titleLogoStage.AddChild(_titleLogo);

        _titleAnimationArea.CustomMinimumSize = new Vector2(840, 184);
        _titleAnimationArea.MouseFilter = Control.MouseFilterEnum.Ignore;
        _titleAnimationArea.ClipContents = true;

        ConfigureTitleSprite(_titleRunner, new Vector2(132f, 132f));
        ConfigureTitleSprite(_titleChaser, new Vector2(124f, 124f));
        _titleAnimationArea.AddChild(_titleRunner);
        _titleAnimationArea.AddChild(_titleChaser);

        _titleBody.BbcodeEnabled = true;
        _titleBody.FitContent = true;
        _titleBody.ScrollActive = false;
        _titleBody.CustomMinimumSize = new Vector2(780, 0);
        GameUi.StyleBody(_titleBody);

        var actions = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center
        };
        actions.AddThemeConstantOverride("separation", 14);

        GameUi.StyleButton(_titleStartButton);
        GameUi.StyleButton(_titleControlsButton);
        GameUi.StyleButton(_titleDifficultyButton);
        GameUi.StyleButton(_titleLevelButton);
        _titleDifficultyButton.CustomMinimumSize = new Vector2(236, 50);
        _titleLevelButton.CustomMinimumSize = new Vector2(196, 50);
        _titleStartButton.Text = "Start Game";
        _titleControlsButton.Text = "Controls";

        _titleStartButton.Pressed += () => _primaryAction?.Invoke();
        _titleControlsButton.Pressed += () => _secondaryAction?.Invoke();
        _titleDifficultyButton.Pressed += () => _tertiaryAction?.Invoke();
        _titleLevelButton.Pressed += () => _quaternaryAction?.Invoke();

        actions.AddChild(_titleStartButton);
        actions.AddChild(_titleControlsButton);
        actions.AddChild(_titleDifficultyButton);
        actions.AddChild(_titleLevelButton);

        card.AddChild(_titleLogoStage);
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

        ConfigureOverlayLogo(_menuLogo, new Vector2(220, 188));

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

        card.AddChild(_menuLogo);
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

        ConfigureOverlayLogo(_timedLogo, new Vector2(220, 188));

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

        timedCard.AddChild(_timedLogo);
        timedCard.AddChild(_timedKicker);
        timedCard.AddChild(portraitCenter);
        timedCard.AddChild(_timedTitle);
        timedCard.AddChild(_timedBody);
        _timedPanel.AddChild(timedCard);
        _timedCenter.AddChild(_timedPanel);
        AddChild(_timedCenter);
    }

    private static void ConfigureOverlayLogo(TextureRect logo, Vector2 size)
    {
        logo.CustomMinimumSize = size;
        logo.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        logo.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        logo.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        logo.MouseFilter = Control.MouseFilterEnum.Ignore;
        logo.Visible = false;
    }

    private void OnViewportSizeChanged()
    {
        RefreshResponsiveLayout();
        if (_titleCenter.Visible)
        {
            UpdateTitleAnimation();
        }
    }

    private void RefreshResponsiveLayout()
    {
        var viewportSize = GetViewport()?.GetVisibleRect().Size ?? Vector2.Zero;
        if (viewportSize.X <= 0f || viewportSize.Y <= 0f)
        {
            return;
        }

        var titlePanelWidth = Mathf.Clamp(viewportSize.X * 0.82f, 720f, 1100f);
        var titleLogoSize = GetScaledLogoSize(
            Mathf.Clamp(titlePanelWidth * 0.46f, 260f, 430f),
            Mathf.Clamp(viewportSize.Y * 0.40f, 190f, 368f));
        var titleStageSize = titleLogoSize + new Vector2(32f, 24f);
        var titleAnimationWidth = Mathf.Clamp(titlePanelWidth - 120f, 560f, 900f);
        var titleAnimationHeight = Mathf.Clamp(viewportSize.Y * 0.24f, 140f, 220f);
        var titleBodyWidth = Mathf.Clamp(titlePanelWidth - 180f, 520f, 860f);

        _titlePanel.CustomMinimumSize = new Vector2(titlePanelWidth, 0f);
        _titleLogoStage.CustomMinimumSize = titleStageSize;
        _titleLogo.CustomMinimumSize = titleLogoSize;
        _titleLogo.Size = titleLogoSize;
        _titleAnimationArea.CustomMinimumSize = new Vector2(titleAnimationWidth, titleAnimationHeight);
        _titleBody.CustomMinimumSize = new Vector2(titleBodyWidth, 0f);
        _titleDifficultyButton.CustomMinimumSize = new Vector2(Mathf.Clamp(titlePanelWidth * 0.22f, 180f, 260f), 50f);
        _titleLevelButton.CustomMinimumSize = new Vector2(Mathf.Clamp(titlePanelWidth * 0.18f, 160f, 220f), 50f);

        var menuPanelWidth = Mathf.Clamp(viewportSize.X * 0.52f, 420f, 700f);
        var menuLogoSize = _useCompactMenuLogo
            ? GetScaledLogoSize(
                Mathf.Clamp(menuPanelWidth * 0.20f, 88f, 136f),
                Mathf.Clamp(viewportSize.Y * 0.11f, 72f, 112f))
            : GetScaledLogoSize(
                Mathf.Clamp(menuPanelWidth * 0.28f, 110f, 180f),
                Mathf.Clamp(viewportSize.Y * 0.16f, 90f, 150f));
        _panel.CustomMinimumSize = new Vector2(menuPanelWidth, 0f);
        _body.CustomMinimumSize = new Vector2(Mathf.Clamp(menuPanelWidth - 40f, 380f, 660f), 0f);
        _menuLogo.CustomMinimumSize = menuLogoSize;

        var timedPanelWidth = Mathf.Clamp(viewportSize.X * 0.40f, 340f, 560f);
        var timedLogoSize = GetScaledLogoSize(
            Mathf.Clamp(timedPanelWidth * 0.55f, 160f, 300f),
            Mathf.Clamp(viewportSize.Y * 0.22f, 120f, 220f));
        _timedPanel.CustomMinimumSize = new Vector2(timedPanelWidth, 0f);
        _timedBody.CustomMinimumSize = new Vector2(Mathf.Clamp(timedPanelWidth - 80f, 260f, 500f), 0f);
        _timedLogo.CustomMinimumSize = timedLogoSize;
    }

    private static Vector2 GetScaledLogoSize(float maxWidth, float maxHeight)
    {
        var scale = Mathf.Min(maxWidth / LogoSourceSize.X, maxHeight / LogoSourceSize.Y);
        scale = Mathf.Max(scale, 0.1f);
        return new Vector2(
            Mathf.Round(LogoSourceSize.X * scale),
            Mathf.Round(LogoSourceSize.Y * scale));
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
        var logoAreaSize = _titleLogoStage.Size;
        if (logoAreaSize.X <= 0f || logoAreaSize.Y <= 0f)
        {
            logoAreaSize = _titleLogoStage.CustomMinimumSize;
        }

        var introProgress = Mathf.Clamp(_titleAnimationTime / TitleLogoIntroDuration, 0f, 1f);
        var easedProgress = 1f - Mathf.Pow(1f - introProgress, 3f);
        var logoBaseSize = _titleLogo.CustomMinimumSize;
        var logoTargetX = (logoAreaSize.X - logoBaseSize.X) * 0.5f;
        var logoTargetY = (logoAreaSize.Y - logoBaseSize.Y) * 0.5f;
        var logoStartX = logoTargetX - 96f;
        var logoBob = Mathf.Sin(_titleAnimationTime * 2.4f) * 4f;
        var logoScale = 0.92f + easedProgress * 0.08f + Mathf.Sin(_titleAnimationTime * 3.1f) * 0.015f;

        _titleLogo.PivotOffset = logoBaseSize * 0.5f;
        _titleLogo.Position = new Vector2(Mathf.Lerp(logoStartX, logoTargetX, easedProgress), logoTargetY + logoBob);
        _titleLogo.Scale = Vector2.One * logoScale;
        _titleLogo.Modulate = new Color(1f, 1f, 1f, 0.35f + easedProgress * 0.65f);

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

    private void ShowTimedCard(Color backdropColor, string kicker, Texture2D portrait, string title, string body, bool showLogo)
    {
        _backdrop.Color = backdropColor;
        _timedLogo.Texture = showLogo ? GameAssets.GetTitleLogo() : null;
        _timedLogo.Visible = showLogo;
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

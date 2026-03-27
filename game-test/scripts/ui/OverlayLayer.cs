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

    private static readonly EnemyKind[] EnemyGuideOrder =
    [
        EnemyKind.Ground,
        EnemyKind.Armored,
        EnemyKind.ProtectedHead,
        EnemyKind.Flying,
        EnemyKind.Shooter
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
    private readonly Button _titleEnemiesButton = new();
    private readonly Button _titleConfigButton = new();
    private readonly Button _titleDifficultyButton = new();

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

    private readonly CenterContainer _enemyGuideCenter = new();
    private readonly PanelContainer _enemyGuidePanel = new();
    private readonly Label _enemyGuideKicker = new();
    private readonly Label _enemyGuideTitle = new();
    private readonly ScrollContainer _enemyGuideScroll = new();
    private readonly VBoxContainer _enemyGuideList = new();
    private readonly Button _enemyGuideBackButton = new();

    private readonly CenterContainer _settingsCenter = new();
    private readonly PanelContainer _settingsPanel = new();
    private readonly Label _settingsKicker = new();
    private readonly Label _settingsTitle = new();
    private readonly Label _musicValue = new();
    private readonly Label _sfxValue = new();
    private readonly Button _musicDownButton = new();
    private readonly Button _musicUpButton = new();
    private readonly Button _sfxDownButton = new();
    private readonly Button _sfxUpButton = new();
    private readonly Button _settingsBackButton = new();

    private Action? _primaryAction;
    private Action? _secondaryAction;
    private Action? _tertiaryAction;
    private Action? _quaternaryAction;
    private Action? _quinaryAction;
    private Action? _musicDownAction;
    private Action? _musicUpAction;
    private Action? _sfxDownAction;
    private Action? _sfxUpAction;
    private Action? _settingsBackAction;
    private Button? _lastHighlightedButton;
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
        BuildEnemyGuide();
        BuildSettingsMenu();

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

    public void ShowTitleScreen(string body, string difficultyText, Action startAction, Action controlsAction, Action enemiesAction, Action configAction, Action difficultyAction)
    {
        RefreshResponsiveLayout();
        _backdrop.Color = new Color(0.03f, 0.04f, 0.08f, 0.98f);
        _titleLogo.Texture = GameAssets.GetTitleLogo();
        _titleBody.Text = body;
        _titleDifficultyButton.Text = difficultyText;

        _primaryAction = startAction;
        _secondaryAction = controlsAction;
        _tertiaryAction = enemiesAction;
        _quaternaryAction = configAction;
        _quinaryAction = difficultyAction;
        _musicDownAction = null;
        _musicUpAction = null;
        _sfxDownAction = null;
        _sfxUpAction = null;
        _settingsBackAction = null;
        _lastHighlightedButton = null;
        AllowsPauseResume = false;
        _titleAnimationTime = 0f;
        _titleEnemyKind = TitleEnemyChoices[GD.RandRange(0, TitleEnemyChoices.Length - 1)];
        _titleLogo.Modulate = new Color(1f, 1f, 1f, 0f);
        _titleLogo.Scale = Vector2.One * 0.9f;

        _titleCenter.Visible = true;
        _menuCenter.Visible = false;
        _timedCenter.Visible = false;
        _enemyGuideCenter.Visible = false;
        _settingsCenter.Visible = false;
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
        _quinaryAction = null;
        _lastHighlightedButton = null;
        AllowsPauseResume = allowPauseResume;

        _titleCenter.Visible = false;
        _menuCenter.Visible = true;
        _timedCenter.Visible = false;
        _enemyGuideCenter.Visible = false;
        _settingsCenter.Visible = false;
        Visible = true;
    }

    public void ShowEnemyGuide(Action backAction)
    {
        RefreshResponsiveLayout();
        _backdrop.Color = new Color(0.03f, 0.04f, 0.08f, 0.98f);
        _primaryAction = backAction;
        _secondaryAction = null;
        _tertiaryAction = null;
        _quaternaryAction = null;
        _quinaryAction = null;
        _lastHighlightedButton = null;
        AllowsPauseResume = false;

        _titleCenter.Visible = false;
        _menuCenter.Visible = false;
        _timedCenter.Visible = false;
        _enemyGuideCenter.Visible = true;
        _settingsCenter.Visible = false;
        Visible = true;
    }

    public void ShowSettings(
        string musicValue,
        string sfxValue,
        Action musicDownAction,
        Action musicUpAction,
        Action sfxDownAction,
        Action sfxUpAction,
        Action backAction)
    {
        RefreshResponsiveLayout();
        _backdrop.Color = new Color(0.03f, 0.04f, 0.08f, 0.98f);
        _musicValue.Text = musicValue;
        _sfxValue.Text = sfxValue;
        _musicDownAction = musicDownAction;
        _musicUpAction = musicUpAction;
        _sfxDownAction = sfxDownAction;
        _sfxUpAction = sfxUpAction;
        _settingsBackAction = backAction;
        _lastHighlightedButton = null;
        AllowsPauseResume = false;

        _titleCenter.Visible = false;
        _menuCenter.Visible = false;
        _timedCenter.Visible = false;
        _enemyGuideCenter.Visible = false;
        _settingsCenter.Visible = true;
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
        _quinaryAction = null;
        _musicDownAction = null;
        _musicUpAction = null;
        _sfxDownAction = null;
        _sfxUpAction = null;
        _settingsBackAction = null;
        _titleCenter.Visible = false;
        _menuCenter.Visible = false;
        _timedCenter.Visible = false;
        _enemyGuideCenter.Visible = false;
        _settingsCenter.Visible = false;
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

        var actions = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
        };
        actions.AddThemeConstantOverride("separation", 12);

        var primaryRow = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
        };
        primaryRow.AddThemeConstantOverride("separation", 14);

        var secondaryRow = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
        };
        secondaryRow.AddThemeConstantOverride("separation", 14);

        GameUi.StyleButton(_titleStartButton);
        GameUi.StyleButton(_titleControlsButton);
        GameUi.StyleButton(_titleEnemiesButton);
        GameUi.StyleButton(_titleConfigButton);
        GameUi.StyleButton(_titleDifficultyButton);
        RegisterMenuButton(_titleStartButton);
        RegisterMenuButton(_titleControlsButton);
        RegisterMenuButton(_titleEnemiesButton);
        RegisterMenuButton(_titleConfigButton);
        RegisterMenuButton(_titleDifficultyButton);
        _titleStartButton.Text = "Start Game";
        _titleControlsButton.Text = "Controls";
        _titleEnemiesButton.Text = "Enemies";
        _titleConfigButton.Text = "Config";

        _titleStartButton.Pressed += () => InvokeMenuAction(_primaryAction);
        _titleControlsButton.Pressed += () => InvokeMenuAction(_secondaryAction);
        _titleEnemiesButton.Pressed += () => InvokeMenuAction(_tertiaryAction);
        _titleConfigButton.Pressed += () => InvokeMenuAction(_quaternaryAction);
        _titleDifficultyButton.Pressed += () => InvokeMenuAction(_quinaryAction);

        primaryRow.AddChild(_titleStartButton);
        primaryRow.AddChild(_titleControlsButton);
        primaryRow.AddChild(_titleEnemiesButton);
        secondaryRow.AddChild(_titleConfigButton);
        secondaryRow.AddChild(_titleDifficultyButton);
        actions.AddChild(primaryRow);
        actions.AddChild(secondaryRow);

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
        RegisterMenuButton(_primaryButton);
        RegisterMenuButton(_secondaryButton);
        _primaryButton.Pressed += () => InvokeMenuAction(_primaryAction);
        _secondaryButton.Pressed += () => InvokeMenuAction(_secondaryAction);

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

    private void BuildEnemyGuide()
    {
        _enemyGuideCenter.AnchorRight = 1;
        _enemyGuideCenter.AnchorBottom = 1;

        _enemyGuidePanel.CustomMinimumSize = new Vector2(820, 0);
        GameUi.StyleOverlayPanel(_enemyGuidePanel);

        var card = new VBoxContainer();
        card.AddThemeConstantOverride("separation", 14);

        _enemyGuideKicker.HorizontalAlignment = HorizontalAlignment.Center;
        _enemyGuideKicker.Text = "Enemy Guide";
        GameUi.StyleAccent(_enemyGuideKicker);

        _enemyGuideTitle.HorizontalAlignment = HorizontalAlignment.Center;
        _enemyGuideTitle.Text = "Know What Each Foe Does";
        GameUi.StyleHeader(_enemyGuideTitle);

        _enemyGuideScroll.CustomMinimumSize = new Vector2(720, 360);
        _enemyGuideScroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        _enemyGuideScroll.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _enemyGuideScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        _enemyGuideList.AddThemeConstantOverride("separation", 12);
        _enemyGuideList.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        foreach (var enemyKind in EnemyGuideOrder)
        {
            _enemyGuideList.AddChild(CreateEnemyGuideCard(enemyKind));
        }

        _enemyGuideScroll.AddChild(_enemyGuideList);

        GameUi.StyleButton(_enemyGuideBackButton);
        RegisterMenuButton(_enemyGuideBackButton);
        _enemyGuideBackButton.Text = "Back";
        _enemyGuideBackButton.Pressed += () => InvokeMenuAction(_primaryAction);

        card.AddChild(_enemyGuideKicker);
        card.AddChild(_enemyGuideTitle);
        card.AddChild(_enemyGuideScroll);
        card.AddChild(_enemyGuideBackButton);

        _enemyGuidePanel.AddChild(card);
        _enemyGuideCenter.AddChild(_enemyGuidePanel);
        AddChild(_enemyGuideCenter);
    }

    private void BuildSettingsMenu()
    {
        _settingsCenter.AnchorRight = 1;
        _settingsCenter.AnchorBottom = 1;

        _settingsPanel.CustomMinimumSize = new Vector2(560, 0);
        GameUi.StyleOverlayPanel(_settingsPanel);

        var card = new VBoxContainer();
        card.AddThemeConstantOverride("separation", 14);

        _settingsKicker.HorizontalAlignment = HorizontalAlignment.Center;
        _settingsKicker.Text = "Configuration";
        GameUi.StyleAccent(_settingsKicker);

        _settingsTitle.HorizontalAlignment = HorizontalAlignment.Center;
        _settingsTitle.Text = "Audio Mix";
        GameUi.StyleHeader(_settingsTitle);

        card.AddChild(_settingsKicker);
        card.AddChild(_settingsTitle);
        card.AddChild(BuildSettingsRow("Music", _musicValue, _musicDownButton, _musicUpButton));
        card.AddChild(BuildSettingsRow("SFX", _sfxValue, _sfxDownButton, _sfxUpButton));

        GameUi.StyleButton(_settingsBackButton);
        RegisterMenuButton(_settingsBackButton);
        _settingsBackButton.Text = "Back";
        _settingsBackButton.Pressed += () => InvokeMenuAction(_settingsBackAction);
        card.AddChild(_settingsBackButton);

        _settingsPanel.AddChild(card);
        _settingsCenter.AddChild(_settingsPanel);
        AddChild(_settingsCenter);

        _musicDownButton.Pressed += () => InvokeMenuAction(_musicDownAction);
        _musicUpButton.Pressed += () => InvokeMenuAction(_musicUpAction);
        _sfxDownButton.Pressed += () => InvokeMenuAction(_sfxDownAction);
        _sfxUpButton.Pressed += () => InvokeMenuAction(_sfxUpAction);
    }

    private Control BuildSettingsRow(string labelText, Label valueLabel, Button downButton, Button upButton)
    {
        var panel = new PanelContainer();
        GameUi.StyleOverlayPanel(panel);

        var row = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        row.AddThemeConstantOverride("separation", 12);

        var label = new Label
        {
            Text = labelText.ToUpperInvariant(),
            CustomMinimumSize = new Vector2(96, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        GameUi.StylePixelCaption(label);

        GameUi.StyleButton(downButton);
        RegisterMenuButton(downButton);
        downButton.Text = "-";
        downButton.CustomMinimumSize = new Vector2(56, 44);

        valueLabel.Text = "---";
        valueLabel.CustomMinimumSize = new Vector2(128, 0);
        valueLabel.HorizontalAlignment = HorizontalAlignment.Center;
        GameUi.StyleHudValue(valueLabel);

        GameUi.StyleButton(upButton);
        RegisterMenuButton(upButton);
        upButton.Text = "+";
        upButton.CustomMinimumSize = new Vector2(56, 44);

        row.AddChild(label);
        row.AddChild(downButton);
        row.AddChild(valueLabel);
        row.AddChild(upButton);
        panel.AddChild(row);
        return panel;
    }

    private Control CreateEnemyGuideCard(EnemyKind kind)
    {
        var cardPanel = new PanelContainer();
        GameUi.StyleOverlayPanel(cardPanel);

        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 16);
        row.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        var portraitWrap = new CenterContainer
        {
            CustomMinimumSize = new Vector2(104, 104),
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
        };

        var portrait = new TextureRect
        {
            CustomMinimumSize = new Vector2(84, 84),
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            Texture = GetEnemyGuideTexture(kind)
        };
        portraitWrap.AddChild(portrait);

        var textColumn = new VBoxContainer();
        textColumn.AddThemeConstantOverride("separation", 6);
        textColumn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        var name = new Label
        {
            Text = GetEnemyGuideTitle(kind),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        GameUi.StylePixelCaption(name);

        var body = new RichTextLabel
        {
            BbcodeEnabled = true,
            FitContent = true,
            ScrollActive = false,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            Text = GetEnemyGuideBody(kind)
        };
        GameUi.StyleBody(body);

        textColumn.AddChild(name);
        textColumn.AddChild(body);

        row.AddChild(portraitWrap);
        row.AddChild(textColumn);
        cardPanel.AddChild(row);
        return cardPanel;
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
        var titlePrimaryWidth = Mathf.Clamp((titlePanelWidth - 84f) / 3f, 148f, 220f);
        var titleSecondaryWidth = Mathf.Clamp((titlePanelWidth - 70f) / 2f, 168f, 260f);
        _titleStartButton.CustomMinimumSize = new Vector2(titlePrimaryWidth, 50f);
        _titleControlsButton.CustomMinimumSize = new Vector2(titlePrimaryWidth, 50f);
        _titleEnemiesButton.CustomMinimumSize = new Vector2(titlePrimaryWidth, 50f);
        _titleConfigButton.CustomMinimumSize = new Vector2(titleSecondaryWidth, 50f);
        _titleDifficultyButton.CustomMinimumSize = new Vector2(titleSecondaryWidth, 50f);

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

        var enemyGuidePanelWidth = Mathf.Clamp(viewportSize.X * 0.78f, 620f, 980f);
        _enemyGuidePanel.CustomMinimumSize = new Vector2(enemyGuidePanelWidth, 0f);
        _enemyGuideScroll.CustomMinimumSize = new Vector2(enemyGuidePanelWidth - 64f, Mathf.Clamp(viewportSize.Y * 0.52f, 300f, 440f));

        var settingsPanelWidth = Mathf.Clamp(viewportSize.X * 0.44f, 420f, 640f);
        _settingsPanel.CustomMinimumSize = new Vector2(settingsPanelWidth, 0f);
        _musicValue.CustomMinimumSize = new Vector2(Mathf.Clamp(settingsPanelWidth * 0.26f, 120f, 168f), 0f);
        _sfxValue.CustomMinimumSize = new Vector2(Mathf.Clamp(settingsPanelWidth * 0.26f, 120f, 168f), 0f);
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
        var runnerSize = _titleRunner.Size;
        if (runnerSize.X <= 0f || runnerSize.Y <= 0f)
        {
            runnerSize = _titleRunner.CustomMinimumSize;
        }

        var chaserSize = _titleChaser.Size;
        if (chaserSize.X <= 0f || chaserSize.Y <= 0f)
        {
            chaserSize = _titleChaser.CustomMinimumSize;
        }

        var runnerY = areaSize.Y - runnerSize.Y - 10f;
        _titleRunner.Position = new Vector2(runnerX, runnerY);
        _titleRunner.FlipH = true;

        var chaserX = runnerX + 148f;
        var chaserY = areaSize.Y - chaserSize.Y - 12f;
        _titleChaser.Position = new Vector2(chaserX, chaserY);
        _titleChaser.FlipH = false;
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
        _quaternaryAction = null;
        _quinaryAction = null;
        _lastHighlightedButton = null;
        AllowsPauseResume = false;
        _titleCenter.Visible = false;
        _menuCenter.Visible = false;
        _timedCenter.Visible = true;
        _enemyGuideCenter.Visible = false;
        _settingsCenter.Visible = false;
        Visible = true;
    }

    private static Texture2D GetEnemyGuideTexture(EnemyKind kind)
    {
        var frames = GameAssets.GetEnemyFrames(kind);
        return kind switch
        {
            EnemyKind.Flying => frames.WalkA,
            EnemyKind.ProtectedHead => frames.WalkA,
            EnemyKind.Shooter => frames.WalkA,
            _ => frames.Idle
        };
    }

    private static string GetEnemyGuideTitle(EnemyKind kind) => kind switch
    {
        EnemyKind.Ground => "Ground Slime",
        EnemyKind.Armored => "Armored Shell",
        EnemyKind.ProtectedHead => "Spike Slime",
        EnemyKind.Flying => "Flying Slime",
        EnemyKind.Shooter => "Barnacle Shooter",
        _ => kind.ToString()
    };

    private static string GetEnemyGuideBody(EnemyKind kind) => kind switch
    {
        EnemyKind.Ground => "[b]Behavior:[/b] Walks left and right across platforms and turns around at edges or patrol limits.\n[b]Weakness:[/b] Safe to stomp from above or hit with your attack when enhanced.",
        EnemyKind.Armored => "[b]Behavior:[/b] Slower than the basic walker, but it keeps patrolling ground platforms.\n[b]Power:[/b] Your projectile is reflected, and you cannot stomp it safely. Jump over it or avoid tight fights.",
        EnemyKind.ProtectedHead => "[b]Behavior:[/b] A faster ground patrol enemy with a dangerous protected top.\n[b]Power:[/b] Do not land on it from above. Attack it from the side when you have the enhanced shot.",
        EnemyKind.Flying => "[b]Behavior:[/b] Floats through the air in a wave pattern instead of following the ground.\n[b]Power:[/b] It ignores pits and platform edges, so watch its vertical movement before jumping.",
        EnemyKind.Shooter => "[b]Behavior:[/b] Holds position, aims in one direction, and fires enemy projectiles on a regular rhythm.\n[b]Power:[/b] It controls space from far away, so close distance carefully or attack between shots.",
        _ => string.Empty
    };

    private void RegisterMenuButton(Button button)
    {
        button.FocusEntered += () => PlayMenuFocus(button);
        button.MouseEntered += () => PlayMenuFocus(button);
    }

    private void PlayMenuFocus(Button button)
    {
        if (_lastHighlightedButton == button)
        {
            return;
        }

        _lastHighlightedButton = button;
        AudioDirector.Instance.PlayUi("menu_focus");
    }

    private static void InvokeMenuAction(Action? action)
    {
        if (action is null)
        {
            return;
        }

        AudioDirector.Instance.PlayUi("menu_button");
        action.Invoke();
    }
}

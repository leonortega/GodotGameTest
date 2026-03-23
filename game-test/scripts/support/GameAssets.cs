using Godot;

namespace GameTest;

public static class GameAssets
{
    private const string ArtRoot = "res://assets/art/kenney_new_platformer_pack";
    private const string UiRoot = "res://assets/ui/kenney_ui-pack-pixel-adventure";
    private const string FontRoot = "res://assets/fonts/Silkscreen";

    private static readonly Dictionary<string, Texture2D> FullTextureCache = [];
    private static readonly Dictionary<string, Texture2D> TrimmedTextureCache = [];
    private static readonly Dictionary<string, Font> FontCache = [];

    public readonly record struct PlayerFrames(Texture2D Idle, Texture2D WalkA, Texture2D WalkB, Texture2D Jump, Texture2D Hit, Texture2D Duck);
    public readonly record struct EnemyFrames(Texture2D Idle, Texture2D WalkA, Texture2D WalkB, Texture2D Alternate);

    public static PlayerFrames GetPlayerFrames(PlayerForm form)
    {
        var color = form switch
        {
            PlayerForm.Powered => "green",
            PlayerForm.Enhanced => "purple",
            _ => "beige"
        };

        var root = $"{ArtRoot}/Sprites/Characters/Default";
        return new PlayerFrames(
            GetTrimmedTexture($"{root}/character_{color}_idle.png"),
            GetTrimmedTexture($"{root}/character_{color}_walk_a.png"),
            GetTrimmedTexture($"{root}/character_{color}_walk_b.png"),
            GetTrimmedTexture($"{root}/character_{color}_jump.png"),
            GetTrimmedTexture($"{root}/character_{color}_hit.png"),
            GetTrimmedTexture($"{root}/character_{color}_duck.png"));
    }

    public static EnemyFrames GetEnemyFrames(EnemyKind kind)
    {
        var root = $"{ArtRoot}/Sprites/Enemies/Default";
        return kind switch
        {
            EnemyKind.Armored => new EnemyFrames(
                GetTrimmedTexture($"{ArtRoot}/Sprites/Enemies/Double/snail_shell.png"),
                GetTrimmedTexture($"{ArtRoot}/Sprites/Enemies/Double/snail_shell.png"),
                GetTrimmedTexture($"{ArtRoot}/Sprites/Enemies/Double/snail_shell.png"),
                GetTrimmedTexture($"{ArtRoot}/Sprites/Enemies/Double/snail_shell.png")),
            EnemyKind.ProtectedHead => new EnemyFrames(
                GetTrimmedTexture($"{ArtRoot}/Sprites/Enemies/Double/slime_spike_rest.png"),
                GetTrimmedTexture($"{ArtRoot}/Sprites/Enemies/Double/slime_spike_walk_a.png"),
                GetTrimmedTexture($"{ArtRoot}/Sprites/Enemies/Double/slime_spike_walk_b.png"),
                GetTrimmedTexture($"{ArtRoot}/Sprites/Enemies/Double/slime_spike_flat.png")),
            EnemyKind.Shooter => new EnemyFrames(
                GetTrimmedTexture($"{root}/barnacle_attack_rest.png"),
                GetTrimmedTexture($"{root}/barnacle_attack_a.png"),
                GetTrimmedTexture($"{root}/barnacle_attack_b.png"),
                GetTrimmedTexture($"{root}/barnacle_attack_b.png")),
            EnemyKind.Flying => new EnemyFrames(
                GetTrimmedTexture($"{root}/slime_fire_rest.png"),
                GetTrimmedTexture($"{root}/slime_fire_walk_a.png"),
                GetTrimmedTexture($"{root}/slime_fire_walk_b.png"),
                GetTrimmedTexture($"{root}/slime_fire_flat.png")),
            _ => new EnemyFrames(
                GetTrimmedTexture($"{root}/slime_block_rest.png"),
                GetTrimmedTexture($"{root}/slime_block_walk_a.png"),
                GetTrimmedTexture($"{root}/slime_block_walk_b.png"),
                GetTrimmedTexture($"{root}/slime_block_jump.png"))
        };
    }

    public static Texture2D GetPickupTexture(PickupType pickupType, bool alternateFrame = false) => pickupType switch
    {
        PickupType.Growth => GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/mushroom_red.png"),
        PickupType.Flame => GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/gem_red.png"),
        PickupType.ExtraLife => GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/gem_green.png"),
        _ => GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/{(alternateFrame ? "coin_gold_side" : "coin_gold")}.png")
    };

    public static Texture2D GetBlockTexture(PickupType reward, bool activated)
    {
        var root = $"{ArtRoot}/Sprites/Tiles/Default";
        if (activated)
        {
            return GetTexture($"{root}/block_empty.png");
        }

        return reward == PickupType.Coin
            ? GetTexture($"{root}/block_coin.png")
            : GetTexture($"{root}/block_exclamation.png");
    }

    public static Texture2D GetGoalFlagTexture(bool alternateFrame) =>
        GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/{(alternateFrame ? "flag_green_b" : "flag_green_a")}.png");

    public static Texture2D GetProjectileTexture() =>
        GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/fireball.png");

    public static Texture2D GetEnemyProjectileTexture() =>
        GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/gem_red.png");

    public static Texture2D GetFallingBlockTexture(StageTheme theme) => GetTexture(theme switch
    {
        StageTheme.Cave => $"{ArtRoot}/Sprites/Tiles/Default/bricks_grey.png",
        StageTheme.Fortress => $"{ArtRoot}/Sprites/Tiles/Default/bricks_grey.png",
        _ => $"{ArtRoot}/Sprites/Tiles/Default/bricks_brown.png"
    });

    public static Texture2D GetCactusTexture() =>
        GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Double/cactus.png");

    public static Texture2D GetHudCoinTexture() =>
        GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/hud_coin.png");

    public static Texture2D GetScoreIcon() =>
        GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/gem_yellow.png");

    public static Texture2D GetTimeIcon() =>
        GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/hud_key_blue.png");

    public static Texture2D GetHeartTexture(bool empty) =>
        GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/{(empty ? "hud_heart_empty" : "hud_heart")}.png");

    public static Texture2D GetStageFlagIcon() =>
        GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/sign_exit.png");

    public static Texture2D GetStarIcon() =>
        GetTrimmedTexture($"{ArtRoot}/Sprites/Tiles/Default/star.png");

    public static Texture2D GetTitleLogo() =>
        GetTexture("res://assets/ui/PixelQuest_Logo.png");

    public static Font GetUiFont(bool bold = false) => GetFont($"{FontRoot}/Silkscreen-{(bold ? "Bold" : "Regular")}.ttf");

    public static Texture2D GetBackdropBase(StageTheme theme) => GetTexture(theme switch
    {
        StageTheme.Cave => $"{ArtRoot}/Sprites/Backgrounds/Default/background_solid_dirt.png",
        StageTheme.Treetop => $"{ArtRoot}/Sprites/Backgrounds/Default/background_solid_sky.png",
        StageTheme.Fortress => $"{ArtRoot}/Sprites/Backgrounds/Default/background_solid_cloud.png",
        _ => $"{ArtRoot}/Sprites/Backgrounds/Default/background_solid_sky.png"
    });

    public static Texture2D GetBackdropMid(StageTheme theme) => GetTexture(theme switch
    {
        StageTheme.Cave => $"{ArtRoot}/Sprites/Backgrounds/Default/background_color_hills.png",
        StageTheme.Treetop => $"{ArtRoot}/Sprites/Backgrounds/Default/background_fade_trees.png",
        StageTheme.Fortress => $"{ArtRoot}/Sprites/Backgrounds/Default/background_fade_desert.png",
        _ => $"{ArtRoot}/Sprites/Backgrounds/Default/background_fade_hills.png"
    });

    public static Texture2D GetBackdropClouds(StageTheme theme) => GetTexture(theme switch
    {
        StageTheme.Cave => $"{ArtRoot}/Sprites/Backgrounds/Default/background_clouds.png",
        StageTheme.Fortress => $"{ArtRoot}/Sprites/Backgrounds/Default/background_color_hills.png",
        _ => $"{ArtRoot}/Sprites/Backgrounds/Default/background_clouds.png"
    });

    public static StyleBoxTexture CreateUiStyleBox(UiChrome chrome)
    {
        var texture = GetTexture(chrome switch
        {
            UiChrome.ButtonHover => $"{UiRoot}/Tiles/Large tiles/Thin outline/tile_0057.png",
            UiChrome.ButtonPressed => $"{UiRoot}/Tiles/Large tiles/Thin outline/tile_0034.png",
            UiChrome.DarkPanel => $"{UiRoot}/Tiles/Large tiles/Thin outline/tile_0012.png",
            _ => $"{UiRoot}/Tiles/Large tiles/Thin outline/tile_0010.png"
        });

        return new StyleBoxTexture
        {
            Texture = texture,
            TextureMarginLeft = 10,
            TextureMarginTop = 10,
            TextureMarginRight = 10,
            TextureMarginBottom = 10,
            ContentMarginLeft = chrome is UiChrome.Button or UiChrome.ButtonHover or UiChrome.ButtonPressed ? 18 : 20,
            ContentMarginTop = chrome is UiChrome.Button or UiChrome.ButtonHover or UiChrome.ButtonPressed ? 10 : 18,
            ContentMarginRight = chrome is UiChrome.Button or UiChrome.ButtonHover or UiChrome.ButtonPressed ? 18 : 20,
            ContentMarginBottom = chrome is UiChrome.Button or UiChrome.ButtonHover or UiChrome.ButtonPressed ? 10 : 18,
            AxisStretchHorizontal = StyleBoxTexture.AxisStretchMode.Stretch,
            AxisStretchVertical = StyleBoxTexture.AxisStretchMode.Stretch
        };
    }

    public static TextureRect CreateIcon(Texture2D texture, Vector2 size)
    {
        return new TextureRect
        {
            Texture = texture,
            CustomMinimumSize = size,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest
        };
    }

    public static void ApplyFittedSprite(Sprite2D sprite, Texture2D texture, Vector2 targetSize, float bottomY, bool preserveFlip = false)
    {
        var flipped = preserveFlip && sprite.FlipH;
        sprite.Texture = texture;
        sprite.Centered = true;
        sprite.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;

        var size = texture.GetSize();
        var scaleFactor = Mathf.Min(targetSize.X / size.X, targetSize.Y / size.Y);
        sprite.Scale = Vector2.One * scaleFactor;
        sprite.Position = new Vector2(0f, bottomY - size.Y * scaleFactor * 0.5f);
        sprite.FlipH = flipped;
    }

    public static Texture2D GetTerrainTexture(StageTheme theme, TerrainVisualKind kind)
    {
        var prefix = theme switch
        {
            StageTheme.Cave => "terrain_stone_block",
            StageTheme.Fortress => "terrain_purple_block",
            _ => "terrain_grass_block"
        };

        var root = $"{ArtRoot}/Sprites/Tiles/Default";
        var path = kind switch
        {
            TerrainVisualKind.PlatformLeft => $"{root}/{prefix.Replace("_block", "_horizontal")}_left.png",
            TerrainVisualKind.PlatformMiddle => $"{root}/{prefix.Replace("_block", "_horizontal")}_middle.png",
            TerrainVisualKind.PlatformRight => $"{root}/{prefix.Replace("_block", "_horizontal")}_right.png",
            TerrainVisualKind.TopLeft => $"{root}/{prefix}_top_left.png",
            TerrainVisualKind.Top => $"{root}/{prefix}_top.png",
            TerrainVisualKind.TopRight => $"{root}/{prefix}_top_right.png",
            TerrainVisualKind.Left => $"{root}/{prefix}_left.png",
            TerrainVisualKind.Center => $"{root}/{prefix}_center.png",
            TerrainVisualKind.Right => $"{root}/{prefix}_right.png",
            TerrainVisualKind.BottomLeft => $"{root}/{prefix}_bottom_left.png",
            TerrainVisualKind.Bottom => $"{root}/{prefix}_bottom.png",
            TerrainVisualKind.BottomRight => $"{root}/{prefix}_bottom_right.png",
            _ => $"{root}/{prefix}_center.png"
        };

        return GetTexture(path);
    }

    public static Texture2D GetRampTexture(StageTheme theme, RampVisualKind kind)
    {
        var prefix = theme switch
        {
            StageTheme.Cave => "terrain_stone",
            StageTheme.Fortress => "terrain_purple",
            _ => "terrain_grass"
        };

        var root = $"{ArtRoot}/Sprites/Tiles/Default";
        var path = kind switch
        {
            RampVisualKind.ShortA => $"{root}/{prefix}_ramp_short_a.png",
            RampVisualKind.ShortB => $"{root}/{prefix}_ramp_short_b.png",
            RampVisualKind.LongA => $"{root}/{prefix}_ramp_long_a.png",
            RampVisualKind.LongB => $"{root}/{prefix}_ramp_long_b.png",
            RampVisualKind.LongC => $"{root}/{prefix}_ramp_long_c.png",
            _ => $"{root}/{prefix}_ramp_short_a.png"
        };

        return GetTexture(path);
    }

    private static Texture2D GetTexture(string path)
    {
        if (!FullTextureCache.TryGetValue(path, out var texture))
        {
            texture = ResourceLoader.Load<Texture2D>(path);
            if (texture is null)
            {
                throw new InvalidOperationException($"Missing texture asset: {path}");
            }

            FullTextureCache[path] = texture;
        }

        return texture;
    }

    private static Font GetFont(string path)
    {
        if (!FontCache.TryGetValue(path, out var font))
        {
            font = ResourceLoader.Load<FontFile>(path);
            if (font is null)
            {
                throw new InvalidOperationException($"Missing font asset: {path}");
            }

            FontCache[path] = font;
        }

        return font;
    }

    private static Texture2D GetTrimmedTexture(string path)
    {
        if (TrimmedTextureCache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        var image = GetTexture(path).GetImage();
        var used = FindOpaqueBounds(image);
        if (used.Size == Vector2I.Zero)
        {
            TrimmedTextureCache[path] = GetTexture(path);
            return TrimmedTextureCache[path];
        }

        var trimmed = Image.CreateEmpty(used.Size.X, used.Size.Y, false, Image.Format.Rgba8);
        trimmed.BlitRect(image, used, Vector2I.Zero);

        var texture = ImageTexture.CreateFromImage(trimmed);
        TrimmedTextureCache[path] = texture;
        return texture;
    }

    private static Rect2I FindOpaqueBounds(Image image)
    {
        var minX = image.GetWidth();
        var minY = image.GetHeight();
        var maxX = -1;
        var maxY = -1;

        for (var y = 0; y < image.GetHeight(); y++)
        {
            for (var x = 0; x < image.GetWidth(); x++)
            {
                if (image.GetPixel(x, y).A <= 0f)
                {
                    continue;
                }

                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
        }

        return maxX < minX || maxY < minY
            ? new Rect2I()
            : new Rect2I(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}

public enum UiChrome
{
    Panel,
    DarkPanel,
    Button,
    ButtonHover,
    ButtonPressed
}

public enum TerrainVisualKind
{
    PlatformLeft,
    PlatformMiddle,
    PlatformRight,
    TopLeft,
    Top,
    TopRight,
    Left,
    Center,
    Right,
    BottomLeft,
    Bottom,
    BottomRight
}

public enum RampVisualKind
{
    ShortA,
    ShortB,
    LongA,
    LongB,
    LongC
}

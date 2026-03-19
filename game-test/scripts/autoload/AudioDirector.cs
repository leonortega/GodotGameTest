using Godot;

namespace GameTest;

public partial class AudioDirector : Node
{
    private const string MusicBusName = "Music";
    private const string SfxBusName = "SFX";
    private const string UiBusName = "UI";
    private const int SampleRate = 22050;

    private static readonly Dictionary<StageTheme, string[]> MusicAssetPaths = new()
    {
        [StageTheme.Grassland] =
        [
            "res://audio/music/03 HoliznaCC0 - Adventure Begins Loop.ogg"
        ],
        [StageTheme.Cave] =
        [
            "res://audio/music/06 HoliznaCC0 - Where It's Safe.ogg"
        ],
        [StageTheme.Treetop] =
        [
            "res://audio/music/06 HoliznaCC0 - Sunny Afternoon.ogg"
        ],
        [StageTheme.Fortress] =
        [
            "res://audio/music/12 HoliznaCC0 - NPC Theme.ogg"
        ]
    };

    private static readonly Dictionary<string, string[]> SfxAssetPaths = new()
    {
        ["jump"] =
        [
            "res://assets/art/kenney_new_platformer_pack/Sounds/sfx_jump.ogg",
            "res://audio/sfx/Movement/Jumping and Landing/sfx_movement_jump7.wav",
            "res://audio/sfx/Movement/Jumping and Landing/sfx_movement_jump12.wav"
        ],
        ["coin"] =
        [
            "res://assets/art/kenney_new_platformer_pack/Sounds/sfx_coin.ogg",
            "res://audio/sfx/General Sounds/Coins/sfx_coin_single1.wav",
            "res://audio/sfx/General Sounds/Coins/sfx_coin_single4.wav"
        ],
        ["powerup"] =
        [
            "res://assets/art/kenney_new_platformer_pack/Sounds/sfx_magic.ogg",
            "res://audio/sfx/General Sounds/Positive Sounds/sfx_sounds_powerup11.wav"
        ],
        ["extra_life"] =
        [
            "res://assets/art/kenney_new_platformer_pack/Sounds/sfx_gem.ogg",
            "res://audio/sfx/General Sounds/Positive Sounds/sfx_sounds_powerup17.wav"
        ],
        ["pickup"] =
        [
            "res://assets/art/kenney_new_platformer_pack/Sounds/sfx_gem.ogg",
            "res://audio/sfx/General Sounds/Coins/sfx_coin_double1.wav"
        ],
        ["stomp"] =
        [
            "res://assets/art/kenney_new_platformer_pack/Sounds/sfx_bump.ogg",
            "res://audio/sfx/General Sounds/Simple Damage Sounds/sfx_damage_hit1.wav"
        ],
        ["enemy_down"] =
        [
            "res://assets/art/kenney_new_platformer_pack/Sounds/sfx_disappear.ogg",
            "res://audio/sfx/General Sounds/Simple Damage Sounds/sfx_damage_hit7.wav"
        ],
        ["damage"] =
        [
            "res://assets/art/kenney_new_platformer_pack/Sounds/sfx_hurt.ogg",
            "res://audio/sfx/General Sounds/Negative Sounds/sfx_sounds_damage3.wav"
        ],
        ["fire"] =
        [
            "res://assets/art/kenney_new_platformer_pack/Sounds/sfx_throw.ogg",
            "res://audio/sfx/Weapons/Lasers/sfx_wpn_laser3.wav"
        ],
        ["life_lost"] =
        [
            "res://audio/sfx/General Sounds/Negative Sounds/sfx_sounds_negative1.wav",
            "res://audio/sfx/Death Screams/Human/sfx_deathscream_human4.wav"
        ],
        ["clear"] =
        [
            "res://assets/art/kenney_new_platformer_pack/Sounds/sfx_select.ogg",
            "res://audio/sfx/General Sounds/Positive Sounds/sfx_sounds_powerup15.wav"
        ],
        ["block"] =
        [
            "res://assets/art/kenney_new_platformer_pack/Sounds/sfx_bump.ogg"
        ],
        ["pause"] =
        [
            "res://audio/sfx/General Sounds/Pause Sounds/sfx_sounds_pause1_in.wav"
        ],
        ["resume"] =
        [
            "res://audio/sfx/General Sounds/Pause Sounds/sfx_sounds_pause1_out.wav"
        ],
        ["menu_open"] =
        [
            "res://audio/sfx/General Sounds/Menu Sounds/sfx_menu_select1.wav",
            "res://assets/art/kenney_new_platformer_pack/Sounds/sfx_select.ogg"
        ]
    };

    public static AudioDirector Instance { get; private set; } = null!;

    private readonly Dictionary<StageTheme, AudioStream> _musicTracks = [];
    private readonly Dictionary<string, AudioStream[]> _sfxCues = [];
    private AudioStreamPlayer _musicPlayer = null!;
    private readonly RandomNumberGenerator _rng = new();

    public string CurrentMusicCue { get; private set; } = string.Empty;

    public override void _Ready()
    {
        Instance = this;
        EnsureBusLayout();

        _musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
        _musicPlayer.Finished += HandleMusicFinished;

        ApplySavedMix();
        BuildCueCache();
    }

    public void PlayMusicForTheme(StageTheme theme)
    {
        var nextCue = theme.ToString();
        if (CurrentMusicCue == nextCue && _musicPlayer.Playing)
        {
            return;
        }

        CurrentMusicCue = nextCue;
        _musicPlayer.StreamPaused = false;
        _musicPlayer.Stream = _musicTracks[theme];
        _musicPlayer.Play();
    }

    public void PlaySfx(string cueName)
    {
        if (_sfxCues.TryGetValue(cueName, out var cues) && cues.Length > 0)
        {
            PlayOneShot(cues[_rng.RandiRange(0, cues.Length - 1)], SfxBusName);
        }
    }

    public void PlayUi(string cueName)
    {
        if (_sfxCues.TryGetValue(cueName, out var cues) && cues.Length > 0)
        {
            PlayOneShot(cues[_rng.RandiRange(0, cues.Length - 1)], UiBusName);
        }
    }

    public void SetPaused(bool isPaused)
    {
        _musicPlayer.StreamPaused = isPaused;
    }

    public void StopMusic()
    {
        CurrentMusicCue = string.Empty;
        _musicPlayer.StreamPaused = false;
        _musicPlayer.Stop();
    }

    public void ApplySavedMix()
    {
        SetBusVolume(MusicBusName, GameSession.Instance.MusicVolumeDb);
        SetBusVolume(SfxBusName, GameSession.Instance.SfxVolumeDb);
        SetBusVolume(UiBusName, Mathf.Lerp(GameSession.Instance.SfxVolumeDb, 0f, 0.35f));
    }

    private void EnsureBusLayout()
    {
        EnsureBus(MusicBusName);
        EnsureBus(SfxBusName);
        EnsureBus(UiBusName);
    }

    private static void EnsureBus(string busName)
    {
        if (AudioServer.GetBusIndex(busName) >= 0)
        {
            return;
        }

        var index = AudioServer.GetBusCount();
        AudioServer.AddBus(index);
        AudioServer.SetBusName(index, busName);
    }

    private static void SetBusVolume(string busName, float volumeDb)
    {
        var index = AudioServer.GetBusIndex(busName);
        if (index >= 0)
        {
            AudioServer.SetBusVolumeDb(index, volumeDb);
        }
    }

    private void BuildCueCache()
    {
        _musicTracks[StageTheme.Grassland] = LoadLoopingMusic(MusicAssetPaths[StageTheme.Grassland]) ?? CreateMusicLoop([523.25f, 659.25f, 783.99f, 659.25f, 587.33f, 659.25f, 493.88f, 392.0f], 128f, 0.16f, 0.09f);
        _musicTracks[StageTheme.Cave] = LoadLoopingMusic(MusicAssetPaths[StageTheme.Cave]) ?? CreateMusicLoop([261.63f, 311.13f, 349.23f, 392.0f, 349.23f, 311.13f, 293.66f, 233.08f], 112f, 0.18f, 0.05f);
        _musicTracks[StageTheme.Treetop] = LoadLoopingMusic(MusicAssetPaths[StageTheme.Treetop]) ?? CreateMusicLoop([392.0f, 523.25f, 659.25f, 783.99f, 698.46f, 659.25f, 587.33f, 523.25f], 138f, 0.15f, 0.11f);
        _musicTracks[StageTheme.Fortress] = LoadLoopingMusic(MusicAssetPaths[StageTheme.Fortress]) ?? CreateMusicLoop([220.0f, 261.63f, 293.66f, 329.63f, 311.13f, 293.66f, 246.94f, 196.0f], 118f, 0.17f, 0.04f);

        _sfxCues["jump"] = LoadOptionalAudioSet(SfxAssetPaths["jump"], CreateBlipCue(610f, 900f, 0.12f, 0.24f, Waveform.Square));
        _sfxCues["coin"] = LoadOptionalAudioSet(SfxAssetPaths["coin"], CreateBlipCue(1200f, 1680f, 0.08f, 0.20f, Waveform.Square));
        _sfxCues["powerup"] = LoadOptionalAudioSet(SfxAssetPaths["powerup"], CreateBlipCue(360f, 820f, 0.34f, 0.22f, Waveform.Triangle));
        _sfxCues["extra_life"] = LoadOptionalAudioSet(SfxAssetPaths["extra_life"], CreateArpeggioCue([660f, 880f, 1320f, 1760f], 0.38f, 0.16f));
        _sfxCues["pickup"] = LoadOptionalAudioSet(SfxAssetPaths["pickup"], CreateBlipCue(540f, 860f, 0.12f, 0.18f, Waveform.Triangle));
        _sfxCues["stomp"] = LoadOptionalAudioSet(SfxAssetPaths["stomp"], CreateBlipCue(240f, 120f, 0.10f, 0.28f, Waveform.Noise));
        _sfxCues["enemy_down"] = LoadOptionalAudioSet(SfxAssetPaths["enemy_down"], CreateBlipCue(330f, 160f, 0.18f, 0.24f, Waveform.Saw));
        _sfxCues["damage"] = LoadOptionalAudioSet(SfxAssetPaths["damage"], CreateBlipCue(220f, 110f, 0.24f, 0.30f, Waveform.Noise));
        _sfxCues["fire"] = LoadOptionalAudioSet(SfxAssetPaths["fire"], CreateBlipCue(760f, 520f, 0.12f, 0.18f, Waveform.Saw));
        _sfxCues["life_lost"] = LoadOptionalAudioSet(SfxAssetPaths["life_lost"], CreateBlipCue(240f, 80f, 0.45f, 0.32f, Waveform.Saw));
        _sfxCues["clear"] = LoadOptionalAudioSet(SfxAssetPaths["clear"], CreateArpeggioCue([523.25f, 659.25f, 783.99f, 1046.5f], 0.62f, 0.18f));
        _sfxCues["block"] = LoadOptionalAudioSet(SfxAssetPaths["block"], CreateBlipCue(440f, 540f, 0.10f, 0.14f, Waveform.Square));
        _sfxCues["pause"] = LoadOptionalAudioSet(SfxAssetPaths["pause"], CreateBlipCue(880f, 660f, 0.10f, 0.16f, Waveform.Square));
        _sfxCues["resume"] = LoadOptionalAudioSet(SfxAssetPaths["resume"], CreateBlipCue(660f, 920f, 0.10f, 0.16f, Waveform.Square));
        _sfxCues["menu_open"] = LoadOptionalAudioSet(SfxAssetPaths["menu_open"], CreateBlipCue(720f, 960f, 0.08f, 0.12f, Waveform.Triangle));
    }

    private static AudioStream? LoadLoopingMusic(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            if (ResourceLoader.Exists(path))
            {
                var stream = GD.Load<AudioStream>(path);
                return stream is null ? null : ConfigureLooping(stream);
            }
        }

        return null;
    }

    private static AudioStream[] LoadOptionalAudioSet(IEnumerable<string> paths, AudioStream fallback)
    {
        var streams = new List<AudioStream>();

        foreach (var path in paths)
        {
            if (!ResourceLoader.Exists(path))
            {
                continue;
            }

            var stream = GD.Load<AudioStream>(path);
            if (stream is not null)
            {
                streams.Add(stream);
            }
        }

        return streams.Count > 0 ? streams.ToArray() : [fallback];
    }

    private void PlayOneShot(AudioStream stream, string busName)
    {
        var player = new AudioStreamPlayer
        {
            Bus = busName,
            Stream = stream
        };

        AddChild(player);
        player.Finished += player.QueueFree;
        player.Play();
    }

    private void HandleMusicFinished()
    {
        if (string.IsNullOrWhiteSpace(CurrentMusicCue) || _musicPlayer.StreamPaused || _musicPlayer.Stream is null)
        {
            return;
        }

        _musicPlayer.Play();
    }

    private static AudioStream ConfigureLooping(AudioStream stream)
    {
        var configured = (AudioStream)stream.Duplicate();

        switch (configured)
        {
            case AudioStreamOggVorbis ogg:
                ogg.Loop = true;
                break;
            case AudioStreamMP3 mp3:
                mp3.Loop = true;
                break;
            case AudioStreamWav wav:
                wav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
                wav.LoopBegin = 0;
                wav.LoopEnd = wav.Data?.Length > 0 ? wav.Data.Length / sizeof(short) : 0;
                break;
        }

        return configured;
    }

    private static AudioStreamWav CreateMusicLoop(float[] leadNotes, float bpm, float leadGain, float bassGain)
    {
        var beatDuration = 60f / bpm;
        var noteDuration = beatDuration * 0.5f;
        var totalDuration = noteDuration * leadNotes.Length;
        var sampleCount = Mathf.CeilToInt(totalDuration * SampleRate);
        var pcm = new short[sampleCount];
        var bassPattern = new[] { leadNotes[0] * 0.5f, leadNotes[3] * 0.5f, leadNotes[4] * 0.5f, leadNotes[7] * 0.5f };

        for (var index = 0; index < sampleCount; index++)
        {
            var time = index / (float)SampleRate;
            var leadIndex = Mathf.Clamp((int)(time / noteDuration), 0, leadNotes.Length - 1);
            var bassIndex = Mathf.Clamp((int)(time / (noteDuration * 2f)) % bassPattern.Length, 0, bassPattern.Length - 1);
            var noteTime = time % noteDuration;
            var envelope = 1f - Mathf.Clamp(noteTime / noteDuration, 0f, 1f);
            var lead = SampleWave(time, leadNotes[leadIndex], Waveform.Square) * leadGain * envelope;
            var bass = SampleWave(time, bassPattern[bassIndex], Waveform.Triangle) * bassGain * (0.8f + 0.2f * Mathf.Sin(time * Mathf.Tau * 2f));
            pcm[index] = (short)(Mathf.Clamp(lead + bass, -0.95f, 0.95f) * short.MaxValue);
        }

        return BuildStream(pcm, true);
    }

    private static AudioStreamWav CreateBlipCue(float startFrequency, float endFrequency, float duration, float gain, Waveform waveform)
    {
        var sampleCount = Mathf.CeilToInt(duration * SampleRate);
        var pcm = new short[sampleCount];

        for (var index = 0; index < sampleCount; index++)
        {
            var progress = index / (float)Mathf.Max(1, sampleCount - 1);
            var time = index / (float)SampleRate;
            var frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
            var envelope = Mathf.Exp(-4.5f * progress);
            pcm[index] = (short)(Mathf.Clamp(SampleWave(time, frequency, waveform) * gain * envelope, -0.95f, 0.95f) * short.MaxValue);
        }

        return BuildStream(pcm, false);
    }

    private static AudioStreamWav CreateArpeggioCue(float[] notes, float duration, float gain)
    {
        var sampleCount = Mathf.CeilToInt(duration * SampleRate);
        var segmentLength = Mathf.Max(1, sampleCount / Mathf.Max(1, notes.Length));
        var pcm = new short[sampleCount];

        for (var index = 0; index < sampleCount; index++)
        {
            var noteIndex = Mathf.Clamp(index / segmentLength, 0, notes.Length - 1);
            var time = index / (float)SampleRate;
            var progress = index / (float)Mathf.Max(1, sampleCount - 1);
            var envelope = 1f - progress;
            pcm[index] = (short)(Mathf.Clamp(SampleWave(time, notes[noteIndex], Waveform.Triangle) * gain * envelope, -0.95f, 0.95f) * short.MaxValue);
        }

        return BuildStream(pcm, false);
    }

    private static AudioStreamWav BuildStream(short[] pcmSamples, bool loop)
    {
        var data = new byte[pcmSamples.Length * sizeof(short)];
        Buffer.BlockCopy(pcmSamples, 0, data, 0, data.Length);

        return new AudioStreamWav
        {
            Data = data,
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = SampleRate,
            Stereo = false,
            LoopMode = loop ? AudioStreamWav.LoopModeEnum.Forward : AudioStreamWav.LoopModeEnum.Disabled,
            LoopBegin = 0,
            LoopEnd = pcmSamples.Length
        };
    }

    private static float SampleWave(float time, float frequency, Waveform waveform)
    {
        var phase = time * frequency;
        return waveform switch
        {
            Waveform.Triangle => 2f * Mathf.Abs(2f * (phase - Mathf.Floor(phase + 0.5f))) - 1f,
            Waveform.Saw => 2f * (phase - Mathf.Floor(phase + 0.5f)),
            Waveform.Noise => Mathf.Sin(time * Mathf.Tau * frequency) * Mathf.Sin(time * Mathf.Tau * (frequency * 0.37f + 27f)),
            _ => Mathf.Sign(Mathf.Sin(time * Mathf.Tau * frequency))
        };
    }

    private enum Waveform
    {
        Square,
        Triangle,
        Saw,
        Noise
    }
}

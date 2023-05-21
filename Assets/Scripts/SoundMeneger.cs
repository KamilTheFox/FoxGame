using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundMeneger : MonoBehaviour
{
    private static Dictionary<Enum, AudioClip> DictionarySounds;

    [SerializeField]
    private AudioSource AudioModifer, AudioDefault, Music;

    public static AudioSource _Music { get; private set; }

    public static AudioSource Background { get; private set; }

    public static SoundMeneger instance;

    public static float Volume = 1F;

    public static float VolumeM = 1F;
    public static float VolumeMusic => _Music ? _Music.volume : 0F;
    public static int CountSoundType { get { return Enum.GetNames(typeof(Sounds)).Length; } }

    private static List<AudioSource> audioSources;

    private static Stack<Musics> PreviousMusic = new Stack<Musics>();

    public static Musics CurrentMusic { get; private set; }

    public static UnityEvent<bool> ChangeValuePauseMusic = new UnityEvent<bool>();

    public static UnityEvent<AudioSource> PlayMusicClipEvent = new UnityEvent<AudioSource>();

    public static UnityEvent<AudioSource> PlayClip = new UnityEvent<AudioSource>();
    public static bool CurrentMusicPause 
    {
        get => _CurrentMusicPause;
        private set
        {
            ChangeValuePauseMusic.Invoke(value);
            _CurrentMusicPause = value;
        }
    }

    private static bool _CurrentMusicPause;
    private static bool SpopCoroutine;
    private IEnumerator PlayingClip()
    {
        do
        {
            if (!CurrentMusicPause && !_Music.isPlaying && _Music.clip != null && _Music.time != _Music.clip.length) PlayRandomMusic();
            PlayClip.Invoke(_Music);
            yield return new WaitForSecondsRealtime(1F);
        }
        while (!SpopCoroutine);
        yield break;
    }

    private void Start()
    {
        if (instance != null)
        {
            Debug.LogWarning(new TextUI(LText.ErrorInitializeObjects, gameObject.ToString).ToString());
            GameObject.Destroy(gameObject);
            return;
        }
        instance = this;
        audioSources = new ();

        _Music = Instantiate(Music, transform).GetComponent<AudioSource>();

        Background = Instantiate(Music, transform).GetComponent<AudioSource>();

        Settings.VolumeSoundChange.AddListener((newValue) => { Volume = newValue; ChangeVolume(newValue); });

        Settings.VolumeMusicChange.AddListener((newValue) => { VolumeM = newValue; ChangeVolumeMusic(newValue); });

        ChangeVolumeMusic(VolumeM);

        SpopCoroutine = false;

        StartCoroutine(PlayingClip());

        DictionarySounds = new ();
        Array NameTypes = Enum.GetValues(typeof(Sounds));
        foreach (var sound in NameTypes)
        {
            Sounds _sound = (Sounds)sound;
            AudioClip clip = AudioLoad(_sound);
            if (clip)
                DictionarySounds.Add(_sound, clip);
            else
                Debug.Log($"Sound not detected in resources {sound}");
        }
        Array NameMusic = Enum.GetValues(typeof(Musics));
        foreach (var music in NameMusic)
        {
            Musics _sound = (Musics)music;
            AudioClip clip = AudioLoad(_sound);
            if (clip)
                DictionarySounds.Add(_sound, clip);
            else
                Debug.Log($"Music not detected in resources {_sound}");
        }
        Debug.Log($"Sucsess Load Audio {DictionarySounds.Count} / {NameTypes.Length + NameMusic.Length}");

        Background.clip = AudioLoad(BackgroundSounds.Forest_Birds);
        Background.Play();
        Background.loop = true;
        audioSources.Add(Background);
#if !UNITY_EDITOR
       // PlayRandomMusic();
#endif
    }
    private static AudioClip AudioLoad(Enum value)
    {
        if(DictionarySounds.ContainsKey(value))
            return DictionarySounds[value];
        return Resources.Load<AudioClip>($"Sounds\\{value.GetType().Name}\\" + value);
    }
    private void OnDestroy()
    {
        instance = null;
        ChangeValuePauseMusic = new UnityEvent<bool>();
        PlayMusicClipEvent = new UnityEvent<AudioSource>();
        PlayClip = new UnityEvent<AudioSource>();
        PreviousMusic = new Stack<Musics>();
        SpopCoroutine = true;
    }
    private static void PlayRandomMusic()
    {
        PlayMusic((Musics)UnityEngine.Random.Range(1, Enum.GetValues(typeof(Musics)).Length));
    }
    public static void PlayPopMusic()
    {
        if(PreviousMusic.Count > 0)
        PlayMusic(PreviousMusic.Pop(), false);
    }
    public static void PlayMusic(Musics music, bool PushStack = true)
    {
        if(PushStack)
            PreviousMusic.Push(music);
        CurrentMusic = music;
        PlayMusic(AudioLoad(music));
    }
    private static void PlayMusic(AudioClip sound)
    {
        CurrentMusicPause = false;
        _Music.clip = sound;
        _Music.time = 0;
        _Music.Play();
        PlayMusicClipEvent.Invoke(_Music);
    }
    public static void PauseMusic()
    {
        CurrentMusicPause = true;
        _Music.Pause();
    }
    public static void UnPauseMusic()
    {
        CurrentMusicPause = false;
        _Music.UnPause();
    }
    public static void StopMusic()
    {
        CurrentMusic = Musics.None;
        _Music.Stop();
    }
    private static AudioClip GetAudio(Sounds type)
    {
        return DictionarySounds[type];
    }
    private static void ChangeVolumeMusic(float value)
    {
        _Music.volume = value;    }
    private static void ChangeVolume(float value)
    {
        EnumAudioSources((audio) => audio.volume = Volume);
    }
    public static void PauseEnable(bool Enable)
    {
        EnumAudioSources((audio) =>
        {
            if (Enable) 
                audio.Pause();
            else 
                audio.UnPause();
        });
    }
    private static void EnumAudioSources(Action<AudioSource> action)
    {
        if (audioSources == null) return;
        AudioSource[] sounds = audioSources.ToArray();
        foreach(AudioSource Source in sounds)
        {
            if(!Source)
            {
                audioSources.Remove(Source);
                continue;
            }
            action.Invoke(Source);
        }
    }
    public static void PlayPoint(Sounds type, Vector3 vector)
    {
        PlayPoint(type, vector, 1F);
    }
    public static void PlayPoint(Sounds type, Vector3 vector, int X = 1)
    {
        PlayPoint(type, vector, 1F, X);
    }
    /// <param name="volume"> значение от 0 до 1. Если больше 1, то звук воспроизводится модифицированным источником, где интенсивность рассеивания звука меньше</param>
    public static void PlayPoint(Sounds type, Vector3 vector, float volume = 1F, int X = 1)
    {
        AudioClip clip = GetAudio(type);
        if (clip)
            for (int i = 0; i < X; i++)
                    PlayClipAtPoint(clip, vector, volume, volume > 1F);

    }
    public static void PlayPoint(Sounds type, Vector3 vector, bool isModiferSourse = true, float volume = 1F)
    {
        AudioClip clip = GetAudio(type);
        if (clip)
                PlayClipAtPoint(clip, vector, volume, isModiferSourse);

    }
    private static void PlayClipAtPoint(AudioClip audioClip, Vector3 vector, float volume, bool isModiferSourse)
    {
        AudioSource source = isModiferSourse ? instance.AudioModifer : instance.AudioDefault;
        source = Instantiate(source.gameObject, vector,Quaternion.identity).GetComponent<AudioSource>();
        source.volume = Volume;
        source.PlayOneShot(audioClip, volume);
        audioSources.Add(source);
        Destroy(source.gameObject, audioClip.length);
    }
#region Overload Play()
    public static void Play(Sounds type)
    {
        Play(type,null);
    }
    public static void Play(Sounds type, AudioSource source)
    {
        Play(type, source, 1);
    }
    public static void Play(Sounds type, AudioSource source, int X = 1)
    {
        Play(type, source, 1F, X);
    }
    public static void Play(Sounds type, AudioSource source, float volume = 1F)
    {
        Play(type, source, volume);
    }
    public static void Play(Sounds type, AudioSource source, bool _3D)
    {
        Play(type, source, 1F, 1, _3D);
    }
#endregion
    /// <param name="volume"> значение от 0 до 1</param>
    public static void Play(Sounds type, AudioSource source = null, float volume = 1F, int X = 1, bool _3D = true)
    {
        AudioClip clip = GetAudio(type);
        if (!source)
        {
            source = CameraControll.CameraSource;
        }
        source.volume = Volume;
        if (source && _3D)
            source.spatialBlend = 1F;
        if (clip&& source)
            for (int i = 0; i < X; i++)
                source.PlayOneShot(clip, volume);
        if(!audioSources.Contains(source))
            audioSources.Add(source);
    }
    public enum Sounds
    {
        Note,
        Warning,
        Explosion,
        TNT_Detonate,
        EatApple,
        ClickButton,
        Tick,
    }
    public enum BackgroundSounds
    {
        Forest_Birds,
    }
    public enum Musics
    {
        None,
        FKJ_Us,
        TomMisch_GypsyWoman,
        Absofacto_Dissolve
    }
}

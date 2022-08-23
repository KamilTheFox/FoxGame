using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundMeneger : MonoBehaviour
{
    static Sound[] ListSound;

    [SerializeField]
    private AudioSource AudioModifer, AudioDefault, Music;

    public static AudioSource _Music { get; private set; }

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
            if (!CurrentMusicPause && !_Music.isPlaying) PlayRandomMusic();
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
            Debug.LogError("You cannot call the sound manager");
            return;
        }
        instance = this;

        audioSources = new ();

        _Music = Instantiate(Music, transform).GetComponent<AudioSource>();

        GroupMenu.Settings.VolumeSoundChange.AddListener((newValue) => { Volume = newValue; ChangeVolume(newValue); });

        GroupMenu.Settings.VolumeMusicChange.AddListener((newValue) => { VolumeM = newValue; ChangeVolumeMusic(newValue); });

        ChangeVolumeMusic(VolumeM);

        SpopCoroutine = false;

        StartCoroutine(PlayingClip());

        List<Sound> sounds = new List<Sound>();
        string[] NameTypes = Enum.GetNames(typeof(Sounds));
        foreach (var sound in NameTypes)
        {
            AudioClip clip = AudioLoad(sound);
            if (clip)
                sounds.Add(new Sound((Sounds)Enum.Parse(typeof(Sounds), sound), clip));
            else
                Debug.Log($"Sound not detected in resources {sound}");
        }
        Debug.Log($"Sucsess Load Audio {sounds.Count} / {NameTypes.Length}");
        ListSound = sounds.ToArray();
        PlayRandomMusic();
    }
    private static AudioClip AudioLoad(string Name)
    {
        return Resources.Load<AudioClip>("Sounds\\" + Name);
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
    public static void PlayMusic(Musics sound, bool PushStack = true)
    {
        if(PushStack)
            PreviousMusic.Push(sound);
        CurrentMusic = sound;
        PlayMusic(AudioLoad(sound.ToString()));
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
    static AudioClip GetAudio(Sounds type)
    {
        foreach (var sound in ListSound)
        {
            if (sound.Type == type)
                return sound.Audio;
        }
        return null;
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
    /// <param name="volume"> значение от 0 до 1. ≈сли больше 1, то звук воспроизводитс€ модифицированным источником, где интенсивность рассеивани€ звука меньше</param>
    public static void PlayPoint(Sounds type, Vector3 vector, float volume = 1F, int X = 1)
    {
        AudioClip clip = GetAudio(type);
        if (clip)
            for (int i = 0; i < X; i++)
                    PlayClipAtPoint(clip, vector, volume, volume > 1F);

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
    }
    public enum Musics
    {
        None,
        FKJ_Us,
        TomMisch_GypsyWoman,
        Absofacto_Dissolve
    }
    private class Sound
        {
        public Sound (Sounds type, AudioClip audio)
        {
            Type = type;
            Audio = audio;
        }
        public Sounds Type;
        public AudioClip Audio;
        }
}

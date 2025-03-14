﻿using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class SoundMeneger : MonoBehaviour
{
    private static Dictionary<Enum, AudioClip> DictionarySounds;

    [SerializeField] private BackgroundSounds backgroundSounds = BackgroundSounds.Forest_Birds;

    [SerializeField] private Musics[] PlayListLevel;

    [SerializeField]
    private AudioSource AudioModifer, AudioDefault, Music, Beckground;

    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private AudioMixerSnapshot SnapshotDefault;

    public static AudioSource _Music { get; private set; }

    public static AudioSource Background { get; private set; }

    public static List<SoundZone> SoundBackgroundZone { get; private set; } = new List<SoundZone>();

    private static SoundZone CurrentSoundBackgroundZone { get; set; }

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
            if (!CurrentMusicPause && !_Music.isPlaying && _Music.clip != null && _Music.time != _Music.clip.length) 
                PlayRandomMusic();
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

        Background = Instantiate(Beckground, transform).GetComponent<AudioSource>();

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

        Background.clip = AudioLoad(backgroundSounds);
        Background.Play();
        Background.loop = true;
        audioSources.Add(Background);
        PlayRandomMusic();
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
        SoundBackgroundZone.Clear();
        ChangeValuePauseMusic = new UnityEvent<bool>();
        PlayMusicClipEvent = new UnityEvent<AudioSource>();
        PlayClip = new UnityEvent<AudioSource>();
        PreviousMusic = new Stack<Musics>();
        SpopCoroutine = true;
    }
    private void FixedUpdate()
    {
        Vector3 positionCam = CameraControll.Instance.transform.position;
        foreach (var zone in SoundBackgroundZone)
        {
            float dist = float.PositiveInfinity;
            foreach (var collider in zone.GetColliders)
            {
                if (!collider.isTrigger) continue;
                var closestPoint = collider.ClosestPoint(positionCam);
                var сorrection = ((closestPoint - positionCam) / 2f).sqrMagnitude;
                if (сorrection < dist)
                    dist = сorrection;
            }
            float blendDistance = zone.BlendDistanse * zone.BlendDistanse;
            if (dist > blendDistance)
            {
                if (CurrentSoundBackgroundZone == zone)
                {
                    CurrentSoundBackgroundZone = null;
                    SnapshotDefault.TransitionTo(0.5f);
                    AudioClip clip = AudioLoad(backgroundSounds);
                    if(clip!= null)
                        if (Background.clip.name.Contains(clip.name))
                        Background.clip = clip;
                }
                continue;
            }
            BlendChangeBackground(zone);
        }
    }
    private void BlendChangeBackground(SoundZone zone)
    {
        if (CurrentSoundBackgroundZone == zone) return;

        CurrentSoundBackgroundZone = zone;
        if (zone.Snapshot != null)
            zone.Snapshot.TransitionTo(0.2f);
        if (zone.AudioZone != null)
            Background.clip = zone.AudioZone;
    }
    private void PlayRandomMusic()
    {
        if (PlayListLevel.Length == 0) return;
        Musics musicRand = PlayListLevel[UnityEngine.Random.Range(0, PlayListLevel.Length)];
        PlayMusic(musicRand);
    }
    public static void PlayPopMusic()
    {
        if(PreviousMusic.Count > 0)
        PlayMusic(PreviousMusic.Pop(), false);
    }
    public static void PlayMusic(Musics music, bool PushStack = true)
    {
        if (music == Musics.None)
            return;
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
        _Music.volume = value;   
    }
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
    public static AudioSource PlayPoint(Sounds type, Vector3 vector, bool isModiferSourse = true, float volume = 1F)
    {
        AudioClip clip = GetAudio(type);
        if (clip)
                return PlayClipAtPoint(clip, vector, volume, isModiferSourse);
        return null;

    }
    private static AudioSource PlayClipAtPoint(AudioClip audioClip, Vector3 vector, float volume, bool isModiferSourse)
    {
        AudioSource source = isModiferSourse ? instance.AudioModifer : instance.AudioDefault;
        source = Instantiate(source.gameObject, vector,Quaternion.identity).GetComponent<AudioSource>();
        source.volume = Volume;
        source.PlayOneShot(audioClip, volume);
        audioSources.Add(source);
        Destroy(source.gameObject, audioClip.length);
        return source;
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
    public static void Play(Sounds type, AudioSource source = null, float volume = 1F, int X = 1, bool _3D = true, bool oneShoot = true)
    {
        AudioClip clip = GetAudio(type);
        if (!source)
        {
            source = CameraControll.CameraSource;
        }
        source.volume = Volume;
        if (source && _3D)
            source.spatialBlend = 1F;
        if (clip && source)
            for (int i = 0; i < X; i++)
                if (oneShoot)
                {
                    source.PlayOneShot(clip, volume);
                }
                else if (!source.isPlaying)
                {
                    source.clip = clip;
                    source.Play();
                }
        if(!audioSources.Contains(source))
            audioSources.Add(source);
    }
    public static void Play(AudioClip clip, AudioSource source = null, float volume = 1F, int X = 1, bool _3D = true, bool oneShoot = true)
    {
        if (clip == null) return;
        if (!source)
        {
            source = CameraControll.CameraSource;
        }
        source.volume = Volume;
        if (source && _3D)
            source.spatialBlend = 1F;
        if (clip && source)
            for (int i = 0; i < X; i++)
            {
                if (oneShoot)
                {
                    source.PlayOneShot(clip, volume);
                }
                else if(!source.isPlaying)
                {
                    if(source.clip != clip)
                        source.clip = clip;
                    source.Play();
                }
            }
        if (!audioSources.Contains(source))
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
        CrushWoodBox,
    }
    public enum BackgroundSounds
    {
        Forest_Birds,
        Horror_Home,
        Warehouse
    }
    public enum Musics
    {
        None,
        FKJ_Us,
        TomMisch_GypsyWoman,
        Absofacto_Dissolve,
        C418_Thunderbird,
        C418_Nest,
        C418_The_President_Is_Dead,
        C418_Leak,
        C418_Cold_Summer,
        AC_DC_TNT,
        Hugo_99Problems

    }
}

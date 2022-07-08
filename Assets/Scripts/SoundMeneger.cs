using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundMeneger : MonoBehaviour
{
    static Sound[] ListSound;
    [SerializeField]
    GameObject AudioSetting;
    public static SoundMeneger instance;
    public static float Volume = 1F;
    public static int CountSoundType { get { return Enum.GetNames(typeof(Sounds)).Length; } }
    private void Start()
    {
        instance=this;
        List<Sound> sounds = new List<Sound>();
        string[] NameTypes = Enum.GetNames(typeof(Sounds));
        foreach (var sound in NameTypes)
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds\\" + sound);
            if (clip)
                sounds.Add(new Sound((Sounds)Enum.Parse(typeof(Sounds), sound), clip));
            else
                Debug.Log($"Sound not detected in resources {sound}");
        }
        Debug.Log($"Sucsess Load Aodio {sounds.Count} / {NameTypes.Length}");
        ListSound = sounds.ToArray();
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
    
    public static void PlayPoint(Sounds type, Vector3 vector)
    {
        PlayPoint(type, vector, 1F);
    }
    public static void PlayPoint(Sounds type, Vector3 vector, int X = 1)
    {
        PlayPoint(type, vector, 1F, X);
    }
    public static void PlayPoint(Sounds type, Vector3 vector, float volume = 1F, int X = 1)
    {
        AudioClip clip = GetAudio(type);
        if (clip)
            for (int i = 0; i < X; i++)
                if (volume > 1F)
                    PlayClipAtPoint(clip, vector, volume * Volume);
                else
                    AudioSource.PlayClipAtPoint(clip, vector, volume);
    }
    static void PlayClipAtPoint(AudioClip audioClip, Vector3 vector, float volume)
    {
        GameObject Sound = Instantiate(instance.AudioSetting,vector,Quaternion.identity);
        Sound.GetComponent<AudioSource>().PlayOneShot(audioClip, volume * Volume);
        Destroy(Sound, audioClip.length);
    }
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
    public static void Play(Sounds type, AudioSource source = null, float volume = 1F, int X = 1)
    {
        AudioClip clip = GetAudio(type);
        if (!source)
        {
            source = CameraControll.MainCamera.GetComponent<AudioSource>();
            Debug.LogWarning("AudioSource has been redefined to the main camera");
        }
        if (source)
            source.spatialBlend = 1F;
        if (clip&& source)
            for (int i = 0; i < X; i++)
                source.PlayOneShot(clip, volume * Volume);
    }
    public enum Sounds
    {
        Note,
        Warning,
        Explosion,
        TNT_Detonate,
        EatApple
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

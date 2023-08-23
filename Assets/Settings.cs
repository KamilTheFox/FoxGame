using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public static class Settings
{
     static Settings()
        {
        LogWisible = new bool[3];
        GetValue();
        }
    


    public static bool[] LogWisible;

    public static UnityEvent<float> VolumeSoundChange = new();

    public static UnityEvent<float> VolumeMusicChange = new();

    public static float SensetiveMouse;

    public static UnityEvent ChangeLanguageEvent = new();

    public static Color ColorText { get; private set; } = Color.white;// new Color(0.8F, 0.8F, 0.8F);

    public static Color ColorOutline { get; private set; } = Color.black;

    public static string NameSkinMenu { get; private set; } = "Default";

    public static void ChangeLanguage(Language language)
    {
        Localisation.Language = language;
        ChangeLanguageEvent?.Invoke();
        Menu.UpdateTextUI();
    }
    public static void SaveValue()
    {
        PlayerPrefs.SetInt("Language", (int)Localisation.Language);
        PlayerPrefs.SetFloat("SensetiveMouse", CameraControll.SensetiveMouse);
        PlayerPrefs.SetFloat("VolumeSound", SoundMeneger.Volume);
        PlayerPrefs.SetFloat("VolumeMusic", SoundMeneger.VolumeM);
        PlayerPrefs.SetInt("Error", LogWisible[0] ? 1 : 0);
        PlayerPrefs.SetInt("Warning", LogWisible[1] ? 1 : 0);
        PlayerPrefs.SetInt("Info", LogWisible[2] ? 1 : 0);
    }
    public static void GetValue()
    {
        ChangeLanguage((Language)PlayerPrefs.GetInt("Language", 0));
        SensetiveMouse = PlayerPrefs.GetFloat("SensetiveMouse", 25F);
        SoundMeneger.Volume = PlayerPrefs.GetFloat("VolumeSound", 1F);
        SoundMeneger.VolumeM = PlayerPrefs.GetFloat("VolumeMusic", 1F);
        LogWisible[0] = PlayerPrefs.GetInt("Error", 0) == 1;
        LogWisible[1] = PlayerPrefs.GetInt("Warning", 0) == 1;
        LogWisible[2] = PlayerPrefs.GetInt("Info", 0) == 1;
    }
    public static void ResetValue()
    {
        PlayerPrefs.DeleteKey("VolumeSound"); PlayerPrefs.DeleteKey("SensetiveMouse"); PlayerPrefs.DeleteKey("Language");
        GetValue();
        Menu.UpdateTextUI();
    }
    public static bool LogView(int index)
    {
        return LogWisible[index];
    }
}

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public static class Settings
{

    private struct ValuesSettings
    {
        public float drawingRangePlant;
        public float quantityRangePlant;
        public float distanceDrawShadow;
        public float distanceDrawCharacters;
    }

    public static InputJobPropertyData InputJobPropertyRendererData { get; private set; } = new InputJobPropertyData()
    {
        maxBottomY = -100,
        maxTopY = 300,
        maxVisibilityDistance = 300,
        VisibilitySprite = 120,
        isCastingShadow = true,
        isReceivingShadow = true,
        calculateAngleToCamera = true,
        shadowVisibilityDistance = 70,
        orderDistance = 20,
        cullAngle = 30
    };

     static Settings()
     {
        LogWisible = new bool[3];
        GetValue();
     }

    private static ValuesSettings values = new ValuesSettings() { drawingRangePlant = 60F, quantityRangePlant = 100F, distanceDrawShadow = 45f };

    public static float DrawingRangePlant
    {
        get
        {
            return values.drawingRangePlant;
        }
        set
        {
            values.drawingRangePlant = value;
        }
    }
    public static float DistanceDrawCharacters
    {
        get
        {
            return values.distanceDrawCharacters;
        }
        set
        {
            values.distanceDrawCharacters = value;
        }
    }

    public static float DistanceDrawShadow
    {
        get
        {
            return values.distanceDrawShadow;
        }
        set
        {
            values.distanceDrawShadow = value;
        }
    }

    public static float QuantityRangePlant
    {
        get
        {
            return values.quantityRangePlant;
        }
        set
        {
            values.quantityRangePlant = value;
        }
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
        foreach (FieldInfo value in typeof(ValuesSettings).GetFields())
        {
            SaveValueReflect(value, value.GetValue(values));
        }
        PlayerPrefs.SetInt("Error", LogWisible[0] ? 1 : 0);
        PlayerPrefs.SetInt("Warning", LogWisible[1] ? 1 : 0);
        PlayerPrefs.SetInt("Info", LogWisible[2] ? 1 : 0);
    }
    private static object GetValueReflect(FieldInfo field)
    {
        if (!PlayerPrefs.HasKey(field.Name))
        {
            object value = field.GetValue(values);
            SaveValueReflect(field, value);
            return value;
        }
        if (field.FieldType == typeof(string))
        {
            return PlayerPrefs.GetString(field.Name);
        }
        else if (field.FieldType == typeof(bool))
        {
            return PlayerPrefs.GetInt(field.Name) == 1;
        }
        else if (field.FieldType == typeof(float))
        {
            return PlayerPrefs.GetFloat(field.Name);
        }
        else if (field.FieldType == typeof(int))
        {
            return PlayerPrefs.GetInt(field.Name);
        }
        Debug.LogWarning($"Field boes not carry an object of the type {field.FieldType.Name}");
        return default;
    }
    private static void SaveValueReflect(FieldInfo field, object obj)
    {
        if (obj is string text)
        {
            PlayerPrefs.SetString(field.Name, text);
            return;
        }
        else if (obj is bool flag)
        {
            PlayerPrefs.SetInt(field.Name, flag ? 1 : 0);
            return;
        }
        else if (obj is float value)
        {
            PlayerPrefs.SetFloat(field.Name, value);
            return;
        }
        else if (obj is int value2)
        {
            PlayerPrefs.SetInt(field.Name, value2);
            return;
        }
        Debug.LogError($"Field boes not carry an object of the type {field.FieldType.Name}");
    }
    public static void GetValue()
    {
        ChangeLanguage((Language)PlayerPrefs.GetInt("Language", 0));
        SensetiveMouse = PlayerPrefs.GetFloat("SensetiveMouse", 25F);
        SoundMeneger.Volume = PlayerPrefs.GetFloat("VolumeSound", 1F);
        SoundMeneger.VolumeM = PlayerPrefs.GetFloat("VolumeMusic", 1F);

        foreach(FieldInfo value in typeof(ValuesSettings).GetFields())
        {
            value.SetValue(values, GetValueReflect(value));
        }

        LogWisible[0] = PlayerPrefs.GetInt("Error", 0) == 1;
        LogWisible[1] = PlayerPrefs.GetInt("Warning", 0) == 1;
        LogWisible[2] = PlayerPrefs.GetInt("Info", 0) == 1;
    }
    public static void ResetValue()
    {
        PlayerPrefs.DeleteKey("VolumeSound"); PlayerPrefs.DeleteKey("SensetiveMouse");
        foreach (FieldInfo value in typeof(ValuesSettings).GetFields())
        {
            PlayerPrefs.DeleteKey(value.Name);
        }
        GetValue();
        Menu.UpdateTextUI();
    }
    public static bool LogView(int index)
    {
        return LogWisible[index];
    }
}

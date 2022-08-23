using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GroupMenu
{
    public class Settings : MainGroup
    {
        private static MenuUI<Button> b_ChangeLanguage , buttonSettings , b_SaveValue, b_ResetValue;

        private static MenuUI<Slider> Sensitive;
        protected static MenuUI<Slider> VolumeSound, VolumeMusic;

        public static UnityEvent<float> VolumeSoundChange;

        public static UnityEvent<float> VolumeMusicChange;

        public override TypeMenu TypeMenu => TypeMenu.Settings;
        protected override void Start()
        {
            VolumeSoundChange = new();
            VolumeMusicChange = new();

            GetValue();

            FindComponent();


            b_ChangeLanguage.OnClick().AddListener(ChangeLanguage);


            VolumeSound.Component.value = SoundMeneger.Volume;

            VolumeSound.OnValueChanged().AddListener((value) =>
            {
                VolumeSoundChange.Invoke(value);
            });

            Sensitive.SetMinMax(0F, 100F);

            Sensitive.Component.value = CameraControll.SensetiveMouse;

            Sensitive.OnValueChanged().AddListener((value) =>
            {
                CameraControll.SensetiveMouse = value;
            });

            VolumeMusic.Component.value = SoundMeneger.VolumeM;

            VolumeMusic.OnValueChanged().AddListener((value) =>
            {
                VolumeMusicChange.Invoke(value);
            });

            buttonSettings = MenuUI<Button>.Find("Hat/Settings", mainGroup.transform, LText.Null);
            buttonSettings.SetImage("Settings");
            buttonSettings.OnClick().AddListener(() => Menu.ActivateMenu(new Settings()));

            b_SaveValue.OnClick().AddListener(SaveValue);


            b_ResetValue.OnClick().AddListener(ResetValue);

            Transform GroupLog = MenuUI<HorizontalLayoutGroup>.Create(GetTransform(), LText.Null, true).Component.transform;

            MenuUI<Text>.Create(GroupLog, "VisibleLog?".GetTextUI());

            MenuUI<Button>.Create(GroupLog, new TextUI(() => new object[] { "Error: ", LogView(0).GetLText() })).OnClick().AddListener(() => { if (DebugConsole.instance) DebugConsole.instance.LogWisible[0] = !DebugConsole.instance.LogWisible[0]; });
            MenuUI<Button>.Create(GroupLog, new TextUI(() => new object[] { "Warning: ", LogView(1).GetLText() })).OnClick().AddListener(() => { if (DebugConsole.instance) DebugConsole.instance.LogWisible[1] = !DebugConsole.instance.LogWisible[1]; });
            MenuUI<Button>.Create(GroupLog, new TextUI(() => new object[] { "Info: ", LogView(2).GetLText() })).OnClick().AddListener(() => { if (DebugConsole.instance) DebugConsole.instance.LogWisible[2] = !DebugConsole.instance.LogWisible[2]; });

        }
        private bool LogView(int index)
        {
            bool _return = false;
            if (DebugConsole.instance)
                _return = DebugConsole.instance.LogWisible[index];
            return _return;
        }
        private void FindComponent()
        {
            b_ChangeLanguage = MenuUI<Button>.Find("ChangeLanguage", GetTransform(), new TextUI(LText.Language, () => new object[] { Localisation.Language, i }), true);

            Sensitive = MenuUI<Slider>.Create("Sensitive", GetTransform(), new TextUI(() => new object[] { LText.Sensitive, ": ", Math.Round(CameraControll.SensetiveMouse) }), true);

            VolumeSound = MenuUI<Slider>.Create("VolumeSound", GetTransform(), new TextUI(LText.Volume, () => new object[] { LText.Sound, Math.Round(SoundMeneger.Volume * 100) }), true);

            VolumeMusic = MenuUI<Slider>.Create("VolumeMusic", GetTransform(), new TextUI(LText.Volume, () => new object[] { LText.Music, Math.Round(SoundMeneger.VolumeMusic * 100) }), true);

            Transform HorizontalGroup = MenuUI<HorizontalLayoutGroup>.Create("Value", GetTransform(), LText.Null, true).Component.transform;

            MenuUI<Text>.Create("Value", HorizontalGroup, LText.Values, false, (rect) =>{ rect.width = 100; return rect; });

            b_SaveValue = MenuUI<Button>.Create("SaveValue", HorizontalGroup, LText.Save);

            b_ResetValue = MenuUI<Button>.Create("ResetValue", HorizontalGroup, LText.Reset);

        }
        private void SaveValue()
        {
            PlayerPrefs.SetInt("Language", (int)Localisation.Language);
            PlayerPrefs.SetFloat("SensetiveMouse" , CameraControll.SensetiveMouse);
            PlayerPrefs.SetFloat("VolumeSound", SoundMeneger.Volume);
            PlayerPrefs.SetFloat("VolumeMusic", SoundMeneger.VolumeM);
            PlayerPrefs.SetInt("Error", DebugConsole.instance.LogWisible[0] ? 1 : 0);
            PlayerPrefs.SetInt("Warning", DebugConsole.instance.LogWisible[1] ? 1 : 0);
            PlayerPrefs.SetInt("Info", DebugConsole.instance.LogWisible[2] ? 1 : 0);
        }
        private void GetValue()
        {
            Localisation.Language = (Language)PlayerPrefs.GetInt("Language", 0);
            CameraControll.SensetiveMouse = PlayerPrefs.GetFloat("SensetiveMouse", 25F);
            SoundMeneger.Volume = PlayerPrefs.GetFloat("VolumeSound", 1F);
            SoundMeneger.VolumeM = PlayerPrefs.GetFloat("VolumeMusic", 1F);
        }
        private void ResetValue()
        {
            PlayerPrefs.DeleteKey("VolumeSound"); PlayerPrefs.DeleteKey("SensetiveMouse"); PlayerPrefs.DeleteKey("Language");
            GetValue();
            Menu.UpdateTextUI();
        }
        protected override void Activate()
        {
            VolumeMusic.Component.value = SoundMeneger.VolumeM;
            VolumeMusic.UpdateText();

            ButtonEnabled(false);
        }
        protected override void Deactivate()
        {
            ButtonEnabled(true);
        }
        public static void ButtonEnabled(bool Activate)
        {
            buttonSettings.gameObject.SetActive(Activate);
        }
        private static int i;
        private void ChangeLanguage()
        {
            i++;
                Localisation.Language = Localisation.Language == Language.En? Language.Ru : Language.En;
            Menu.UpdateTextUI();
        }
    }
}

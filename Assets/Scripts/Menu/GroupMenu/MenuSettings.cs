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
    public class MenuSettings : MainGroup
    {
        private static MenuUI<Button> b_ChangeLanguage , buttonSettings , b_SaveValue, b_ResetValue;

        private static MenuUI<Slider> Sensitive;
        protected static MenuUI<Slider> VolumeSound, VolumeMusic;

        protected static MenuUI<Slider> DrawingRangePlant, QuantityRangePlant;

        public override TypeMenu TypeMenu => TypeMenu.MenuSettings;

        private void InitSliderRangePlant()
        {
            DrawingRangePlant = MenuUI<Slider>.Create("DrawingRangePlant", GetTransform(), new TextUI(
                () => new object[] { LText.Distance_Drawing , ": " , Math.Round(Settings.DrawingRangePlant) }), true);
            QuantityRangePlant = MenuUI<Slider>.Create("QuantityRangePlant", GetTransform(), new TextUI(
                () => new object[] { LText.Range, ": ", Math.Round(Math.Round(Settings.QuantityRangePlant)) }), true);


            var drawing = Settings.DrawingRangePlant;
            var quantity = Settings.QuantityRangePlant;
            DrawingRangePlant.OnValueChanged().AddListener((value) =>
            {
                Settings.DrawingRangePlant = value;
            });

            DrawingRangePlant.SetMinMax(20F, 200F);

            QuantityRangePlant.OnValueChanged().AddListener((value) =>
            {
                Settings.QuantityRangePlant = value;
            });

            QuantityRangePlant.SetMinMax(20F, 200F);
            DrawingRangePlant.Component.value = drawing;
            QuantityRangePlant.Component.value = quantity;

        }
        protected override void Start()
        {

            FindComponent();

            InitSliderRangePlant();

            b_ChangeLanguage.OnClick().AddListener(() => Settings.ChangeLanguage(Localisation.Language == Language.En ? Language.Ru : Language.En));


            VolumeSound.Component.value = SoundMeneger.Volume;

            VolumeSound.OnValueChanged().AddListener((value) =>
            {
                Settings.VolumeSoundChange.Invoke(value);
            });

            Sensitive.SetMinMax(0F, 100F);

            Sensitive.Component.value = CameraControll.SensetiveMouse;

            Sensitive.OnValueChanged().AddListener((value) =>
            {
                Settings.SensetiveMouse = value;
            });

            VolumeMusic.Component.value = SoundMeneger.VolumeM;

            VolumeMusic.OnValueChanged().AddListener((value) =>
            {
                Settings.VolumeMusicChange.Invoke(value);
            });

            buttonSettings = MenuUI<Button>.Find("Hat/Settings", mainGroup.transform, LText.Null);
            buttonSettings.SetImage("Settings", true);
            buttonSettings.OnClick().AddListener(Menu.ActivateMenu<MenuSettings>);

            b_SaveValue.OnClick().AddListener(Settings.SaveValue);


            b_ResetValue.OnClick().AddListener(Settings.ResetValue);

            Transform GroupLog = MenuUI<HorizontalLayoutGroup>.Create(GetTransform(), LText.Null, true).Component.transform;

            MenuUI<Text>.Create(GroupLog, "VisibleLog?".GetTextUI());

            MenuUI<Button>.Create(GroupLog, new TextUI(() => new object[] { "Error: ", Settings.LogView(0).GetLText() })).OnClick().AddListener(() => { if (DebugConsole.instance) Settings.LogWisible[0] = !Settings.LogWisible[0]; });
            MenuUI<Button>.Create(GroupLog, new TextUI(() => new object[] { "Warning: ", Settings.LogView(1).GetLText() })).OnClick().AddListener(() => { if (DebugConsole.instance) Settings.LogWisible[1] = !Settings.LogWisible[1]; });
            MenuUI<Button>.Create(GroupLog, new TextUI(() => new object[] { "Info: ", Settings.LogView(2).GetLText() })).OnClick().AddListener(() => { if (DebugConsole.instance) Settings.LogWisible[2] = !Settings.LogWisible[2]; });

        }
        
        private void FindComponent()
        {
            b_ChangeLanguage = MenuUI<Button>.Find("ChangeLanguage", GetTransform(), new TextUI(LText.Language, () => new object[] { Localisation.Language }), true);

            Sensitive = MenuUI<Slider>.Create("Sensitive", GetTransform(), new TextUI(() => new object[] { LText.Sensitive, ": ", Math.Round(CameraControll.SensetiveMouse) }), true);

            VolumeSound = MenuUI<Slider>.Create("VolumeSound", GetTransform(), new TextUI(LText.Volume, () => new object[] { LText.Sound, Math.Round(SoundMeneger.Volume * 100) }), true);

            VolumeMusic = MenuUI<Slider>.Create("VolumeMusic", GetTransform(), new TextUI(LText.Volume, () => new object[] { LText.Music, Math.Round(SoundMeneger.VolumeMusic * 100) }), true);

            Transform HorizontalGroup = MenuUI<HorizontalLayoutGroup>.Create("Value", GetTransform(), LText.Null, true).Component.transform;

            MenuUI<Text>.Create("Value", HorizontalGroup, LText.Values, false, (rect) =>{ rect.width = 100; return rect; });

            b_SaveValue = MenuUI<Button>.Create("SaveValue", HorizontalGroup, LText.Save);

            b_ResetValue = MenuUI<Button>.Create("ResetValue", HorizontalGroup, LText.Reset);

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
    }
}

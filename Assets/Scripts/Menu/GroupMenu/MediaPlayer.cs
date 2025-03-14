﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GroupMenu
{
    public class MediaPlayer : MainGroup
    {
        public override TypeMenu TypeMenu => TypeMenu.MediaPlayer;

        private static MenuUI<Button> buttonMediaPlayer;

        private static MenuUI<Slider> Volume, PlayLine;

        private static MenuUI<Dropdown> ListMusic;
        protected override void Start()
        {
            Volume = MenuUI<Slider>.Create(GetTransform(),
                new TextUI(LText.Volume, () => new object[] { LText.Music, Math.Round(SoundMeneger.VolumeMusic * 100) }), true);
            Volume.Component.value = PlayerPrefs.GetFloat("VolumeMusic", 1F);

            Volume.OnValueChanged().AddListener(Settings.VolumeMusicChange.Invoke);

            buttonMediaPlayer = MenuUI<Button>.Find("Hat/MediaPlayer", mainGroup.transform, LText.Null);

            buttonMediaPlayer.SetImage("Music", true);

            buttonMediaPlayer.OnClick().AddListener(Menu.ActivateMenu<MediaPlayer>);

            MenuUIAutoRect.GetRect();
            MenuUIAutoRect.GetRect();
            MenuUI<Button>[] menuUIs = new MenuUI<Button>[]
            {
                MenuUI<Button>.Find("Play", GetTransform(), LText.Null),
                MenuUI<Button>.Find("Return", GetTransform(), LText.Null),
            };
            foreach(MenuUI<Button> button in menuUIs)
            {
                button.SetImage(button.Component.name, true);
            }
            SoundMeneger.ChangeValuePauseMusic.AddListener((value) =>
            {
                menuUIs[0].SetImage(value ? "Play" : "Pause", true);
            });
            menuUIs[0].OnClick().AddListener(() =>
            {
                if(SoundMeneger.CurrentMusicPause)
                    SoundMeneger.UnPauseMusic();
                else
                    SoundMeneger.PauseMusic();
            });
            menuUIs[1].OnClick().AddListener(SoundMeneger.PlayPopMusic);

            PlayLine = MenuUI<Slider>.Find("TimeClip", GetTransform(), LText.Null);
            
            SoundMeneger.PlayClip.AddListener((value) =>
            {
                ListMusic.UpdateText();
                PlayLine.Component.value = value.time;
            });
            PlayLine.OnValueChanged().AddListener((value) =>
            {
                if (SoundMeneger._Music.clip)
                {
                    if (value < SoundMeneger._Music.clip.length)
                        SoundMeneger._Music.time = value;
                }
            });

            ListMusic = MenuUI<Dropdown>.Find("ListMusic", GetTransform(), LText.Null);

            ListMusic.Component.AddOptions(Enum.GetNames(typeof(SoundMeneger.Musics)).ToList());

            ListMusic.Component.onValueChanged.AddListener((value) => SoundMeneger.PlayMusic((SoundMeneger.Musics)value));

            SoundMeneger.PlayMusicClipEvent.AddListener((value) =>
            {
                PlayLine.SetMinMax(0, value.clip.length);
                PlayLine.SetText(new TextUI(() => new object[] { TimeSpan.FromSeconds(value.time).ToString("mm\\:ss"), "/", TimeSpan.FromSeconds(value.clip.length).ToString("mm\\:ss") }));
                ResetTextDownDrop();
            });
            Component[] conponents = new Component[]
            {
                menuUIs[0].Component,
                menuUIs[1].Component,
                PlayLine.Component,
                ListMusic.Component,
            };
            MenuUIAutoRect.Add_Queue(conponents);
        }
        private static void ResetTextDownDrop()
        {
            ListMusic.SetText(SoundMeneger.CurrentMusic.ToString());
        }
        public static void ButtonEnabled(bool Activate)
        {
            buttonMediaPlayer.gameObject.SetActive(Activate);
        }
        protected override void Activate()
        {
            Volume.Component.value = SoundMeneger.VolumeM;
            Volume.UpdateText();

            ResetTextDownDrop();

            ButtonEnabled(false);
        }
        protected override void Deactivate()
        {
            ButtonEnabled(true);
        }
    }
}

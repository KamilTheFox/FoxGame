﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


namespace GroupMenu
{
    public class MainMenu : MainGroup
    {
        public override TypeMenu TypeMenu => TypeMenu.MainMenu;

        private MenuUI<Dropdown> SellectDifficulty;
        protected override bool IsActiveBackHot => false;

        private static GameState.TypeModeGame StartMode;


        private static object[] GetTextDifficulty()
        {
            return new object[]
            {
                (LText)Enum.Parse(typeof(LText),$"Difficulty{GameState.Difficulty}")
            };
        }

        protected override void Start()
        {
            StartMode = GameState.TypeModeGame.Creative;

            MenuUI<Button>.Create("LoadPolygone", GetTransform(), "Polygone", AutoRect: true).OnClick(StartPolygon);

            MenuUI<Button>.Create("StartLVL1", GetTransform(), "PrototypeBackyard", AutoRect: true).OnClick(StartTestLevel1);

            MenuUI<Button>.Create("StartLVL2", GetTransform(), "PrototypeBigHome", AutoRect: true).OnClick(StartTestLevel2);

            MenuUI<Button>.Create("StartLVL3", GetTransform(), "PrototypeFactory", AutoRect: true).OnClick(StartTestLevel3);

            MenuUI<Button>.Create("StartLVL4", GetTransform(), "PrototypeBoss", AutoRect: true).OnClick(StartTestLevel4);

            Transform SellectDifficultyHorizontal = MenuUI<HorizontalLayoutGroup>.Create("SellectDifficulty", GetTransform(), LText.Null, true).gameObject.transform;

            MenuUI<Text>.Create("SellectDifficultyText", SellectDifficultyHorizontal, LText.Difficulty, false);

            SellectDifficulty = MenuUI<Dropdown>.Create("SellectDifficulty", SellectDifficultyHorizontal, new TextUI(LText.None, GetTextDifficulty), false, MenuUIAutoRect.SetWidth(140F));
            SellectDifficulty.Component.AddOptions(Enum.GetNames(typeof(GameState.TypeDifficulty)).ToList());
            SellectDifficulty.Component.onValueChanged.AddListener((index) =>
            {
                GameState.Difficulty = (GameState.TypeDifficulty)index;
            });
            SellectDifficulty.Component.value = (int)GameState.Difficulty;

            MenuUI<Toggle> toggle = MenuUI<Toggle>.Create("CreativeMode", GetTransform(), LText.Creative, true);
            toggle.OnValueChanged
                ( (value) => StartMode = value ? GameState.TypeModeGame.Creative : GameState.TypeModeGame.Adventure);

            toggle.Component.isOn = true;

            MenuUI<Button>.Create("Exit", GetTransform(), LText.Exit, AutoRect: true).OnClick(Menu.ExitGame);

        }
        private void StartPolygon()
        {
            MovingAvatarAndCamera.MoveWayMenu("Play", () =>
            {
                GameState.StartGame(StartMode, 1);
            });
        }
        private void StartTestLevel1()
        {
            MovingAvatarAndCamera.MoveWayMenu("Play", () =>
            {
                GameState.StartGame(StartMode, 2);
            });
        }
        private void StartTestLevel2()
        {
            MovingAvatarAndCamera.MoveWayMenu("Play", () =>
            {
                GameState.StartGame(StartMode, 3);
            });
        }
        private void StartTestLevel3()
        {
            MovingAvatarAndCamera.MoveWayMenu("Play", () =>
            {
                GameState.StartGame(GameState.TypeModeGame.Adventure, 4);
            });
        }
        private void StartTestLevel4()
        {
            MovingAvatarAndCamera.MoveWayMenu("Play", () =>
            {
                GameState.StartGame(GameState.TypeModeGame.Adventure, 5);
            });
        }
        protected override void Activate()
        {
            CallBackActivate(false);
            Menu.PauseEnableGame(false);
        }
    }
}

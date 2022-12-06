using System;
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

        protected override bool IsActiveBackHot => false;

        private static GameState.TypeModeGame StartMode;

        protected override void Start()
        {
            StartMode = GameState.TypeModeGame.Creative;

            MenuUI<Button>.Create("LoadPoligon", GetTransform(), LText.Start_Game, AutoRect: true).OnClick(StartPoligon);

            MenuUI<Toggle>.Create("CreativeMode", GetTransform(), LText.Creative, true).OnValueChanged
                ( (value) => StartMode = value ? GameState.TypeModeGame.Creative : GameState.TypeModeGame.Adventure);

            MenuUI<Button>.Create("Exit", GetTransform(), LText.Exit, AutoRect: true).OnClick(Menu.ExitGame);

        }
        private void StartPoligon()
        {
            GameState.StartGame(StartMode, 1);
        }
        protected override void Activate()
        {
            CallBeckActivate(false);
            Menu.PauseEnableGame(false);
        }
    }
}

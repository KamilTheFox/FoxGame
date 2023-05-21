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

            MenuUI<Button>.Create("LoadPrototype", GetTransform(), LText.Start_Prototype, AutoRect: true).OnClick(StartPrototype);

            MenuUI<Text>.Create("InfoPrototype", GetTransform(), LText.Start_Prototype_Info, AutoRect: true, (rect) => rect = new Rect(rect.x, rect.y, rect.width, rect.height * 2));

            MenuUI<Toggle>.Create("CreativeMode", GetTransform(), LText.Creative, true).OnValueChanged
                ( (value) => StartMode = value ? GameState.TypeModeGame.Creative : GameState.TypeModeGame.Adventure);

            MenuUI<Button>.Create("Exit", GetTransform(), LText.Exit, AutoRect: true).OnClick(Menu.ExitGame);

        }
        private void StartPoligon()
        {
            GameState.StartGame(StartMode, 1);
        }
        private void StartPrototype()
        {
            GameState.StartGame(StartMode, 2);
        }
        protected override void Activate()
        {
            CallBeckActivate(false);
            Menu.PauseEnableGame(false);
        }
    }
}

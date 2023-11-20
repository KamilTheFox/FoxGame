using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GroupMenu
{
    internal class MenuWinLose : MainGroup
    {
        public override TypeMenu TypeMenu => TypeMenu.MenuWinLose;

        private static MenuUI<Text> TitleWin;

        public static bool isRestart { get; set; }

        protected override void Start()
        {
            MenuUI<Button>.Create("BackMainMenu", GetTransform(), LText.MainMenu, true).OnClick(() => GameState.StartGame(GameState.TypeModeGame.MainMenu));
            TitleWin = MenuUI<Text>.Create("WinLoseText", GetTransform(), "The game is in full swing", true, (rect) => new Rect(rect.x, rect.y, rect.width, 100));
            MenuUI<Button>.Create("Restart", GetTransform(), "Restart", true).OnClick(() => GameState.StartGame(GameState.TypeModeGame.Adventure, SceneManager.GetActiveScene().buildIndex));
            MenuUI<Button>.Create("Exit", GetTransform(), LText.Exit, true).OnClick(Menu.ExitGame);
        }
        protected override void Activate()
        {
            MainTitle.SetText(LText.Pause);
            CallBackActivate(false);
        }
        public static void SetTextTitle(TextUI text)
        {
            MainTitle.SetText(LText.MenuWinLose);
            TitleWin.SetText(text);
        }
    }
}

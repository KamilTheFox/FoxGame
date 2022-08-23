using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace GroupMenu
{
    public class MainMenu : MainGroup
    {
        private static Transform mainMenu;
        private static MenuUI<Button> LoadPoligon, ExitGameButton, Test;
        public override TypeMenu TypeMenu => TypeMenu.MainMenu;

        protected override bool IsActiveBackHot => false;
        protected override void Start()
        {
            FindUI();
            ExitGameButton.OnClick().AddListener(() => Menu.ExitGame());
            LoadPoligon.OnClick().AddListener(
                () => SceneManager.LoadScene(1)
                );
        }
        protected override void Activate()
        {
            CallBeckActivate(false);
            Menu.PauseEnableGame(false);
        }
        private void FindUI()
        {
            mainMenu = GetTransform();

            LoadPoligon = MenuUI<Button>.Create(nameof(LoadPoligon), mainMenu, LText.Start_Game,AutoRect: true);

            ExitGameButton = MenuUI<Button>.Create("Exit", mainMenu, LText.Exit, AutoRect: true);

            MenuUI<Text>.Create("Test", mainMenu, "TestScrollRect".GetTextUI(), true).UpdateText();



        }
    }
}

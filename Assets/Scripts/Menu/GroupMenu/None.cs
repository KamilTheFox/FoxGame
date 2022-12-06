using System;
using UnityEngine;
using UnityEngine.UI;

namespace GroupMenu
{
    public class None : IActivatableMenu
    {
        public TypeMenu TypeMenu => TypeMenu.None;

        private static MenuUI<Text> InfoEntity;

        void IActivatableMenu.Activate()
        {
            Cursor.lockState = CursorLockMode.Locked;
                Menu.PauseEnableGame(false);
        }

        void IActivatableMenu.Deactivate()
        {
            Cursor.lockState = CursorLockMode.None;
            Menu.PushMenu();
            Menu.PauseEnableGame(true);
            SetInfoEntity(false);
        }
        public static void SetInfoEntity(bool Activate, Func<object> Text = null)
        {
            SetInfoEntity(Activate, new TextUI(Text));
        }
        public static void SetInfoEntity(bool Activate, TextUI Text)
        {
            InfoEntity.gameObject.SetActive(Activate);
            if (!Activate)
            {
                return;
            }
            InfoEntity.SetText(Text);
        }
        void IActivatableMenu.Start()
        {
            InfoEntity = MenuUI<Text>.Find("None/InfoEntity", null, LText.None);
            SetInfoEntity(false);
        }

        void IActivatableMenu.Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Escape))
                Menu.ActivateMenu<Lobby>();
        }
    }
}

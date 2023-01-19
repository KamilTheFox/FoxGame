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
        public static void SetInfoEntity(bool Activate, EntityEngine entity = null)
        {
            InfoEntity.gameObject.SetActive(Activate);
            if (!Activate)
            {
                return;
            }
            InfoEntity.SetText(entity.GetTextUI());
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

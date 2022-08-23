using System;
using UnityEngine;
using UnityEngine.UI;

namespace GroupMenu
{
    public class None : IActivatable
    {
        public TypeMenu TypeMenu => TypeMenu.None;

        private static MenuUI<Text> InfoEntity;

        public void Activate()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Menu.PauseEnableGame(false);
        }

        public void Deactivate()
        {
            Cursor.lockState = CursorLockMode.None;
            Menu.PushMenu();
            Menu.PauseEnableGame(true);
            SetInfoEntity(false);
        }
        public static void SetInfoEntity(bool Activate, Func<object> Text = null)
        {
            InfoEntity.gameObject.SetActive(Activate);
            if (!Activate || Text == null)
            {
                return;
            }
            InfoEntity.SetText(new TextUI(Text));
        }
        public void Start()
        {
            InfoEntity = MenuUI<Text>.Find("None/InfoEntity", null, LText.None);
            SetInfoEntity(false);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Escape))
                Menu.ActivateMenu(new Lobby());
        }
    }
}

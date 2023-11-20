using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace GroupMenu
{
    public class None : IActivatableMenu
    {
        public TypeMenu TypeMenu => TypeMenu.None;

        private static MenuUI<Text> InfoEntity;

        private static MenuUI<Image> Aim;

        private static UnityEvent<bool> onActivate = new UnityEvent<bool>();

        public static event UnityAction<bool> OnActivate
        {
            add
            {
                onActivate.AddListener(value);
            }
            remove 
            {
                onActivate.RemoveListener(value);
            }
        }

        public static void EnableAim(bool Ensable = true)
        {
            Aim.gameObject.SetActive(Ensable);
        }
        public static void EnableInfoEntity(bool Ensable = true)
        {
            InfoEntity.gameObject.SetActive(Ensable);
        }

        void IActivatableMenu.Activate()
        {
            Cursor.lockState = CursorLockMode.Locked;
            if (Aim != null)
                Aim.Component.enabled = true;
                Menu.PauseEnableGame(false);;
            onActivate.Invoke(true);
        }

        void IActivatableMenu.Deactivate()
        {
            Cursor.lockState = CursorLockMode.None;
            if (Aim != null)
                Aim.Component.enabled = false;
            Menu.PushMenu();
            Menu.PauseEnableGame(true);
            SetInfoEntity(false);
            onActivate.Invoke(false);
        }
        public static void SetInfoEntity(bool Activate, EntityEngine entity = null)
        {
            if(InfoEntity != null && InfoEntity.Component != null)
                InfoEntity.Component.enabled = Activate;
            if (!Activate)
            {
                return;
            }
            InfoEntity.SetText(entity.GetTextUI());
        }
        void IActivatableMenu.Start()
        {
            InfoEntity = MenuUI<Text>.Find("None/InfoEntity", null, LText.None);
            Aim = MenuUI<Image>.Find("None/Aim", null, LText.None);
            SetInfoEntity(false);
        }

        void IActivatableMenu.Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Escape))
                Menu.ActivatePauseMenu();
        }
    }
}

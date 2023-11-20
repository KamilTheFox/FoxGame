using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace GroupMenu
{
    public abstract class MainGroup : IActivatableMenu
    {

        private static MenuUI<Button> buttonCallBack;

        protected static MenuUI<Text> MainTitle;
        protected static GameObject mainGroup { get; private set; }

        private static Dictionary<TypeMenu, GameObject> MenuChildren = new();
        public abstract TypeMenu TypeMenu { get; }

        protected bool IsPushMenu = true;
        /// <summary>
        /// Для разовой активации статических полей Группы меню.
        /// </summary>
        protected abstract void Start();
        /// <summary>
        /// Происходит при Диактивации меню, Инициализируются компоненты группы
        /// </summary>
        protected virtual void Deactivate() { }
        /// <summary>
        /// Происходит при Активации меню, Инициализируются компоненты группы
        /// </summary>
        protected virtual void Activate() { }

        protected virtual void Update() { }

        public static GameObject GetMenu(TypeMenu menu)
        {
            return MenuChildren[menu];
        }

        protected virtual bool IsActiveBackHot => true;

        void IActivatableMenu.Update()
            {
            if ((Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Escape)) && IsActiveBackHot)
                Menu.PopMenu(isCallBack: true);
            Update();
            }

        public static void CallBackActivate(bool isActive, Action action = null)
        {
            buttonCallBack.Component.gameObject.SetActive(isActive);
            if (action != null)
            {
                buttonCallBack.OnClick().AddListener(new UnityAction(action));
            }
        }
        private void Initialize()
        {
            MenuChildren = new();
            Menu.onDestroy.AddListener(() => { mainGroup = null; MenuChildren.Clear(); });

            string mainGroupName = nameof(MainGroup);

            mainGroup = Menu.Find(mainGroupName);

            Image imageSkinMainGroup = SetSkinComponent(mainGroup, mainGroupName);
            imageSkinMainGroup.pixelsPerUnitMultiplier = 0.5F;
            foreach (Transform child in mainGroup.transform)
            {
                string name = child.name;
                if (name == "Hat")
                {
                    SetSkinComponent(child.gameObject, name);
                    continue;
                }
                if (Enum.TryParse(typeof(TypeMenu), name, out object value))
                {
                    MenuChildren.Add((TypeMenu)value, child.gameObject);
                }
                else { Debug.LogWarning($"Was not found menu: {name} in Enum TypeMenu"); }
            }
            MainTitle = MenuUI<Text>.Find("Hat/Title", mainGroup.transform);

            buttonCallBack = MenuUI<Button>.Find("Hat/CallBack", mainGroup.transform, new TextUI(LText.Back));

        }
        public static Image SetSkinComponent(GameObject Component , string name)
        {
            Image im = Component.GetComponent<Image>();
            MenuUI.SetImageMenu(im, name);
            im.type = Image.Type.Sliced;
            im.color = Color.white;
            return im;
        }
        public GameObject GetObject()
        {
            return MenuChildren[TypeMenu];
        }
        public Transform GetTransform()
        {
            GameObject value;
            if (MenuChildren.TryGetValue(TypeMenu ,out value))
                return value.transform;
            Debug.LogError("Menu not initialized");
            return null;
        }
        // Желательно всю логику поиска элементов главной группы писать в метод: Initialize()
        void IActivatableMenu.Start()
        {
            if (!mainGroup)
                Initialize();
            Start();
        }
        private void SetActive(bool active)
        {
            mainGroup.SetActive(active);
            MenuChildren[TypeMenu].SetActive(active);
        }
        void IActivatableMenu.Activate()
        {
            SetActive(true);
            buttonCallBack.OnClick().RemoveAllListeners();

            MainTitle.SetText(new TextUI(TypeMenu.ToString().GetLText()));

            CallBackActivate(true, () => Menu.PopMenu(isCallBack: true));

            Activate();
        }
        void IActivatableMenu.Deactivate()
        {
            if (IsPushMenu)
                Menu.PushMenu();
            SetActive(false);
            Deactivate();
        }
    }
}

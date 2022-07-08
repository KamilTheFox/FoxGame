using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GroupMenu
{
    public abstract class MainGroup : IActivatable
    {

        private static Button buttonCallBack;
        protected static GameObject instance { get; private set; }

        private static Dictionary<TypeMenu, GameObject> MenuChildren = new();
        public abstract TypeMenu TypeMenu { get; }

        /// <summary>
        /// Для разовой активации статических полей Группы меню.
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// Происходит при Диактивации меню, Инициализируются компоненты группы
        /// </summary>
        public abstract void Deactivate();
        /// <summary>
        /// Происходит при Активации меню, Инициализируются компоненты группы
        /// </summary>
        public abstract void Activate();
        private void Initialize()
        {
            instance = Menu.FindUIByPath(nameof(MainGroup));
            foreach (Transform child in instance.transform)
            {
                string name = child.name;
                if (name == "Hat") continue;
                if (Enum.TryParse(typeof(TypeMenu), name, out object value))
                {
                    MenuChildren.Add((TypeMenu)value, child.gameObject);
                }
                else { Debug.LogError($"Was not found menu: {name} in Enum TypeMenu"); }
                    }
            buttonCallBack = Menu.FindUIByPath<Button>("Hat/CallBack", instance.transform);
        }
        protected GameObject GetThisMenu()
        {
            return MenuChildren[TypeMenu];
        }
        // Желательно всю логику поиска элементов главного меню писать в метод: Initialize()
        void IActivatable.Start()
        {
            if(!instance)
            Initialize();
            Start();
        }
        private void SetActive(bool active)
        {
            instance.SetActive(active);
            MenuChildren[TypeMenu].SetActive(active);
        }
        void IActivatable.Activate()
        {
            SetActive(true);
            Activate();
        }
        void IActivatable.Deactivate()
        {
            SetActive(false);
            Deactivate();
        }
    }
}

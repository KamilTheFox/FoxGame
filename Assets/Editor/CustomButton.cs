using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Assets.Editors
{

    // Кастомный редактор для всех компонентов
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonAttributeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Отрисовываем стандартный инспектор
            DrawDefaultInspector();

            // Получаем целевой объект

            var mono = target as MonoBehaviour;

            // Получаем все методы с атрибутом Button
            var methods = mono.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(typeof(ButtonAttribute), false).Length > 0);

            // Для каждого метода создаем кнопку
            foreach (var method in methods)
            {
                var buttonAttr = (ButtonAttribute)method.GetCustomAttributes(typeof(ButtonAttribute), false)[0];
                string buttonName = string.IsNullOrEmpty(buttonAttr.ButtonName) ? method.Name : buttonAttr.ButtonName;

                if (GUILayout.Button(buttonName))
                {
                    method.Invoke(mono, null);
                }
            }
        }
    }
}

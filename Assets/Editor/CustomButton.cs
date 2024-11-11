using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using PlayerDescription;

namespace Assets.Editors
{
    [CustomEditor(typeof(MonoBehaviour), true), CanEditMultipleObjects]
    public class ButtonAttributeEditor : Editor
    {
        private static Type targetType;
        private IEnumerable<MethodInfo> methods;
        public void OnEnable()
        {
            targetType = target.GetType();
            methods = targetType
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(typeof(ButtonAttribute), false).Length > 0);
        }
        
        public Transform transform = null;
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            // Получаем тип целевого объекта
            foreach (var method in methods)
            {
                var buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
                string buttonName = string.IsNullOrEmpty(buttonAttr.ButtonName) ? method.Name : buttonAttr.ButtonName;

                if (GUILayout.Button(buttonName))
                {
                    foreach (var targetObject in targets)
                    {
                        var targetMono = targetObject as MonoBehaviour;
                        if (targetMono != null)
                        {
                            Undo.RecordObject(targetMono, $"Execute {buttonName}");
                            method.Invoke(targetMono, null);
                        }
                    }
                }
            }
        }
    }
}

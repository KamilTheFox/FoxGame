using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

using Random = UnityEngine.Random;

namespace Assets.Editors
{
    public class CreateMenuEntity : EditorWindow
    {
        public static Type TypeEntity;
        public static EditorWindow This;
        private static bool OnlyCreate = true;
        private static bool Stationary;
        

        [MenuItem("GameObject/3D Object/Item")]
        public static void CreateItems()
        {
            This = GetWindow<CreateMenuEntity>("Create Item");
            TypeEntity = typeof(TypeItem);
        }
        [MenuItem("GameObject/3D Object/Plants")]
        public static void CreatePlants()
        {
            This = GetWindow<CreateMenuEntity>("Create Plant");
            TypeEntity = typeof(TypePlant);
        }
        [MenuItem("GameObject/3D Object/Animal")]
        public static void CreateAnimals()
        {
            This =  GetWindow<CreateMenuEntity>("Create Animal");
            TypeEntity = typeof(TypeAnimal);
        }
        private float Scroll;

        private void Test(int id)
        {
            GUILayout.Button("Test");
            GUI.DragWindow();
        }
        Rect MenuTest = new Rect(10, 10, 100, 100);
        private void OnGUI()
        {
            Array array = Enum.GetValues(TypeEntity);
            Scroll = GUI.VerticalScrollbar(new Rect(0, 0, 20, This.position.height), Scroll, 1F, 0F, This.position.height + 1F);
            OnlyCreate = EditorGUI.Toggle(new Rect(20, -Scroll, This.position.width - 20F, 20), "Create only?", OnlyCreate);
            Stationary = EditorGUI.Toggle(new Rect(20, -Scroll + 25F, This.position.width - 20F, 20), "Stationary?", Stationary);
            for (int i = 2; i < array.Length + 2; i++)
            {
                if (GUI.Button(new Rect(20, -Scroll + i * 25F, This.position.width - 20F, 20), array.GetValue((i - 2)).ToString()))
                {
                    CreateEntity((Enum)Enum.Parse(TypeEntity, array.GetValue((i - 2)).ToString()));
                    if(OnlyCreate)
                    This.Close();
                };
            }
        }
        private static GameObject CreateEntity(Enum Entity)
        {
            IEntityCreated entity = EntityCreate.GetEntity(Entity, Vector3.zero, Quaternion.identity, Stationary);
            return entity.GetPrefab;
        }
    }
}

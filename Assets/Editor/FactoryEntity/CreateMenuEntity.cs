using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public static bool CreateToScene;
        public static int IndexCreateToScene;


        [MenuItem("GameObject/3D Object/Rebuild All Entity")]
        public static void RebuildAllEntity()
        {
            Transform[] entities = GameObject.Find("Entityes").GetComponentsInChildren<Transform>();
            for (int i = 0; i < entities.Length; i++)
                RebuildEntity(entities[i]);
        }

        [MenuItem("GameObject/3D Object/Rebuild Selected Entities")]
        public static void RebuildSelectedEntities()
        {
            Transform[] entities = Selection.transforms;
            for (int i = 0; i < entities.Length; i++)
                RebuildEntity(entities[i]);
        }

        public static void RebuildEntity(Transform transform)
        {
            if (!transform.TryGetComponent(out EntityEngine entityEngine)) return;

            Undo.RecordObject(transform, transform.name + " RebuildEntitynull");
            if (entityEngine is ItemEngine itemEngine)
            {
                ItemEngine.AddItem(itemEngine.itemType, transform.position, transform.rotation, itemEngine.Stationary);
            }
            else if (entityEngine is AnimalEngine animal)
            {
                AnimalEngine.AddAnimal(animal.TypeAnimal, transform.position, transform.rotation);
            }
            else if (entityEngine is PlantEngine plant)
            {
                PlantEngine.AddPlant(plant.typePlant, transform.position, transform.rotation);
            }
            entityEngine.transform.localScale = transform.localScale;
            Undo.RecordObject(transform.gameObject, transform.name + " Delete RebuildEntitynull");
            GameObject.DestroyImmediate(transform.gameObject);

        }
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
            CreateToScene = EditorGUI.Toggle(new Rect(20, -Scroll + 50F, This.position.width - 20F, 20), "CreateToScene?", CreateToScene);
            for (int i = 3; i < array.Length + 3; i++)
            {
                if (GUI.Button(new Rect(20, -Scroll + i * 25F, This.position.width - 20F, 20), array.GetValue((i - 3)).ToString()))
                {
                    IndexCreateToScene = i - 3;
                    if (CreateToScene)
                        return;
                    CreateEntity((Enum)Enum.Parse(TypeEntity, array.GetValue(IndexCreateToScene).ToString()));
                    if(OnlyCreate && !CreateToScene)
                        This.Close();
                };
            }
            
        }
        public static GameObject GetCurrentCreateEntity()
        {
            return CreateEntity((Enum)Enum.Parse(TypeEntity, Enum.GetValues(TypeEntity).GetValue(IndexCreateToScene).ToString()));
        }
        private static GameObject CreateEntity(Enum Entity)
        {
            IEntityCreated entity = EntityCreate.GetEntity(Entity, Vector3.zero, Quaternion.identity, Stationary);
            return entity.GetPrefab;
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace Assets.Editors
{
    [CustomEditor(typeof(Transform), true), CanEditMultipleObjects]
    public class CustomTransform : Editor
    {
        private static bool ViewCustomFunction;
        private static bool IsEntity;

        private bool IsRandomScale;
        private float RandomMin, RandomMax;

        private static bool IsMoveClick;
        private static Transform Instanse;

        private static IAlive iAlive;
        private static bool isAlive;

        [MenuItem("CONTEXT/Transform/RandomScale")]
        public static void RandomScale(MenuCommand command)
        {
            Transform _object = (Transform)command.context;
            Undo.RecordObject(_object, _object.name);
            _object.localScale = Vector3.one * Random.Range(0.2F, 2F);
        }
        [MenuItem("CONTEXT/Transform/Rebuild Entity", false)]
        private static void RebuildEntitynull()
        {
            Undo.RecordObject(Instanse, Instanse.name + " Random rotate");

            if (entityEngine is ItemEngine itemEngine)
            {
                ItemEngine.AddItem(itemEngine.itemType, Instanse.position, Instanse.rotation, itemEngine.Stationary);
            }
            else if (entityEngine is AnimalEngine animal)
            {
                AnimalEngine.AddAnimal(animal.TypeAnimal, Instanse.position, Instanse.rotation);
            }
            else if (entityEngine is PlantEngine plant)
            {
                PlantEngine.AddPlant(plant.typePlant, Instanse.position, Instanse.rotation);
            }
            Undo.RecordObject(Instanse.gameObject, Instanse.name + " Delete");
            GameObject.DestroyImmediate(Instanse.gameObject);
        }
        [MenuItem("CONTEXT/Transform/Rebuild Entity", true)]
        private static bool RebuildEntity()
        {
            return IsEntity;
        }
        private static EntityEngine entityEngine;
        public void OnEnable()
        {
            Instanse = (Transform)target;
            IsEntity = Instanse.TryGetComponent(out entityEngine);
            isAlive = Instanse.TryGetComponent(out iAlive);
        }
        void OnSceneGUI()
        {
            if (IsMoveClick && Event.current.button == 1 && Event.current.type == EventType.MouseDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector3 newPosition = hit.point + ray.direction.normalized * 0.01F;
                    Undo.RecordObject(Instanse, "MovePosition");
                    Instanse.position = newPosition;
                    Selection.activeGameObject = Instanse.gameObject;
                }
            }
        }
        public override void OnInspectorGUI()
        {
            if (!EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth / 3;
            }
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            IsMoveClick = EditorGUILayout.Toggle("Set position to Click", IsMoveClick);

            if (!IsEntity)
                goto EndGUI;
           
            ViewCustomFunction = GUILayout.Toggle(ViewCustomFunction, "     View Custom Function");
           
            if (!ViewCustomFunction)
                goto EndGUI;
            
            if (GUILayout.Button("Apply position to Terrain"))
            {
                Ray ray = new Ray(Instanse.position, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit Hit))
                {
                    Undo.RecordObject(Instanse, Instanse.gameObject.name + "Applay position to Terrain");
                    Instanse.position = Hit.point + ray.direction.normalized * 0.01F;
                }
            }

            if (GUILayout.Button("Random rotate"))
            {
                Undo.RecordObject(Instanse, Instanse.name + " Random rotate");
                Instanse.rotation = Quaternion.AngleAxis(Random.Range(0F, 360), Vector3.up);
            }
            
            EndGUI:
            if(isAlive)
            {
                if(!iAlive.IsDead && GUILayout.Button("Kill"))
                {
                    iAlive.Dead();
                }
            }
            GUILayout.BeginHorizontal();
            IsRandomScale = GUILayout.Toggle(IsRandomScale, "Random?");
            RandomMin = EditorGUILayout.FloatField(RandomMin);
            if(IsRandomScale)
            {
                RandomMax = EditorGUILayout.FloatField(RandomMax);
            }
            if(GUILayout.Button("Set Scale"))
            {
                Instanse.localScale = Vector3.one * (IsRandomScale ? Random.Range(RandomMin, RandomMax) : RandomMin);
            }
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}

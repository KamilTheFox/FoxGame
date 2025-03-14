﻿using UnityEditor;
using System.Linq;
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
        

        private static IDiesing iAlive;
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
            CreateMenuEntity.RebuildEntity(Instanse);
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
            if (CreateMenuEntity.CreateToScene && Event.current.button == 1 && Event.current.type == EventType.MouseDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector3 newPosition = hit.point + ray.direction.normalized * 0.01F;
                    GameObject game = CreateMenuEntity.GetCurrentCreateEntity();
                    Undo.RecordObject(game, "Create Entity");
                    game.transform.position = newPosition;
                    Selection.activeGameObject = game.gameObject;
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
            DrawDefaultInspector();

            EditorGUI.BeginChangeCheck();
            IsMoveClick = EditorGUILayout.Toggle("Set position to Click", IsMoveClick);

            if (!IsEntity)
                goto EndGUI;
           
            ViewCustomFunction = GUILayout.Toggle(ViewCustomFunction, "     View Custom Function");
           
            if (!ViewCustomFunction)
                goto EndGUI;
            
            if (GUILayout.Button("Apply position to Terrain"))
            {
                if(targets.Length > 1)
                {
                    foreach (var target in targets)
                    {
                        Transform transform = (Transform)target;
                        Ray ray = new Ray(transform.position, Vector3.down);
                        if (Physics.Raycast(ray, out RaycastHit Hit1))
                        {
                            Undo.RecordObject(transform, transform.gameObject.name + "Applay position to Terrain");
                            transform.position = Hit1.point + ray.direction.normalized * 0.01F;
                        }
                    }
                    return;
                }
                Ray ray1 = new Ray(Instanse.position, Vector3.down);
                if (Physics.Raycast(ray1, out RaycastHit Hit))
                {
                    Undo.RecordObject(Instanse, Instanse.gameObject.name + "Applay position to Terrain");
                    Instanse.position = Hit.point + ray1.direction.normalized * 0.01F;
                }
            }

            if (GUILayout.Button("Random rotate"))
            {
                if (Selection.transforms.Length > 1)
                    Selection.transforms.ToList().ForEach(t => t.rotation = Quaternion.AngleAxis(Random.Range(0F, 360), Vector3.up));
                else
                    Instanse.rotation = Quaternion.AngleAxis(Random.Range(0F, 360), Vector3.up);
            }
            
            EndGUI:
            if(isAlive)
            {
                if(!iAlive.IsDie && GUILayout.Button("Kill"))
                {
                    iAlive.Death();
                }
                if (!iAlive.IsDie && GUILayout.Button("Kill AddForse"))
                {
                    iAlive.Death();
                    iAlive.Transform.GetChild(0).GetComponentsInChildren<Rigidbody>().ToList().ForEach((r) => r.AddForce(Vector3.up));
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
                if (Selection.transforms.Length > 1)
                    Selection.transforms.ToList().ForEach(t => t.localScale = Vector3.one * (IsRandomScale ? Random.Range(RandomMin, RandomMax) : RandomMin));
                else
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

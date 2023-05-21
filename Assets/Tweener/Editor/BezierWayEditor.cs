using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tweener;
using UnityEditor;

namespace Assets.Editors
{
    [CustomEditor(typeof(BezierWay)), CanEditMultipleObjects]
    public class BezierWayEditor : Editor
    {
        public BezierWay BezierWay;
        public static bool ViewList;

        [MenuItem("Tweener/Create Bezier Way")]
        public static void CreateBezierWay()
        {
            new GameObject("BezierWay", typeof(BezierWay));
        }
        public void OnEnable()
        {
            BezierWay = (BezierWay)target;
        }
        private void SetSellectPoint(GameObject obj)
        {
            Selection.activeGameObject = obj;
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();


            GUILayout.BeginHorizontal();
            GUILayout.Label($"Point {BezierWay.Count}: ");
            if (GUILayout.Button("Add"))
            {
                BezierPoint bezier = new BezierPoint();
#pragma warning disable 0618
                bezier._Point = new GameObject("Point").transform;
                bezier._Point.position = Vector3.zero;
                bezier._Point.SetParent(BezierWay.transform);
                bezier._Enter = new GameObject("Enter").transform;
                bezier._Enter.localPosition = Vector3.up;
                bezier._Enter.SetParent(bezier._Point);
                BezierWay.pointsBezier.Add(bezier);
            }
            if (GUILayout.Button("Remove"))
            {
                int index = BezierWay.Count - 1;
                GameObject.DestroyImmediate(BezierWay[index]._Point.gameObject);
                BezierWay.pointsBezier.RemoveAt(index);
            }
            GUILayout.EndHorizontal();
            GUILayout.Label($"Distance: {BezierWay.DistanceWay}");
            BezierWay.Visible = EditorGUILayout.Toggle("Draw Bezier?", BezierWay.Visible);
            ViewList = EditorGUILayout.Toggle("List Point", ViewList);
            if (BezierWay.Count > 0 && ViewList)
            {
                foreach (BezierPoint point in BezierWay)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(point.ToString());
                    if (GUILayout.Button("Point"))
                    {
                        SetSellectPoint(point._Point.gameObject);
                    }
                    if (GUILayout.Button("Enter"))
                    {
                        point.ControllerForEnter = true;
                        SetSellectPoint(point._Enter.gameObject);
                    }
                    if (GUILayout.Button("Exit"))
                    {
                        point.ControllerForEnter = false;
                        SetSellectPoint(point._Enter.gameObject);
                    }
                    if (GUILayout.Button("View"))
                    {
                        point.Visible = !point.Visible;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}

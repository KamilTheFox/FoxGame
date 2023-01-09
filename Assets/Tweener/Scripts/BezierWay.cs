using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tweener
{
    [Serializable]
    public class BezierWay
    {
        public BezierWay(BezierPoint[] _points)
        {
            points = _points.ToList();
        }
        public List<BezierPoint> points;
        public BezierPoint this[int index]
        {
            get { return points[index]; }
            set { points[index] = value; }
        }
        public int Count => points.Count;
        public float DistanceWay
        {
            get
            {
                float Distance = 0F;
                for (int lines = 1; lines < Count; lines++)
                    Distance += GetDistance(lines);
                return Distance;
            }
        }
        public float GetSegmentPercentage(int indexSegmentWay)
        {
            return GetDistance(indexSegmentWay) / DistanceWay;
        }
        public int GetSegment(float progressWay)
        {
            float procent = 0F;
            for(int i =1; i< points.Count-1; i++)
            {
                procent += GetSegmentPercentage(i);
                if (procent > progressWay)
                    return i; 
            }
            return points.Count - 1;
        }
        /// <param name="indexSegmentWay">The number of segments starts with one</param>
        /// <returns></returns>
        public float GetDistance(int indexSegmentWay)
        {
            if (this.Count <= 1 && this.Count > indexSegmentWay)
            {
                Debug.LogWarning("The path has no sigment");
                return 0f;
            }
            int sigmentsNumber = 30;
            Vector3 preveousePoint;
            float Distance = 0F;

            preveousePoint = this[indexSegmentWay - 1].Point;
            for (int i = 0; i < sigmentsNumber + 1; i++)
            {
                float paremeter = (float)i / sigmentsNumber;
                Vector3 point = Bezier.GetPoint(
                    this[indexSegmentWay - 1].Point, this[indexSegmentWay - 1].Exit,
                    this[indexSegmentWay].Entrance, this[indexSegmentWay].Point,
                    paremeter);
                Distance += Vector3.Distance(point, preveousePoint);
                preveousePoint = point;
            }
            return Distance;
        }
        public void ReverseWay()
        {
            foreach (BezierPoint point in points)
                point.ReverseEntranceExit();
            points.Reverse();
        }
#if UNITY_EDITOR
        public bool Visible = true;
        public void OnGizmos()
        {
            if (Visible)
                drawString("Dist:" + DistanceWay.ToString(), this[0].Point + Vector3.up, Color.white);
            int sigmentsNumber = 20;
            Vector3 preveousePoint;
            for (int lines = 1; lines < Count; lines++)
            {
                preveousePoint = this[lines - 1].Point;
                for (int i = 0; i < sigmentsNumber + 1; i++)
                {
                    float paremeter = (float)i / sigmentsNumber;
                    Vector3 point = Bezier.GetPoint(this[lines - 1].Point, this[lines - 1].Exit, this[lines].Entrance, this[lines].Point, paremeter);
                    Gizmos.color = Color.white;
                    if (Visible)
                        Gizmos.DrawLine(preveousePoint, point);
                    preveousePoint = point;
                }
                if (!Visible) continue;
                this[lines - 1].OnGizmos();
                drawString((lines).ToString(), this[lines - 1].Point, Color.white);
                if (lines == Count - 1)
                {
                    drawString((lines + 1).ToString(), this[lines].Point, Color.white);
                    this[lines].OnGizmos();
                }
            }
        }
        static void drawString(string text, Vector3 worldPos, Color? colour = null)
        {
            UnityEditor.Handles.BeginGUI();
            if (colour.HasValue) GUI.color = colour.Value;
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - (size.x), view.position.height - (screenPos.y + 1F), size.x, size.y), text);
            UnityEditor.Handles.EndGUI();
        }
#endif
    }
}

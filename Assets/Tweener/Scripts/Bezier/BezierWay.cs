using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#pragma warning disable 0618
namespace Tweener
{
    public class BezierWay : MonoBehaviour, IEnumerable
    {
        public BezierWay(BezierPoint[] _points)
        {
            pointsBezier = _points.ToList();
        }
        public List<BezierPoint> pointsBezier;
        public BezierPoint this[int index]
        {
            get { return pointsBezier[index]; }
            set { pointsBezier[index] = value; }
        }
        public int Count => pointsBezier.Count;
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
            for(int i =1; i< pointsBezier.Count-1; i++)
            {
                procent += GetSegmentPercentage(i);
                if (procent > progressWay)
                    return i; 
            }
            return pointsBezier.Count - 1;
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
                    this[indexSegmentWay].Enter, this[indexSegmentWay].Point,
                    paremeter);
                Distance += Vector3.Distance(point, preveousePoint);
                preveousePoint = point;
            }
            return Distance;
        }
        public static (Vector3, Vector3) GetPositionInProgress(BezierWay way, float percentage)
        {
            float progressPrevious = 0F;

            int CurrentLine = way.GetSegment(percentage);

            for (int i = 1; i < CurrentLine; i++)
                progressPrevious += way.GetSegmentPercentage(i);

            float ProgressLine = (percentage - progressPrevious) / way.GetSegmentPercentage(CurrentLine);

            return (Bezier.GetPoint(way[CurrentLine - 1].Point, way[CurrentLine - 1].Exit, way[CurrentLine].Enter, way[CurrentLine].Point, ProgressLine),
            Bezier.GetDirection(way[CurrentLine - 1].Point, way[CurrentLine - 1].Exit, way[CurrentLine].Enter, way[CurrentLine].Point, ProgressLine));
        }
        public void ReverseWay()
        {
            isReverse = !isReverse;
               pointsBezier.Reverse();
               foreach (BezierPoint point in pointsBezier)
                point.ReverseEntranceExit();
        }
        public IEnumerator GetEnumerator()
        {
            if (pointsBezier == null)
                pointsBezier = new();
            return pointsBezier.GetEnumerator();
        }
        private bool isReverse;
#if UNITY_EDITOR
        [Obsolete("For EditorController. #if UNITY_EDITOR")]
        public BezierWay Parent;
        public bool Visible = true;
        private void OnDrawGizmos()
        {
            if (Parent != null)
            {
                for (int i = 0; i < Parent.pointsBezier.Count; i++)
                {
                    pointsBezier[i]._Point.position = Parent.pointsBezier[isReverse ? Parent.pointsBezier.Count - 1 - i : i].Point;
                    BezierPoint bezier = Parent.pointsBezier[isReverse ? Parent.pointsBezier.Count - 1 - i : i];
                    pointsBezier[i]._Enter.localPosition = isReverse ? bezier.ControllerForEnter? bezier.ExitLocal: bezier.EnterLocal : bezier.ControllerForEnter ? bezier.EnterLocal : bezier.ExitLocal;
                }
            }
            foreach (BezierPoint bezier in this)
            {
                bezier.OnGizmos();
            }
            if (Count < 1)
                return;
            if (Visible)
                drawString("Dist:" + DistanceWay.ToString(), this[0].Point + Vector3.up, Color.white);
            int sigmentsNumber = 20;
            Vector3 preveousePoint;
            for (int lines = 1; lines < Count; lines++)
            {
                preveousePoint = this[lines - 1].Point;
                if (Visible)
                for (int i = 0; i < sigmentsNumber + 1; i++)
                {
                    float paremeter = (float)i / sigmentsNumber;
                    Vector3 point = Bezier.GetPoint(this[lines - 1].Point, this[lines - 1].Exit, this[lines].Enter, this[lines].Point, paremeter);
                    Gizmos.color = Color.white;
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

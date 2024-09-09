using System;
using UnityEngine;

namespace Tweener
{
    [Serializable]
    public class BezierPoint
    {
        public Vector3 Enter => EnterLocal + Point;
        public Vector3 Exit => ExitLocal + Point;

        public Vector3 Point, EnterLocal, ExitLocal;
        public void ReverseEntranceExit()
        {
            Vector3 vector = EnterLocal;
            EnterLocal = ExitLocal;
            ExitLocal = vector;
#if UNITY_EDITOR
#pragma warning disable 0618
            _Enter.localPosition = ControllerForEnter ? EnterLocal : ExitLocal;
#endif
        }
        public Transform _Enter;
        public Transform _Point;
#if UNITY_EDITOR

        [Obsolete("For EditorController. #if UNITY_EDITOR")]
        public bool ControllerForEnter;
        [Obsolete("For Editor. #if UNITY_EDITOR")]
        public bool Visible;
        [Obsolete("For Editor. #if UNITY_EDITOR")]
        public void OnGizmos()
        {
            Point = _Point.position;
            if (ControllerForEnter)
            {
                EnterLocal = _Enter.localPosition;
                ExitLocal = -EnterLocal;

            }
            else
            {
                ExitLocal = _Enter.localPosition;
                EnterLocal = -ExitLocal;
            }
            if (!Visible) return;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(Enter, 0.1F);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Exit, 0.1F);
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(Point, 0.2F);
        }
#endif
        public override string ToString()
        {
            return $"{Point}; {Enter}";
        }
    }

}

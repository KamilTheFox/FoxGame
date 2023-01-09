using System;
using UnityEngine;

namespace Tweener
{
    [Serializable]
    public class BezierPoint
    {
        public BezierPoint(Vector3 mainPoint, Vector3 entrance)
        {
            _Point = new GameObject("Point").transform;
            _Point.transform.position = mainPoint;
            _Point.position = mainPoint;
            _Entrance = new GameObject("PointEntrance").transform;
            _Entrance.SetParent(_Point);
            _Entrance.transform.localPosition = entrance;
        }

        public Transform _Entrance;

        public Transform _Point;

        public Vector3 EntranceLocal => _Entrance.localPosition;
        public Vector3 Point => _Point.position;
        public Vector3 Entrance => _Entrance.position;
        public Vector3 Exit => -EntranceLocal + Point;
        public Vector3 ExitLocal => Exit - Point;
        public void ReverseEntranceExit()
        {
            _Entrance.localPosition = ExitLocal;
        }
        public bool Visible;
        public void OnGizmos()
        {
            if (!Visible) return;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(Entrance, 0.2F);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Exit, 0.2F);
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(Point, 0.4F);
        }
    }

}

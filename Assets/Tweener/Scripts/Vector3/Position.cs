using System;
using UnityEngine;

namespace Tweener
{
    internal class Position : Vector3Tween
    {
        protected override string NameOperation => "Position";
        public Position(Transform _transform, Vector3 position, float _time, bool isAdd) : base(_transform, position, _time, isAdd)
        {
        }
        protected override void OnUpdate(float percentage)
        {
            transform.position = Vector3.Lerp(oldValue, strivingValue, percentage);
        }

        protected override Vector3 GetValue(Transform transform)
        {
            return transform.position;
        }
    }
}

using System;
using UnityEngine;

namespace Tweener
{
    internal class Scale : Vector3Tween
    {
        protected override string NameOperation => "Scale";
        public Scale(Transform _transform, Vector3 position, float _time, bool isAdd) : base(_transform, position, _time, isAdd)
        {
        }
        protected override void OnUpdate(float percentage)
        {
            transform.localScale = Vector3.Lerp(oldValue, strivingValue, percentage);
        }

        protected override Vector3 GetValue(Transform transform)
        {
            return transform.localScale;
        }
    }
}

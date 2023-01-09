using System;
using UnityEngine;

namespace Tweener
{
    internal abstract class Vector3Tween : Tweener
    {
        protected bool oldTweenIsReverse;
        private bool rewrite;

        private readonly bool isAdd = false;

        protected Vector3 oldValue;
        protected Vector3 strivingValue;

        protected Vector3 oldStrivingValue;
        public Vector3Tween(Transform _transform, Vector3 newRotation, float _time , bool _isAdd) : base(_transform, _time)
        {
            isAdd = _isAdd;
                oldValue = GetValue(_transform);
            strivingValue = newRotation;
            if (isAdd)
                strivingValue += rewrite? oldStrivingValue : oldValue ;
        }
        protected override void Rewrite(ITweenable tweenable)
        {
            rewrite = true;
            Vector3Tween vector3 = (Vector3Tween)tweenable;
            oldStrivingValue = vector3.strivingValue;
            oldTweenIsReverse = vector3.reverseProgress;
        }
        protected abstract Vector3 GetValue(Transform transform);
        protected override void RewriteReverseValue()
        {
            ReverseValues(ref oldValue, ref strivingValue);
            if (oldTweenIsReverse)
                strivingValue = oldStrivingValue;
        }
    }
}

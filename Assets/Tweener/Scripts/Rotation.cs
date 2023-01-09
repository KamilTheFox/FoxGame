using UnityEngine;

namespace Tweener
{
    internal class Rotation : Vector3Tween
    {
        protected override string NameOperation => "Rotation";
        public Rotation(Transform _transform, Vector3 position, float _time, bool isAdd) : base(_transform, position, _time, isAdd)
        {
        }
        protected override void OnUpdate(float percentage)
        {
            transform.localRotation = Quaternion.Lerp(Quaternion.Euler(oldValue), Quaternion.Euler(strivingValue), percentage);
        }

        protected override Vector3 GetValue(Transform transform)
        {
            return transform.localRotation.eulerAngles;
        }
    }
}

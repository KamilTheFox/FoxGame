using UnityEngine;

namespace PlayerDescription
{
    public struct Bone
    {
        public Vector3 position;
        public Quaternion rotation;

        public static implicit operator Bone(Transform transform)
        {
            return new Bone()
            {
                position = transform.position,
                rotation = transform.rotation,
            };
        }
    }
}

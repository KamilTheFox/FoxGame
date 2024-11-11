using System;
using UnityEngine;

namespace PlayerDescription
{
    public interface IInputCaracter
    {
        public void Enable() { }

        public void Disable() { }

        public bool IsRun { get; }

        public bool IsCrouch { get; }

        public Vector3 Move(Transform source, out bool isMove);

        public bool Space();

        public bool Shift() => false;

        public bool Ctrl() => false;

        public bool Alt() => false;
    }
}

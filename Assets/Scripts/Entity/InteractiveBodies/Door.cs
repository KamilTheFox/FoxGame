using System.Collections.Generic;
using Tweener;
using UnityEngine;

namespace InteractiveBodies
{
    public class Door : InteractiveBody, ILockable, IInteractive
    {
        private bool isClosed = true;
        public bool IsClosed 
        {
            get
            {
                return isClosed;
            }
            set
            {
                isClosed = value;
                if(!value)
                    locked = null;
                Tween.AddRotation(Transform, new Vector3(0f, value ? 90F : -90F, 0))
                .ChangeEase(Ease.CubicRoot);
            }
        }
        Rigidbody IInteractive.Rigidbody => Rigidbody;

        private ILocking locked;
        public bool isLocked
        {
            get
            {
                if (locked == null)
                    return false;
                return !locked.UnLock(() =>
                {
                    IsClosed = false;
                });
            }
        }

        protected override void OnStart()
        {
            List<MeshCollider> meshes = new List<MeshCollider>();
            meshes.AddRange(GetComponentsInChildren<MeshCollider>());
            meshes.ForEach(c => c.convex = true);
        }
        protected override void OnAwake()
        {
            base.OnAwake();
            if (!TryGetComponent(out Rigidbody))
                Rigidbody = gameObject.AddComponent<Rigidbody>();
            Rigidbody.isKinematic = true;
        }
        public override void Interaction()
        {
            if(isLocked || locked != null) return;
            IsClosed = !IsClosed;
        }

        public void Lock(ILocking locking)
        {
            if (!isClosed)
                return;
            if(locked != null && locked.Transform.parent == Transform && !locked.UnLock())
            {
                return;
            }
            locked = locking;
            locked.Lock();
            locked.Transform.SetParent(Transform);
        }
        public override TextUI GetTextUI()
        {
            return new TextUI(() => new object[]
            {
            LText.Door,
            "\n[",LText.KeyCodeF ,"] -", IsClosed ? LText.Open : LText.Close
            });
        }
    }
}

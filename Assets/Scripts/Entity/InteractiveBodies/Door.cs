using System.Collections.Generic;
using Tweener;
using UnityEngine;
using UnityEngine.AI;

namespace InteractiveBodies
{
    public class Door : InteractiveBody, ILockable, IInteractive
    {
        private bool isClosed = true;

        private ILocking Locker
        {
            get
            {
                if(locker == null) return null;
                return locker.GetComponent<ILocking>();
            }
        }
        [SerializeField] private GameObject locker;

        [SerializeField] private Vector3 OpenEuler = new Vector3(0,-90,0);
        public bool IsClosed 
        {
            get
            {
                return isClosed;
            }
            private set
            {
                isClosed = value;
                if(!value)
                    locked = null;
                Tween.AddRotation(Transform, OpenEuler * (!value ? 1F : -1F))
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
            if (Locker == null) return;
                Lock(Locker);
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

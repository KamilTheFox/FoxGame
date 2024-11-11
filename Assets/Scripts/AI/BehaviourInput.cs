using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PlayerDescription;

namespace AIInput
{
    [Serializable]
    public abstract class BehaviourInput :  IInputCaracter
    {
        public BehaviourInput(CharacterBody body)
        {
            Character = body;
        }
        [field: SerializeField] public CharacterBody Character { get; private set; }

        [field: SerializeField] public bool IsRun { get; protected set; }

        [field: SerializeField] protected Vector3 CurrentTorward { private get; set; }

        [field: SerializeField] protected bool CurrentSpace { get; set; }

        public abstract void Enable();

        public abstract void Disable();

        public bool IsCrouch => false;

        protected abstract Vector3 Move(Transform source);

        Vector3 IInputCaracter.Move(Transform source, out bool isMove)
        {
            Vector3 torward = Move(source);
            isMove = torward != Vector3.zero;
            return torward;
        }

        public virtual bool Space()
        {
            return CurrentSpace;

        }
    }
}

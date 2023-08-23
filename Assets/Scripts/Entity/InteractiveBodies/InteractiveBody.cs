using System.Collections;
using UnityEngine;

namespace InteractiveBodies
{
    public abstract class InteractiveBody : EntityEngine, IInteractive
    {
        public override TypeEntity typeEntity => TypeEntity.InteractiveBody;

        protected override abstract void OnStart();
        public virtual void Interaction()
        {
            Debug.Log("No Intecact Action");
        }
        public override TextUI GetTextUI()
        {
            return new TextUI(() => new object[]
            {
            LText.Interactive, " ", LText.Body,
            "\n[",LText.KeyCodeF ,"] -", LText.Interactive
            });
        }
    }

}

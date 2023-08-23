using System;
using UnityEngine;

namespace InteractiveBodies
{
    public class GameButton : InteractiveBody
    {
        public event Action OnClick;

        public event Action OnDubleClick;

        public event Action OnTripleClick;

        private sbyte state = 0;
        protected override void OnStart()
        {
            Destroy(GetComponent<Renderer>());
            MeshCollider collider = GetComponent<MeshCollider>();
            collider.enabled = true;
            collider.convex = true;
        }
        public override void Interaction()
        {
            SoundMeneger.PlayPoint(SoundMeneger.Sounds.ClickButton, Transform.position);
            if (state == 0)
            {
                Invoke(nameof(InvokeClick), 0.5F);
            }
            state++;
        }

        private void InvokeClick()
        {
            switch (state)
            {
                case 2:
                    if (OnDubleClick == null)
                        goto default;
                    OnDubleClick.Invoke();
                    break;
                case 3:
                    if (OnTripleClick == null)
                        goto case 2;
                    OnTripleClick.Invoke();
                    break;
                default:
                    OnClick?.Invoke();
                    break;
            }
            state = 0;
        }

        public override TextUI GetTextUI()
        {
            return new TextUI(() => new object[]
            {
            LText.Button,
            "\n[",LText.KeyCodeF ,"] -", LText.Press
            });
        }
    }
}

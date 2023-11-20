using System;
using UnityEngine;
using UnityEngine.Events;

namespace InteractiveBodies
{
    public class GameButton : InteractiveBody
    {
        public event UnityAction OnClick
        {
            add
            {
                if (onClick == null)
                    onClick = new();
                onClick.AddListener(value);
            }
            remove => onClick?.RemoveListener(value);
        }


        public event UnityAction OnDubleClick
        {
            add
            {
                if (onDubleClick == null)
                    onDubleClick = new();
                onDubleClick.AddListener(value);
            }
            remove => onDubleClick?.RemoveListener(value);
        }


        public event UnityAction OnTripleClick
        {
            add
            {
                if (onTripleClick == null)
                    onTripleClick = new ();
                onTripleClick.AddListener(value);
            }
            remove => onTripleClick?.RemoveListener(value);
        }

        [SerializeField] private bool enableRender;

        [SerializeField] private UnityEvent onClick, onDubleClick, onTripleClick;

        private sbyte state = 0;
        protected override void OnStart()
        {
            GetComponent<Renderer>().enabled = enableRender;
            if (TryGetComponent(out MeshCollider collider))
            {
                collider.enabled = true;
                collider.convex = true;
            }
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
                    if (onDubleClick == null)
                        goto default;
                    onDubleClick.Invoke();
                    break;
                case 3:
                    if (onTripleClick == null)
                        goto case 2;
                    onTripleClick.Invoke();
                    break;
                default:
                    onClick?.Invoke();
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

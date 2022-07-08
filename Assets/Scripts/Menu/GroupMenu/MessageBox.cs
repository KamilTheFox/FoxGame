using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GroupMenu
{
    public class MessageBox : IActivatable
    {
        public static bool IsEnable { get { return Box && Box.activeInHierarchy; } }
        private static Button Ok, Cancel;
        private static Text Message;
        private static Image Icon;
        private static RectTransform MessageTransform;
        private static GameObject Box;
        private static Dictionary<MessageIcon, Sprite> Icons = new();
        private readonly Action ActionOK;
        private string message;
        private MessageIcon messageIcon;
        private IActivatable activatable;
        public TypeMenu TypeMenu => TypeMenu.MessageBox;
        public enum MessageIcon
        {
            None,
            Warning,
            Error,
            Info
        }
        private struct InfoRect
        {
            public InfoRect(float _x, float _Width)
            {
                x = _x;
                Width = _Width;
            }
            public float x, Width;
        }
        public MessageBox() { }
        public MessageBox(string _messageFormated, MessageIcon icon = MessageIcon.None)
        {
            messageIcon = icon;
            SetMessage(_messageFormated);
        }
        public MessageBox(string _messageFormated, Action _action, MessageIcon icon = MessageIcon.None)
        {
            messageIcon = icon;
            SetMessage(_messageFormated);
            ActionOK = _action;
        }
        public void Start()
        {
            FindUI();
            Cancel.onClick.AddListener(Default);
        }

        public void SetMessage(string messageFormated)
        {
            message = messageFormated.Trim();
        }
        private void OnClick()
        {
            ActionOK?.Invoke();
            Default();
        }
        private void SellectImageAndRect()
        {
            bool isImage = messageIcon != MessageIcon.None;
            InfoRect rect = isImage?  new InfoRect(45, 200) : new InfoRect(0, 270);
            Icon.gameObject.SetActive(isImage);
            MessageTransform.sizeDelta = new Vector2(rect.Width, MessageTransform.sizeDelta.y);
            MessageTransform.anchoredPosition = new Vector2(rect.x, MessageTransform.anchoredPosition.y);
        }
        private void FindUI()
        {
            foreach (MessageIcon icon in Enum.GetValues(typeof(MessageIcon)))
                if (icon != MessageIcon.None)
                    Icons.Add(icon, loadIcon(icon));

                Box = Menu.FindUIByPath(nameof(MessageBox));
                Cancel = Menu.FindUIByPath<Button>(nameof(Cancel), Box.transform);
                Message = Menu.FindUIByPath<Text>(Box.transform);
                MessageTransform = Message.gameObject.GetComponent<RectTransform>();
                Ok = Menu.FindUIByPath<Button>(nameof(Ok), Box.transform);
                Icon = Menu.FindUIByPath<Image>(nameof(Icon), Box.transform);
        }
        private void Default()
        {
            Cancel.gameObject.SetActive(true);
            Message.text = "None";
            Box.SetActive(false);
                Menu.ActivateMenu(activatable);
        }
        private Sprite loadIcon(MessageIcon icon)
        {
            Sprite sprite = Menu.GetSprite(icon.ToString());
            if (!sprite)
                Debug.Log($"Was Not found icon: {icon}");
            return sprite;
        }
        public void Activate()
        {
            activatable = Menu.IActiveMenu;
            if(messageIcon != MessageIcon.None)
                Icon.sprite = Icons[messageIcon];
            Box.SetActive(true);
            Message.text = message;
            Ok.onClick.RemoveAllListeners();
            Ok.onClick.AddListener(OnClick);
            SellectImageAndRect();
            if (ActionOK == null)
                Cancel.gameObject.SetActive(false);
        }
        public void Deactivate()
        {
            if(IsEnable)
            SoundMeneger.Play(SoundMeneger.Sounds.Warning);
        }
    }
}

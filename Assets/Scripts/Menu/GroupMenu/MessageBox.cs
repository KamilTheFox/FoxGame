using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GroupMenu
{
    public class MessageBox : IActivatableMenu
    {
        public static bool IsEnable { get { return Box && Box.activeInHierarchy; } }

        private static MenuUI<Button> Ok, Cancel;

        private static MenuUI<Text> MessageText, TitleText;

        private static MenuUI<Image> Icon;

        private static RectTransform MessageTransform;

        private static GameObject Box;

        private readonly Action ActionOK;

        private MessageIcon messageIcon;

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
        private TextUI TextMessage = new TextUI(LText.Null);
        private TextUI TextTitle = new TextUI(LText.Null);

        private static Queue<MessageBox> QueueMessage = new();
        public MessageBox() { }
        public MessageBox(TextUI _message, MessageIcon icon = MessageIcon.None)
        {
            messageIcon = icon;
            TextMessage = _message;
            AddQueue();
        }
        public MessageBox(TextUI _message, TextUI _title,  MessageIcon icon = MessageIcon.None)
        {
            messageIcon = icon;
            TextMessage =_message;
            TextTitle = _title;

            AddQueue();
        }
        public MessageBox(TextUI _message, Action _action, MessageIcon icon = MessageIcon.None)
        {
            messageIcon = icon;
            TextMessage = _message;
            ActionOK = _action;
            AddQueue();
        }
        public MessageBox(TextUI _message, TextUI _title, Action _action, MessageIcon icon = MessageIcon.None)
        {
            messageIcon = icon;
            TextMessage = _message;
            TextTitle = _title;
            ActionOK = _action;
            AddQueue();
        }
        public static void Warning(string Text, Action Ok = null)
        {
            Menu.ActivateMenu(new MessageBox(Text.GetTextUI(), LText.Warning.GetTextUI(), Ok, MessageIcon.Warning));
        }
        public static void Info(string Text, Action Ok = null)
        {
            Menu.ActivateMenu(new MessageBox(Text.GetTextUI(), LText.Information.GetTextUI(), Ok, MessageIcon.Info));
        }
        public static void Error(string Text, Action Ok = null)
        {
            Menu.ActivateMenu(new MessageBox(Text.GetTextUI(), LText.Error.GetTextUI(), Ok, MessageIcon.Error));
        }
        public static void Message(string Text, Action Ok = null)
        {
            Menu.ActivateMenu(new MessageBox(Text.GetTextUI(), Ok));
        }
        private void AddQueue()
        {
            if(Menu.CurrentMenu == TypeMenu)
            QueueMessage.Enqueue(this);
        }
        public void Start()
        {
            FindUI();
            Cancel.OnClick().AddListener(Default);
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

            Box = Menu.Find(nameof(MessageBox));

            TitleText = MenuUI<Text>.Find("Hat/Title", Box.transform, LText.Null);

            MessageText = MenuUI<Text>.Find("Message",Box.transform, LText.Null);

            MessageTransform = MessageText.gameObject.GetComponent<RectTransform>();

            Cancel = MenuUI<Button>.Find(nameof(Cancel), Box.transform, LText.Canсel);
            Ok = MenuUI<Button>.Find(nameof(Ok), Box.transform, LText.Ok);

            Icon = MenuUI<Image>.Find(nameof(Icon), Box.transform);
        }
        private void Default()
        {
            if(QueueMessage.Count>0)
            {
                Box.SetActive(false);

                QueueMessage.Dequeue().Activate();
                return;
            }
            Cancel.gameObject.SetActive(true);
            TitleText.SetText(new TextUI(LText.Null));
            Box.SetActive(false);
            Menu.PopMenu();
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

            bool isIcon = messageIcon != MessageIcon.None;

            if (messageIcon == MessageIcon.Warning)
                SoundMeneger.Play(SoundMeneger.Sounds.Warning);
            else if (messageIcon == MessageIcon.Error)
                SoundMeneger.Play(SoundMeneger.Sounds.Note);

            Icon.gameObject.SetActive(isIcon);
            if (isIcon)
                Icon.SetImage(messageIcon.ToString());

            Box.SetActive(true);

            MessageText.SetText(TextMessage);
            TitleText.SetText(TextTitle);

            Ok.OnClick().RemoveAllListeners();
            Ok.OnClick().AddListener(OnClick);

            SellectImageAndRect();
            if (ActionOK == null)
                Cancel.gameObject.SetActive(false);

        }
    }
}

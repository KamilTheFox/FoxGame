using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GroupMenu;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public interface IUpdateUIElement
{
    void UpdateText();
}
 static class MenuUIAutoRect
{
    static MenuUIAutoRect()
    {
        Menu.EventInitializeComponent += Reset;
    }
    private static void Reset()
    {
        NumberComponent = 0;
        Components.Clear();
        Binding_Queue.Clear();
        Binding_Heigth = 30;
        ScrollRect = null;
        ContentScrollBar = null;
        RectScrollBar = null;
    }
    public enum TypeRect
    {
        Default,
        Button,
        Slider,
    }
    private static MenuUI<ScrollRect> ScrollRect;
    private static Transform ContentScrollBar;
    private static RectTransform RectScrollBar;

    public static Queue<RectTransform> Binding_Queue = new ();
    public static int Binding_Heigth = 30;

    public static void Add_Queue(Component[] components)
    {
        foreach(Component component in components)
        {
            Add_Queue(component);
        }
    }
    public static void Add_Queue(Component component)
    {
        Binding_Queue.Enqueue(component.GetComponent<RectTransform>());
    }
    public static void AutoPositionComponent(Component _component, TypeRect type = TypeRect.Default, Func<Rect, Rect> ModiferAutoRect = null)
    {
        RectTransform rectTransform = _component.GetComponent<RectTransform>();
        AddComponent(rectTransform);
        Rect rect = GetRect(type, ModiferAutoRect);
        rectTransform.anchoredPosition = rect.position;
        rectTransform.sizeDelta = rect.size;
    }
    public static Rect GetRect(TypeRect typeRect = TypeRect.Default, Func<Rect,Rect> ModiferAutoRect = null)
    {
        Rect newrect = new Rect(new Vector2(0, 0), new Vector2(450, 50));
        if (ModiferAutoRect != null)
        {
            newrect = ModiferAutoRect(newrect);
            newrect.y -= newrect.height / 2 - 15;
        }
        newrect.y -= CountHeigth();
        Binding_Heigth += (int)newrect.height + 15;
        NumberComponent++;
        if (Components.Count < NumberComponent)
            Components.Add(null);
        if (CountHeigth() > 440 && ScrollRect == null)
        {
            Transform Parent = Components[0].parent;
            ScrollRect = MenuUI<ScrollRect>.Create("ScrollRect", Parent, LText.Null);
            ContentScrollBar = Menu.Find("Viewport/Content", ScrollRect.gameObject.transform).transform;
            RectScrollBar = ContentScrollBar.gameObject.GetComponent<RectTransform>();
            foreach (RectTransform newRect in Components)
            {
                SetNewParent(newRect, ContentScrollBar);
            }
            foreach(RectTransform _Queue in Binding_Queue)
            {
                SetNewParent(_Queue, ContentScrollBar);
            }
            SetRectScroll();
        }
        else if(CountHeigth() > 440)
        {
            RectTransform newRect = Components[NumberComponent - 1];
            if (newRect != null)
                newRect.SetParent(ContentScrollBar);
            SetRectScroll();
        }
        return newrect;
    }
    private static void SetNewParent(RectTransform rect, Transform parent)
    {
        if (rect == null) return;
        Vector2 AnchoredPosition = rect.anchoredPosition;
        Vector2 Size = rect.sizeDelta;
        rect.transform.SetParent(parent);
        rect.sizeDelta = Size;
        rect.anchoredPosition = AnchoredPosition;
    }
    public static Func<Rect,Rect> SetWidth(float width)
    {
        return (rect) => { rect.width = width; return rect; };
    }
    public static Func<Rect, Rect> SetHeigth(float height)
    {
        return (rect) => { rect.height = height; return rect; };
    }
    private static int CountHeigth()
    {
        return Binding_Heigth;
    }
    private static void SetRectScroll()
    {
        int Heigth = CountHeigth();
        RectScrollBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Heigth);
        RectScrollBar.anchoredPosition += new Vector2(0, -Heigth);

    }
    private static List<RectTransform> Components = new List<RectTransform>();

    public static void AddComponent(RectTransform rect)
    {
        Components.Add(rect);
    }
    private static int NumberComponent;
}
public class MenuUI<T> : IUpdateUIElement where T : Component
{
    private TextUI TextUI;
    public string Text { get => text != null ? text.text : null; private set { if (text) text.text = value; } }

    private Text text;

    private Text GetTextComponent
    {
        get
        {
            if(Component == null)
                return null;
            if(isText) 
                return text;
            if(text == null)
            {
                text = component.GetComponentInChildren<Text>();
            }
            return text;
        }
    }
    public bool isUpdateText { get; set; } = true;
    private bool isText;
    private Image image;

    private T component;
    public T Component
    {
        get { return isText ? (T)(Component)text : component; }
    }
    public GameObject gameObject => Component?.gameObject;

    private MenuUI(T component, TextUI textUI = new TextUI(), bool AutoRect = false, Func<Rect, Rect> ModiferAutoRect = null)
    {
        Init(component, textUI, AutoRect, ModiferAutoRect);
    }

    public static MenuUI<T> Find(T component, LText lText, bool AutoRect = false, Func<Rect, Rect> ModiferAutoRect = null)
    {
        return new MenuUI<T>(component, new TextUI(lText), AutoRect, ModiferAutoRect);
    }
    public static MenuUI<T> Find(T component, TextUI textUI = new TextUI(), bool AutoRect = false, Func<Rect, Rect> ModiferAutoRect = null)
    {
        return new MenuUI<T>(component, textUI, AutoRect, ModiferAutoRect);
    }
    public static MenuUI<T> Find(string path, Transform _Menu, LText lText, bool AutoRect = false, Func<Rect, Rect> ModiferAutoRect = null)
    {
        return new MenuUI<T>(Menu.Find<T>(path, _Menu), new TextUI(lText), AutoRect, ModiferAutoRect);
    }  
    public static MenuUI<T> Find(string path, Transform _Menu, TextUI textUI = new TextUI(), bool AutoRect = false, Func<Rect, Rect> ModiferAutoRect = null)
    {
        return new MenuUI<T>(Menu.Find<T>(path, _Menu), textUI, AutoRect, ModiferAutoRect);
    }
    private void Init(T _component, TextUI textUI = new TextUI(), bool AutoRect = false, Func<Rect, Rect> ModiferAutoRect = null)
    {
        _component.gameObject.SetActive(true);
        TextUI = textUI;
        Menu.ListTextUpdate.Add(this);
        MenuUIAutoRect.TypeRect type = MenuUIAutoRect.TypeRect.Default;

        if (AutoRect)
        {
            MenuUIAutoRect.AutoPositionComponent(_component, type, ModiferAutoRect);
        }
        else if(!AutoRect && ModiferAutoRect != null)
        {
            RectTransform rectTransform = _component.GetComponent<RectTransform>();
            Rect rect = ModiferAutoRect(new Rect());
            Vector2 ancorLeftTop = new Vector2(0, 1);
            rectTransform.anchorMin = ancorLeftTop;
            rectTransform.anchorMax = ancorLeftTop;
            rectTransform.pivot = ancorLeftTop;
            rectTransform.sizeDelta = rect.size;
            rectTransform.anchoredPosition = new Vector2(rect.position.x, -rect.position.y);
        }
        if (ModiferAutoRect != null && !AutoRect)
        {
            LayoutElement tElement = _component.gameObject.AddComponent<LayoutElement>();
            Rect rect = _component.GetComponent<RectTransform>().rect;
            rect.size = Vector2.zero;
            rect = ModiferAutoRect(rect);

            if (rect.size.x != 0)
                tElement.minWidth = rect.size.x;
            if (rect.size.y != 0)
                tElement.minHeight = rect.size.y;
        }


        EventTrigger trigger = null;
        if (_component.gameObject.TryGetComponent(out EventTrigger _trigger))
        {
            trigger = _trigger;
            trigger.triggers.Clear();
        }
        else
        {
            trigger = _component.gameObject.AddComponent<EventTrigger>();
        }
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        if (_component is Button button)
        {
            button.onClick.RemoveAllListeners();
            entry.eventID = EventTriggerType.PointerClick;
        }
        else if (_component is Scrollbar scrollbar)
        {
            scrollbar.onValueChanged.RemoveAllListeners();
            scrollbar.onValueChanged.AddListener((value) => UpdateText());
        }
        else if (_component is Slider slider)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener((value) => UpdateText());
        }
        else if (component is Text item)
        {
            isText = true;
            text = item;
        }
        
        foreach (Image image in _component.gameObject.GetComponentsInChildren<Image>(true))
        {
            MenuUI.SetImageMenu(image, image.name);
            SetpixelsPerUnitMultiplier(image);
        }
        Image mainImage;
        if (typeof(T) != typeof(Image) &&
            (mainImage = _component.gameObject.GetComponent<Image>()) != null)
        {
            MenuUI.SetImageMenu(mainImage, "NewUISprite");
            SetpixelsPerUnitMultiplier(mainImage);
        }

        entry.callback.AddListener((date) => UpdateText());
        trigger.triggers.Add(entry);
        if (!isText)
        {
            component = _component;
            text = component.GetComponentInChildren<Text>();
        }
        foreach (Text textElement in component.GetComponentsInChildren<Text>(true))
        {
            textElement.color = Settings.ColorText;
            Outline outline = textElement.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(1.5F, 1.5F);
            outline.effectColor = Settings.ColorOutline;
        }
    }
    private static void SetpixelsPerUnitMultiplier(Image image)
    {
        float deltaSize = image.rectTransform.rect.width < 35 ? image.rectTransform.rect.width / 35 : image.rectTransform.rect.height / 50;
        image.type = Image.Type.Sliced;
        if (image.rectTransform.rect.height <= 20)
        {
            image.pixelsPerUnitMultiplier = 2.3F;
            return;
        }
        if (image.rectTransform.rect.height <= 15)
        {
            image.pixelsPerUnitMultiplier = 3;
            return;
        }
        if (deltaSize < 1)
        {
            deltaSize += 1;
        }
        else if (deltaSize > 1)
        {
            deltaSize -= 1;
        }
        if (deltaSize > 1.5f)
        {
            deltaSize = 1.5f;
        }
        if (deltaSize < 0.05F)
        {
            deltaSize = 0.05F;
        }
        image.pixelsPerUnitMultiplier = deltaSize;
    }
    public Image SetImage(string nameImage,bool Outline = false)
    {
        Image image = GetImage();
        image.color = Settings.ColorText;
        if (Outline && !image.gameObject.TryGetComponent(out Outline outline))
        {
            outline = image.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(1.5F, 1.5F);
            outline.effectColor = Settings.ColorOutline;
        }
        image.sprite = Menu.GetSprite(nameImage);
        return image;
    }
    public Image SetImage(Texture2D texture)
    {
        Image image = GetImage();
        image.sprite = Sprite.Create(texture,new Rect(0F,0F, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        return image;
    }
    private Image GetImage()
    {
        if (component is Image oldImage)
        {
            return oldImage;
        }
        if (image == null)
        {
            GameObject gameObject = new GameObject("ImageComponent");
            image = GameObject.Instantiate(gameObject, Component.transform).AddComponent<Image>();
            image.GetComponent<RectTransform>().sizeDelta = component.GetComponent<RectTransform>().sizeDelta - new Vector2(6F, 6F);
            GameObject.Destroy(gameObject);
        }
        return image;
    }
    public static MenuUI<T> Create(string Name, Transform _Parent, LText textUI, bool AutoRect = false, Func<Rect, Rect> ModiferAutoRect = null)
    {
        return Create(Name, _Parent, textUI.GetTextUI(), AutoRect, ModiferAutoRect);
    }
    public static MenuUI<T> Create(string Name, Transform _Parent, TextUI textUI, bool AutoRect = false, Func<Rect, Rect> ModiferAutoRect = null)
    {
        var obj = Create(_Parent, textUI, AutoRect, ModiferAutoRect);
        obj.gameObject.name = Name + MenuUI.PrefixCreate;
        return obj;
    }
    public static MenuUI<T> Create(Transform _Parent, LText textUI, bool AutoRect = false, Func<Rect, Rect> ModiferAutoRect = null)
    {
        return Create(_Parent, textUI.GetTextUI(), AutoRect, ModiferAutoRect);
    }
    public static MenuUI<T> Create(Transform _Parent, TextUI textUI, bool AutoRect = false, Func<Rect, Rect> ModiferAutoRect = null)
    {
        GameObject gameObject = Resources.Load<GameObject>($"Menu\\{typeof(T).Name}");
        if (gameObject == null)
        {
            gameObject = new GameObject(typeof(T).Name, typeof(T));
        }
        T newObject = GameObject.Instantiate(gameObject, _Parent).GetComponent<T>();

        newObject.gameObject.name = gameObject.name + MenuUI.PrefixCreate;

        return new MenuUI<T>(newObject, textUI, AutoRect, ModiferAutoRect);
    }
    public MenuUI<T> SetText(TextUI textUI = new TextUI())
    {
        if (!text && (text = component.GetComponentInChildren<Text>()) == null)
        {
            return this;
        }
        TextUI = textUI;
        UpdateText();
        return this;
    }
    public void UpdateText()
    {
        if (!isUpdateText) return;
        if (!text) return;
        if (component is Dropdown)
        {
            return;
        }
        Text = TextUI.Text;
    }

    public object Get()
    {
        return Component;
    }
}

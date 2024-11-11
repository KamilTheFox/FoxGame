using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using GroupMenu;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Reflection;

public class Menu : MonoBehaviour
{
    public static Menu instance { get; private set; }

    public static UnityEvent onDestroy;

    private IActivatableMenu ICurrentMenu;
    public static TypeMenu CurrentMenu => instance.ICurrentMenu.TypeMenu;
    public static IActivatableMenu IActiveMenu => instance.ICurrentMenu;

    public static bool IsEnabled
        {
            get
            {
            return instance && instance.ICurrentMenu != null  && instance.ICurrentMenu.TypeMenu != TypeMenu.None;
            }
        }

    private static Sprite[] SpriteAtlasMenu;

    public IHootKeys hootKeys;

    public static List<IUpdateUIElement> ListTextUpdate { get; private set; }

    public static event Action EventInitializeComponent = () => { };

    private static Stack<IActivatableMenu> PreviousMenu;

    private static bool isClickBack;

    public static IActivatableMenu CurrentPauseMenu { get; set; }

    public static bool Pause => Time.timeScale == 0;

    public static void ActivatePauseMenu()
    {
        ActivateMenu(CurrentPauseMenu);
    }
    private void Awake()
    {
        if (instance)
        {
            Debug.LogWarning(new TextUI(LText.ErrorInitializeObjects, gameObject.ToString).ToString());
            GameObject.Destroy(gameObject);
            return;
        }
        CurrentPauseMenu = new Lobby();
        PreviousMenu = new();

        instance = this;
        onDestroy = new();
        ListTextUpdate = new();

        if (SpriteAtlasMenu == null)
        SpriteAtlasMenu = Resources.LoadAll<Sprite>("Menu\\Icons");

        ICurrentMenu = new None();

        InitializeComponent();

    }
    private void Start()
    {
        ICurrentMenu.Activate();
    }
    public static void PauseEnableGame(bool Enable)
    {
        Time.timeScale = Enable ? 0 : 1;
        SoundMeneger.PauseEnable(Enable);
    }
    public static Sprite GetSprite(string Name)
    { 
        return SpriteAtlasMenu.ToList().Find(find => find.name == Name);
    }
    private void InitializeComponent()
    {
        List<string> NameMenus = Enum.GetNames(typeof(TypeMenu)).ToList();
        int countMenu = NameMenus.Count();
        Type[] massClass = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == nameof(GroupMenu) && !t.IsAbstract && !t.IsInterface).ToArray();
        for (int i = 0; i < massClass.Length; i++)
        {
            if (Activator.CreateInstance(Type.GetType(massClass[i].FullName)) is IActivatableMenu activatable)
            {
                activatable.Start();
                EventInitializeComponent.Invoke();
                NameMenus.Remove(massClass[i].Name);
            }
        }
        Debug.Log($"Success start menu: {countMenu - NameMenus.Count()}/{countMenu}");
        foreach (string name in NameMenus)
            Debug.LogWarning($"Not started menu: {name} ");

        UpdateTextUI();
    }
    public enum TypeRebuildMainGroup
    {
        Only_Default,
        Only_Custom,
        Default_Custom,
        Custom_Default
    }
    public static void RebuildMenuInMainGroup<T>(TypeRebuildMainGroup type, Action<MainGroup> RebuildMenuUI = null) where T : MainGroup, IActivatableMenu, new()
    {
        string Prefix = MenuUI.PrefixCreate;
        T MenuParent = new ();
        List<Transform> Delited = new();
        foreach (Transform Components in MenuParent.GetTransform())
        {
            Components.gameObject.SetActive(false);
            if(Components.name.Contains("ScrollRect" + Prefix))
            {
                Transform[] content = Menu.Find("Viewport/Content", Components).transform.GetChilds();
                foreach (Transform child in content)
                    child.SetParent(MenuParent.GetTransform());
            }
            if (Components.name.Contains(Prefix))
                Delited.Add(Components);
        }
        foreach (Transform Components in Delited)
            GameObject.Destroy(Components.gameObject);
        switch (type)
        {
            case TypeRebuildMainGroup.Only_Default:
                MenuParent.Start();
                break;
            case TypeRebuildMainGroup.Only_Custom:
                RebuildMenuUI.Invoke(MenuParent);
                break;
                case TypeRebuildMainGroup.Custom_Default:
                RebuildMenuUI.Invoke(MenuParent);
                MenuParent.Start();
                break;
            default:
                MenuParent.Start();
                RebuildMenuUI.Invoke(MenuParent);
                break;
        }
        EventInitializeComponent.Invoke();

        UpdateTextUI();
    }
    public static void ExitGame()
    {
        onDestroy.RemoveAllListeners();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    /// <param name="isCallBack"> если это обратный вызов, то меню не занесется в стек </param>
    public static void PopMenu(bool isCallBack = false)
    {
        isClickBack = isCallBack;
        if (PreviousMenu.Count == 0)
            ActivateMenu(new None());
        else
            ActivateMenu(PreviousMenu.Pop());
    }
    public static void PushMenu()
    {
        if (isClickBack)
        {
            isClickBack = false;
            return;
        }
        if (IActiveMenu is MessageBox) return;
        PreviousMenu.Push(IActiveMenu);
    }
    public static void ActivateMenu<T>() where T : IActivatableMenu, new()
    {
        ActivateMenu(new T());
    }

    public static void ActivateMenu(IActivatableMenu menu)
    {
        if (CurrentMenu == menu.TypeMenu) return;
        instance.ICurrentMenu?.Deactivate();

        if (MessageBox.IsEnable)
        {
            if (PreviousMenu.ToArray().Length > 0 && PreviousMenu.ToArray()[0].TypeMenu != menu.TypeMenu)
            {
                PreviousMenu.Push(menu);
            }
            return;
        }
        menu.Activate();
        instance.ICurrentMenu = menu;
    }

    private void Update()
    {
        ICurrentMenu?.Update();
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ScreenCapture.CaptureScreenshotAsTexture();
        }
        if(hootKeys != null)
        {
            hootKeys.Action();
            return;
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            CameraControll.instance.OnFirstPerson();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            CameraControll.instance.OnThirdPerson();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            CameraControll.instance.OnThirdUnlookPerson();
        }
    }

    public static void UpdateTextUI()
    {
        if(ListTextUpdate != null)
        foreach (IUpdateUIElement update in ListTextUpdate)
            update?.UpdateText();
    }
    public void OnDestroy()
    {
        onDestroy?.Invoke();
        instance = null;
    }
    #region RedirectionMessageBox
    public static void Info(string message, Action OkButton = null)
    {
        MessageBox.Info(message, OkButton);
    }
    public static void Error(string message, Action OkButton = null)
    {
        MessageBox.Error(message, OkButton);
    }
    public static void Warning(string message, Action OkButton = null)
    {
        MessageBox.Warning(message, OkButton);
    }
    #endregion
    #region Find Component in Object
    public static GameObject Find(string path, Transform Parent = null)
    {
        Transform Intermediate;
        if(Parent)
            Intermediate = Parent;
        else
            Intermediate = instance.transform;
        if (string.IsNullOrEmpty(path))
            return Intermediate.gameObject;
        string[] UIcomponents = path.Split('/');
        for (int i = 0; i < UIcomponents.Length; i++)
        {
            string component = UIcomponents[i];
            GameObject newObject = Intermediate.Find(component)?.gameObject;
            if (!newObject)
            {
                Debug.LogWarning($"Was not Found UI By Path {path} / component: {component}");
                return null;
            }
            Intermediate = newObject.transform;
        }
        if (Intermediate)
            return Intermediate.gameObject;
        Debug.LogWarning($"Was not found GameObject in Path {path}");
        return null;
    }
    public static T Find<T>(Transform parent) where T : Component
    {
        GameObject gameObject = Find(null, parent);
        T component = gameObject.GetComponent<T>();
        if(component == null)
            component = gameObject.GetComponentInChildren<T>();
        if (component == null)
            Debug.LogWarning($"Component: {nameof(T)} was not found in Object {parent.name} and it's Children");
        return component;

    }
    
    public static T Find<T>(string path, Transform Parent = null, bool SearchInChildren = false) where T : Component
    {
        GameObject findObject = Find(path, Parent);
        if(!findObject)
        {
            return default;
        }

        T objComponent = SearchInChildren ? findObject.GetComponentInChildren<T>() : findObject.GetComponent<T>();
        if(objComponent != null)
        return objComponent;
        Debug.LogWarning($"Was not found Component in GameObject: {findObject.name}");
        return default;
    }
    #endregion
}


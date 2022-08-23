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

    private IActivatable ICurrentMenu;
    public static TypeMenu CurrentMenu => instance.ICurrentMenu.TypeMenu;
    public static IActivatable IActiveMenu => instance.ICurrentMenu;

    public static bool IsEnabled => instance && instance.ICurrentMenu != null  && instance.ICurrentMenu.TypeMenu != TypeMenu.None;

    private static Sprite[] SpriteAtlasMenu;

    public static List<IUpdateMenuUI> ListTextUpdate { get; private set; }

    public static event Action EventInitializeComponent = () => { };

    private static Stack<IActivatable> PreviousMenu;

    private static bool isClickBack;


    private void Awake()
    {
        if (instance)
        {
            Debug.LogError("Multiple menus cannot be initialized");
            return;
        }

        PreviousMenu = new();

        instance = this;
        onDestroy = new();
        ListTextUpdate = new();

        if (SpriteAtlasMenu == null)
        SpriteAtlasMenu = Resources.LoadAll<Sprite>("Menu\\Icons");

        ICurrentMenu = new None();

        ICurrentMenu.Activate();

        InitializeComponent();

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
            if (Activator.CreateInstance(Type.GetType(massClass[i].FullName)) is IActivatable activatable)
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
    public static void ExitGame()
    {
        onDestroy.RemoveAllListeners();
        Application.Quit();
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

    public static void ActivateMenu(IActivatable menu)
    {
        if (CurrentMenu == menu.TypeMenu) return;
        instance.ICurrentMenu?.Deactivate();

        if (MessageBox.IsEnable)
        {
            if (PreviousMenu.ToArray().Length >= 0 && PreviousMenu.ToArray()[0].TypeMenu != menu.TypeMenu)
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
        if(Input.GetKeyDown(KeyCode.F5))
        {
            ScreenCapture.CaptureScreenshot("D:\\ScreenFox.png");
        }
        
        ICurrentMenu?.Update();
    }

    public static void UpdateTextUI()
    {
        foreach (IUpdateMenuUI update in ListTextUpdate)
            update.UpdateText();
    }
    public void OnDestroy()
    {
        onDestroy?.Invoke();
        instance = null;
    }
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
public enum TypeMenu
{
    None,
    MessageBox,
    Lobby,
    Settings,
    MainMenu,
    Statistics,
    MediaPlayer,
    DebugAnimation,
    ConsoleMenu
}

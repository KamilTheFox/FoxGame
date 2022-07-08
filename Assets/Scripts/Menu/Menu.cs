using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using GroupMenu;
using System.Reflection;

public class Menu : MonoBehaviour
{
    public static Menu instance { get; private set; }

    private IActivatable ICurrentMenu;
    public static TypeMenu CurrentMenu => instance.ICurrentMenu.TypeMenu;
    public static IActivatable IActiveMenu => instance.ICurrentMenu;

    public static bool IsEnabled => instance && instance.ICurrentMenu != null  && instance.ICurrentMenu.TypeMenu != TypeMenu.None;

    private static Sprite[] SpriteAtlasMenu;
    public void Awake()
    {
        if (instance) return;
        instance = this;
        SpriteAtlasMenu = Resources.LoadAll<Sprite>("Other\\Menu");
        ICurrentMenu = new None();
        InitializeComponentInGroupMenu();
    }
    public static Sprite GetSprite(string Name)
    { 
        return SpriteAtlasMenu.Where(sprite => sprite.name == Name).ToArray()[0];
    }
    private void InitializeComponentInGroupMenu()
    {
        Type[] massClass = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == nameof(GroupMenu) && !t.IsAbstract && !t.IsInterface).ToArray();
        for (int i = 0; i < massClass.Length; i++)
        {
            if(Activator.CreateInstance(Type.GetType(massClass[i].FullName)) is IActivatable activatable)
                activatable.Start();
        }
    }
    public static void ActivateMenu(IActivatable menu)
    {
        instance.ICurrentMenu?.Deactivate();
        if (MessageBox.IsEnable) return;
        menu.Activate();
        instance.ICurrentMenu = menu;
    }

    #region Find Component in Object
    public static GameObject FindUIByPath(string path, Transform Parent = null)
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
    public static T FindUIByPath<T>(Transform parent)
    {
        GameObject gameObject = FindUIByPath(null, parent);
        T component = gameObject.GetComponent<T>();
        if(component == null)
            component = gameObject.GetComponentInChildren<T>();
        if (component == null)
            Debug.LogWarning($"Component: {nameof(T)} was not found in Object {parent.name} and it's Children");
        return component;

    }
    
    public static T FindUIByPath<T>(string path, Transform Parent = null, bool SearchInChildren = false)
    {
        GameObject findObject = FindUIByPath(path, Parent);
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
}

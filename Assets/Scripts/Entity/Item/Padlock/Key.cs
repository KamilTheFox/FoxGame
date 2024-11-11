using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using GroupMenu;

public class Key : ItemEngine, IDiesing, ITakenEntity, IInteractive
{
    public bool IsDie => false;

    private static IDictionary<Padlock.TypeKey, int> keys = new Dictionary<Padlock.TypeKey,int>();

    [SerializeField] private Padlock.TypeKey typeKey = Padlock.TypeKey.Red;

    private bool isUsed;
    protected override void OnAwake()
    {
        base.OnAwake();

        if(keys.ContainsKey(typeKey))
            keys[typeKey]++;
        else
            keys.Add(typeKey, 1);
    }
    public static int GetCountKey(Padlock.TypeKey key)
    {
        if(!keys.ContainsKey(key)) return 0;
        return keys[key];
    }
    protected override void onDestroy()
    {
        base.onDestroy();
    }
    public void Interaction()
    {
        if (isUsed)
            return;
        isUsed = true;
        if (keys[typeKey] > 0)
            MessageBox.Info($"Ключ {typeKey} найден." + (keys[typeKey] - 1 > 1 ? $"Осталось найти: {keys[typeKey] - 1}" : ""));
        keys[typeKey]--;
        Tweener.Tween.SetColor(Transform, new Color(0F,0F,0F,0F)).IgnoreAdd(Tweener.IgnoreARGB.RGB).ToCompletion(() => Delete());
        
    }
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
        {
            itemType.ToString(),
            new TextUI(() => new object[] {"\n[", LText.KeyCodeE ,"] - ",LText.Take ,"/",LText.Leave }),
            new TextUI(() => new object[] { "\n[", LText.KeyCodeF , "] -",  LText.Take }),
            new TextUI(() => new object[] { "\n[", LText.KeyCodeMouse0, "] - ", LText.Drop }),
        });
    }
    public void Death()
    {
    }
}

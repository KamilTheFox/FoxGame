using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using GroupMenu;

public class ItemEngine : EntityEngine, ITakenEntity, IDropEntity
{
    public override TypeEntity typeEntity => TypeEntity.Item;
    
    public static ItemEngine[] GetItems 
    {   
        get 
        { 
            List<ItemEngine> itemEngines = new List<ItemEngine>();
            Entities[TypeEntity.Item].ForEach(engine => { if (engine is ItemEngine item) { itemEngines.Add(item); } });
                return itemEngines.ToArray();
        }
    }

    [HideInInspector] public bool isController;

    public static int CountItems => GetItems.Length;

    [HideInInspector] public TypeItem itemType;
    Rigidbody ITakenEntity.Rigidbody => Rigidbody;

    private bool isDetectionModeContinuousDynamic;
    public bool IsDetectionModeContinuousDynamic
    {
        get => isDetectionModeContinuousDynamic;
        set
        {
            isDetectionModeContinuousDynamic = value;
            if(value)
                Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    public static int CountItemTypes { get { return Enum.GetNames(typeof(TypeItem)).Length; } }
    public int ID
    {
        get
        {
            for (int i = 0; i < GetItems.Length; i++)
            {
                if (GetItems[i] == this)
                    return i;
            }
            return -1;
        }
    }
    private bool IsTake => (Rigidbody == null && GameState.IsAdventure) || (GameState.IsAdventure && Rigidbody.mass > 20f);

    private bool IsDrop => (GameState.IsCreative &!Stationary && Rigidbody ) || (!Stationary && Rigidbody && Rigidbody.mass <= 5f);
    Rigidbody IDropEntity.Rigidbody => IsDrop ? Rigidbody : null;

    public event Action OnTake;
    public static ItemEngine AddItem(TypeItem type, Vector3 position, Quaternion quaternion, bool isStatic = true)
    {
        if (type == TypeItem.None) return null;
        ItemEngine item = AddEntity<ItemEngine>(type, position, quaternion, isStatic);
        return item;
    }
    protected static GameObject GetEffect(EffectItem type, Vector3 position)
    {
        return EntityCreate.GetEntity(type, position, Quaternion.identity).GetPrefab;
    }
    public static void RemoveItemAll()
    {
        ItemEngine[] items = GetItems;
        foreach (ItemEngine item in items)
        {
            item.Delete();
        }
    }
    public static void RemoveItemAt(int ItemIndex)
    {
        Remove:
        if (ItemIndex < 0 || ItemIndex > GetItems.Length)
            return;
        if (GetItems[ItemIndex].gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            ItemIndex--;
            goto Remove;
        }
        GetItems[ItemIndex].Delete();
    }
    public virtual ITakenEntity Take()
    {
        if(IsTake)
            return null;

        OnTake?.Invoke();
        return this;
    }
    private void FixedUpdate()
    {
        if (Rigidbody && !isDetectionModeContinuousDynamic)
        {
            if (Rigidbody.collisionDetectionMode != CollisionDetectionMode.Continuous && Rigidbody.velocity.magnitude < 2F)
                Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            else if (Rigidbody.collisionDetectionMode == CollisionDetectionMode.Discrete && Rigidbody.velocity.magnitude > 2F)
                Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        if (transform.position.y < -100F)
            Delete();
    }
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
            {
            itemType.ToString(),
            IsTake ? "" : new TextUI(() => new object[]
            { "\n[",LText.KeyCodeE ,"] -", LText.Take, "/", LText.Leave }),
            !IsDrop ? "" : new TextUI(() => new object[] { "\n[", LText.KeyCodeMouse0, "] - ", LText.Drop })
            });
    }
}




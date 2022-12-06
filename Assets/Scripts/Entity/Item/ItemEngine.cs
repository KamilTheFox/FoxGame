using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using GroupMenu;

public class ItemEngine : EntityEngine, ITakenEntity
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

    protected virtual bool CanTake => true;
    public static int CountItems => GetItems.Length;

    [HideInInspector] public TypeItem itemType;
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
    public bool IsTake { get; private set; }

    Rigidbody ITakenEntity.Rigidbody => Rigidbody;

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
    void ITakenEntity.Throw()
    {
        IsTake = false;
        TakeThrow();
    }
    ITakenEntity ITakenEntity.Take()
    {
        if (!CanTake) return null;
        IsTake = true;
        TakeThrow();
        return this;
    }
    private void TakeThrow()
    {
        if (Rigidbody != null && !Stationary)
        {
            Rigidbody.useGravity = !IsTake;
            if(Rigidbody.isKinematic)
            Rigidbody.isKinematic = false;
            Rigidbody.velocity = Vector3.zero;
        }
        gameObject.layer = IsTake ? LayerMask.NameToLayer("Ignore Raycast") : LayerMask.NameToLayer("Entity");
        foreach (Transform chield in Transform.GetComponentsInChildren<Transform>())
        {
            chield.gameObject.layer = gameObject.layer;
        }
    }
    public override void Interaction()
    {
        if(!Stationary&& isController)
        CameraControll.instance.EntranceBody(this.gameObject);
    }
    private void FixedUpdate()
    {
        if (Rigidbody)
        {
            if (Rigidbody.collisionDetectionMode != CollisionDetectionMode.Discrete && Rigidbody.velocity.magnitude < 4F)
                Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            else if (Rigidbody.collisionDetectionMode == CollisionDetectionMode.Discrete && Rigidbody.velocity.magnitude > 4F)
                Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        if (transform.position.y < -100F)
            Delete();
    }
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
        {
            new TextUI(() => new object[] {itemType.ToString() }),
            new TextUI(() => new object[] { "\n[", LText.KeyCodeE ,"] - ",LText.Take ,"/",LText.Leave }),
            new TextUI(() => new object[] {"\n[", LText.KeyCodeMouse0 ,"] - ",LText.Drop }),
            new TextUI(() => new object[] {"\n[",LText.KeyCodeF ,"] -", LText.Interactive })
        });
    }
}




using FactoryLesson;
using System;
using System.Collections.Generic;
using UnityEngine;
using GroupMenu;

public class ItemEngine : EntityEngine
{
    static ItemEngine()
    {
#if !UNITY_EDITOR
        Menu.onDestroy.AddListener(() => RemoveItemAll());
#endif
    }
    public static ItemEngine[] GetItems { get { return Items.ToArray(); } }

    public static int CountItems => Items.Count;
    static List<ItemEngine> Items = new();
    public TypeItem itemType;
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

    private bool IsTake;

    static bool DontShowMessage;
    public static ItemEngine AddItem(TypeItem type, Vector3 position, Quaternion quaternion, bool isStatic = true)
    {
        if (type == TypeItem.None) return null;
        ItemEngine item = AddEntity<ItemEngine>(type, position, quaternion, isStatic);
        Items.Add(item);
        return item;
    }
    protected static GameObject GetEffect(EffectItem type, Vector3 position)
    {
        return EntityFactory.GetEntity(type, position, Quaternion.identity).GetPrefab;
    }
    public static void RemoveItemAll()
    {
        ItemEngine[] items = Items.ToArray();
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
        if (Items[ItemIndex].gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (DontShowMessage)
            MessageBox.Warning($"Нельзя удалить объект {Items[ItemIndex].itemType} с игроком внутри\n" +
                "Будет удален следующий предмет\n" +
                "Нажмите ОК, что бы не показывать это сообщение\n",
                () =>
                {
                    DontShowMessage = true;
                });
            ItemIndex--;
            goto Remove;
        }
        Items[ItemIndex].Delete();
    }
    public ItemEngine Throw()
    {
        if (!IsTake)
            return null;
        IsTake = false;
        return TakeThrow();
    }
    public ItemEngine Take()
    {
        if (IsTake)
            return this;
        IsTake = true;
        return TakeThrow();
    }
    private ItemEngine TakeThrow()
    {
        if (Rigidbody != null)
        {
            Rigidbody.useGravity = !IsTake;
            Rigidbody.velocity = Vector3.zero;
        }
        gameObject.layer = IsTake ? LayerMask.NameToLayer("Ignore Raycast") : LayerMask.NameToLayer("Entity");
        foreach (Transform chield in gameObject.transform)
        {
            chield.gameObject.layer = gameObject.layer;
        }
        if (!IsTake)
            return null;
        return this;
    }
    public virtual void Interaction()
    {
        if(!Stationary)
        CameraControll.instance.EntranceBody(this.gameObject);
    }
    public override void Delete(float time = 0F)
    {
        base.Delete(time);
        Items.Remove(this);
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
}
public enum TypeItem
{
    None,
    Apple,
    Basket,
    Table,
    Chair,
    TNT
}
public enum EffectItem
{
    TNT_Detonate,
    TNT_Explosion
}


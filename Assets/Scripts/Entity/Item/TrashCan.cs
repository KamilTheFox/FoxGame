using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrashCan : ItemEngine
{
    List<ItemEngine> objectsDelete = new();
    protected override void OnStart()
    {
        base.OnStart();
        TriggerDetect trigger = GetComponentInChildren<TriggerDetect>();
        trigger.Enter += Enter;
        trigger.Exit += Exit;
    }
    public override void Interaction()
    {
        objectsDelete.ForEach(obj => Destroy(obj?.gameObject));
        objectsDelete.Clear();
        objectsDelete.Capacity = 10;
    }
    private bool GetItem(GameObject obj ,out ItemEngine entity)
    {
        entity = obj.gameObject.GetComponentInParent<ItemEngine>();
        return entity == null || entity.Transform == Transform;
    }
    private void Enter(Collider other)
    {
        if(!GetItem(other.gameObject, out ItemEngine item))
        objectsDelete.Add(item);
    }
    private void Exit(Collider other)
    {
        if (!GetItem(other.gameObject, out ItemEngine item))
            objectsDelete.Remove(item);
    }
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
        {
        base.GetTextUI(),
        "\n[",LText.KeyCodeF ,"] -", LText.Clear
        });
    }
}

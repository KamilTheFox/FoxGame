using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Container : ItemEngine
{
    protected List<ItemEngine> objectsContainment = new();
    protected override void OnStart()
    {
        base.OnStart();
        TriggerDetect trigger = GetComponentInChildren<TriggerDetect>();
        trigger.Enter += Enter;
        trigger.Exit += Exit;
        InitializeEvents();
    }
    protected virtual void InitializeEvents()
    {
        OnTake += () =>
        {
            SetParent();
            ChangeStateContainment(true);
        };
        OnTrown += () => ChangeStateContainment(false);
        OnSetMoveblePlatform += ChangeStateContainment;
    }
    private void ChangeStateContainment(bool isContainment)
    {
        for(int i = objectsContainment.Count - 1; i >= 0 ; i--)
        {
            var item = objectsContainment[i];
            if (item.Rigidbody == null) continue;
            item.Rigidbody.isKinematic = isContainment;
            if (isContainment)
            {
                item.SetParent(Transform);
            }
            else
            {
                SetDefaultProperty(item);
            }
        }
    }
    private bool GetItem(GameObject obj, out ItemEngine entity)
    {
        entity = obj.gameObject.GetComponentInParent<ItemEngine>();
        return entity != null && entity.Transform != Transform;
    }
    private void SetDefaultProperty(ItemEngine item)
    {
        objectsContainment.Remove(item);
        if (item.Transform.parent == Transform)
        {
            item.SetParent();
        }
    }
    private void Enter(Collider other)
    {
        if (GetItem(other.gameObject, out ItemEngine item))
        {
            if(objectsContainment.Contains(item) == false)
                objectsContainment.Add(item);
        }
    }
    private void Exit(Collider other)
    {
        if (GetItem(other.gameObject, out ItemEngine item))
        {
            if (item.Rigidbody != null && item.Rigidbody.isKinematic) return;
            SetDefaultProperty(item);
        }
    }
}

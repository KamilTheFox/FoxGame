using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FactoryLesson;
using System;

public abstract class EntityEngine : MonoBehaviour
{
    public Rigidbody Rigidbody;

    protected IEntityFamily InfoEntity;
    public Transform Transform { get; private set; }
    protected virtual void OnAwake() { }

    private void Awake()
    {
        Transform = transform;
        OnAwake();
    }
    public bool Stationary { get { return InfoEntity.isStatic; } }
    public TypeEntity typeEntity { get { return InfoEntity.TypeEntity; } }
    protected static T AddEntity<T>(Enum _enum, Vector3 position, Quaternion quaternion, bool isStatic = true) where T : EntityEngine
    {
        IEntityFamily InfoEntity = EntityFactory.GetEntity(_enum, position, quaternion, isStatic);
        T Value = InfoEntity.GetEngine as T;
        Value.InfoEntity = InfoEntity;
        return Value;
    }
    public virtual void Delete(float time = 0F)
    {
        if(gameObject != null)
        Destroy(gameObject, time);
    } 
}

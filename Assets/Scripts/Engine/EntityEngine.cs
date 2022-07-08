using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FactoryLesson;

public abstract class EntityEngine : MonoBehaviour
{
    public Rigidbody Rigidbody;

    protected IEntityFamily InfoEntity;
    public Transform Transform { get; private set; }
    private void Awake()
    {
        Transform = transform;
    }
    public bool Stationary { get { return InfoEntity.isStatic; } }
    public TypeEntity typeEntity { get { return InfoEntity.TypeEntity; } }

    public virtual void Delete(float time = 0F)
    {
        Destroy(gameObject, time);
    } 
}

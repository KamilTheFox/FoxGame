using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class EntityEngine : MonoBehaviour
{

    public Rigidbody Rigidbody;
    public Transform Transform { get; private set; }

    public abstract TypeEntity typeEntity { get; }
    public bool IsItem => typeEntity == TypeEntity.Item;
    public bool IsAnimal => typeEntity == TypeEntity.Animal;
    public bool IsPlant => typeEntity == TypeEntity.Plant;
    public EntityEngine GetEngine => this;

    protected static Dictionary<TypeEntity, List<EntityEngine>> Entities { get; private set; } = new()
    {
        [TypeEntity.Item] = new(),
        [TypeEntity.Animal] = new(),
        [TypeEntity.Plant] = new(),
    };
    protected virtual void OnStart() { }
    protected virtual void OnAwake() { }
    protected virtual void onDestroy() { }

    private void OnDestroy()
    {
        Entities[typeEntity].Remove(this);
        onDestroy();
    }
    public virtual void Interaction() { }

    private void Awake()
    {
        Transform = transform;
        Entities[typeEntity].Add(this);
        OnAwake();
    }
    private void Start()
    {
        OnStart();
    }
    [HideInInspector] public bool Stationary;
    protected static T AddEntity<T>(Enum _enum, Vector3 position, Quaternion quaternion, bool isStatic = true) where T : EntityEngine
    {
        return EntityCreate.GetEntity(_enum, position, quaternion, isStatic).GetEngine as T;
    }
    public virtual void Delete(float time = 0F)
    {
        if(gameObject != null)
        Destroy(gameObject, time);
    } 
    public virtual TextUI GetTextUI()
    {
        return typeEntity.ToString().GetTextUI();
    }

    protected T GetResources<T>(TypeFolderEntity nameFolder,string nameFile) where T : Component
    {
        return Resources.Load<T>(typeEntity + $"\\{nameFolder}\\{nameFile}");
    }
}

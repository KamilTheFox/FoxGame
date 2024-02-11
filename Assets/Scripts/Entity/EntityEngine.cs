using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FactoryEntity;
[SelectionBase, DisallowMultipleComponent]
public abstract class EntityEngine : MonoBehaviour
{
    public Rigidbody Rigidbody;
    public Transform Transform { get; private set; }


    [SerializeField] protected LayerMask layer;
    public LayerMask Layer
    {
        get { return layer; }
        set 
        { 
            layer = value;
        }
    }
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

    public static GameObject GetGroup => EntityStencilCreating.EntityGroup;
    protected virtual void OnStart() { }
    protected virtual void OnAwake() { }
    protected virtual void onDestroy() { }

    private void OnDestroy()
    {
        if (typeEntity != TypeEntity.InteractiveBody)
            Entities[typeEntity].Remove(this);
        onDestroy();
    }
    private void Awake()
    {
        Transform = transform;
        if(typeEntity != TypeEntity.InteractiveBody)
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
        T entity = EntityCreate.GetEntity(_enum, position, quaternion, isStatic).GetEngine as T;
        return entity;
    }
    public void CancelDelete()
    {
        CancelInvoke("DestroyInvoke");
    }
    public virtual void Delete(float time = 0F)
    {
        if (gameObject != null)
            Invoke("DestroyInvoke", time);
    }
    private void DestroyInvoke()
    {
        Destroy(gameObject);
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

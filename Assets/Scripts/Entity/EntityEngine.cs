using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FactoryEntity;
[SelectionBase, DisallowMultipleComponent]
public abstract class EntityEngine : MonoBehaviour, IAtWater
{
    public float distanceOrder = 100f;

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

    public RendererBuffer rendererBuffer;

    public static EntityEngineBase Base => EntityStencilCreating.EntityGroup;

    public static GameObject GetGroup => EntityStencilCreating.EntityGroup.gameObject;

    [field: SerializeField] public float VolumeObject { get; protected set; } = 100F;

    public bool isSwim { get; private set; }

    protected virtual void OnStart() { }
    protected virtual void OnAwake() { }
    protected virtual void onDestroy() { }

    private void OnDestroy()
    {
        if (typeEntity != TypeEntity.InteractiveBody)
            Base[typeEntity].Remove(this);
        onDestroy();
    }
    private void Awake()
    {
        Transform = transform;
        if (typeEntity != TypeEntity.InteractiveBody)
            Base[typeEntity].Add(this);
        OnAwake();
    }
    private void Start()
    {
        rendererBuffer = new RendererBuffer(gameObject);
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

    public virtual void OnCollisionEnter(Collision collision)
    {
        IMoveablePlatform platform;
        if(Vector3.Angle(collision.contacts[0].normal, Vector3.up) < 45)
            if ((platform = collision.gameObject.GetComponent<IMoveablePlatform>()) != null)
            {
                this.transform.SetParent(platform.Guide);
            }
    }
    public virtual void OnCollisionExit(Collision collision)
    {
        if (Transform.parent == collision.transform.parent)
            Transform.SetParent(GetGroup.transform);
    }


    public virtual void EnterWater()
    {
        isSwim = true;
    }

    public virtual void ExitWater()
    {
        isSwim = false;
    }

    public virtual void OnBatchDistanceCalculated(bool enable) { }


}

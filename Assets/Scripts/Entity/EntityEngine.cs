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

    private Transform _customParent;
    public Transform CustomParent
    {
        get => _customParent ?? GetGroup.transform;
        private set => _customParent = value;
    }


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

    private IMoveablePlatform platform;

    public static EntityEngineBase Base => EntityStencilCreating.EntityGroup;

    public static GameObject GetGroup => EntityStencilCreating.EntityGroup.gameObject;

    [field: SerializeField] public float VolumeObject { get; protected set; } = 100F;

    public bool isSwim { get; private set; }

    private UnityEngine.Events.UnityEvent<bool> onSetMoveblePlatform = new UnityEngine.Events.UnityEvent<bool>();

    public event Action<bool> OnSetMoveblePlatform
    {
        add
        {
            onSetMoveblePlatform.AddListener(value.Invoke);
        }
        remove
        {
            onSetMoveblePlatform.RemoveListener(value.Invoke);
        }
    }

    [field: SerializeField] public InputJobPropertyData inputJobProperty { get; private set; }

    protected virtual void OnStart() { }
    protected virtual void OnAwake() { }
    protected virtual void onDestroy() { }

    private void OnDestroy()
    {
        if (typeEntity != TypeEntity.InteractiveBody)
            if(Base != null)
                Base[typeEntity].Remove(this);
        OptimizedRenderer.RemoveRendererBuffer(rendererBuffer);
        rendererBuffer.Dispose();
        onDestroy();
    }
    private void Awake()
    {
        Transform = transform;
        if (typeEntity != TypeEntity.InteractiveBody)
            if (Base != null)
                Base[typeEntity].Add(this);
        rendererBuffer = new RendererBuffer(gameObject);
        OptimizedRenderer.AddRendererBuffer(rendererBuffer);
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

    public virtual void OnCollisionEnter(Collision collision)
    {
        if(Vector3.Angle(collision.contacts[0].normal, Vector3.up) < 45)
            if ((platform = collision.gameObject.GetComponent<IMoveablePlatform>()) != null)
            {
                onSetMoveblePlatform.Invoke(true);
                this.transform.SetParent(platform.Guide);
            }
    }
    public virtual void OnCollisionExit(Collision collision)
    {
        if (platform == null) return;
        if (platform.Guide == collision.transform.parent && Transform.parent == collision.transform.parent)
        {
            onSetMoveblePlatform.Invoke(false);
            SetParent();
        }
    }

    public void SetParent(Transform parent = null)
    {
        CustomParent = parent;
        Transform.SetParent(CustomParent);
    }

    public virtual void EnterWater()
    {
        isSwim = true;
    }

    public virtual void ExitWater()
    {
        isSwim = false;
    }
}

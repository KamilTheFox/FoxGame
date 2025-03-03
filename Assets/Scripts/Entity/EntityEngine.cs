using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FactoryEntity;
using VulpesTool;

[SelectionBase, DisallowMultipleComponent]
public abstract class EntityEngine : VulpesMonoBehaviour, IAtWater
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
            if (Base != null)
                Base[typeEntity].Remove(this);
        onDestroy();
    }
    private void Awake()
    {
        Transform = transform;
        if (typeEntity != TypeEntity.InteractiveBody)
            if (Base != null)
                Base[typeEntity].Add(this);
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

    public virtual void OnCollisionEnter(Collision collision)
    {
        if (Vector3.Angle(collision.contacts[0].normal, Vector3.up) < 45)
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
    [CreateGUI(title: "Info Item", color: ColorsGUI.InfoBlue)]
    private void CustomEditorGUI()
    {
        GUIStyle gUI = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        GUILayout.Label("Stationary: " + this.Stationary + "; Type Entity: " + this.typeEntity.ToString(), gUI);
        if (this is AnimalEngine engine)
        {
            GUILayout.Label($"TypeAnimal: {engine.TypeAnimal}", gUI);
            GUILayout.Label($"AI: {engine.NameAI} / Behavior: {engine.Behavior}", gUI);
            GUILayout.Label($"All Animal: {AnimalEngine.AnimalList.Count}", gUI);
        }
        if (this is ItemEngine Item)
        {
            GUILayout.Label($"TypeItem: {Item.itemType}", gUI);
            if (Item.isController)
                GUILayout.Label($"IsPlayerController", gUI);
            GUILayout.Label($"All Item: {ItemEngine.GetItems.Length}", gUI);
        }
        if (this is PlantEngine plant)
        {
            GUILayout.Label($"TypePlant: {plant.typePlant}", gUI);
            GUILayout.Label($"All Plant: {PlantEngine.GetPlants.Length}", gUI);
        }
    }
}

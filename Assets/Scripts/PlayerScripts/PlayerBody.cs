﻿using System;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Events;
using GroupMenu;
using Tweener;

[RequireComponent(typeof(PlayerInput), typeof(Rigidbody))]
public class PlayerBody : MonoBehaviour, IDiesing
{
    const float SpeedDefault = 4F;
    public Rigidbody Rigidbody => PlayerInput.Rigidbody;

    private PlayerInput _inputs;

    public IDiesing UniqueDeathscenario;

    private static UnityEvent onDied = new UnityEvent();

    public static event UnityAction OnDied
    {
        add
        {
            onDied.AddListener(value);
        }
        remove
        {
            onDied.RemoveListener(value);
        }
    }

    public PlayerInput PlayerInput 
    {
        get
        {
            if(!_inputs)
                _inputs = GetComponent<PlayerInput>();
            return _inputs;
        }
    }

    [SerializeField]
    private bool _isItemController;

    public Action<Collision, GameObject> BehaviorFromCollision => null;

    public float RecommendedHeight
    {
        get
        {
            Bounds bounds = transform.GetComponentInChildren<MeshRenderer>().bounds;
            return (bounds.max.y - Transform.position.y) * 0.75F;
        }
    }

    public bool IsDie { get; private set; }

    public IRegdoll Regdool { get; private set; }

    private Animator Animator;

    private ViewInteractEntity interactEntity;

    public Vector3? TargetView
    {
        get
        {
            if (interactEntity != null)
                return interactEntity.pointTarget;
            return null;
        }
    }

    public Transform Transform => PlayerInput.Transform;

    public bool isItemController
    {
        get { return _isItemController; }
        private set
        {
            if (isItemController)
            {
                Rigidbody.freezeRotation = !value;
                gameObject.layer = value ? MasksProject.Entity : MasksProject.Player;
                foreach (Transform transform in transform)
                    transform.gameObject.layer = gameObject.layer;
            }
        }
    }
    public void GiveItem(ItemEngine item)
    {
        interactEntity.ItemTake(item);
    }
    void Start()
    {
        _isItemController = gameObject.layer == MasksProject.Entity;

        PlayerInput.AddFuncStopMovement(() => Menu.IsEnabled || !CameraControll.instance.IsPlayerControll(this));

        Animator = GetComponentInChildren<Animator>();
        if(!_isItemController)
            Regdool = new RegdollPlayer(Animator, this);
    }
    private void ChangeLayerIsItemToPlayer(bool IsEnabled)
    {
        if (isItemController)
            isItemController = !IsEnabled;
    }
    public void EntrancePlayerControll(CameraControll camera)
    {
        ChangeLayerIsItemToPlayer(true);
        Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        if(isItemController)
        {
            foreach(var collider in GetComponentsInChildren<Collider>())
                collider.material = PlayerInput.PhysicMaterial;
        }
        interactEntity = new ViewInteractEntity(transform);
    }
    public void ExitPlayerControll(CameraControll camera)
    {
        Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        if (isItemController)
        {
            foreach (var collider in GetComponentsInChildren<Collider>())
                collider.material = null;
        }
        interactEntity?.ItemThrow();
        None.SetInfoEntity(false);
        ChangeLayerIsItemToPlayer(false);
        camera.Transform.parent = null;
        interactEntity = null;
    }
    public void Death()
    {
        if (UniqueDeathscenario != null)
        {
            UniqueDeathscenario.Death();
            return;
        }
        Die();
    }
    public void StendUp(bool Resurrection = false)
    {
        if(Resurrection && IsDie)
        {
            IsDie = false;
            Regdool.Deactivate();
            Animator.SetTrigger("StendUp");
            return;
        }
    }
    public void Die()
    {
        if (IsDie) return;
        IsDie = true;
        if (Regdool != null && !isItemController)
            Regdool.Activate();
        PlayerInput.Fly(Off: true);
        if (CameraControll.instance.IsPlayerControll(this))
        {
            CameraControll.instance.ExitBody();
        }
        onDied.Invoke();
    }
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            StendUp(true);
        if (Menu.IsEnabled || !CameraControll.instance.IsPlayerControll(this))
            return;
        interactEntity?.RayCast();
    }
}
using System;
using UnityEngine;
using GroupMenu;
using Tweener;

public class PlayerControll : MonoBehaviour, IAlive
{
    const float SpeedDefault = 4F;

    public float ForseJump = 4F;
    public float Speed { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public SphereCollider SphereCollider { get; private set; }

    [SerializeField]
    private bool _isItemController;

    public bool isFly { get; private set; }

    public Action<Collision, GameObject> BehaviorFromCollision => null;

    public float RecommendedHeight
    {
        get
        {
            Bounds bounds = transform.GetComponentInChildren<MeshRenderer>().bounds;
            return (bounds.max.y - Transform.position.y) * 0.75F;
        }
    }

    public bool IsDead { get; private set; }

    public IRegdoll Regdool { get; private set; }

    private Animator Animator;

    private ViewInteractEntity interactEntity;

    public Transform Transform { get; private set; }
    private bool _isGrounded
    {
        get
        {
            Vector3 bottomCenterPoint = new Vector3(SphereCollider.bounds.center.x, SphereCollider.bounds.min.y, SphereCollider.bounds.center.z);
            return Physics.CheckCapsule(SphereCollider.bounds.center, bottomCenterPoint, SphereCollider.bounds.size.x / 2 * 0.3f, MasksProject.RigidObject);
        }
    }
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
    void Awake()
    {
        Transform = transform;
        Speed = SpeedDefault;
        
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        SphereCollider = GetComponent<SphereCollider>();
        if (SphereCollider == null)
        {
            SphereCollider = gameObject.AddComponent<SphereCollider>();
        }
        SphereCollider.radius = 0.3F;
        SphereCollider.isTrigger = true;

        _isItemController = gameObject.layer == MasksProject.Entity;

        Animator = GetComponent<Animator>();
        if(Animator)
        Regdool = new Regdoll(Animator, this);
    }
    public void SetSpeed(float speed = 1F)
    {
        Speed = SpeedDefault * speed;
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
        camera.Transform.rotation = Transform.rotation;
        camera.Transform.position = Transform.position + (RecommendedHeight * Transform.up);
        camera.Transform.parent = Transform;
        interactEntity = new ViewInteractEntity(Transform);
    }
    public void ExitPlayerControll(CameraControll camera)
    {
        Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        interactEntity.ItemThrow();
        None.SetInfoEntity(false);
        ChangeLayerIsItemToPlayer(false);
        camera.Transform.parent = null;
        interactEntity = null;
    }
    public void Dead()
    {
        IsDead = true;
        Rigidbody.freezeRotation = false;
        if (Regdool != null)
            Regdool.Activate();
        Fly(Off: true);
        if (CameraControll.instance.IsPlayerControll(this))
        {
            CameraControll.instance.ExitBody();
        }
        if (!isItemController)
            Destroy(gameObject, 15F);
    }
    private void Moving()
    {
        MovementMode.MovementWASDVelocity(Rigidbody, Speed);
        if (isFly)
            MovementMode.MovementFlySpaseLSift(Rigidbody, Speed, _isGrounded);
        else if (_isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, ForseJump, Rigidbody.velocity.z);
        }
        Rigidbody.MoveRotation(Quaternion.Euler(CameraControll.instance.EulerHorizontal));
    }
    public void Fly(bool Off = false)
    {
        isFly = !isFly;
        if (Off)
            isFly = false;
        Rigidbody.drag = isFly ? 9 : 0;
        Rigidbody.useGravity = !isFly;
    }
    public void Update()
    {
        if (Menu.IsEnabled || !CameraControll.instance.IsPlayerControll(this))
            return;
        interactEntity.RayCast();
        Moving();
    }
}

using System;
using UnityEngine;
using GroupMenu;

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

    private const float ForceKick = 700F;

    private ITakenEntity MoveEntity;

    public IRegdoll Regdool { get; private set; }
    private Animator Animator;

    LayerMask RigidObject;
    LayerMask RigidEntity;
    public Transform Transform { get; private set; }
    private bool _isGrounded
    {
        get
        {
            Vector3 bottomCenterPoint = new Vector3(SphereCollider.bounds.center.x, SphereCollider.bounds.min.y, SphereCollider.bounds.center.z);
            return Physics.CheckCapsule(SphereCollider.bounds.center, bottomCenterPoint, SphereCollider.bounds.size.x / 2 * 0.3f, RigidObject);
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
                gameObject.layer = value ? LayerMask.NameToLayer("Entity") : LayerMask.NameToLayer("Player");
                foreach (Transform transform in transform)
                    transform.gameObject.layer = gameObject.layer;
            }
        }
    }
    void Awake()
    {
        Transform = transform;
        Speed = SpeedDefault;
        RigidObject = LayerMask.GetMask(new string[] { "Terrain", "Entity" , "Default"});
        RigidEntity = 1 << LayerMask.NameToLayer("Entity");
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        SphereCollider = GetComponent<SphereCollider>();
        if (SphereCollider == null)
        {
            SphereCollider = gameObject.AddComponent<SphereCollider>();
        }
        SphereCollider.radius = 0.3F;
        SphereCollider.isTrigger = true;

        _isItemController = gameObject.layer == LayerMask.NameToLayer("Entity");

        Animator = GetComponent<Animator>();
        if(Animator)
        Regdool = new Regdoll(Animator, this);
    }
    public void SetSpeed(float speed = 1F)
    {
        Speed = SpeedDefault * speed;
    }
    private void ViewItem()
    {
        Ray ray = CameraControll.RayCastCenterScreen;
        Vector3 newPosition = ray.GetPoint(3F);
        bool isItemMove = CheckMoveEntity();
        bool DistanseMin2 = false;
        LayerMask layerMask = isItemMove ? RigidObject : RigidEntity;
        Action<EntityEngine, bool> buttonClicks = (item, isMove) =>
        {
            if (Menu.IsEnabled)
                return;
            bool clickMouse0 = Input.GetKeyDown(KeyCode.Mouse0);
            bool clickDelete = Input.GetKeyDown(KeyCode.Delete);
            bool clickE = Input.GetKeyDown(KeyCode.E);
            if (Input.GetKeyDown(KeyCode.F))
                item.Interaction();
            if ((clickE && isMove) || clickMouse0 || clickDelete)
            {
                ItemThrow();
                if (clickMouse0 && item.Rigidbody && !item.Stationary)
                {
                    item.Transform.position = DistanseMin2? transform.position + transform.forward * 0.35F + Vector3.up * 0.6F : newPosition - transform.forward * 0.2F;
                    item.Rigidbody.AddForce(CameraControll.MainCamera.transform.forward * ForceKick * item.Rigidbody.mass * UnityEngine.Random.Range(0.8F, 1F));
                }
                if (clickDelete)
                {
                    item.Delete();
                }
            }
            else if (clickE && !isMove && item is ITakenEntity taken)
                ItemTake(taken);
        };

        if (Physics.Raycast(ray, out RaycastHit raycastHit, CameraControll.instance.DistanseRay, layerMask))
        {
            if (raycastHit.collider.gameObject)
                newPosition = raycastHit.point + ray.direction.normalized * 0.01F;
            if (!isItemMove)
            {
                EntityEngine Entity = raycastHit.collider.gameObject.GetComponentInParent<EntityEngine>();
                if (Entity)
                {
                    None.SetInfoEntity(true, Entity.GetTextUI());
                    if (Entity is IAlive alive && !alive.IsDead && alive is AnimalEngine animalEngine)
                    {
                        if (Input.GetKeyDown(KeyCode.F))
                        {
                            animalEngine.Interaction();
                            None.SetInfoEntity(false);
                        }
                    }
                    buttonClicks(Entity, isItemMove);
                }
            }
        }
        else
            None.SetInfoEntity(false);
        if (isItemMove)
        {
            if (MoveEntity is EntityEngine Entity)
            {
                None.SetInfoEntity(true, Entity.GetTextUI());
                float distanse = Vector3.Distance(transform.position, newPosition);
                if (distanse < 2F)
                {
                    newPosition = transform.forward * 1.25F + transform.position;
                    DistanseMin2 = true;
                }
                Entity.Transform.position = newPosition;
                float Scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Scroll > 0)
                    Between.AddRotation(Entity.Transform, new Vector3(0F, 45F ,0F));
                if (Scroll < 0)
                    Between.AddRotation(Entity.Transform, new Vector3(0F, -45F, 0F));
                buttonClicks(Entity, isItemMove);
            }
        }
    }
    public void GiveItem(ItemEngine item)
    {
        ItemTake(item);
    }
    private void ItemThrow()
    {
        if (MoveEntity == null)
            return;
        MoveEntity.Throw();
        MoveEntity = null;
    }
    private bool CheckMoveEntity()
    {
        bool Enable = MoveEntity != null;
        if(Enable && MoveEntity.Transform == null)
        {
            Enable = false;
            MoveEntity = null;
        }
        return Enable;
    }
    private void ItemTake(ITakenEntity entity)
    {
        if (CheckMoveEntity())
            ItemThrow();
        if (entity != null)
        {
            MoveEntity = entity.Take();
            MoveEntity.Transform.rotation = Quaternion.AngleAxis(MoveEntity.Transform.rotation.eulerAngles.y, Vector3.up);
            if(MoveEntity.Rigidbody)
                MoveEntity.Rigidbody.angularVelocity = MoveEntity.Rigidbody.velocity = Vector3.zero;
        }
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
    }
    public void ExitPlayerControll(CameraControll camera)
    {
        Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        ItemThrow();
        None.SetInfoEntity(false);
        ChangeLayerIsItemToPlayer(false);
        camera.Transform.parent = null;
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
        ViewItem();
        Moving();
    }
}

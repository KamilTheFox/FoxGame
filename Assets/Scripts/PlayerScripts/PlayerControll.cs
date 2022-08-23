using System;
using UnityEngine;
using GroupMenu;

public class PlayerControll : MonoBehaviour, IAlive
{
    const float SpeedDefault = 4F;
    public float Speed { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public SphereCollider SphereCollider { get; private set; }

    [SerializeField]
    private bool _isItemController;

    public bool isFly { get; private set; }

    public Action<Collision> BehaviorFromCollision => null;

    private float RecommendedHeight
    {
        get
        {
            return 0.75F;
        }
    }

    public bool IsDead { get; private set; }

    private const float ForceKick = 700F;
    [SerializeField]
    ItemEngine itemMove;

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
        bool isItemMove = itemMove;
        bool DistanseMin2 = false;
        LayerMask layerMask = isItemMove ? RigidObject : RigidEntity;
        Action<ItemEngine, bool> buttonClicks = (item, isMove) =>
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
                if (clickMouse0 && item.Rigidbody)
                {
                    item.Transform.position = DistanseMin2? transform.position + transform.forward * 0.35F + Vector3.up * 0.6F : newPosition - transform.forward * 0.2F;
                    item.Rigidbody.AddForce(CameraControll.MainCamera.transform.forward * ForceKick * item.Rigidbody.mass);
                }
                if (clickDelete)
                    item.Delete();
            }
            else if (clickE && !isMove)
                ItemTake(item);
        };

        if (Physics.Raycast(ray, out RaycastHit raycastHit, CameraControll.instance.DistanseRay, layerMask))
        {
            if (raycastHit.collider.gameObject)
                newPosition = raycastHit.point + ray.direction.normalized * 0.01F;
            if (!isItemMove)
            {
                GameObject gameObject = raycastHit.collider.gameObject;
                
                    IAlive alive = gameObject.GetComponentInParent<IAlive>();
                    if (alive != null && !alive.IsDead && alive is AnimalEngine animalEngine)
                    {
                      None.SetInfoEntity(true, () => "Pserss F to change animation");
                          if (Input.GetKeyDown(KeyCode.F))
                          {
                              animalEngine.Interactive();
                              None.SetInfoEntity(false);
                          }
                    }
                ItemEngine i = gameObject.GetComponentInParent<ItemEngine>();
                if (i)
                {
                    InfoItem(i.itemType);
                    buttonClicks(i, isItemMove);
                }
            }
        }
        else
            None.SetInfoEntity(false);
        if (isItemMove)
        {
            InfoItem(itemMove.itemType);
            float distanse = Vector3.Distance(transform.position, newPosition);
            if (distanse < 2F)
            {
                newPosition = transform.forward * 1.25F + transform.position;
                DistanseMin2 = true;
            }
            itemMove.Transform.position = newPosition;
            itemMove.Transform.rotation = transform.rotation;
            buttonClicks(itemMove, isItemMove);
        }
    }
    private void InfoItem(TypeItem typeItem)
    {
        None.SetInfoEntity(true, () => new object[]
        {
            typeItem.ToString(), "\n",
            "[",LText.KeyCodeE ,"] - ",LText.Take ,"/",LText.Leave ," \n [", LText.KeyCodeMouse0 ,"] - ",LText.Drop ,"\n[",LText.KeyCodeF ,"] -", LText.Interactive
        });

    }
    public void GiveItem(ItemEngine item)
    {
        ItemTake(item);
    }
    private void ItemThrow()
    {
        if (itemMove)
        {
            itemMove = itemMove.Throw();
        }
    }
    private void ItemTake(ItemEngine item)
    {
        if (itemMove)
            ItemThrow();
        if (item)
        {
            itemMove = item.Take();
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
        camera.Transform.rotation = Transform.rotation;
        camera.Transform.position = Transform.position + (RecommendedHeight * Transform.up);
        camera.Transform.parent = Transform;
    }
    public void ExitPlayerControll(CameraControll camera)
    {
        ItemThrow();
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
        MovementMode.MovementWASD(Rigidbody, Speed);
        if (isFly)
            MovementMode.MovementFlySpaseLSift(Rigidbody, Speed, _isGrounded);
        else if (_isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.AddForce(Vector3.up * 200 * Rigidbody.mass);
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

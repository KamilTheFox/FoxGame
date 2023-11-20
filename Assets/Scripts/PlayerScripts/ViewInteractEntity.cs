using System;
using System.Collections.Generic;
using GroupMenu;
using Tweener;
using UnityEngine;

public class ViewInteractEntity
{
    private Transform transform;
    private const float ForceKick = 700F;

    private static ViewInteractEntity viewInteract;

    public static bool isMoveItem => viewInteract ==null ? false : (bool)(viewInteract?.IsMoveEntity);
    public ViewInteractEntity(Transform _transform)
    {
        transform = _transform;
        viewInteract = this;
    }

    private ITakenEntity MoveEntity;

    private IExpansionTween Rotation;

    private Dictionary<Rigidbody, InfoRigidBody> RigidbodyInterpolations = new();

    public Vector3 pointTarget { get; private set; }
    private struct InfoRigidBody
    {
        public InfoRigidBody(RigidbodyInterpolation _interpolation, bool _kinematic)
        {
            interpolation = _interpolation;
            isKinematic = _kinematic;
        }
        public RigidbodyInterpolation interpolation;
        public bool isKinematic;
    }
    public void ItemThrow()
    {
        if (MoveEntity == null)
            return;
        MoveEntity.Transform.gameObject.layer = MoveEntity.GetEngine.Layer;
        foreach (Transform chield in MoveEntity.Transform.GetComponentsInChildren<Transform>())
        {
            chield.gameObject.layer = MoveEntity.Transform.gameObject.layer;
        }
        foreach(Rigidbody rigidbody in RigidbodyInterpolations.Keys)
        {
            if (rigidbody)
            {
                rigidbody.interpolation = RigidbodyInterpolations[rigidbody].interpolation;
                rigidbody.isKinematic = RigidbodyInterpolations[rigidbody].isKinematic;
            }
        }
        if (Rotation != null)
        {
            Tween.Stop(Rotation);
            Rotation = null;
        }
        MoveEntity = null;
        RigidbodyInterpolations = new();
    }
    public void ItemTake(ITakenEntity entity)
    {
        if (IsMoveEntity)
            ItemThrow();

        if (entity == null)
        {
            Debug.Log("Entity: Null Reference");
            return;
        }

        MoveEntity = entity.Take();

        if (MoveEntity == null)
        {
            Debug.Log("MoveEntity: Null Reference");
            return;
        }

        MoveEntity.Transform.rotation = Quaternion.AngleAxis(MoveEntity.Transform.rotation.eulerAngles.y, Vector3.up);
        MoveEntity.Transform.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (Transform chield in MoveEntity.Transform.GetComponentsInChildren<Transform>())
        {
            chield.gameObject.layer = MoveEntity.Transform.gameObject.layer;
        }
        if (MoveEntity.Rigidbody)
        {
            MoveEntity.Rigidbody.angularVelocity = MoveEntity.Rigidbody.velocity = Vector3.zero;
            foreach(Rigidbody rigidbody in MoveEntity.GetEngine.gameObject.GetComponentsInChildren<Rigidbody>())
            {
                RigidbodyInterpolations.Add(rigidbody, new InfoRigidBody(rigidbody.interpolation, rigidbody.isKinematic));
                rigidbody.interpolation = RigidbodyInterpolation.None;
                rigidbody.isKinematic = true;
            }
            }
    }
    private bool IsMoveEntity
    {
        get
        {
            bool Enable = MoveEntity != null;
            if (Enable && MoveEntity.Transform == null)
            {
                Enable = false;
                MoveEntity = null;
            }
            return Enable;
        }
    }
    public void RayCast()
    {
        Ray ray = CameraControll.RayCastCenterScreen;
        Vector3 newPosition = ray.GetPoint(CameraControll.instance.DistanseRay);

        ITakenEntity Taken = null;

        IInteractive interactive = null;

        None.SetInfoEntity(false);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, CameraControll.instance.DistanseRay, 
            IsMoveEntity ? MasksProject.RigidObject : MasksProject.RigidEntity))
        {
            if (raycastHit.collider.gameObject)
                newPosition = raycastHit.point + ray.direction.normalized * 0.01F;
            Taken = raycastHit.collider.gameObject.GetComponentInParent<ITakenEntity>();
            interactive = raycastHit.collider.gameObject.GetComponent<IInteractive>();
            if(interactive == null)
                interactive = raycastHit.collider.gameObject.GetComponentInParent<IInteractive>();

        }

        if (IsMoveEntity)
        {
            Taken = MoveEntity;
            if (MoveEntity is IInteractive value)
                interactive = value;
            MoveEntity.Transform.position = newPosition;
            float Scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Scroll > 0)
                Rotation = Tween.AddRotation(MoveEntity.Transform, new Vector3(0F, 10F, 0F)).ChangeEase(Ease.CubicRoot);
            if (Scroll < 0)
                Rotation = Tween.AddRotation(MoveEntity.Transform, new Vector3(0F, -10F, 0F)).ChangeEase(Ease.CubicRoot);
        }
        if (Taken != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (IsMoveEntity)
                {
                    ItemThrow();
                    if (Taken is IThrowed throwed)
                        throwed.ToThrow();
                }
                else
                    ItemTake(Taken.Take());
            }
            if (Input.GetKeyDown(KeyCode.Mouse0) && Taken is IDropEntity drop)
            {
                if (drop.Rigidbody != null)
                {
                    ItemThrow();
                    if (Taken is IDropped toDrop)
                        toDrop.ToDrop();
                    Taken.Transform.position = newPosition - transform.forward * 0.2F;
                    drop.Rigidbody.AddForce(CameraControll.MainCamera.transform.forward * ForceKick * drop.Rigidbody.mass * UnityEngine.Random.Range(0.8F, 1F));
                }
            }
            if (Input.GetKeyDown(KeyCode.Delete) && GameState.IsCreative)
                Taken.GetEngine.Delete();
        }
        if (interactive == null && Taken == null)
            return;
        if (Input.GetKeyDown(KeyCode.F) && interactive != null)
            interactive.Interaction();
        pointTarget = newPosition;
        None.SetInfoEntity(true, interactive != null? interactive.GetEngine : Taken.GetEngine);
    }
    
}

using System;
using GroupMenu;
using Tweener;
using UnityEngine;

public class ViewInteractEntity
{
    private Transform transform;
    private const float ForceKick = 700F;
    public ViewInteractEntity(Transform _transform)
    {
        transform = _transform;
    }

    private ITakenEntity MoveEntity;
    public void GiveItem(ItemEngine item)
    {
        ItemTake(item);
    }
    public void ItemThrow()
    {
        if (MoveEntity == null)
            return;
        MoveEntity.Throw();
        MoveEntity = null;
    }
    public void ItemTake(ITakenEntity entity)
    {
        if (CheckMoveEntity())
            ItemThrow();
        
        if (entity != null)
        {
            MoveEntity = entity.Take();
            if (MoveEntity == null) return;
            MoveEntity.Transform.rotation = Quaternion.AngleAxis(MoveEntity.Transform.rotation.eulerAngles.y, Vector3.up);
            if (MoveEntity.Rigidbody)
                MoveEntity.Rigidbody.angularVelocity = MoveEntity.Rigidbody.velocity = Vector3.zero;
        }
    }
    private bool CheckMoveEntity()
    {
        bool Enable = MoveEntity != null;
        if (Enable && MoveEntity.Transform == null)
        {
            Enable = false;
            MoveEntity = null;
        }
        return Enable;
    }
    public void RayCast()
    {
        Ray ray = CameraControll.RayCastCenterScreen;
        Vector3 newPosition = ray.GetPoint(3F);
        bool isItemMove = CheckMoveEntity();
        bool DistanseMin2 = false;
        LayerMask layerMask = isItemMove ? MasksProject.RigidObject : MasksProject.RigidEntity;
        Action<EntityEngine, bool> buttonClicks = (Entity, isMove) =>
        {
            if (Menu.IsEnabled)
                return;
            None.SetInfoEntity(true, Entity);
            bool clickMouse0 = Input.GetKeyDown(KeyCode.Mouse0);
            bool clickDelete = Input.GetKeyDown(KeyCode.Delete);
            bool clickE = Input.GetKeyDown(KeyCode.E);
            if (Input.GetKeyDown(KeyCode.F))
                Entity.Interaction();
            if ((clickE && isMove) || clickMouse0 || clickDelete)
            {
                ItemThrow();
                if (clickMouse0 && Entity.Rigidbody && !Entity.Stationary)
                {
                    Entity.Transform.position = DistanseMin2 ? transform.position + transform.forward * 0.35F + Vector3.up * 0.6F : newPosition - transform.forward * 0.2F;
                    Entity.Rigidbody.AddForce(CameraControll.MainCamera.transform.forward * ForceKick * Entity.Rigidbody.mass * UnityEngine.Random.Range(0.8F, 1F));
                }
                if (clickDelete)
                {
                    Entity.Delete();
                }
            }
            else if (clickE && !isMove && Entity is ITakenEntity taken)
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
                    buttonClicks(Entity, isItemMove);
                }
            }
        }
        else
            None.SetInfoEntity(false);

        if (isItemMove)
        {
                float distanse = Vector3.Distance(transform.position, newPosition);
                if (distanse < 2F)
                {
                    newPosition = transform.forward * 1.25F + transform.position;
                    DistanseMin2 = true;
                }
                MoveEntity.Transform.position = newPosition;
                float Scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Scroll > 0)
                    Tween.AddRotation(MoveEntity.Transform, new Vector3(0F, 45F, 0F)).ChangeEase(Ease.CubicRoot);
                if (Scroll < 0)
                    Tween.AddRotation(MoveEntity.Transform, new Vector3(0F, -45F, 0F)).ChangeEase(Ease.CubicRoot);
                buttonClicks(MoveEntity.GetEngine, isItemMove);
        }
    }
}

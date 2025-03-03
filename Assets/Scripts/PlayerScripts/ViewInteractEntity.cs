using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using CameraScripts;
using GroupMenu;
using Tweener;
using UnityEngine;

namespace PlayerDescription
{
    public class ViewInteractEntity : ICameraCastSubscriber
    {
        private Transform transform;

        private const float ForceKick = 700F;

        private static ViewInteractEntity viewInteract;

        private CharacterBody character;

        public static bool isMoveItem => viewInteract == null ? false : (bool)(viewInteract?.IsMoveEntity);

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

        public ViewInteractEntity(Transform _transform, CharacterBody body)
        {
            transform = _transform;
            viewInteract = this;
            character = body;
        }

        private ITakenEntity MoveEntity;

        private int lauerTakeEntity;

        private IExpansionTween Rotation;

        private Dictionary<Rigidbody, InfoRigidBody> RigidbodyInterpolations = new();


        public Vector3 pointTarget { get; private set; }

        public void ResetTarget()
        {
            pointTarget = character.Head.position + character.Head.forward * 100f + Vector3.down * 2F;
        }
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
            MoveEntity.Transform.gameObject.layer = lauerTakeEntity;
            if (MoveEntity is IWieldable wieldable)
            {

                MoveEntity.Transform.SetParent(EntityEngine.GetGroup.transform);

                wieldable.DisableWeapon();

                wieldable.RootParent = null;

                character.Drop();

                character.wieldable = null;

                MoveEntity.Throw();

                MoveEntity.Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                MoveEntity.Rigidbody.isKinematic = false;

                MoveEntity = null;

                return;
            }

            foreach (Transform chield in MoveEntity.Transform.GetComponentsInChildren<Transform>())
            {
                chield.gameObject.layer = MoveEntity.Transform.gameObject.layer;
            }
            foreach (Rigidbody rigidbody in RigidbodyInterpolations.Keys)
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
            MoveEntity.Throw();
            MoveEntity = null;
            RigidbodyInterpolations = new();
        }
        public void ItemTake(ITakenEntity entity)
        {
            if (IsMoveEntity)
                ItemThrow();

            if (entity == null)
            {
                return;
            }

            MoveEntity = entity.Take();

            if (MoveEntity == null)
            {
                return;
            }
            lauerTakeEntity = MoveEntity.Transform.gameObject.layer;
            MoveEntity.Transform.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            if (MoveEntity is IWieldable wieldable)
            {
                Debug.Log("Take IWieldable");
                //MoveEntity.Transform.SetParent(character.RightHand);

                MoveEntity.Rigidbody.isKinematic = true;

                MoveEntity.Rigidbody.interpolation = RigidbodyInterpolation.None;

                wieldable.RootParent = character.Transform;

                Transform hand = character.RightHand.GetChilds().FirstOrDefault(name => name.name.Contains("Wield" + wieldable.UpAxis));

                if(hand == null)
                {
                    Debug.Log("Null Hand");
                    ItemThrow();
                }

                wieldable.SetWieldHand(hand);

                character.wieldable = wieldable;

                character.Took();

                return;
            }

            MoveEntity.Transform.rotation = Quaternion.AngleAxis(MoveEntity.Transform.rotation.eulerAngles.y, Vector3.up);
            
            foreach (Transform chield in MoveEntity.Transform.GetComponentsInChildren<Transform>())
            {
                chield.gameObject.layer = MoveEntity.Transform.gameObject.layer;
            }
            if (MoveEntity.Rigidbody)
            {
                MoveEntity.Rigidbody.angularVelocity = MoveEntity.Rigidbody.velocity = Vector3.zero;
                foreach (Rigidbody rigidbody in MoveEntity.GetEngine.gameObject.GetComponentsInChildren<Rigidbody>())
                {
                    RigidbodyInterpolations.Add(rigidbody, new InfoRigidBody(rigidbody.interpolation, rigidbody.isKinematic));
                    rigidbody.interpolation = RigidbodyInterpolation.None;
                    rigidbody.isKinematic = true;
                }
            }
        }
        
        public void RayCast()
        {
            Ray ray = CameraControll.RayCastCenterScreen;
            Vector3 newPosition = ray.GetPoint(CameraControll.instance.DistanseRay);
            pointTarget = newPosition;
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
                if (interactive == null)
                    interactive = raycastHit.collider.gameObject.GetComponentInParent<IInteractive>();
                
            }

            if (IsMoveEntity)
            {
                Taken = MoveEntity;
                if (MoveEntity is not IWieldable)
                {
                    if (MoveEntity is IInteractive value)
                        interactive = value;
                    MoveEntity.Transform.position = newPosition;
                    float Scroll = Input.GetAxis("Mouse ScrollWheel");
                    if (Scroll > 0)
                        Rotation = Tween.AddRotation(MoveEntity.Transform, new Vector3(0F, 10F, 0F)).ChangeEase(Ease.CubicRoot);
                    if (Scroll < 0)
                        Rotation = Tween.AddRotation(MoveEntity.Transform, new Vector3(0F, -10F, 0F)).ChangeEase(Ease.CubicRoot);
                }
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
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (MoveEntity is IWieldable)
                    {
                        character.Attack();
                    }
                    else if (Taken is IDropEntity drop)
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
                }
                if (Input.GetKeyDown(KeyCode.Delete) && GameState.IsCreative)
                    Taken.GetEngine.Delete();
            }
            if (interactive == null && Taken == null)
                return;
            if (Input.GetKeyDown(KeyCode.F) && interactive != null)
                interactive.Interaction();
            None.SetInfoEntity(true, interactive != null ? interactive.GetEngine : Taken.GetEngine);
            pointTarget = newPosition;
        }

        public void OnCameraCasting(RaycastHit hit)
        {
            Ray ray = CameraControll.RayCastCenterScreen;
            Vector3 newPosition = ray.GetPoint(CameraControll.instance.DistanseRay);
            pointTarget = newPosition;

            ITakenEntity Taken = null;

            IInteractive interactive = null;

            None.SetInfoEntity(false);

            bool isValidateDistance = Vector3.Distance(ray.origin, hit.point) <= CameraControll.instance.DistanseRay;

            LayerMask mask = IsMoveEntity ? MasksProject.RigidObject : MasksProject.RigidEntity;
            int layer = hit.collider.gameObject.layer;

            bool isValidateMask = (mask & (1 << layer)) != 0;

            if (isValidateMask && isValidateMask)
            {
                if (hit.collider.gameObject)
                    newPosition = hit.point + ray.direction.normalized * 0.01F;
                Taken = hit.collider.gameObject.GetComponentInParent<ITakenEntity>();
                interactive = hit.collider.gameObject.GetComponent<IInteractive>();
                if (interactive == null)
                    interactive = hit.collider.gameObject.GetComponentInParent<IInteractive>();

            }

            if (IsMoveEntity)
            {
                Taken = MoveEntity;
                if (MoveEntity is not IWieldable)
                {
                    if (MoveEntity is IInteractive value)
                        interactive = value;
                    MoveEntity.Transform.position = newPosition;
                    float Scroll = Input.GetAxis("Mouse ScrollWheel");
                    if (Scroll > 0)
                        Rotation = Tween.AddRotation(MoveEntity.Transform, new Vector3(0F, 10F, 0F)).ChangeEase(Ease.CubicRoot);
                    if (Scroll < 0)
                        Rotation = Tween.AddRotation(MoveEntity.Transform, new Vector3(0F, -10F, 0F)).ChangeEase(Ease.CubicRoot);
                }
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
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (MoveEntity is IWieldable)
                    {
                        character.Attack();
                    }
                    else if (Taken is IDropEntity drop)
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
                }
                if (Input.GetKeyDown(KeyCode.Delete) && GameState.IsCreative)
                    Taken.GetEngine.Delete();
            }
            if (interactive == null && Taken == null)
                return;
            if (Input.GetKeyDown(KeyCode.F) && interactive != null)
                interactive.Interaction();
            None.SetInfoEntity(true, interactive != null ? interactive.GetEngine : Taken.GetEngine);
            pointTarget = newPosition;
        }
    }
}

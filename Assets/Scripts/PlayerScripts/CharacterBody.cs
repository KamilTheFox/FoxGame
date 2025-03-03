using System;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Events;
using GroupMenu;
using Tweener;
using UnityEngine.AI;
using AIInput;

namespace PlayerDescription
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterInput), typeof(AnimatorCharacterInput), typeof(Rigidbody))]
    public class CharacterBody : MonoBehaviour, IDiesing, IHunted, ICollideableDoll, IGlobalUpdates
    {
        public Rigidbody Rigidbody => gameObject.GetComponent<Rigidbody>();

        private CharacterInput _inputs;

        public IDiesing UniqueDeathscenario;

        private AnimatorCharacterInput animationInput;

        public AnimatorCharacterInput AnimatorInput
        {
            get
            {
                if (animationInput == null)
                    animationInput = GetComponent<AnimatorCharacterInput>();
                return animationInput;
            }
        }

        public IWieldable wieldable;

        private AnimationAttack attack;

        private Transform head;
        public Transform Head
        {
            get
            {
                if (head == null)
                    head = AnimatorInput.AnimatorHuman.GetBoneTransform(HumanBodyBones.Head);
                return head;
            }
        }

        private Transform hips;
        public Transform Hips
        {
            get
            {
                if (hips == null)
                    hips = AnimatorInput.AnimatorHuman.GetBoneTransform(HumanBodyBones.Hips);
                return hips;
            }
        }

        private Transform chest;
        public Transform Chest
        {
            get
            {
                if (chest == null)
                    chest = AnimatorInput.AnimatorHuman.GetBoneTransform(HumanBodyBones.Chest);
                return chest;
            }
        }

        private Transform rightHand;
        public Transform RightHand
        {
            get
            {
                if (rightHand == null)
                    rightHand = AnimatorInput.AnimatorHuman.GetBoneTransform(HumanBodyBones.RightHand);
                return rightHand;
            }
        }

        private Transform leftHand;
        public Transform LeftHand
        {
            get
            {
                if (leftHand == null)
                    leftHand = AnimatorInput.AnimatorHuman.GetBoneTransform(HumanBodyBones.LeftHand);
                return leftHand;
            }
        }

        private ViewInteractEntity interactEntity;

        public ViewInteractEntity InteractEntity => interactEntity;

        private UnityEvent onDied = new UnityEvent();

        private UnityEvent onFell = new UnityEvent();

        private float smoothRotateBody, smooth;

        private float targetRotate;

        public bool debug;

        public bool talkingTargetInteractEntity = true;


        public event UnityAction OnDied
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

        public event UnityAction OnFell
        {
            add
            {
                onFell.AddListener(value);
            }
            remove
            {
                onFell.RemoveListener(value);
            }
        }

        public CharacterInput CharacterInput
        {
            get
            {
                if (!_inputs)
                    _inputs = GetComponent<CharacterInput>();
                return _inputs;
            }
        }

        [SerializeField]
        private bool _isItemController;


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

        public Vector3? TargetView
        {
            get
            {
                if (interactEntity != null)
                    return interactEntity.pointTarget;
                return null;
            }
        }

        public Transform Transform => CharacterInput.Transform;

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
        private void Awake()
        {
            this.AddListnerUpdate();

            AnimatorInput.OnAwake();
            CharacterInput.OnAwake();

            _isItemController = gameObject.layer == MasksProject.Entity;

            CharacterInput.AddFuncStopMovement(() =>
            {
                return Regdool.isActive;
            });
            if (!_isItemController)
                Regdool = new RegdollPlayer(AnimatorInput.AnimatorHuman, this);
        }
        private void FixedUpdate()
        {
            if (targetRotate > 180)
                targetRotate -= 360;
            if (smoothRotateBody > 180)
                smoothRotateBody -= 360;
            smooth = targetRotate - smoothRotateBody;
            smooth = Mathf.Clamp(-2, 2, Mathf.Lerp(smooth, 0, Time.deltaTime * 10));
            //animationInput.Animator.SetFloat("Trun", smooth);
        }
        void IGlobalUpdates.Update()
        {
            if (Menu.IsEnabled || !CameraControll.instance.IsPlayerControll(this))
                return;

            //if (Input.GetKeyDown(KeyCode.Alpha2))
            //    Attack();

            interactEntity?.RayCast();
        }

        public void RotateBody(Quaternion quaternion)
        {
            if (animationInput.IsPlayStateAnimator(TypeAnimation.Climbing) || animationInput.applyRootMotion)
                return;
            smoothRotateBody = transform.rotation.eulerAngles.y;
            targetRotate = quaternion.eulerAngles.y;

            Rigidbody.MoveRotation(quaternion);
        }
        public void GiveItem(ItemEngine item)
        {
            interactEntity.ItemTake(item);
        }
        public void EntrancePlayerControll(CameraControll camera)
        {
            ChangeLayerIsItemToPlayer(true);
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            if (isItemController)
            {
                foreach (var collider in GetComponentsInChildren<Collider>())
                    collider.material = CharacterInput.PhysicMaterial;
            }
            CharacterInput.IntroducingCharacter = new InputDefault(this);
            interactEntity = new ViewInteractEntity(transform, this);
        }
        public void ExitPlayerControll(CameraControll camera)
        {
            if (isItemController)
            {
                foreach (var collider in GetComponentsInChildren<Collider>())
                    collider.material = null;
            }
            None.SetInfoEntity(false);
            ChangeLayerIsItemToPlayer(false);
            CharacterInput.IntroducingCharacter = null;
            camera.Transform.parent = null;
            ClearInteractEntity();
        }
        public void ResetTargetLook()
        {
            if (interactEntity == null) return;
            interactEntity.ResetTarget();
        }
        public void ClearInteractEntity()
        {
            if (interactEntity == null) return;
            interactEntity.ItemThrow();
            interactEntity = null;
        }
        private void ChangeLayerIsItemToPlayer(bool IsEnabled)
        {
            if (isItemController)
                isItemController = !IsEnabled;
        }
        public void Death()
        {
            if (enabled == false) return;
            if (UniqueDeathscenario != null)
            {
                UniqueDeathscenario.Death();
                return;
            }
            Die();
        }
        [ContextMenu("StendUp")]
        public void StendUpDebug()
        {
            StendUp();
        }
        public void StendUp(bool Resurrection = false)
        {
            if (Resurrection && IsDie)
            {
                IsDie = false;
            }
            if (!Regdool.isActive && !IsDie) return;
            AnimatorInput.BlinkingEyes(0F);
            AnimatorInput.StendUp();
        }

        public void Took()
        {
            AnimatorInput.StartUniqueAnimation<AnimationTook>();
        }

        public void Drop()
        {
            if(attack != null)
            {
                AttackFinished();
            }
            AnimatorInput.StartUniqueAnimation<AnimationDrop>();
        }

        public void Attack()
        {
            if (wieldable == null || attack != null) return;

            attack = AnimatorInput.StartUniqueAnimation<AnimationAttack>();

            attack.OnAnimationStarted += wieldable.EnableWeapon;

            attack.OnAnimationFinished += AttackFinished;

        }
        private void AttackFinished()
        {
            attack.OnAnimationStarted -= wieldable.EnableWeapon;
            wieldable.DisableWeapon();
            attack.OnAnimationFinished -= AttackFinished;
            attack = null;
        }


        [ContextMenu("Fell")]
        public void Fell()
        {
            if (enabled == false) return;
            if (Regdool != null && !isItemController)
                Regdool.Activate();
            AnimatorInput?.BlinkingEyes(100F);
            CharacterInput.Fly(Off: true);
            onFell.Invoke();
        }
        public void Die()
        {
            if (enabled == false) return;
            if (IsDie) return;

            IsDie = true;
            Fell();
            
            if (CameraControll.instance.IsPlayerControll(this))
            {
                CameraControll.instance.ExitBody();
            }
            onDied.Invoke();
        }

        public void OnCollision(Collision collision, GameObject sourceObject)
        {
            if (sourceObject == gameObject) return;

            //Debug.Log($"collision.collider Name: {collision.collider.name} /// source Object name: {sourceObject.name}");
        }

        public class InputDefault : IInputCaracter
        {
            public InputDefault(CharacterBody _input)
            {
                input = _input;
                tacking = new TackingIK(_input.AnimatorInput.AnimatorHuman);
                tacking.LookIKSpeed = 10F;
                tacking.LookIKWeight = 1f;
                tacking.BodyWeight = 0.35f;
                tacking.ClampWeight = 0.5f;
                tacking.EyesWeight = 1f;
                tacking.HeadWeight = 0.75f;
            }
            private CharacterBody input;

            private TackingIK tacking;
            public bool IsRun => Input.GetKey(KeyCode.LeftShift);

            public bool IsCrouch => Input.GetKey(KeyCode.C);

            public void Enable()
            {
                tacking.Target = new GameObject("TargetInputDefault").transform;
                tacking.Target.SetParent(input.gameObject.transform);
                input.AnimatorInput.AddListenerIK(tacking);
            }

            public void Disable()
            {
                input.AnimatorInput.RemoveListenerIK(tacking);
                GameObject.Destroy(tacking.Target.gameObject);
            }

            bool IInputCaracter.Space()
            {
               return input.CharacterInput.isSwim ? Input.GetKey(KeyCode.Space) : Input.GetKeyDown(KeyCode.Space);
            }

            bool IInputCaracter.Shift()
            {
                return IsRun;
            }

            Vector3 IInputCaracter.Move(Transform source, out bool isMove)
            {
                if(input.talkingTargetInteractEntity)
                if(tacking != null && input.interactEntity != null)
                    tacking.Target.position = input.interactEntity.pointTarget;
                return MovementMode.WASD(source, 1F, out isMove, true);
            }

        }
    }
}

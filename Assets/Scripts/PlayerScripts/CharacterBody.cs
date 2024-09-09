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
    [RequireComponent(typeof(CharacterInput), typeof(AnimatorCharacterInput), typeof(Rigidbody))]
    public class CharacterBody : MonoBehaviour, IDiesing, IHunted, ICollideableDoll, IGlobalUpdates
    {
        public Rigidbody Rigidbody => CharacterInput.Rigidbody;

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


        private Transform head;
        public Transform Head
        {
            get
            {
                if (head == null)
                    head = AnimatorInput.Animator.GetBoneTransform(HumanBodyBones.Head);
                return head;
            }
        }

        private Transform hips;
        public Transform Hips
        {
            get
            {
                if (hips == null)
                    hips = AnimatorInput.Animator.GetBoneTransform(HumanBodyBones.Hips);
                return hips;
            }
        }

        private Transform chest;
        public Transform Chest
        {
            get
            {
                if (chest == null)
                    chest = AnimatorInput.Animator.GetBoneTransform(HumanBodyBones.Chest);
                return chest;
            }
        }

        private Transform rightHand;
        public Transform RightHand
        {
            get
            {
                if (rightHand == null)
                    rightHand = AnimatorInput.Animator.GetBoneTransform(HumanBodyBones.RightHand);
                return rightHand;
            }
        }

        private Transform leftHand;
        public Transform LeftHand
        {
            get
            {
                if (leftHand == null)
                    leftHand = AnimatorInput.Animator.GetBoneTransform(HumanBodyBones.LeftHand);
                return leftHand;
            }
        }

        private ViewInteractEntity interactEntity;

        private UnityEvent onDied = new UnityEvent();

        private UnityEvent onFell = new UnityEvent();

        private float smoothRotateBody, smooth;

        private float targetRotate;

        public RendererBuffer rendererBuffer;

        private const float SMOTH_DELAY = 50f;


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
            CharacterInput.OnAwake();
            AnimatorInput.OnAwake();

            this.AddListnerUpdate();

            rendererBuffer = new RendererBuffer(gameObject);

            FactoryEntity.EntityEngineBase.AddCharacterInfoEntity(this);

            _isItemController = gameObject.layer == MasksProject.Entity;

            CharacterInput.AddFuncStopMovement(() =>
            {
                return Regdool.isActive;
            });
            if (!_isItemController)
                Regdool = new RegdollPlayer(AnimatorInput.Animator, this);


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
            interactEntity?.RayCast();
        }

        public void RotateBody(Quaternion quaternion)
        {
            if (AnimatorInput.IsPlayStateAnimator(TypeAnimation.Climbing))
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
            FactoryEntity.EntityEngineBase.RemoveCharacterInfoEntity(this);
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            if (isItemController)
            {
                foreach (var collider in GetComponentsInChildren<Collider>())
                    collider.material = CharacterInput.PhysicMaterial;
            }
            CharacterInput.IntroducingCaracter = new InputDefault(this);
            interactEntity = new ViewInteractEntity(transform);
        }
        public void ExitPlayerControll(CameraControll camera)
        {
            if (isItemController)
            {
                foreach (var collider in GetComponentsInChildren<Collider>())
                    collider.material = null;
            }
            FactoryEntity.EntityEngineBase.AddCharacterInfoEntity(this);
            interactEntity?.ItemThrow();
            None.SetInfoEntity(false);
            ChangeLayerIsItemToPlayer(false);
            CharacterInput.IntroducingCaracter = null;
            camera.Transform.parent = null;
            interactEntity = null;
        }
        private void ChangeLayerIsItemToPlayer(bool IsEnabled)
        {
            if (isItemController)
                isItemController = !IsEnabled;
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
        [ContextMenu("Fell")]
        public void Fell()
        {
            if (Regdool != null && !isItemController)
                Regdool.Activate();
            AnimatorInput?.BlinkingEyes(100F);
            CharacterInput.Fly(Off: true);
            onFell.Invoke();
        }
        public void Die()
        {
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
                tacking = new TackingIK(_input.AnimatorInput.Animator);
                tacking.LookIKWeight = 1f;
                tacking.BodyWeight = 0.2f;
                tacking.ClampWeight = 0.5f;
                tacking.EyesWeight = 1f;
                tacking.HeadWeight = 0.9f;
            }
            private CharacterBody input;

            private TackingIK tacking;
            public bool IsRun => Input.GetKey(KeyCode.LeftShift);

            public void Enable()
            {
                tacking.Target = new GameObject("Target").transform;
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
                tacking.Target.position = input.interactEntity.pointTarget;
                return MovementMode.WASD(source, 1F, out isMove, true);
            }

        }
    }
}

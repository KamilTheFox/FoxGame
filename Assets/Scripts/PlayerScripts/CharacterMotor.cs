using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using Unity.VisualScripting;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst.CompilerServices;
using System.Diagnostics.Contracts;
using UnityEngine.SocialPlatforms;
using UnityEditor.Experimental.GraphView;
using TMPro;

namespace PlayerDescription
{
    public partial class CharacterMotor : MonoBehaviour, IGlobalUpdates, IAtWater, ICharacterAdaptivator
    {
        CharacterMediator mediator;

        private CapsuleCollider CharacterCollider => mediator.MainCollider;

        private Bounds BoundsCollider => CharacterCollider.bounds;

        private Vector2 oldSizeCollider = Vector2.zero;

        private IMoveablePlatform platform;

        private Rigidbody Rigidbody => mediator.MainRigidbody;

        public bool isRun
        {
            get
            {
                return CurrentState.HasState(StateCharacter.MovementRun);
            }
            set
            {
                if (value)
                    CurrentState = CurrentState.AddState(StateCharacter.MovementRun);
                else
                    CurrentState = CurrentState.RemoveState(StateCharacter.MovementRun);
            }
        }
        public bool isPressCrouch { get; private set; }

        public EventMotor EventsMotor { get; private set; } = new EventMotor();

        [field: SerializeField]
        public StateCharacter CanStateCharacter { get; set; }

        [field: SerializeField]
        public StateCharacter CurrentState { get; set; } = StateCharacter.Movement;

        [field: Header("Speeds")]

        [field: SerializeField] public float Speed { get; private set; }
        [field: SerializeField] public float SpeedRun { get; private set; }

        [field: SerializeField] public float SpeedCrouch { get; private set; }

        [field: SerializeField]
        public float SpeedSwim { get; private set; }

        public Vector3 Direction { get; private set; }

        [field: Header("Settings")]
        [SerializeField] private float maxAngleMove = 60F, stepSteirs = 0.3F;

        public float MaxAngleMove => maxAngleMove;

        [SerializeField] private float movementThreshold = 0.1f;

        [SerializeField] private AnimationCurve curveJump;

        [field: SerializeField] private float forseJump = 4F;

        public float ForseJump => forseJump;

        RigidBodyMovement rigidBodyMovement;

        private bool UseGravity { get; set; }

        public float StepSteirs => stepSteirs;

        private CastGroundChecked castGroundChecked;

        public RaycastHit GroundCheckClimbind => castGroundChecked.hitClimbind;

        public bool IsInAir => !isGrounded && !isSwim;

        public bool CanClimbing => CanStateCharacter.HasState(StateCharacter.Climbing) && IsInAir;

        private static PhysicMaterial physicMaterial;

        public static PhysicMaterial PhysicMaterial
        {
            get
            {
                if (!physicMaterial)
                {
                    physicMaterial = Resources.Load<PhysicMaterial>("Player/Player");
                }
                return physicMaterial;
            }
        }

        private Transform forwardTransform;
        public Transform ForwardTransform
        {
            private get
            {
                if (!forwardTransform)
                {
                    forwardTransform = transform;
                }
                return forwardTransform;
            }
            set
            {
                forwardTransform = value;
            }
        }

        private List<Func<bool>> eventStopMovement = new();

        public bool IsStopMovement
        {
            get
            {
                foreach (var e in eventStopMovement)
                {
                    if (e())
                        return true;
                }
                return false;
            }
        }

        private bool IsMaxAngleSurface => castGroundChecked.angleSurface > maxAngleMove;

        public IInputCharacter IntroducingCharacter { get; private set; }

        private Stack<IInputCharacter> inputPreviousCaracters = new Stack<IInputCharacter>();

        private Vector3 lastPosition;

        public bool isGrounded => castGroundChecked.isGrounded;

        public bool isCrouch
        {
            get
            {
                return CurrentState.HasState(StateCharacter.Crouch);
            }
            set
            {
                if (value)
                    CurrentState = CurrentState.AddState(StateCharacter.Crouch);
                else
                    CurrentState = CurrentState.RemoveState(StateCharacter.Crouch);
            }
        }
        public bool Climbind => castGroundChecked.isClimbind;

        public AudioSource AudioSource => mediator.MainAudioSource;

        [field: SerializeField] private AudioCharacter AudioCharacter { get; set; }

        #region Water
        [field: SerializeField] public float VolumeObject { get; private set; }

        public bool isSwim
        {
            get
            {
                return CurrentState.HasState(StateCharacter.Swim);
            }
            set
            {
                if (value)
                    CurrentState = CurrentState.AddState(StateCharacter.Swim);
                else
                    CurrentState = CurrentState.RemoveState(StateCharacter.Swim);
            }
        }


        public void EnterWater()
        {
            if (isPressCrouch)
            {
                isPressCrouch = false;
                EventsMotor[TypeAnimation.Crouch]?.Invoke();
            }
            CapsuleCollider cap = CharacterCollider;
            if (cap)
            {
                oldSizeCollider = new Vector2(cap.radius, cap.height);

                cap.radius = 0.5F;
                cap.height = 1F;
            }
            EventsMotor[TypeAnimation.Swimming].Invoke();
            isSwim = true;
        }

        public void ExitWater()
        {
            CapsuleCollider cap = CharacterCollider;
            if (oldSizeCollider != Vector2.zero && cap)
            {
                cap.radius = oldSizeCollider.x;
                cap.height = oldSizeCollider.y;
            }
            EventsMotor[TypeAnimation.DontSwimming].Invoke();
            isSwim = false;
        }
        private bool IsWaterLine(Vector3 direction)
        {
            return Physics.Raycast(
                new Ray(Rigidbody.worldCenterOfMass + (Vector3.up * 0.15f), Vector3.down),
                direction.normalized.y * Speed,
                MasksProject.Water,
                QueryTriggerInteraction.Collide);
        }
        #endregion
        public void OnAwake()
        {
            castGroundChecked = new CastGroundChecked(this);
            rigidBodyMovement = new RigidBodyMovement(this);

            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            CharacterCollider.material = PhysicMaterial;

            if (AudioCharacter != null)
                AudioCharacter.AddListnerEventInput(this);

            UseGravity = true;
            Rigidbody.useGravity = false;
        }
        
        public void SetMediator(CharacterMediator adapter)
        {
            mediator = adapter;
        }

        public void SetInputCharacter(IInputCharacter input, bool setStack = false)
        {
            if (IntroducingCharacter != null)
                IntroducingCharacter.Disable();
            IInputCharacter current = input;
            if (current == null &&
                inputPreviousCaracters.TryPeek(out IInputCharacter previous))
            {
                IntroducingCharacter = previous;
            }
            else
                IntroducingCharacter = current;

            if (IntroducingCharacter != null)
                IntroducingCharacter.Enable();
            if (current == null) return;
            string name = current.GetType().Name.ToLower();
            if (setStack)
                inputPreviousCaracters.Push(current);
        }
        public void AddFuncStopMovement(Func<bool> func)
        {
            eventStopMovement.Add(func);
        }

        private bool HasMoved()
        {
            return Vector3.Distance(transform.position, lastPosition) > movementThreshold;
        }

        private void FixedUpdate()
        {
            castGroundChecked.Cast();

            if (castGroundChecked.isGrounded)
            {
                if (castGroundChecked.hitGround.collider.TryGetComponent(out Rigidbody entity))
                {
                    castGroundChecked.ResertSurface();
                }

                if ((platform = castGroundChecked.hitGround.collider.GetComponent<IMoveablePlatform>()) != null)
                {
                    this.transform.SetParent(platform.Guide);
                }
            }

            rigidBodyMovement.Movement();

            rigidBodyMovement.CalculateGravity();
        }

        private void Update()
        {
            EventsMotor[TypeAnimation.Landing]?.Invoke();

            if (IntroducingCharacter == null || IsStopMovement)
                return;

            EventsMotor[TypeAnimation.Crouch]?.Invoke();
            isRun = IntroducingCharacter.IsRun;
            isPressCrouch = IntroducingCharacter.IsCrouch;

            rigidBodyMovement.Jump();
        }

        public bool CanStandUp()
        {
            Vector3 top = transform.position + Vector3.up * (CharacterCollider.height - 0.3F);

            float radius = CharacterCollider.radius * 2.3F;

            Debug.DrawRay(top, Vector3.up * radius, Color.magenta);

            return !Physics.Raycast(top, Vector3.up, radius, MasksProject.RigidObject, QueryTriggerInteraction.Ignore);
        }
        private struct CastGroundChecked
        {
            CharacterMotor motor;

            public bool isSteirs;
            public RaycastHit hitStairs;

            public bool isGrounded;
            public RaycastHit hitGround;

            public bool isClimbind;
            public RaycastHit hitClimbind;

            public Vector3 normalSurface;

            public float angleSurface;

            public CastGroundChecked(CharacterMotor motor)
            {
                this.motor = motor;
                isClimbind = isGrounded = isSteirs = false;
                hitClimbind = hitGround = hitStairs = default;
                angleSurface = 0;
                normalSurface = Vector3.up;
            }
            public void Cast()
            {
                CastGround();
                if(isGrounded)
                    CastSteirs();
                if(motor.CanClimbing)
                    CastClimbind();
            }

            private void CastGround()
            {
                CapsuleCollider capsule = motor.CharacterCollider;
                Ray directionCast = new Ray(motor.transform.position + Vector3.up * capsule.radius, Vector3.down);

                float newRadius = capsule.radius - 0.02f;
                isGrounded = Physics.SphereCast(directionCast,
                    newRadius,
                    out hitGround,
                    0.05F,
                    MasksProject.RigidObject,
                    QueryTriggerInteraction.Ignore);

                if (isGrounded)
                    SetAngleSurface(hitGround.normal);
                else
                    ResertSurface();
            }
            private void SetAngleSurface(Vector3 normal)
            {
                normalSurface = normal;
                angleSurface = Vector3.Angle(Vector3.up, normal);
            }
            public void ResertSurface()
            {
                normalSurface = Vector3.up;
                angleSurface = 0;
            }

            private void CastSteirs()
            {
                Vector3 point = hitGround.point;
                float y = motor.transform.InverseTransformPoint(point).y;
                motor.UseGravity = true;
                isSteirs = false;
                if (y < motor.stepSteirs && y > 0.01F)
                {
                    if (Physics.Raycast(
                        point + Vector3.up * 0.7F + motor.Direction.normalized * motor.Speed * Time.fixedDeltaTime,
                        Vector3.down,
                        out hitStairs,
                        1F,
                        MasksProject.RigidObject,
                        QueryTriggerInteraction.Ignore))
                    {
                        float surfaceAngle = Vector3.Angle(hitStairs.normal, Vector3.up);

                        angleSurface = (int)surfaceAngle;

                        if (surfaceAngle < 90 - motor.maxAngleMove)
                        {
                            float angleDifference = Vector3.Angle(hitStairs.normal, normalSurface);

                            if (angleDifference > 2F)
                            {
                                isSteirs = true;
                                motor.UseGravity = false;
                            }
                        }
                    }
                }
            }

            private void CastClimbind()
            {
                Ray rayCheckPlane = new Ray(
                    motor.transform.position + motor.transform.up * 2F + motor.transform.forward * motor.CharacterCollider.radius * 1.35F,
                    Vector3.down * 1.025F);
                isClimbind = Physics.Raycast(rayCheckPlane, out RaycastHit hit, 1.3f, MasksProject.Climbinding, QueryTriggerInteraction.Ignore);
                if (hit.rigidbody != null)
                    isClimbind = false;
                if (isClimbind)
                {
                    isClimbind = !Physics.Raycast(hit.point + hit.normal * 0.01F, Vector3.up, motor.CharacterCollider.height);
                }

                hitClimbind = isClimbind ? hit : default;
            }

        }

        private struct RigidBodyMovement
        {
            CharacterMotor motor;

            AnimationCurve curveJump;

            public Vector3 direction;

            private bool startJump;

            private bool useJumpDouble, startJumpDouble;

            private bool isJumping;

            public float forceJump;

            private float gravity => Physics.gravity.y;

            private float velosityGravity;

            private float timeJump;

            private Rigidbody rigidbody;

            private bool CanDoubleJump => motor.CurrentState.HasState(StateCharacter.DoubleJump);

            public RigidBodyMovement(CharacterMotor motor)
            {
                this.motor = motor;
                direction = Vector3.zero;
                rigidbody = motor.Rigidbody;
                forceJump = 4;
                timeJump = 0;
                velosityGravity = 0;
                startJump = false;
                useJumpDouble = startJumpDouble = isJumping = false;
                curveJump = motor.curveJump;
            }

            public void Movement()
            {
                if (motor.IsStopMovement)
                {
                    motor.Rigidbody.velocity = new Vector3(0, 0, 0);
                    motor.EventsMotor.eventMovement?.Invoke(Vector3.zero, false);
                    return;
                }

                bool isMove = false;

                if (motor.IntroducingCharacter != null)
                    direction = motor.IntroducingCharacter.Move(motor.ForwardTransform, out isMove).normalized;

               if(motor.isGrounded)
                {
                    velosityGravity = 0f;
                }

                Move(ref isMove);

                TrySpace();

                motor.EventsMotor.eventMovement.Invoke(direction, isMove);
            }

            private void Move(ref bool isMove)
            {
                if (motor.CurrentState.HasAnyState(StateCharacter.Movement, StateCharacter.MovementRun, StateCharacter.Crouch))
                {
                    MoveDefault(ref isMove);
                }
                else if (motor.CurrentState.HasAnyState(StateCharacter.Swim, StateCharacter.Fly))
                {
                    MoveFly(ref isMove);
                }
                motor.Direction = direction;
            }

            private void MoveDefault(ref bool isMove)
            {
                direction = DirectionFromSrface(direction);

                if (motor.castGroundChecked.isSteirs == false && motor.IsMaxAngleSurface &&
                    !IsDirectionInverseNormal(direction, motor.castGroundChecked.normalSurface))
                {
                    isMove = false;
                    direction = Vector3.zero;
                }
                Move(direction * (motor.isRun ? motor.SpeedRun : motor.Speed));
            }
            private void MoveFly(ref bool isMove)
            {
                if (motor.IntroducingCharacter.Shift())
                    direction.y -= (motor.isRun ? motor.SpeedRun :
                        (motor.isSwim ? motor.SpeedSwim : motor.Speed));
                if (motor.IntroducingCharacter.Space())
                    direction.y += (motor.isRun ? motor.SpeedRun :
                        (motor.isSwim ? motor.SpeedSwim : motor.Speed));

                rigidbody.MovePosition(direction);
            }


            private void TrySpace()
            {
                if (((motor.isSwim || motor.CanStateCharacter.HasState(StateCharacter.Climbing)) && startJump))
                {
                    if (motor.Climbind)
                    {
                        if ((motor.platform =
                            motor.castGroundChecked.hitClimbind.collider.gameObject.GetComponent<IMoveablePlatform>()) != null)
                        {
                            motor.transform.SetParent(motor.platform.Guide);
                        }
                        motor.EventsMotor[TypeAnimation.Climbing].Invoke();
                    }
                    goto endTryJump;
                }
                if (isJumping)
                {
                    if (startJumpDouble && useJumpDouble == false)
                    {
                        useJumpDouble = true;
                        motor.EventsMotor[TypeAnimation.Jump]?.Invoke();
                    }
                    goto endTryJump;
                }
                if (!motor.castGroundChecked.isSteirs && motor.IsMaxAngleSurface)
                {
                    goto endTryJump;
                }
                if (motor.isGrounded && startJump)
                {
                    isJumping = true;
                    motor.castGroundChecked.ResertSurface();
                    motor.EventsMotor[TypeAnimation.Jump]?.Invoke();
                }
endTryJump:
                startJump = false;
            }
            private bool IsDirectionInverseNormal(Vector3 direction, Vector3 normal)
            {
                return Vector3.Dot(direction, normal) > 0;
            }

            private Vector3 DirectionFromSrface(Vector3 direction)
            {
                return direction - Vector3.Dot(direction, motor.castGroundChecked.normalSurface) * motor.castGroundChecked.normalSurface;
            }

            public void Jump()
            {
                if (motor.isSwim)
                    return;

                bool isJump = motor.IntroducingCharacter.Space();

                if (CanDoubleJump && isJump && isJumping &&
                    !motor.isGrounded && !startJumpDouble)
                {
                    startJumpDouble = true;
                }

                if (motor.isCrouch && !motor.CanStandUp() && !motor.IsInAir)
                    isJump = false;

                if (isJump && (motor.isGrounded || (!motor.isGrounded && motor.Climbind)))
                {
                    startJump = true;
                    motor.castGroundChecked.isGrounded = false;
                }
            }

            public void CalculateGravity()
            {
                if (motor.isSwim || motor.CurrentState.HasState(StateCharacter.Fly))
                    return;

                if (isJumping)
                {
                    if (motor.isGrounded)
                    {
                        isJumping = false;
                        timeJump = 0;
                        startJumpDouble = false;
                        useJumpDouble = false;
                        velosityGravity = 0f;
                    }
                    else
                    {
                        velosityGravity += Math.Abs(forceJump * curveJump.Evaluate(timeJump));
                        timeJump += Time.fixedDeltaTime;
                        if (timeJump > 1f)
                        {
                            isJumping = false;
                            timeJump = 0;
                        }
                    }
                }
                else if (motor.UseGravity && !motor.isGrounded && !motor.castGroundChecked.isSteirs)
                {
                    velosityGravity += gravity * Time.fixedDeltaTime * 0.025f;
                    Ray ray = new Ray(
                        motor.transform.position + Vector3.up * 0.01f,
                        Vector3.down);

                    Vector3 newPosition;
                    if (Physics.Raycast(ray,
                        out RaycastHit hit,
                        velosityGravity * -1,
                        MasksProject.RigidObject,
                        QueryTriggerInteraction.Ignore))
                    {
                        motor.castGroundChecked.isGrounded = false;
                        newPosition = hit.point;
                    }
                    else
                    {
                        newPosition = ray.GetPoint(velosityGravity * -1);
                    }

                    StartInterpolation(newPosition);
                }
                else
                    velosityGravity = 0f;
            }
            private void StartInterpolation(Vector3 newTarget)
            {
                motor.StartCoroutine(InterpolateMovement(rigidbody.position, newTarget));
            }

            private IEnumerator InterpolateMovement(Vector3 currentPosition, Vector3 newTarget)
            {
                float interpolationTime = 0f;
                while (interpolationTime < Time.fixedDeltaTime && motor.isGrounded == false)
                {
                    interpolationTime += Time.deltaTime;
                    float t = interpolationTime / Time.fixedDeltaTime;

                    Vector3 newPosition = Vector3.Lerp(currentPosition, newTarget, t);
                    rigidbody.MovePosition(new Vector3(rigidbody.position.x, newPosition.y, rigidbody.position.z));

                    yield return null;
                }

                // Финальное положение
                rigidbody.MovePosition(new Vector3(rigidbody.position.x, newTarget.y, rigidbody.position.z));
            }

            public void Move(Vector3 direction)
            {
                if (motor.isGrounded)
                {
                    rigidbody.velocity = new Vector3(direction.x, direction.y, direction.z);
                }
                else
                    rigidbody.velocity = new Vector3(direction.x, 0f, direction.z);
            }
        }

        [Serializable]
        public class EventMotor
        {
            public UnityEvent<Vector3, bool> eventMovement = new UnityEvent<Vector3, bool>();
            public event UnityAction<Vector3, bool> EventMovement
            {
                add
                {
                    eventMovement.AddListener(value);
                }
                remove { eventMovement.RemoveListener(value); }
            }

            [SerializeField] private List<UnityEvent> Events = new List<UnityEvent>();
            [SerializeField] private List<TypeAnimation> NameEvent = new List<TypeAnimation>();
            public UnityEvent this[TypeAnimation anim]
            {
                get
                {
                    int index;
                    if (NameEvent.Contains(anim))
                    {
                        index = NameEvent.IndexOf(anim);
                        if (Events.Count <= index)
                            return null;
                        return Events[index];
                    }
                    else
                        NameEvent.Add(anim);
                    UnityEvent unityEvent = new UnityEvent();
                    Events.Add(unityEvent);
                    return unityEvent;
                }
                set
                {
                    int index;
                    NameEvent.Add(anim);
                    index = NameEvent.IndexOf(anim);
                    for (int i = Events.Count; i <= index; i++)
                    {
                        Events.Add(null);
                    }
                    Events[index] = value;
                }
            }
            public void Clear()
            {
                NameEvent.Clear();
                Events.Clear();
            }
        }
    }
}

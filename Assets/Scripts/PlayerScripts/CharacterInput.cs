using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Unity.Jobs;
using Unity.Collections;
using AIInput;

namespace PlayerDescription
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class CharacterInput : MonoBehaviour, IGlobalUpdates, IAtWater, ICharacterAdaptivator
    {

        private const float DIVIDER_TO_FOOT_TOUCH = 10F;

        private const float MAX_KICK_MASS_BODY = 5F;

        private const float MAX_TIME_FALLING = 0.25F;

        CharacterMediator adapter;

        public bool _isGroundedCast => isGroundedCast;
        public bool _isGrounded
        {
            get
            {
                if (!_isContactsFoot)
                {
                    return _isGroundedCast;
                }
                return contactsFoot.Count() > 0;
            }
        }
        public bool _isGroundedContact
        {
            get
            {
                return _isContactsFoot;
            }
        }
        public bool _isContactsFoot
        {
            get
            {
                return contactsFoot != null && contactsFoot.Count > 0;
            }
        }
        public bool IsInAir => !_isGrounded && !isSwim;

        [field: SerializeField] public bool IsCanClimbinding { get; set; } = true;

        public bool IsClimbinding => IsCanClimbinding && IsInAir;

        [field: SerializeField] public bool IsEdgePlaneClimbing { get; private set; }

        [field: SerializeField] public RaycastHit PointEdgePlaneClimbing { get; private set; }

        private bool IsSurfaseWall => angleSurface == 90;

        private bool IsMaxAngleSurface
        {
            get
            {
                if (IsSurfaseWall)
                    return false;
                return angleSurface > maxAngleMove;
            }
        }

        public float MaxAngleMove => maxAngleMove;

        public float StepSteirs => stepSteirs;

        public bool isSwim { get; private set; }

        private CapsuleCollider CharacterCollider => adapter.MainCollider;

        [field: SerializeField] public float VolumeObject { get; private set; } = 100F;

        private Bounds BoundsCollider => CharacterCollider.bounds;

        private bool UseGravity => Rigidbody.useGravity;

        private Rigidbody Rigidbody => adapter.MainRigidbody;

        [field: SerializeField] private float forseJump = 4F;

        public float ForseJump => forseJump;

        #region ServiceField

        private List<Func<bool>> eventStopMovement = new();

        private bool startJump;

        private Vector3 lastPosition;

        [SerializeField] private float movementThreshold = 0.1f;

        [SerializeField] private float maxAngleMove = 60F, stepSteirs = 0.3F;

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

        private List<ContactPoint> contactsFoot = new(), contacts = new(), contactPointsAll = new();

        private bool isSteirs;

        private Collider Collider;

        private Vector3 normalSurfaces, velocityBody;

        private IMoveablePlatform platform;

        private bool checkCollisionExit = false;

        private bool hasContact;

        private bool hasStop, isJumping;

        private int angleSurface;

        private bool isGroundedCast;

        private RaycastHit groundCastHit;

        private Rigidbody _rigidbody;

        #endregion

        [SerializeField] private float doubleJump = 0.9f, doubleJumpMax = 1.5F;


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



        [field: SerializeField] public bool isMultyJump { get; private set; }

        private Transform Transform => adapter.Transform;

        public AudioSource AudioSource => adapter.MainAudioSource;

        [field: SerializeField] private AudioCharacter AudioCharacter { get; set; }

        [field: SerializeField] public float Speed { get; private set; }
        [field: SerializeField] public float SpeedRun { get; private set; }

        [field: SerializeField] public float SpeedCrouch { get; private set; }

        [field: SerializeField] public bool CanSwim { get; private set; } = true;

        public float SpeedSwim => (Speed + SpeedRun) / 2;
        [field: SerializeField] public bool isFly { get; private set; }

        public IInputCharacter IntroducingCharacter
        {
            get
            {
                return inputCaracter;
            }
            set
            {
                if (inputCaracter != null)
                    inputCaracter.Disable();
                IInputCharacter current = value;
                if (current == null &&
                    inputPreviousCaracters.TryPeek(out IInputCharacter previous))
                {
                    inputCaracter = previous;
                }
                else
                    inputCaracter = current;

                if (inputCaracter != null)
                    inputCaracter.Enable();
                if (current == null) return;
                string name = current.GetType().Name.ToLower();
                if (!name.Contains("Default".ToLower()) && !name.Contains("_"))
                    inputPreviousCaracters.Push(current);
            }
        }

        private IInputCharacter inputCaracter;

        private Stack<IInputCharacter> inputPreviousCaracters = new Stack<IInputCharacter>();

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

        public Vector3 VelosityDefaultDirection { get; private set; }

        public bool isRun { get; private set; }

        public bool isPressCrouch { get; private set; }

        public bool isCrouch { get; set; }

        public Vector3 Velosity => velocityBody;

        [field: SerializeField] public EventInputAnimation eventInput { get; private set; } = new EventInputAnimation();

        public void SetMediator(CharacterMediator _adapter)
        {
            this.adapter = _adapter;
        }
        public void OnAwake()
        {
            this.AddListnerUpdate();
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            CharacterCollider.material = PhysicMaterial;

            base.StartCoroutine(FallInvoke());

            if (AudioCharacter != null)
                AudioCharacter.AddListnerEventInput(this);
        }
        private void OnEnable()
        {
            if (IntroducingCharacter != null) IntroducingCharacter.Enable();
        }
        private void OnDisable()
        {
            if (IntroducingCharacter != null) IntroducingCharacter.Disable();
        }
        private void Reset()
        {
            eventInput.Clear();
        }
        public void Fly(bool Off = false)
        {
            isFly = !isFly;
            if (Off)
                isFly = false;

            isSwim = isFly;

            Rigidbody.drag = isFly ? 9 : 0;
        }
        private void CalculateEdgePlaneClimbing()
        {
            if (!isSwim && !IsClimbinding) return;
            Ray rayCheckPlane = new Ray(transform.position + transform.up * 2F + transform.forward * CharacterCollider.radius * 1.35F, Vector3.down * 1.1F);
            Debug.DrawRay(rayCheckPlane.origin, rayCheckPlane.direction);
            IsEdgePlaneClimbing = Physics.Raycast(rayCheckPlane, out RaycastHit hit, 1.3f, MasksProject.Climbinding, QueryTriggerInteraction.Ignore);
            if (hit.rigidbody != null)
                IsEdgePlaneClimbing = false;
            if (IsEdgePlaneClimbing)
            {
                IsEdgePlaneClimbing = !Physics.Raycast(hit.point + hit.normal * 0.01F, Vector3.up, CharacterCollider.height);
            }

            PointEdgePlaneClimbing = IsEdgePlaneClimbing ? hit : default;
        }
        private void CalculateGroundCast()
        {
            CapsuleCollider capsule = CharacterCollider;
            Ray directionCast = new Ray(transform.position + Vector3.up * capsule.radius, Vector3.down);

            float newRadius = capsule.radius - 0.02F;
            isGroundedCast = Physics.SphereCast(directionCast, newRadius, out RaycastHit hit, 0.4F, MasksProject.RigidObject, QueryTriggerInteraction.Ignore);
            groundCastHit = isGroundedCast ? hit : default;
        }
        private void ClearContactAndNormal()
        {
            if (checkCollisionExit && this.Collider == null)
            {
                ClearNurmalSurface();
                contacts.Clear();
                contactPointsAll.Clear();
                checkCollisionExit = false;
            }
        }
        public void OnCollisionEnter(Collision collision)
        {
            isJumping = false;
            RefreshContact(collision);
            if (_isGrounded && !isSwim)
            {
                if (collision.gameObject.TryGetComponent(out Rigidbody rigidbody) && rigidbody.mass < MAX_KICK_MASS_BODY)
                    Rigidbody.velocity.Set(Rigidbody.velocity.x, 0, Rigidbody.velocity.z);
                if (!IsSurfaseWall)
                    if (_isContactsFoot)
                        if ((platform = collision.gameObject.GetComponent<IMoveablePlatform>()) != null)
                        {
                            this.transform.SetParent(platform.Guide);
                        }
                Rigidbody.useGravity = false;
            }
        }
        public void OnCollisionStay(Collision collision)
        {
            RefreshContact(collision);
        }
        private void RefreshContact(Collision collision)
        {
            if (isSwim || hasContact || HasMoved() == false) return;
            hasContact = true;
            this.Collider = collision.collider;
            checkCollisionExit = true;
            contacts.Clear();
            contactsFoot.Clear();
            contactPointsAll.Clear();
            contactPointsAll = collision.contacts.ToList();

            foreach (var poit in contactPointsAll)
            {
                if (transform.InverseTransformPoint(poit.point).y > (BoundsCollider.max.y - BoundsCollider.min.y) / DIVIDER_TO_FOOT_TOUCH)
                {
                    contacts.Add(poit);
                }
                else
                    contactsFoot.Add(poit);
            }
            if (collision.gameObject.TryGetComponent(out Rigidbody entity))
                return;
            if (contactsFoot.Count > 0)
            {
                normalSurfaces = contactsFoot[0].normal;
            }
            else if (!_isGrounded)
            {
                ClearNurmalSurface();
                return;
            }
            foreach (var contact in contactsFoot)
            {
                if (Vector3.Angle(Vector3.up, contact.normal) < Vector3.Angle(Vector3.up, normalSurfaces))
                {
                    normalSurfaces = contact.normal;
                }
            }
            angleSurface = (int)Vector3.Angle(Vector3.up, normalSurfaces);
        }
        private void Move(Vector3 direction, ref bool isMove)
        {
            VelosityDefaultDirection = direction.normalized;

            direction = DirectionFromSrface(direction).normalized;

            bool isWall = CheckWall(direction);

            if (!isSteirs && IsMaxAngleSurface && !IsDirectionInverseNormal(direction, normalSurfaces) || isWall)
            {
                isMove = false;
                direction = Vector3.zero;
            }
            if (!isJumping)
            {
                bool shouldResetVerticalVelocity = false;
                if (hasStop && !isMove)
                {
                    shouldResetVerticalVelocity = true;
                    hasStop = false;
                }
                else if (isMove)
                {
                    hasStop = true;
                    if (!_isGroundedContact)
                    {
                        shouldResetVerticalVelocity = true;
                    }
                }
                if (shouldResetVerticalVelocity)
                {
                    if (Rigidbody.velocity.y > 0)
                    {
                        direction.y = 0F;
                        Rigidbody.velocity = direction;
                    }
                    else if (!isSteirs && !_isGroundedContact && _isGroundedCast &&
                        Vector3.Distance(groundCastHit.point, Transform.position) < 0.25F)
                    {
                        Transform.position += Vector3.down * 0.09F;
                        normalSurfaces = groundCastHit.normal;
                        direction = direction - Vector3.Dot(direction, normalSurfaces) * normalSurfaces;
                    }
                }
            }
            
            float speed = (isRun ? SpeedRun : Speed);
            if(isCrouch)
            {
                speed = SpeedCrouch;
            }
            if (isSwim)
            {
                speed = SpeedSwim;
                if (IntroducingCharacter != null)
                {
                    if (startJump)
                    {
                        Vector3 newDir = new Vector3(direction.x, 1F, direction.z);
                        if (!IsWaterLine(newDir))
                        {
                            direction = newDir;
                        }
                    }
                    if (IntroducingCharacter.Shift())
                    {
                        direction = new Vector3(direction.x, -1F, direction.z);
                    }
                }
            }
            velocityBody = direction.normalized * speed;

            float VelosityUp = Velosity.y;

            if ((UseGravity || !_isContactsFoot) && !isSwim || isWall)
            {
                VelosityUp = Rigidbody.velocity.y;
            }

            Rigidbody.velocity = new Vector3(Velosity.x, VelosityUp, Velosity.z);
        }

        public bool CanStandUp()
        {
            Vector3 top = transform.position + Vector3.up * (CharacterCollider.height - 0.3F);

            float radius = CharacterCollider.radius * 2.3F;

            Debug.DrawRay(top, Vector3.up * radius, Color.magenta);

            return !Physics.Raycast(top, Vector3.up, radius, MasksProject.RigidObject, QueryTriggerInteraction.Ignore);
        }

        private bool IsWaterLine(Vector3 direction)
        {
            return Physics.Raycast(new Ray(Rigidbody.worldCenterOfMass + (Vector3.up * 0.15f), Vector3.down), direction.normalized.y * Speed, MasksProject.Water, QueryTriggerInteraction.Collide);
        }
        private bool IsDirectionInverseNormal(Vector3 direction, Vector3 normal)
        {
            return Vector3.Dot(direction, normal) > 0;
        }
        private Vector3 DirectionFromSrface(Vector3 direction)
        {
            Vector3 localNormalSurfase = normalSurfaces;

            if (contacts != null && contactsFoot != null &&
                contacts.Count + contactsFoot.Count > 0 &&
                contactPointsAll.Count > 0)
            {
                Vector3 point = contactPointsAll[0].point;
                float y = transform.InverseTransformPoint(point).y;

                if (y < stepSteirs && y > 0.01F)
                {
                    // Делаем один каст сверху
                    RaycastHit hit;
                    if (Physics.Raycast(
                        point + Vector3.up * 0.7F + direction.normalized * Speed * Time.fixedDeltaTime,
                        Vector3.down,
                        out hit,
                        1F,
                        MasksProject.RigidObject,
                        QueryTriggerInteraction.Ignore))
                    {
                        float surfaceAngle = Vector3.Angle(hit.normal, Vector3.up);

                        isSteirs = false;
                        if(isDrawDebug)
                        {
                            Debug.DrawRay(hit.point, hit.normal,Color.red);
                        }

                        normalSurfaces = hit.normal;

                        angleSurface = (int)surfaceAngle;

                        if (surfaceAngle < 90 - MaxAngleMove)
                        {
                            float angleDifference = Vector3.Angle(hit.normal, contactPointsAll[0].normal);

                            if (angleDifference > 2F)
                            {
                                localNormalSurfase = contactPointsAll[0].normal;
                                if (!contactsFoot.Contains(contactPointsAll[0]))
                                {
                                    contactsFoot.Insert(0, contactPointsAll[0]);
                                }
                                isSteirs = true;
                                Rigidbody.useGravity = false;
                            }
                        }
                    }
                }
            }

            return direction - Vector3.Dot(direction, localNormalSurfase) * localNormalSurfase;
        }
        private void TrySpace()
        {
            if (((isSwim || IsClimbinding) && startJump))
            {
                if (IsEdgePlaneClimbing)
                {
                    if ((platform = PointEdgePlaneClimbing.collider.gameObject.GetComponent<IMoveablePlatform>()) != null)
                    {
                        this.transform.SetParent(platform.Guide);
                    }
                    eventInput[TypeAnimation.Climbing].Invoke();
                }
                goto endTryJump;
            }
            if (isJumping)
                goto endTryJump;
            if (!isSteirs && IsMaxAngleSurface)
            {
                goto endTryJump;
            }
            if (_isGrounded && startJump)
            {
                isJumping = true;
                ClearNurmalSurface();
                Vector3 direction = Rigidbody.velocity;
                direction.y = 0F;
                Rigidbody.velocity = direction;
                Rigidbody.MovePosition(Vector3.up * 0.01F + Transform.position);
                Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, forseJump * (isMultyJump ? doubleJump : 1F), Rigidbody.velocity.z);
                doubleJump = 0.9f;
                eventInput[TypeAnimation.Jump]?.Invoke();
            }
endTryJump:
            startJump = false;
        }

        void IGlobalUpdates.FixedUpdate()
        {
            if (_isContactsFoot == false)
                CalculateGroundCast();
            else if (Rigidbody.velocity.y <= 0.1f)
                isJumping = false;

            ClearContactAndNormal();

            CalculateEdgePlaneClimbing();

            Movement();

            hasContact = false;
        }

        private void Movement()
        {
            if (IsStopMovement)
            {
                Rigidbody.velocity = new Vector3(0, UseGravity || isFly ? Rigidbody.velocity.y : 0, 0);
                eventInput.eventMovement?.Invoke(Vector3.zero, false);
                return;
            }

            bool isMove = false;
            Vector3 direction = Vector3.zero;

            if (IntroducingCharacter != null)
                direction = IntroducingCharacter.Move(ForwardTransform, out isMove);
            Move(direction, ref isMove);
            TrySpace();
            eventInput.eventMovement.Invoke(VelosityDefaultDirection, isMove);
        }

        private bool CheckWall(Vector3 direction)
        {
            int angle;
            if (contacts == null || contacts.Count == 0 || _isGrounded)
                return false;
            foreach (var point in contacts)
            {
                angle = (int)Vector3.Angle(Vector3.up, point.normal);
                if (angle > maxAngleMove && !IsDirectionInverseNormal(direction, point.normal))
                    return true;
            }
            return false;
        }
        void IGlobalUpdates.Update()
        {
            eventInput[TypeAnimation.Landing]?.Invoke();
            if (IntroducingCharacter == null || IsStopMovement)
                return;
            isRun = IntroducingCharacter.IsRun;
            isPressCrouch = IntroducingCharacter.IsCrouch;
            eventInput[TypeAnimation.Crouch]?.Invoke();
            
            bool isJump = IntroducingCharacter.Space();
            if (isCrouch && !CanStandUp() && !IsInAir)
                isJump = false;
            if (isMultyJump && isJump)
            {
                if (doubleJumpMax > doubleJump)
                {
                    doubleJump += Time.deltaTime;
                    if (doubleJump > doubleJumpMax)
                        doubleJump = doubleJumpMax;
                }
            }
            if (isJump && (isSwim || _isGrounded || (!_isGrounded && IsEdgePlaneClimbing)))
            {
                startJump = true;
            }
        }
        public void AddFuncStopMovement(Func<bool> func)
        {
            eventStopMovement.Add(func);
        }

        bool HasMoved()
        {
            return Vector3.Distance(transform.position, lastPosition) > movementThreshold;
        }


        private float waitSecondFall;
        private IEnumerator FallInvoke()
        {
            while (true)
            {
retContinue:
                yield return new WaitUntil(() => !_isGrounded && enabled && !Rigidbody.isKinematic);
                while (waitSecondFall < MAX_TIME_FALLING)
                {
                    waitSecondFall += Time.fixedDeltaTime;
                    if (_isGrounded)
                    {
                        waitSecondFall = 0f;
                        goto retContinue;
                    }
                    yield return new WaitForFixedUpdate();
                }
                waitSecondFall = 0f;
                if (_isGrounded || isSwim || Rigidbody.isKinematic)
                {
                    goto retContinue;
                }
                eventInput[TypeAnimation.Fall]?.Invoke();
            }
        }
        private Vector2 oldSizeCollider = Vector2.zero;

        [Serializable]
        public class EventInputAnimation
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
        public void OnCollisionExit(Collision collision)
        {
            ClearNurmalSurface();
            contacts.Clear();
            if (platform == null) return;
            if (IsEdgePlaneClimbing && PointEdgePlaneClimbing.transform.parent == platform.Guide &&
                PointEdgePlaneClimbing.transform.parent == collision.transform.parent)
                return;
            if (platform.Guide == transform.parent && transform.parent == collision.transform.parent)
            {
                platform = null;
                transform.SetParent(null);
            }
        }

        private void ClearNurmalSurface()
        {
            normalSurfaces = Vector3.up;
            angleSurface = 0;
            contactsFoot.Clear();
            contactPointsAll.Clear();
            platform = null;
            if (!IsEdgePlaneClimbing)
                transform.SetParent(null);
            Rigidbody.useGravity = true;
            if (isFly)
            {
                eventInput[TypeAnimation.Fly]?.Invoke();
                return;
            }
        }
        public void EnterWater()
        {
            if(isPressCrouch)
            {
                isPressCrouch = false;
                eventInput[TypeAnimation.Crouch]?.Invoke();
            }
            CapsuleCollider cap = CharacterCollider;
            if (cap)
            {
                oldSizeCollider = new Vector2(cap.radius, cap.height);

                cap.radius = 0.5F;
                cap.height = 1F;
            }
            eventInput[TypeAnimation.Swimming].Invoke();
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
            eventInput[TypeAnimation.DontSwimming].Invoke();
            startJump = isSwim = false;
        }

        [SerializeField] private bool isDrawDebug;

#if UNITY_EDITOR
        #region Debug

        private void OnDrawGizmos()
        {
            if (!isDrawDebug)
                return;
            Gizmos.color = Color.red;

            Gizmos.DrawRay(groundCastHit.point, groundCastHit.normal);

            Gizmos.DrawRay(transform.position, Velosity.normalized);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, normalSurfaces.normalized);
            Gizmos.color = Color.green;
            if (!CharacterCollider)
                return;
            Gizmos.DrawSphere(transform.position + Vector3.up * (CharacterCollider.bounds.max.y - CharacterCollider.bounds.min.y) / DIVIDER_TO_FOOT_TOUCH, 0.05F);
            if (contacts != null)
            {
                Gizmos.color = Color.red;
                contacts.ToList().ForEach(contact =>
                {
                    Gizmos.DrawSphere(contact.point, 0.05F);
                });
            }
            if (contactsFoot == null || contactsFoot.Count < 0)
                return;
            Gizmos.color = Color.magenta;
            contactsFoot.ToList().ForEach(contact =>
            {
                Gizmos.DrawSphere(contact.point, 0.05F);
            });
        }
        #endregion
#endif
    }
}

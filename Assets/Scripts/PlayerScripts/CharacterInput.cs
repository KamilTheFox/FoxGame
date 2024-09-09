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
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class CharacterInput : MonoBehaviour, IAtWater, IGlobalUpdates
    {
        private const float DIVIDER_TO_FOOT_TOUCH = 10F;

        private const float MAX_KICK_MASS_BODY = 5F;

        private const float MAX_TIME_FALLING = 0.25F;

        [field: SerializeField] public float VolumeObject { get; private set; } = 100F;

        public bool isSwim { get; private set; }

        [SerializeField] private float forseJump = 4F, maxAngleMove = 60F, stepSteirs = 0.3F;

        public float MaxAngleMove => maxAngleMove;

        public float StepSteirs => stepSteirs;

        public float ForseJump => forseJump;

        [SerializeField] private Vector3 normalSurfaces, velocityBody;

        private bool UseGravity => Rigidbody.useGravity;

        #region ServiceField
        
        private static PhysicMaterial physicMaterial;

        private List<Func<bool>> eventStopMovement = new();

        private List<ContactPoint> contactsFoot = new(), contacts = new(), contactPointsAll = new();

        private bool startJump;

        private int angleSurface;

        private bool checkCollisionExit = false;

        private bool hasContact;

        private bool isSteirs;

        private Collider Collider;

        private Rigidbody _rigidbody;

        private IInputCaracter inputCaracter;

        private Stack<IInputCaracter> inputPreviousCaracters = new Stack<IInputCaracter>();

        private bool isGroundedCast;

        private Vector3 lastPosition;

        [SerializeField] private float movementThreshold = 0.1f;

        #endregion

        [SerializeField] private float doubleJump = 0.9f, doubleJumpMax = 1.5F;
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

        public CapsuleCollider CharacterCollider { get; private set; }

        [field: SerializeField] public bool isMultyJump { get; private set; }
        public Bounds BoundsCollider => CharacterCollider.bounds;
        
        public Rigidbody Rigidbody
        {
            get
            {
                if (!_rigidbody)
                {
                    _rigidbody = GetComponent<Rigidbody>();
                }
                return _rigidbody;
            }
        }

        public Transform Transform { get; private set; }

        public AudioSource AudioSource { get; private set; }

        [field: SerializeField] private AudioCharacter AudioCharacter { get; set; }

        [field: SerializeField] public float Speed { get; private set; }
        [field: SerializeField] public float SpeedRun { get; private set; }

        [field: SerializeField] public bool CanSwim { get; private set; } = true;

        public float SpeedSwim => (Speed + SpeedRun) / 2;
        [field: SerializeField] public bool isFly { get; private set; }
        public IInputCaracter IntroducingCaracter
        { 
            get
            {
                return inputCaracter;
            }
            set
            {
                if(inputCaracter != null)
                    inputCaracter.Disable();
                IInputCaracter current = value;
                if (current == null)
                {
                    if (inputPreviousCaracters.TryPeek(out IInputCaracter previous))
                    {
                        inputCaracter = previous;
                        goto goEnd;
                    }
                }
                inputCaracter = value;
                goEnd:
                if(inputCaracter != null)
                    inputCaracter.Enable();
                if (current == null) return;
                if(!current.GetType().Name.ToLower().Contains("Default".ToLower()))
                    inputPreviousCaracters.Push(value);
            }
        }

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

        public Vector3 Velosity => velocityBody;

        public Vector3 VelosityDefaultDirection { get; private set; }

        public bool isRun { get; private set; }

        public bool IsPlaneUpWater { get; private set; }

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
        public bool _isContactsFoot
        {
            get
            {
                return contactsFoot != null && contactsFoot.Count > 0;
            }
        }

        [field: SerializeField] public EventInputAnimation eventInput { get; private set; } = new EventInputAnimation();

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
        private void OnEnable()
        {
            if (IntroducingCaracter != null) IntroducingCaracter.Enable();
        }
        private void OnDisable()
        {
            if (IntroducingCaracter != null) IntroducingCaracter.Disable();
        }
        public void OnAwake()
        {
            this.AddListnerUpdate();
            Transform = transform;
            AudioSource = GetComponent<AudioSource>();
            CharacterCollider = GetComponent<CapsuleCollider>();
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            CharacterCollider.material = PhysicMaterial;
            base.StartCoroutine(FallInvoke());
            if (AudioCharacter != null)
                AudioCharacter.AddListnerEventInput(this);
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
        private void Move(Vector3 direction, ref bool isMove)
        {
            VelosityDefaultDirection = direction.normalized;

            direction = DirectionFromSrface(direction);

            if (!isSteirs && IsMaxAngleSurface && IsDirectionInverseNormal(direction, normalSurfaces) || CheckWall(direction))
            {
                isMove = false;
                direction = Vector3.zero;
            }
            float speed = (isRun ? SpeedRun : Speed);
            if (isSwim)
            {
                speed = SpeedSwim;
                if (IntroducingCaracter != null)
                {
                    if (IntroducingCaracter.Space())
                    {
                        Vector3 newDir = new Vector3(direction.x, 0.02F, direction.z);
                        if (!IsWaterLine(newDir))
                        {
                            direction = newDir;
                        }
                    }
                    if (IntroducingCaracter.Shift())
                    {
                        direction = new Vector3(direction.x, -0.02F, direction.z);
                    }
                }
            }
            velocityBody = direction.normalized * speed;

            float VelosityUp = Velosity.y;

            if((UseGravity || !_isContactsFoot) && !isSwim)
            {
                VelosityUp = Rigidbody.velocity.y;
            }

            Rigidbody.velocity = new Vector3(Velosity.x, VelosityUp , Velosity.z);
        }
        private bool IsWaterLine(Vector3 direction)
        {
            return Physics.Raycast(new Ray(Rigidbody.worldCenterOfMass + direction, Vector3.down), direction.normalized.y * Speed, MasksProject.Water , QueryTriggerInteraction.Collide);
        }
        private bool IsDirectionInverseNormal(Vector3 direction, Vector3 normal)
        {
            return Vector3.Dot(direction, normal) > 0;
        }
        private Vector3 DirectionFromSrface(Vector3 direction)
        {
            Vector3 localNormalSurfase = normalSurfaces;
            if (contacts != null && contactsFoot != null && contacts.Count + contactsFoot.Count > 0 && contactPointsAll.Count > 0)
            {
                Vector3 point = contactPointsAll[0].point;
                float y = transform.InverseTransformPoint(point).y;
                float maxY = stepSteirs;
                float minY = 0.01F;
                if (y < maxY && y > minY)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(point + Vector3.up * 0.7F + direction.normalized * Speed * Time.fixedDeltaTime, Vector3.down, out hit, MasksProject.RigidObject))
                    {
                        if (Vector3.Angle(hit.normal, normalSurfaces) > 2F)
                        {
                            localNormalSurfase = contactPointsAll[0].normal;
                            if (!contactsFoot.Contains(contactPointsAll[0]))
                            {
                                contactsFoot.Insert(0, contactPointsAll[0]);
                            }
                            Debug.DrawRay(point, localNormalSurfase, Color.yellow);
                            isSteirs = true;
                            Rigidbody.useGravity = false;
                        }
                        else
                            isSteirs = false;
                    }

                }
            }
            return direction - Vector3.Dot(direction, localNormalSurfase) * localNormalSurfase;
        }
        private void TrySpace()
        {
            if (isSwim && startJump)
            {
                if (IsPlaneUpWater)
                {
                    eventInput[TypeAnimation.Climbing].Invoke();
                }
                goto endTryJump;
            }

            if (!isSteirs && IsMaxAngleSurface)
            {
                goto endTryJump;
            }
            
            if (_isGrounded && startJump)
            {
                ClearNurmalSurface();
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
            CalculateGroundCast();

            ClearContactAndNormal();

            CalculatePlaneUpWater();

            Movement();

            hasContact = false;
        }

        private void CalculatePlaneUpWater()
        {
            if (!isSwim) return;
            Ray rayCheckPlane = new Ray(transform.position + transform.up * 2F + transform.forward, Vector3.down);
            IsPlaneUpWater = Physics.Raycast(rayCheckPlane, 1f, MasksProject.RigidObject, QueryTriggerInteraction.Ignore);
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

            if (IntroducingCaracter != null)
                direction = IntroducingCaracter.Move(ForwardTransform, out isMove);
            Move(direction, ref isMove);
            TrySpace();
            eventInput.eventMovement.Invoke(VelosityDefaultDirection, isMove);
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
        private void CalculateGroundCast()
        {
            if (_isContactsFoot)
                return;
            CapsuleCollider capsule = CharacterCollider;
            Ray directionCast = new Ray(Transform.position + Vector3.up * capsule.radius, Vector3.down);
            float newRadius = capsule.radius - 0.02F;
            isGroundedCast = Physics.SphereCast(directionCast, newRadius, 0.4F, MasksProject.RigidObject, QueryTriggerInteraction.Ignore);
        }
        private bool CheckWall(Vector3 direction)
        {
            int angle;
            if (contacts == null || _isGrounded)
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
            if (IntroducingCaracter == null || IsStopMovement)
                return;
            isRun = IntroducingCaracter.IsRun;
            bool isJump = IntroducingCaracter.Space();
            if (isMultyJump && isJump)
            {
                if (doubleJumpMax > doubleJump)
                {
                    doubleJump += Time.deltaTime;
                    if (doubleJump > doubleJumpMax)
                        doubleJump = doubleJumpMax;
                }
            }
            if (IntroducingCaracter.Space() && (isSwim || _isGrounded))
            {
                startJump = true;
            }
        }
        public void AddFuncStopMovement(Func<bool> func)
        {
            eventStopMovement.Add(func);
        }
        public void OnCollisionEnter(Collision collision)
        {
            RefreshContact(collision);
            if (_isGrounded && !isSwim)
            {
                if (collision.gameObject.TryGetComponent(out Rigidbody rigidbody) && rigidbody.mass < MAX_KICK_MASS_BODY)
                    Rigidbody.velocity.Set(Rigidbody.velocity.x, 0, Rigidbody.velocity.z);
                IMoveablePlatform platform;
                if(!IsSurfaseWall)
                    if(_isContactsFoot)
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
        bool HasMoved()
        {
            return Vector3.Distance(transform.position, lastPosition) > movementThreshold;
        }
        public void OnCollisionExit(Collision collision)
        {
            ClearNurmalSurface();
            contacts.Clear();
            if (Transform.parent == collision.transform.parent)
                Transform.SetParent(null);
        }
        private void ClearNurmalSurface()
        {
            normalSurfaces = Vector3.up;
            angleSurface = 0;
            contactsFoot.Clear();
            contactPointsAll.Clear();
            Rigidbody.useGravity = true;
            if (isFly)
            {
                eventInput[TypeAnimation.Fly]?.Invoke();
                return;
            }
        }
        private float waitSecondFall;
        private IEnumerator FallInvoke()
        {
            while (true)
            {
                retContinue:
                yield return new WaitUntil(() => !_isGrounded);
                while (waitSecondFall < MAX_TIME_FALLING)
                {
                    waitSecondFall += Time.fixedDeltaTime;
                    if(_isGrounded)
                    {
                        waitSecondFall = 0f;
                        goto retContinue;
                    }
                    yield return new WaitForFixedUpdate();
                }
                waitSecondFall = 0f;
                if (_isGrounded || isSwim)
                {
                    goto retContinue;
                }
                eventInput[TypeAnimation.Fall]?.Invoke();
            }
        }
        private Vector2 oldSizeCollider = Vector2.zero;
        public void EnterWater()
        {
            if (CanSwim == false) return;

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
            if (CanSwim == false) return;

            CapsuleCollider cap = CharacterCollider;
            if (oldSizeCollider != Vector2.zero && cap)
            {
                cap.radius = oldSizeCollider.x;
                cap.height = oldSizeCollider.y;
            }    
            eventInput[TypeAnimation.DontSwimming].Invoke();
            startJump = isSwim = false;
        }
#if UNITY_EDITOR
        #region Debug
        [SerializeField] private bool isDrawDebug;

        private void OnDrawGizmos()
        {
            if (!isDrawDebug)
                return;
            Gizmos.color = Color.red;
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
                contacts.ToList().ForEach (contact =>
                 {
                     Gizmos.DrawSphere(contact.point, 0.05F);
                 }) ;
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

    }
}

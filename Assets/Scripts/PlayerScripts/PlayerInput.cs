using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace PlayerDescription
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class PlayerInput : MonoBehaviour
    {
        private const float DividerToFootTouch = 8.5F;

        [SerializeField] private float forseJump = 4F, maxAngleMove = 60F;

        private List<Func<bool>> eventStopMovement = new();

        [SerializeField] private Vector3 normalSurfaces, velocityBody;

        private ContactPoint[] contacts;

        private bool startJump, useGravity = true;

        private int angleSurface;

        [SerializeField] private float doubleJump = 0.9f, doubleJumpMax = 1.5F;

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

        public Collider CharacterBody { get; private set; }

        [field: SerializeField] public bool isMultyJump { get; private set; }
        public Bounds BoundsCollider => CharacterBody.bounds;

        private Rigidbody _rigidbody;
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

        [field: SerializeField] public float Speed { get; private set; }
        [field: SerializeField] public float SpeedRun { get; private set; }
        [field: SerializeField] public bool isFly { get; private set; }

        private bool IsStopMovement
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

        public bool isRun { get; private set; }

        public bool _isGrounded
        {
            get
            {
                if (contacts == null || contacts.Length <= 0)
                {
                    return Physics.Raycast(new Ray(Transform.position + Vector3.up * 0.1F, Vector3.down * 0.2F), 0.2F);
                }
                return contacts.Count() > 0;
            }
        }

        [field: SerializeField] public EventInputAnimation eventInput { get; private set; } = new EventInputAnimation();

        private bool IsSurfaseUp => angleSurface == 90;

        private bool IsMaxAngleSurface
        {
            get
            {
                if (IsSurfaseUp)
                    return false;
                return angleSurface > maxAngleMove;
            }
        }

        void Awake()
        {
            Transform = transform;
            CharacterBody = GetComponent<Collider>();
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            CharacterBody.material = PhysicMaterial;
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
            Rigidbody.drag = isFly ? 9 : 0;
            Rigidbody.useGravity = !isFly;
        }
        private void Move(Vector3 direction)
        {
            if (!IsSurfaseUp)
            {
                direction = DirectionFromSrface(direction);
            }
            velocityBody = direction.normalized * (isRun ? SpeedRun : Speed);
            Rigidbody.velocity = new Vector3(velocityBody.x, useGravity ? Rigidbody.velocity.y : velocityBody.y, velocityBody.z);
        }
        private bool IsDirectionInverseNormal(Vector3 direction)
        {
            return Vector3.Dot(direction, normalSurfaces) > 0;
        }
        private Vector3 DirectionFromSrface(Vector3 direction)
        {
            Vector3 localNormalSurfase = normalSurfaces;
            return direction - Vector3.Dot(direction, localNormalSurfase) * localNormalSurfase;
        }
        private void TryJump()
        {
            if (IsMaxAngleSurface)
                startJump = false;
            if (_isGrounded && startJump)
            {
                ClearNurmalSurface();
                Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, forseJump * (isMultyJump ? doubleJump : 1F), Rigidbody.velocity.z);
                startJump = false;
                doubleJump = 0.9f;
                eventInput[TypeAnimation.Jump]?.Invoke();
            }
        }

        public void FixedUpdate()
        {
            if (IsStopMovement)
            {
                Rigidbody.velocity = new Vector3(0, useGravity ? Rigidbody.velocity.y : 0, 0);
                eventInput.eventMovement?.Invoke(Vector3.zero, false);
                return;
            }
            Vector3 direction = MovementMode.WASD(ForwardTransform, Speed, out bool isMove, true);
            if (IsMaxAngleSurface && !IsDirectionInverseNormal(direction))
            {
                isMove = false;
                direction = Vector3.zero;
            }
            Move(direction);
            if (isFly)
            {
                MovementMode.MovementFlySpaseLSift(Rigidbody, Speed, _isGrounded);
            }
            TryJump();
            eventInput.eventMovement.Invoke(velocityBody, isMove);
        }

        public void Update()
        {
            isRun = Input.GetKey(KeyCode.LeftShift);
            eventInput[TypeAnimation.Landing]?.Invoke();
            if (IsStopMovement)
                return;
            if (isMultyJump && Input.GetKey(KeyCode.Space))
            {
                if (doubleJumpMax > doubleJump)
                {
                    doubleJump += Time.deltaTime;
                    if (doubleJump > doubleJumpMax)
                        doubleJump = doubleJumpMax;
                }
            }
            if ((isMultyJump ? Input.GetKeyUp(KeyCode.Space) : Input.GetKeyDown(KeyCode.Space)) && _isGrounded)
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
            if (_isGrounded)
            {
                if (collision.gameObject.TryGetComponent(out Rigidbody rigidbody) && rigidbody.mass < 5F)
                    Rigidbody.velocity.Set(Rigidbody.velocity.x, 0, Rigidbody.velocity.z);
                IMoveablePlatform platform;
                if ((platform = collision.gameObject.GetComponent<IMoveablePlatform>()) != null)
                {
                    this.transform.SetParent(platform.Guide);
                }
                useGravity = false;
            }
        }
        public void OnCollisionStay(Collision collision)
        {
            RefreshContact(collision);
        }
        private void RefreshContact(Collision collision)
        {
            contacts = collision.contacts.Where((p) => transform.InverseTransformPoint(p.point).y < (BoundsCollider.max.y - BoundsCollider.min.y) / DividerToFootTouch).ToArray();
            if (!_isGrounded)
            {
                ClearNurmalSurface();
                return;
            }
            if (collision.gameObject.TryGetComponent(out Rigidbody entity))
                return;
            if (contacts.Length > 0)
                normalSurfaces = contacts[0].normal;
            else
            {
                normalSurfaces = Vector3.up;
                return;
            }
            foreach (var contact in contacts)
            {
                if (Vector3.Angle(Vector3.up, contact.normal) < Vector3.Angle(Vector3.up, normalSurfaces))
                {
                    normalSurfaces = contact.normal;
                }
            }
            angleSurface = (int)Vector3.Angle(Vector3.up, normalSurfaces);
        }
        public void OnCollisionExit(Collision collision)
        {
            if (_isGrounded)
            {
                ClearNurmalSurface();
            }
            if (Transform.parent == collision.transform.parent)
                Transform.SetParent(null);
        }
        private void ClearNurmalSurface()
        {
            normalSurfaces = Vector3.up;
            contacts = null;
            useGravity = true;
            if (_isGrounded) return;
            eventInput[TypeAnimation.Fall]?.Invoke();
        }

#if UNITY_EDITOR
        #region Debug
        [SerializeField] private bool isDrawDebug;
        private void OnDrawGizmos()
        {
            if (!isDrawDebug)
                return;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, velocityBody.normalized);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, normalSurfaces.normalized);
            Gizmos.color = Color.green;
            if (!CharacterBody)
                return;
            Gizmos.DrawSphere(transform.position + Vector3.up * (CharacterBody.bounds.max.y - CharacterBody.bounds.min.y) / DividerToFootTouch, 0.1F);
            if (contacts == null || contacts.Length < 0)
                return;
            Gizmos.color = Color.magenta;
            contacts.ToList().ForEach(contact =>
            {
                Gizmos.DrawSphere(contact.point, 0.1F);
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

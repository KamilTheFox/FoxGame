using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerInput : MonoBehaviour
{
    
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float SpeedRun { get; private set; }

    [field: SerializeField] public bool isFly { get; private set; }

    [SerializeField] private float ForseJump = 4F, MaxAngleMove = 60F;

    private static PhysicMaterial physicMaterial;
    public static PhysicMaterial PhysicMaterial
    { 
        get
        {
            if(!physicMaterial)
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

    private List<Func<bool>> EventStopMovement = new();

    public void AddFuncStopMovement(Func<bool> func)
    {
        EventStopMovement.Add(func);
    }

    private bool StopMovement()
    {
        bool flag = false;
        foreach (var e in EventStopMovement)
        {
            if (e())
                flag = true;
        }
        return flag;
    }

    [SerializeField] private Vector3 normalSurfaces, debugVelosity;

    public Vector3 Velosity => debugVelosity;

    private ContactPoint[] contacts;

    private bool GoJump, isJumping, useGravity = true;

    [SerializeField] private float doubleJump = 0.9f, doubleJumpMax = 1.5F;

    public bool isRun { get; private set; }

    public bool _isGrounded
    {
        get
        {
            if (contacts == null || contacts.Length <= 0)
            {
                return Physics.Raycast(new Ray(Transform.position + Vector3.up * 0.1F, Vector3.down * 0.2F),0.2F);
            }
            return contacts.Count() > 0;
        }
    }
    [field: SerializeField] public EventInputAnimation eventInput { get; private set; } = new EventInputAnimation();

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
    private void Moving()
    {
        Vector3 velosity;
        float angleSurface = Vector3.Angle(Vector3.up, normalSurfaces);

        bool isTrueMaxAngleMove = angleSurface > MaxAngleMove;
        if ((int)angleSurface == 90)
            isTrueMaxAngleMove = false;

        velosity = MovementMode.WASD(ForwardTransform, Speed, out bool isMove, true);

        velosity = velosity.normalized;
        bool isMoveDiretionInverseNormal = Vector3.Dot(velosity, normalSurfaces) > 0;
        bool jumpAnim = false;
        if (isTrueMaxAngleMove)
            GoJump = false;
        else if (_isGrounded && GoJump)
        {
            ClearNurmalSurface();
            Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, ForseJump * (isMultyJump ? doubleJump : 1F), Rigidbody.velocity.z);
            isJumping = true;
            GoJump = false;
            doubleJump = 0.9f;
            jumpAnim = true;
        }
        if (isJumping && Rigidbody.velocity.y < 0)
        {
            isJumping = false;
        }
        if (isMove && isTrueMaxAngleMove && !isMoveDiretionInverseNormal)
        {
            velosity = Vector3.zero;
            isMove = false;
        }
        else
        {
            Vector3 localNormalSurfase = normalSurfaces;
            if ((int)angleSurface != 90)
                velosity = velosity - Vector3.Dot(velosity, localNormalSurfase) * localNormalSurfase;
            velosity = velosity * (isRun ? SpeedRun : Speed);
        }
        Rigidbody.velocity = new Vector3(velosity.x, useGravity ? Rigidbody.velocity.y : velosity.y,velosity.z);
        if (isFly)
        {
            MovementMode.MovementFlySpaseLSift(Rigidbody, Speed, _isGrounded);
        }
        eventInput.eventMovement?.Invoke(velosity.normalized, isMove);
        if(jumpAnim)
            eventInput[TypeAnimation.Jump]?.Invoke();
        debugVelosity = velosity;
    }
        
        
    public void FixedUpdate()
    {
        if (StopMovement())
        {
            Rigidbody.velocity = new Vector3(0, useGravity ? Rigidbody.velocity.y : 0, 0);
            eventInput.eventMovement?.Invoke(Vector3.zero, false);
            return;
        }
        Moving();
    }
    
    public void Update()
    {
        isRun = Input.GetKey(KeyCode.LeftShift);
        if (StopMovement())
            return;
        eventInput[TypeAnimation.Landing]?.Invoke();
        if (isMultyJump && Input.GetKey(KeyCode.Space))
        {
            if (doubleJumpMax > doubleJump)
            {
                doubleJump += Time.deltaTime;
                if(doubleJump > doubleJumpMax)
                    doubleJump = doubleJumpMax;
            }
        }
        if ((isMultyJump ? Input.GetKeyUp(KeyCode.Space) : Input.GetKeyDown(KeyCode.Space)) &&  _isGrounded)
        {
            GoJump = true;
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        RefreshContact(collision);
        if (_isGrounded)
        {
            if (collision.gameObject.TryGetComponent(out Rigidbody rigidbody) && rigidbody.mass < 5F)
                Rigidbody.velocity.Set(Rigidbody.velocity.x,0, Rigidbody.velocity.z);
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
        if (StopMovement())
            return;
        contacts = collision.contacts.Where((p) => transform.InverseTransformPoint(p.point).y < (BoundsCollider.max.y - BoundsCollider.min.y) / 8.5F).ToArray();
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
        if (!isJumping && !_isGrounded)
            eventInput[TypeAnimation.Fall]?.Invoke();
    }

#if UNITY_EDITOR
    [SerializeField] private bool isDrawDebug;
    private void OnDrawGizmos()
    {
        if (!isDrawDebug)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, debugVelosity.normalized);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, normalSurfaces.normalized);
        Gizmos.color = Color.green;
        if (!CharacterBody)
            return;
        Gizmos.DrawSphere(transform.position + Vector3.up * (CharacterBody.bounds.max.y - CharacterBody.bounds.min.y) / 8.5F, 0.1F);
        if (contacts == null || contacts.Length < 0)
            return;
        Gizmos.color = Color.magenta;
        contacts.ToList().ForEach(contact =>
        {
                Gizmos.DrawSphere(contact.point, 0.1F);
        });
    }
#endif
    [Serializable]
    public class EventInputAnimation
    {
        public UnityEvent<Vector3,bool> eventMovement = new UnityEvent<Vector3, bool>();
        public event UnityAction<Vector3, bool> EventMovement
        {
            add
            {
                eventMovement.AddListener(value);
            }
            remove { eventMovement.RemoveListener(value);}
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

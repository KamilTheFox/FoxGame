using UnityEngine;
using GroupMenu;
using CameraScripts;
using System.Linq;
public class CameraControll : MonoBehaviour
{
    public PlayerBody CPlayerBody { get; private set; }

    [SerializeField]
    GameObject PrefabPlayerBody;
    public static Camera MainCamera { get; private set; }

    public static AudioSource CameraSource { get; private set;}
    public float DistanseRay => viewedCamera.DistanceView;

    public Transform Transform { get; private set; }

    public static CameraControll instance { get; private set; }

    public static Ray RayCastCenterScreen
    {
        get
        {
            return MainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }
    }
    public static float SensetiveMouse => Settings.SensetiveMouse;

    [SerializeField]
    float MouseVertical, MouseHorizontal;
    public Vector3 EulerVertical { get; private set; }
    public Vector3  EulerHorizontal { get; private set; }

    [SerializeField]
    private bool isCameraMove;

    private IViewedCamera viewedCamera;

    private Transform[] viewedCameraPositions;

    CoiceSkin coiceSkin = new();
    class CoiceSkin
    {
        public Color OldColor;
        public Color NewColor { get { return Color.red; } }
        public Material material;
        public void ReplacementColor(Material material)
        {
            if (this.material == null)
            {
                this.material = material;
                OldColor = material.color;
                this.material.color = NewColor;
            }
        }
        public void SetOldColor()
        {
            if (material != null)
            {
                material.color = OldColor;
                material = null;
            }
        }
    }
    void Awake()
    {
        if (instance) 
        {
            Debug.LogError("You cannot create 2 camera controllers");
            Destroy(gameObject);
        }
        instance = this;
        Transform = transform;
        MainCamera = gameObject.GetComponent<Camera>();
        MouseHorizontal = 90;
        CameraSource = GetComponent<AudioSource>();
        CPlayerBody = GetComponentInParent<PlayerBody>();
        if (CPlayerBody)
        {
            EntranceBody(CPlayerBody.gameObject);
            return;
        }
        OnFreeCamera();
        if (isCameraMove && !GameState.IsCreative)
            GiveBody();
       
    }
    private void OnDestroy()
    {
        instance = null;
    }
    void ChoiceSkinPlayer()
    {
        if (Physics.Raycast(RayCastCenterScreen, out RaycastHit raycast, 3F, 1 << LayerMask.NameToLayer("Player")))
        {
            GameObject game = raycast.collider.gameObject;
            Renderer renderer = game.GetComponentInChildren<Renderer>();
            if (renderer)
            {
                Material material = renderer.material;
                if (material != coiceSkin.material)
                {
                    coiceSkin.SetOldColor();
                }
                coiceSkin.ReplacementColor(material);
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    coiceSkin.SetOldColor();
                    EntranceBody(game);
                }
            }
        }
        else
        {
            coiceSkin.SetOldColor();
        }
    }
    private static bool IsCanInstall(ref GameObject gameObject)
    {
        PlayerBody _PlayerControll = gameObject.GetComponent<PlayerBody>();
        if (_PlayerControll == null)
        {
            _PlayerControll = gameObject.transform.GetComponentInParent<PlayerBody>();
            if (_PlayerControll == null)
                return false;
            gameObject = gameObject.transform.parent.gameObject;
        }
        if (_PlayerControll.IsDie)
        {
            return false;
        }
        return true;

    }
    public void OnFirstPerson()
    {
        ChangeViewPerson(new FirstPerson(this, viewedCameraPositions.ToList().Find((t) => t.name.ToLower().Contains("firstperson"))));
    }
    public void OnThirdPerson()
    {
        ChangeViewPerson(new ThirdPerson(this, viewedCameraPositions.ToList().Find((t) => t.name.ToLower().Contains("thirdperson"))));
    }
    private void OnFreeCamera()
    {
        ChangeViewPerson(new FreeCamera(this));
    }
    
    public void ChangeViewPerson(IViewedCamera viewed)
    {
        viewedCamera?.Dispose();
        viewedCamera = viewed;
        viewedCamera.Construct();
        if (MouseVertical < viewedCamera.ViewAxisMaxVertical.x) 
            MouseVertical = viewedCamera.ViewAxisMaxVertical.x;
        if (MouseVertical > viewedCamera.ViewAxisMaxVertical.y)
            MouseVertical = viewedCamera.ViewAxisMaxVertical.y;

        if (MouseHorizontal >= viewedCamera.ViewAxisMaxHorizontal.x) 
            MouseHorizontal = viewedCamera.ViewAxisMaxHorizontal.x;
        if(MouseHorizontal <= viewedCamera.ViewAxisMaxHorizontal.y)
            MouseHorizontal = viewedCamera.ViewAxisMaxHorizontal.y;
    }
    private void CameraMove()
    {
        float mouseMove = Input.GetAxis("Mouse Y") * SensetiveMouse * 0.3F;
        MouseVertical += mouseMove;
        Vector2 maxVertical = viewedCamera.ViewAxisMaxVertical;
        if (MouseVertical < maxVertical.x || MouseVertical > maxVertical.y) MouseVertical -= mouseMove;

        MouseHorizontal += Input.GetAxis("Mouse X") * SensetiveMouse * 0.3F;
        Vector2 maxHorizontal = viewedCamera.ViewAxisMaxHorizontal;
        if (MouseHorizontal >= maxHorizontal.x || MouseHorizontal <= maxHorizontal.y) MouseHorizontal -= MouseHorizontal;

        EulerVertical = Vector3.left * MouseVertical;
        EulerHorizontal = Vector3.up * MouseHorizontal;

        viewedCamera.ViewAxis(transform, EulerVertical + EulerHorizontal);
    }
    public void EntranceBody(GameObject @object)
    {
        if (!IsCanInstall(ref @object))
        {
            MessageBox.Info("Объект Мертв");
            return;
        }
        if (CPlayerBody)
            ExitBody();
        CPlayerBody = @object.GetComponent<PlayerBody>();
        CPlayerBody.EntrancePlayerControll(instance);
        viewedCameraPositions = 
        CPlayerBody.transform.GetComponentsInChildren<Transform>().Where(ViewT => ViewT.name.ToLower().Contains("person")).ToArray();
        OnFirstPerson();
    }
    public void ExitBody()
    {
        CPlayerBody?.ExitPlayerControll(instance);
        CPlayerBody = null;
        OnFreeCamera();
    }
    void CameraNoClip()
    {
        MovementMode.MovementWASD(transform, 10F);
    }
    public void GiveBody()
    {
        if (IsPlayerControll())
        {
            GameObject body = Instantiate(PrefabPlayerBody, transform.localPosition, Quaternion.Euler(EulerHorizontal));
            EntranceBody(body);
        }
    }
    public bool IsPlayerControll(PlayerBody playerControll = null)
    {
        return CPlayerBody == playerControll;
    }
    void LateUpdate()
    {
        if (Menu.IsEnabled || !isCameraMove)
            return;

        CameraMove();
        if(CPlayerBody != null && viewedCamera != null)
            CPlayerBody.Rigidbody.MoveRotation(Quaternion.Euler(viewedCamera.RotateBody()));

        if (!GameState.IsCreative) 
            return;

        if (IsPlayerControll())
        {
            CameraNoClip();
            ChoiceSkinPlayer();
            return;
        }
        if ( Input.GetKeyDown(KeyCode.P))
        {
            ExitBody();
        }
    }
    private class FreeCamera : IViewedCamera
    {
        CameraControll cameraControll;
        public FreeCamera(CameraControll camera)
        {
            cameraControll = camera;
        }
        public Vector2 ViewAxisMaxVertical => new Vector2(-90, 90);
        public Vector3 RotateBody()
        {
            return cameraControll.EulerHorizontal;
        }
        public void ViewAxis(Transform camera, Vector3 euler)
        {
            camera.localEulerAngles = euler;
        }
        public void Dispose()
        {

        }

        public void Construct()
        {
            cameraControll.Transform.parent = null;
        }
    }
}

using UnityEngine;
using GroupMenu;
using System.Collections.Generic;
using CameraScripts;
using System.Linq;
using PlayerDescription;
using System;

public class CameraControll : MonoBehaviour , ICameraCastObserver
{
    public CharacterMediator Player { get; private set; }

    [SerializeField]
    private GameObject[] PrefabsPlayerBody;
    public static Camera MainCamera { get; private set; }

    public static AudioSource CameraSource { get; private set;}
    public float DistanseRay => viewedCamera.DistanceView;

    public Transform Transform { get; private set; }

    public static CameraControll Instance { get; private set; }

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

    [SerializeField] private Transform[] viewedCameraPositions;

    private List<ICameraCastSubscriber> castSubscribersCast = new List<ICameraCastSubscriber>();

    void Awake()
    {
        if (Instance) 
        {
            Debug.LogError("You cannot create 2 camera controllers");
            Destroy(gameObject);
        }
        Instance = this;
        Transform = transform;
        MainCamera = gameObject.GetComponent<Camera>();
        MouseHorizontal = 90;
        CameraSource = GetComponent<AudioSource>();
        Player = GetComponentInParent<CharacterMediator>();
    }
    private void OnEnable()
    {
        if (Player)
        {
            EntranceBody(Player.gameObject);
            return;
        }
        if (isCameraMove)
            OnFreeCamera();
        if (isCameraMove && !GameState.IsCreative)
            GiveBody();
    }
    private void OnDestroy()
    {
        Instance = null;
    }
    void ChoiceSkinPlayer()
    {
        if (Physics.Raycast(RayCastCenterScreen, out RaycastHit raycast, 3F, 1 << LayerMask.NameToLayer("Player")))
        {
            GameObject game = raycast.collider.gameObject;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                EntranceBody(game);
            }
        }
    }
    private static bool IsCanInstall(ref GameObject gameObject)
    {
        CharacterBody _PlayerControll = gameObject.GetComponent<CharacterBody>();
        if (_PlayerControll == null)
        {
            _PlayerControll = gameObject.transform.GetComponentInParent<CharacterBody>();
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
        Transform position = viewedCameraPositions.ToList().Find((t) => t.name.ToLower().Contains("firstperson"));
        if(position == null)
        {
            OnThirdPerson();
            return;
        }
        ChangeViewPerson(position.gameObject.GetComponent<FirstPerson>());
    }
    public void OnThirdPerson()
    {
        Transform transform =  viewedCameraPositions.ToList().Find((t) => t.name.ToLower().Contains("thirdperson"));
        if (transform != null)
            ChangeViewPerson(new ThirdPerson(this, transform));

    }
    public void OnThirdUnlookPerson()
    {
        if (Player != null)
        {
            ChangeViewPerson(new ThirdUnlookPerson(this, Player, MouseHorizontal));
        }
    }
    private void OnFreeCamera()
    {
        ChangeViewPerson(new FreeCamera(this));
        None.EnableAim(false);
    }
    public bool IsTypeViewPerson(Type type)
    {
        return viewedCamera.GetType().Name == type.Name;
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
    private void AxisView()
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
        if (Player)
            ExitBody();
        Player = @object.GetComponent<CharacterMediator>();
        Player.SetCameraController(Instance);
        Player.Body.EntrancePlayerControll(Instance);
        viewedCameraPositions =
        Player.transform.GetComponentsInChildren<Transform>().Where(ViewT => ViewT.name.ToLower().Contains("person")).ToArray();
        None.EnableAim(true);
        OnFirstPerson();
    }
    public void ExitBody()
    {
        Player?.Body?.ExitPlayerControll(Instance);
        Player.SetCameraController(null);
        viewedCameraPositions = new Transform[0];
        Player = null;
        OnFreeCamera();
    }
    void CameraNoClip()
    {
        MovementMode.MovementWASD(transform, 10F);
    }
    public void GiveBody(int indexBody = 0)
    {
        if (IsPlayerControll())
        {
            GameObject body = Instantiate(PrefabsPlayerBody[indexBody], transform.localPosition, Quaternion.Euler(EulerHorizontal));
            EntranceBody(body);
        }
    }
    public bool IsPlayerControll(CharacterBody playerControll = null)
    {
        if (Player == null)
            return true;
        return Player.Body == playerControll;
    }

    private void OnGUI()
    {
        viewedCamera?.OnGUI();
    }

    private void Update()
    {
        Ray ray = CameraControll.RayCastCenterScreen;
        if(Physics.Raycast(ray,out RaycastHit hit))
            foreach(var subscribe in castSubscribersCast)
            {
                subscribe.OnCameraCasting(hit);
            }
    }
    void LateUpdate()
    {
        if (Menu.IsEnabled || !isCameraMove)
            return;

        AxisView();
        if(Player != null && viewedCamera != null)
            Player.Body.RotateBody(Quaternion.Euler(viewedCamera.RotateBody()));

        if (!GameState.IsCreative) 
            return;

        if (Player == null)
        {
            CameraNoClip();
            ChoiceSkinPlayer();
            return;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ExitBody();
        }
    }

    public void AddSubscribeToCast(params ICameraCastSubscriber[] cameraCastSubscriber)
    {
        castSubscribersCast.AddRange(cameraCastSubscriber);
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

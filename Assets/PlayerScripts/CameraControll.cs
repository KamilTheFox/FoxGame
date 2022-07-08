using UnityEngine;

public class CameraControll : MonoBehaviour
{

    public PlayerControll PlayerControll { get; private set; }

    [SerializeField]
    GameObject PrefabPlayerBody;
    public static Camera MainCamera { get; private set; }
    public float DistanseRay { get; private set; }

    public Transform Transform { get; private set; }

    public static CameraControll instance { get; private set; }

    public static Ray RayCastCenterScreen
    {
        get
        {
            return MainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }
    }
    public float SensetiveMouse { get; set; }
    [SerializeField]
    float MouseVertical, MouseHorizontal;
    public Vector3 EulerVertical { get; private set; }
    public Vector3  EulerHorizontal { get; private set; }

    [SerializeField]
    bool StartMenu;

    [SerializeField]
    bool isCameraMove;

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
    // Start is called before the first frame update
    void Start()
    {
        Transform = transform;
        DistanseRay = 3F;
        MainCamera = FindObjectOfType<Camera>();
        instance = this;
        MouseHorizontal = 90;
        SensetiveMouse = 25F;
        if (StartMenu)
            DontDestroyOnLoad(new GameObject().AddComponent<Console>());
    }

    void ChoiceSkinPlayer()
    {
        if (Physics.Raycast(RayCastCenterScreen, out RaycastHit raycast, 3F, 1 << LayerMask.NameToLayer("Player")))
        {
            GameObject game = raycast.collider.gameObject;
            Renderer renderer = game.GetComponent<Renderer>();
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
    private bool IsCanInstall(ref GameObject gameObject)
    {
        PlayerControll _PlayerControll = gameObject.GetComponent<PlayerControll>();
        if (_PlayerControll == null)
        {
            _PlayerControll = gameObject.transform.parent.gameObject.GetComponent<PlayerControll>();
            if (_PlayerControll == null)
                return false;
            gameObject = gameObject.transform.parent.gameObject;
        }
        if (_PlayerControll.isDead)
        {
            return false;
        }
        return true;

    }
    public void EntranceBody(GameObject @object)
    {
        if (!IsCanInstall(ref @object))
        {
            Console.MessageShow("Объект Мертв");
            return;
        }
        if (PlayerControll)
            ExitBody();
        PlayerControll = @object.GetComponent<PlayerControll>();
        PlayerControll.EntrancePlayerControll(instance);
    }
    public void ExitBody()
    {
        PlayerControll?.ExitPlayerControll(instance);
        PlayerControll = null;
    }
    void CameraMove()
    {
        float newMouseVertical = Input.GetAxis("Mouse Y") * SensetiveMouse * 0.3F;
        MouseVertical += newMouseVertical;
        if (MouseVertical > 90 || MouseVertical < -90) MouseVertical -= newMouseVertical;

        MouseHorizontal += Input.GetAxis("Mouse X") * SensetiveMouse * 0.3F;
        if (MouseHorizontal >= 360 || MouseHorizontal <= -360) MouseHorizontal -= MouseHorizontal;

        EulerVertical = Vector3.left * MouseVertical;
        EulerHorizontal = Vector3.up * MouseHorizontal;

        if (IsPlayerControll())
            transform.localEulerAngles = EulerVertical + EulerHorizontal;
        else
            transform.localEulerAngles = Vector3.left * MouseVertical;
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
    public bool IsPlayerControll(PlayerControll playerControll = null)
    {
        return PlayerControll == playerControll;
    }
    void Update()
    {
        if (Console.IsEnabled() || !isCameraMove)
            return;
        CameraMove();
        if (IsPlayerControll())
        {
            CameraNoClip();
            ChoiceSkinPlayer();
            return;
        }
        if (Input.GetKeyDown(KeyCode.P))//Временно. предрелизная
        {
            ExitBody();
        }
    }
}

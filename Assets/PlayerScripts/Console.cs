using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GroupMenu;
using UnityEngine;
    public class Console : MonoBehaviour
    {
    public static Console instance;
    CameraControll cameraControll;
    private void Start()
    {
        instance = this;
        cameraControll = FindObjectOfType<CameraControll>();
    }
    bool isEnabled = true;
    public TypeItem ViewingItem = TypeItem.None;
    public bool isViewingItem;
    public bool isStaticItem;


    public TypeItem addItemType = TypeItem.Apple;
    Vector2 BeginScroll;

    float Speed = 1F;

    SoundMeneger.Sounds Sounds = SoundMeneger.Sounds.Explosion;
    Vector2 BeginScrollSounds;
    bool SoundTesting;
    private void OnGUI()
    {
        GUI.color = new Color(0.9F, 0.4F, 0F);
        if (!isEnabled)
            goto Game;
        if(isMessage)
        {
            GUILayout.Window(2, new Rect(Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 50), delegate (int i)
                   {
                       GUILayout.Label(Message);
                       GUI.color = new Color(0.9F, 0.4F, 0F);
                       GUILayout.BeginHorizontal();
                       Action ClearMessage = () =>
                       {
                           ClickOK = null;
                           isMessage = isEnabled = false;
                           CursorView(false);
                           Message = "";
                       };
                       if (GUILayout.Button("Ok",new GUILayoutOption[]
                       {
                           GUILayout.Width(50F)
                       }))
                       {
                           ClickOK?.Invoke();
                           ClearMessage();
                       }
                       if (GUILayout.Button("Clance", new GUILayoutOption[]
                       {
                           GUILayout.Width(50F)
                       }))
                       {
                           ClearMessage();
                       }
                       GUILayout.EndHorizontal();
       
                   },"Message");
            return;
        }
        GUILayout.Window(0, new Rect(10, 10, 200, 100), delegate (int id)
        {
            GUI.color = new Color(0.9F, 0.4F, 0F);
            GUILayout.Label("Mouse Sensetive: "+cameraControll.SensetiveMouse.ToString("0"));
            cameraControll.SensetiveMouse = GUILayout.HorizontalScrollbar(cameraControll.SensetiveMouse, 10F, 0F, 110F);
            GUILayout.Label("Volume Sound: " + (SoundMeneger.Volume * 100).ToString("0"));
            SoundMeneger.Volume = GUILayout.HorizontalScrollbar(SoundMeneger.Volume, 0.1F, 0F, 1.1F);
            BeginScroll = GUILayout.BeginScrollView(BeginScroll,new GUILayoutOption[] {GUILayout.Height(100F) });
            addItemType = (TypeItem)GUILayout.SelectionGrid((int)addItemType, Enum.GetNames(typeof(TypeItem)), 1);
            GUILayout.EndScrollView();
            if (GUILayout.Button("Add Item"))
            {
                ItemEngine.AddItem(addItemType, Vector3.zero, Quaternion.identity, isStaticItem);
            }
            if (GUILayout.Button("Add Plant"))
            {
                PlantEngine.AddPlant(TypePlant.Tree_1 , Vector3.zero, Quaternion.identity);
            }
            if (GUILayout.Button("Give Item"))
            {
                PlayerControll player = cameraControll.PlayerControll;
                if (player)
                {
                    player.GiveItem(ItemEngine.AddItem(addItemType, Vector3.zero, Quaternion.identity, isStaticItem));
                }
            }
            if (GUILayout.Button("Add Item X20"))
            {
                for(int i = 0; i<20;i++)
                ItemEngine.AddItem(addItemType, Vector3.up * (float)i, Quaternion.identity, isStaticItem);
            }
            isStaticItem = GUILayout.Toggle(isStaticItem, "isStatic");
            if (GUILayout.Button("Remove Item"))
            {
                ItemEngine.RemoveItemAt(ItemEngine.GetItems.Length-1);
            }
            if (GUILayout.Button("Clear All Item"))
            {
                ItemEngine.RemoveItemAll();
            }
            if (CameraControll.instance.PlayerControll == null && GUILayout.Button("Add Player"))
            {
                CameraControll.instance.GiveBody();
            }
            GUILayout.Box("Count Item: " + ItemEngine.GetItems.Length.ToString());
            SoundTesting = GUILayout.Toggle(SoundTesting, "SoundStend");
            if (SoundTesting)
            {
                int CountTypeSound = SoundMeneger.CountSoundType <= 4 ? SoundMeneger.CountSoundType : 4;
                BeginScrollSounds = GUILayout.BeginScrollView(BeginScrollSounds, new GUILayoutOption[]
                {
                GUILayout.Height((float)(25*CountTypeSound))
                });
                Sounds = (SoundMeneger.Sounds)GUILayout.SelectionGrid((int)Sounds, Enum.GetNames(typeof(SoundMeneger.Sounds)), 1);
                GUILayout.EndScrollView();
                if (GUILayout.Button("Test Sound"))
                {
                    SoundMeneger.Play(Sounds);
                }
            }
            if (cameraControll.PlayerControll?.Rigidbody)
            if (GUILayout.Button("Fly " + (!cameraControll.PlayerControll?.Rigidbody.useGravity).ToString()))
            {
                if (cameraControll.PlayerControll != null)
                    cameraControll.PlayerControll.Fly();
            }
            Speed = GUILayout.HorizontalScrollbar(Speed, 1F, 1F, 4F);
            if (GUILayout.Button("Set Speed: " + Speed.ToString("0.0")))
            {
                if (cameraControll.PlayerControll != null)
                    cameraControll.PlayerControll.SetSpeed(Speed);
            }
            if (GUILayout.Button("ExitGame"))
            {
                Application.Quit();
            }
        }
        , "Settings");
    Game:
        if (ViewingItem != TypeItem.None)
        {
            GUI.Label(new Rect(Screen.width / 2 - 20, Screen.height / 2 - 10, 100, 100), ViewingItem.ToString());
        }
    }
    bool isMessage;
    string Message;
    Action ClickOK;
    
    public static void MessageShow(string message, Action _ClickOK = null)
    {
        SoundMeneger.Play(SoundMeneger.Sounds.Note);
        instance.isMessage = instance.isEnabled = true;
        instance.CursorView(true);
        instance.Message = message;
        instance.ClickOK = _ClickOK;
    }
    public static void ErrorShow(string message)
    {
        MessageShow("Error: " + message);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Y))
        {
            Menu.ActivateMenu(new MessageBox($"Это текст тупо",MessageBox.MessageIcon.None));
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            Menu.ActivateMenu(new MessageBox($"Это предупреждение", MessageBox.MessageIcon.Warning));
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            Menu.ActivateMenu(new MessageBox($"Это ошибка", MessageBox.MessageIcon.Error));
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            Menu.ActivateMenu(new MessageBox($"Это Информация", MessageBox.MessageIcon.Info));
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Menu.ActivateMenu(new Lobby());
        }
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            isEnabled = !isEnabled;
            CursorView(isEnabled);
        }
    }
    private void CursorView(bool isEnabled)
    {
        Cursor.lockState = isEnabled ? CursorLockMode.None : CursorLockMode.Locked;
    }
    public static bool IsEnabled()
        {
            if(instance==null)
            return false;
        return Cursor.lockState == CursorLockMode.None;
        }
    }

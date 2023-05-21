using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;


public class TimerDetonator : MonoBehaviour
{
    private GameButton Button_1;
    private GameButton Button_2;
    private GameButton Button_Reset;

    private Detonator detonator;

    private TMP_Text textBox, TextButtonReset;

    private AudioSource audioSource;

    private int startTime;

    public int StartTime { 
        get 
        { return startTime;
        } 
        private set 
        {
            int CurrentCeconds = value;
            if (CurrentCeconds > MaxTime)
            {
                CurrentCeconds = CurrentCeconds - MaxTime;
                if(CurrentCeconds < MinTime)
                CurrentCeconds = MinTime;
            }
            else
            if (CurrentCeconds < MinTime)
                CurrentCeconds = MaxTime;
            startTime = CurrentCeconds;
            UpdateTimeBox();
        } }

    private DateTime FixedStartTime;

    private const int MaxTime = 600;
    private const int MinTime = 3;

    public void Initialized(Detonator _detonator)
    {
        detonator = _detonator;
        detonator.onInteractionDetonator += () => 
        {
            FixedStartTime = DateTime.Now.AddSeconds(StartTime);
            StartCoroutine(UpdateTick());
        };
    }
    private static void SetUIOfTransformTimer(Transform UIRect)
    {
        Renderer rendererRect = UIRect.gameObject.GetComponent<Renderer>();
        RectTransform Rect = GameObject.Instantiate(Resources.Load<RectTransform>($"Item\\UI\\DetonatorTimer\\{UIRect.name}"), UIRect);
        Rect.GetComponent<Canvas>().worldCamera = CameraControll.MainCamera;
        Bounds bounds = rendererRect.bounds;
        if(rendererRect.name.Contains("Button"))
        rendererRect.enabled = false;
        Rect.localPosition = Vector3.up * -0.001F;
        Rect.localRotation = Quaternion.Euler(new Vector3(-90F, 180F, 0F));
        Rect.sizeDelta = new Vector2(bounds.size.x * 2, bounds.size.y * 2);
    }
    private IEnumerator UpdateTick()
    {
        TimeSpan previos = FixedStartTime - DateTime.Now;
        while (detonator.isActivate)
        {
            yield return null;
            if(Menu.Pause)
            yield return new WaitUntil(() =>
            {
                if(!Menu.Pause)
                {
                    FixedStartTime = DateTime.Now.AddTicks(previos.Ticks);
                }
                return !Menu.Pause;
            });
            TimeSpan span = FixedStartTime - DateTime.Now;
            if (previos.Seconds != span.Seconds)
            {
                SoundMeneger.Play(SoundMeneger.Sounds.Tick, audioSource);
                previos = span;
            }
            SetTimeToText(span);
        }
        yield break;
    }
    private void SetTimeToText(TimeSpan span)
    {
        textBox.text = span.ToString("mm\\:ss\\:ff");
    }
    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        Button_1 = transform.Find("Button_1").gameObject.AddComponent<GameButton>();
        SetUIOfTransformTimer(Button_1.transform);
        Button_2 = transform.Find("Button_2").gameObject.AddComponent<GameButton>();
        SetUIOfTransformTimer(Button_2.transform);
        Button_Reset = transform.Find("Button_Reset").gameObject.AddComponent<GameButton>();
        SetUIOfTransformTimer(Button_Reset.transform);

        TextButtonReset = Button_Reset.transform.GetComponentInChildren<TMP_Text>();

        Settings.ChangeLanguageEvent.AddListener(() =>
        {
            TextButtonReset
            .text = LText.Reset.GetTextUI().ToString();
        });

        Transform Dial = transform.Find("Dial");
        SetUIOfTransformTimer(Dial);
        textBox = Dial.GetComponentInChildren<TMP_Text>();

        Button_1.OnClick += () => IncreaseTime(1);
        Button_2.OnClick += () => ReduceTime(1);
        Button_1.OnDubleClick += () => IncreaseTime(60);
        Button_2.OnDubleClick += () => ReduceTime(60);
        Button_Reset.OnClick += ClickReset;
        Button_Reset.OnTripleClick += () =>
        {
            TimeSpan previos = FixedStartTime - DateTime.Now;
            int Second = (int)previos.TotalSeconds;
            if (Second < MinTime)
                Second = MinTime;
            StartTime = Second;
            StopAllCoroutines();
            detonator.Diactivate();
            detonator.CancelInvoke();
        };

        ClickReset();

    }
    private void UpdateTimeBox()
    {
        SetTimeToText(TimeSpan.Zero.Add(new TimeSpan(0, 0, StartTime)));
    }
    private void ClickReset()
    {
        if(!detonator.isActivate)
        {
            StartTime = 10;
            return;
        }
    }
    private void IncreaseTime(int Add)
    {
        if (detonator.isActivate) return;
        StartTime+= Add;
    }
    private void ReduceTime(int Revert)
    {
        if (detonator.isActivate) return;
        StartTime-= Revert;
    }
}

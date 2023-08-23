using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Tweener;
using Random = UnityEngine.Random;

public class Apple : ItemEngine, IGerminatable, ITakenEntity, IInteractive, IDropEntity
{
    public Renderer MeshRenderer;

    private bool isActivated;

    public bool IsRipen { get; private set; }

    EntityEngine IGerminatable.GetEntity => this;

    Rigidbody IDropEntity.Rigidbody => Rigidbody;

    public IExpansionTween tweenApple, tweenPostion, tweenColor;

    private UnityEvent OnRipen = new();


    event Action IGerminatable.OnRipen
    {
        add
        {
            OnRipen.AddListener(value.Invoke);
        }

        remove
        {
            OnRipen.RemoveListener(value.Invoke);
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        IsRipen = true;
        MeshRenderer = GetComponent<Renderer>();
    }
    
    public void Interaction()
    {
        if (isActivated || !IsRipen)
            return;
        isActivated = true;
        SoundMeneger.Play(SoundMeneger.Sounds.EatApple);
        Delete(0.5F);
    }

    void IGerminatable.Start(Vector3 growthPoint)
    {
        IsRipen = false;
        base.OnTake += OnRipen.Invoke;
        Rigidbody.isKinematic = true;
        float timeSpeedTween = Random.Range(50F, 500F);

        tweenPostion = Tween.AddPosition(Transform,
            new Vector3(0, -Transform.GetComponent<Renderer>().bounds.size.y, 0),
            timeSpeedTween
            );

        tweenColor = (IExpansionTween)Tween.SetColor(Transform, Color.green, timeSpeedTween).
            ReverseProgress().ChangeEase(Ease.FourthDegree);

        tweenApple = Tween.SetScale(Transform, Vector3.zero, timeSpeedTween).ReverseProgress()
        .ToCompletion(OnRipen.Invoke);
    }
    void IGerminatable.Stop()
    {
        IsRipen = tweenColor.Timer > 0.85F;
        if (tweenApple != null)
        {
            Tween.Stop(tweenApple); Tween.Stop(tweenPostion); Tween.Stop(tweenColor);
        }
        if (!Rigidbody) return;
        Rigidbody.WakeUp();
        Rigidbody.isKinematic = false;
        OnRipen.RemoveAllListeners();
    }
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
            {
            base.GetTextUI(),
            IsRipen ? new TextUI(() => new object[] {"\n[",LText.KeyCodeF ,"] -", LText.Eat }) : ""
            });
    }
}

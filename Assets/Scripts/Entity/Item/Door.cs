using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Tweener;

    public class Door : ItemEngine, IAlive
    {
    Transform DoorObject;

    private bool isClosed = true;

    public bool isLocked { get; private set; }

    protected override bool CanTake => GameState.IsCreative;

    public bool IsDead { get; private set; }

    protected override void OnStart()
    {
        DoorObject = Transform.Find("Door");
        
    }
    
    public override void Interaction()
    {
        if (IsDead || isLocked) return;
        isClosed = !isClosed;
        Tween.AddRotation(DoorObject, new Vector3(0f, isClosed ? 90F : -90F, 0))
            .ChangeEase(Ease.CubicRoot)
            .ToCompletion(() => Debug.Log(isClosed? "Close" : "Open"));
    }
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[] { itemType.ToString(),
            CanTake ? new TextUI(() => new object[] { "\n[", LText.KeyCodeE ,"] - ",LText.Take ,"/",LText.Leave }) : null,
            IsDead ? null : new TextUI(() => new object[] { "\n[", LText.KeyCodeF, "] -", LText.Interactive }) });
    }
    private void ChangeColorDoor()
    {
          Tween.SetColor(DoorObject, new Color(0F,0F,0F,0F), 1F).IgnoreAdd(IgnoreARGB.RGB).
            TypeOfColorChange(TypeChangeColor.ObjectAndHierarchy).
            ToCompletion(() => Destroy(DoorObject.gameObject),
            CallWhenDestroy: true);
    }
    public void Dead()
    {
        if (IsDead) return;
        IsDead = true;
        Rigidbody.isKinematic = false;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        Rigidbody = null;
        DoorObject.parent = null;
        Invoke(nameof(ChangeColorDoor), 29F);
    }
}

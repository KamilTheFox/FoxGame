using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
        Between.AddRotation(DoorObject, new Vector3(0f, isClosed ? 90F : -90F , 0));
    }
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[] { itemType.ToString(),
            CanTake ? new TextUI(() => new object[] { "\n[", LText.KeyCodeE ,"] - ",LText.Take ,"/",LText.Leave }) : null,
            IsDead ? null : new TextUI(() => new object[] { "\n[", LText.KeyCodeF, "] -", LText.Interactive }) });
    }
    private void ChangeColorDoor()
    {
        DoorObject.GetComponentsInChildren<Renderer>().ToList().ForEach(rend =>
        {
            rend.material.ToFadeMode();
            Color newColor = rend.material.color;
            newColor.a = 0F;
            Between.SetColorLerp(rend.transform, newColor, 1F);

        });
    }
    public void Dead()
    {
        if (IsDead) return;
        IsDead = true;
        Rigidbody.isKinematic = false;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        Rigidbody = null;
        DoorObject.parent = null;
        GameObject.Destroy(DoorObject.gameObject, 30F);
        Invoke(nameof(ChangeColorDoor), 29F);
    }
}

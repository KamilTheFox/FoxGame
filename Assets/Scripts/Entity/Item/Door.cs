using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Tweener;
using InteractiveBodies;

public class Door : ItemEngine, IDiesing, IInteractive, ITakenEntity
{
    private IInteractive DoorObject;

    public bool IsDie { get; private set; }

    protected override void OnStart()
    {
        foreach (Transform findDoor in Transform)
            if (findDoor != Transform && findDoor.name.Contains("Door"))
            {
                DoorObject = findDoor.gameObject.AddComponent<InteractiveBodies.Door>();
                break;
            }
        if (DoorObject != null)
        {
            Rigidbody = DoorObject.Rigidbody;
            ILocking locking = GetComponentInChildren<ILocking>();
            if (locking != null)
                ((ILockable)DoorObject).Lock(locking);
        }
    }
    public override ITakenEntity Take()
    {
        if(GameState.IsCreative)
            return base.Take();
        return null;
    }
    public void Interaction()
    {
        if (IsDie) return;

        DoorObject.Interaction();
    }
    private void ChangeColorDoor()
    {
          Tween.SetColor(DoorObject.Transform, new Color(0F,0F,0F,0F), 1F).IgnoreAdd(IgnoreARGB.RGB).
            TypeOfColorChange(TypeChangeColor.ObjectAndHierarchy).
            ToCompletion(() => Destroy(DoorObject.Transform.gameObject),
            CallWhenDestroy: true);
    }
    public void Death()
    {
        if (IsDie) return;
        IsDie = true;
        Rigidbody.isKinematic = false;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        Rigidbody = null;
        DoorObject.Transform.parent = null;
        Invoke(nameof(ChangeColorDoor), 29F);
    }
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
            {
            itemType.ToString(),
            GameState.IsAdventure ? "" : new TextUI(() => new object[]
            {"\n[",LText.KeyCodeE ,"] -", LText.Take, "/", LText.Leave }),
            IsDie ? "" : new TextUI(() => new object[] { "\n[", LText.KeyCodeF, "] - ", LText.Open, "/", LText.Close  })
            });
    }
}

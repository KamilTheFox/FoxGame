using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using GroupMenu;
using System;
using System.Linq;

public class Padlock : ItemEngine, ILocking, ITakenEntity, IInteractive, IDropEntity, IThrowed
{

    [SerializeField] private TypeKey typeKey = TypeKey.Red;

    private Vector3 positionLockers;
    private bool isLock;
    private bool _unlock;
    Rigidbody IDropEntity.Rigidbody => !isLock ? Rigidbody : null;
    public bool UnLock(UnityAction unlock = null)
    {
        if (!isLock || _unlock)
        {
            unlock?.Invoke();
            return true;
        }
        int counKey = Key.GetCountKey(typeKey);
        if (counKey == 0)
        {
            _unlock = true;
            Transform.SetParent(GetGroup.transform);
            Tweener.Tween.AddPosition(Transform.GetChild(0), Vector3.up * 0.2F * Transform.localScale.z).ChangeEase(Tweener.Ease.CubicRoot).ToCompletion(() =>
            {
                Rigidbody.isKinematic = false;
                isLock = false;
                _unlock = false;
                unlock?.Invoke();
            });
        }
        else
        {
            MessageBox.Info($"Нужно найти еще {counKey} ключей");
        }    
        return !isLock;
    }
    protected override void OnAwake()
    {
        base.OnAwake();
        positionLockers = Transform.GetChild(0).localPosition;
        if (Transform.parent == GetGroup.transform || Transform.parent == null)
            Unlock();
    }
    public void Interaction()
    {
        if (!_unlock)
            UnLock();
    }
    public override ITakenEntity Take()
    {
        if (isLock)
            return null;
        return base.Take();
    }
    private void Unlock()
    {
        Rigidbody.isKinematic = false;
        isLock = false;
        _unlock = false;
        Transform.GetChild(0).localPosition = Vector3.up * 0.2F * Transform.localScale.z;
    }
    void ILocking.Lock()
    {
        if(Key.GetCountKey(typeKey) == 0)
            AddItem(Enum.Parse<TypeItem>(Enum.GetNames(typeof(TypeItem))
                    .ToList().Where(name => name.Contains("Key" + typeKey.ToString())).ToArray()[0]),
                    Transform.position + Transform.forward, Quaternion.identity, false).transform.localScale = Transform.localScale;
        Transform.GetChild(0).localPosition = positionLockers;
        isLock = true;
        Rigidbody.isKinematic = true;
    }
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
        {
            itemType.ToString(),
            isLock ? "" : new TextUI(() => new object[] {"\n[", LText.KeyCodeE ,"] - ",LText.Take ,"/",LText.Leave }),
            !isLock ? "" : new TextUI(() => new object[] { "\n[", LText.KeyCodeF , "] -",  LText.Open }),
            isLock ? "" : new TextUI(() => new object[] { "\n[", LText.KeyCodeMouse0, "] - ", LText.Drop }),
        });
    }
    public void ToThrow()
    {
        if (isLock) return;
        
        ILockable lockable;
        if (Physics.Raycast(Transform.position + Transform.forward * 0.2F, Transform.forward * -1, out RaycastHit hit, 0.4F, MasksProject.RigidEntity))
        {
            if ((lockable = hit.collider.gameObject.GetComponentInParent<ILockable>()) != null)
            {
                lockable.Lock(this);
            }
        }
    }

    public enum TypeKey : byte
    {
        Red,
        Green,
        Blue,
        Yellow,
        White,
        Black
    }
}

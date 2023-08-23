using UnityEngine.Events;
using UnityEngine;
public interface ILocking
{
    public bool UnLock(UnityAction Unlock = null);

    public Transform Transform { get; }

    public void Lock();

}

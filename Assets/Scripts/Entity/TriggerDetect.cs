using UnityEngine;
using System;


public class TriggerDetect : MonoBehaviour
{
    public event Action<Collider> Enter;
    public event Action<Collider> Exit;
    public event Action<Collider> Stay;

    private void OnTriggerEnter(Collider other)
    {
        Enter?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        Exit?.Invoke(other);
    }
    private void OnTriggerStay(Collider other)
    {
        Stay?.Invoke(other);
    }
    private void OnDestroy()
    {
        Enter = null;
        Exit = null;
        Stay = null;
    }
}

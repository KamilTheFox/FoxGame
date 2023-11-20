using System;
using UnityEngine;

public interface IMoveablePlatform
{
    void NewMove(Vector3 vector);

    void StopMove();
    Transform transform { get; }
    Transform Guide => transform.parent;
}

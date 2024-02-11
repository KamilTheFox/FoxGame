using System;
using UnityEngine;

public interface IMoveablePlatform
{
    Vector3 Velosity { get; }
    Transform transform { get; }
    Transform Guide => transform.parent;

}

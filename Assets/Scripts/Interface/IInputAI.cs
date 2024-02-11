using System;
using UnityEngine;

public interface IInputAI
{
    public Vector3 Move(Transform source, out bool isMove);

    public bool Jump();
}

using System;
using UnityEngine;
public interface ICollideableDoll
{
    public void OnCollision(Collision collision, GameObject sourceObject);
}

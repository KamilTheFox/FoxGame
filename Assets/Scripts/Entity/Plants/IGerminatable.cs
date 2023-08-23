using UnityEngine;
using System;
public interface IGerminatable
{
    void Start(Vector3 growthPoint);
    void Stop();
    EntityEngine GetEntity { get; }

    event Action OnRipen;

}

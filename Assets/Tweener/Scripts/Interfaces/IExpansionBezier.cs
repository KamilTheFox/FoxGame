using UnityEngine;
using UnityEngine.Events;

namespace Tweener
{
    public interface IExpansionBezier : IExpansionTween<IExpansionBezier>
    {
        Vector3 CurrentPosition { get; }
        Vector3 CurrentRotation { get; }

        UnityEvent onUpdate { get; }
    }
}

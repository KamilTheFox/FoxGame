using UnityEngine;

namespace Tweener
{
    public interface IExpansionBezier : IExpansionTween<IExpansionBezier>
    {
        Vector3 CurrentPosition { get; }
        Vector3 CurrentRotation { get; }
    }
}

using System;
using UnityEngine;

namespace Tweener
{
    public interface IExpansionTween<T>
    {
        float Timer { get; }
        T ChangeEase(Ease type);
        /// <summary>
        /// It works when the tween is completed
        /// <param name="CallWhenDestroy">It works when the tween is completed or Object Destroy</param>
        /// </summary>
        T ToCompletion(Action action, bool CallWhenDestroy = false);
        /// <summary>
        /// It works when the tween is changed
        /// </summary>
        T ToChanging(Action action);

        T ReverseProgress();

        T ChangeLoop(TypeLoop loop);
    }
    public interface IExpansionTween
    {
        float Timer { get; }
        IExpansionTween ChangeEase(Ease type);
        /// <summary>
        /// It works when the tween is completed
        /// <param name="CallWhenDestroy">It works when the tween is completed or Object Destroy</param>
        /// </summary>
        IExpansionTween ToCompletion(Action action, bool CallWhenDestroy = false);
        /// <summary>
        /// It works when the tween is changed
        /// </summary>
        IExpansionTween ToChanging(Action action);

        IExpansionTween ReverseProgress();

        IExpansionTween ChangeLoop(TypeLoop loop);
    }
    public interface IExpansionColor : IExpansionTween<IExpansionColor>
    {
        IExpansionColor TypeOfColorChange(TypeChangeColor type);
        IExpansionColor IgnoreAdd(IgnoreARGB ARGB);
    }
    public interface IExpansionBezier : IExpansionTween<IExpansionBezier>
    {
        Vector3 CurrentPosition { get; }
        Vector3 CurrentRotation { get; }


    }
}


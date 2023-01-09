using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;
using Tweener;

public class Test : MonoBehaviour
{
    public BezierWay bezierWay;
    public void Start()
    {
        Tween.GoWay(transform, bezierWay, 10F).ChangeLoop(TypeLoop.Loop);
        Tween.SetColor(transform, Color.red).TypeOfColorChange(TypeChangeColor.ObjectAndChilds)
            .ToCompletion(() => Tween.SetColor(transform, Color.blue).TypeOfColorChange(TypeChangeColor.ObjectAndChilds).ChangeLoop(TypeLoop.PingPong));
    }
    public void OnDrawGizmos()
    {
        bezierWay.OnGizmos();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;
using Tweener;

public class Test : MonoBehaviour
{
    private void Start()
    {
        Tween.AddPosition(transform, new Vector3(3, 0, 0), 2f).ChangeLoop(TypeLoop.Loop).ChangeEase(Ease.CubicRoot);
    }
}

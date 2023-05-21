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
        Tween.SetScale(transform, Vector3.zero, 1).ReverseProgress();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tweener;

public class TestSwitchFade : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Tween.SetColor(transform, new Color(0, 0, 0, 0), 4, TypeComponentChangeColor.Material).ChangeLoop(TypeLoop.PingPong);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

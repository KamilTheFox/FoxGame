using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;
using Tweener;

public class Test : MonoBehaviour, IMoveablePlatform
{
    [SerializeField] bool reverse;
    GameObject obj2;
    public void NewMove(Vector3 vector)
    {
        Tween.Stop(move);
        move = Tween.AddPosition(transform.parent, vector, 2f).ChangeLoop(TypeLoop.PingPong);
    }

    public void StopMove()
    {
        Tween.Stop(move);
    }
    IExpansionTween move;

    public Vector3 Velosity => Vector3.up;

    private void Start()
    {
        GameObject obj = new GameObject("targeTest");
        obj2 = new GameObject("targeTest2");
        transform.SetParent(obj.transform);
        move = Tween.AddPosition(obj2.transform, new Vector3(0, 0, -3), 2f).ChangeLoop(TypeLoop.PingPong);
        if (reverse)
            move.ReverseProgress();
    }
    private void FixedUpdate()
    {
        transform.parent.transform.position = obj2.transform.position;
    }
}

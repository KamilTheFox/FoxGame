using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;
using Tweener;

public class Test : MonoBehaviour, IMoveablePlatform
{
    [SerializeField] bool reverse;

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
    private void Start()
    {
        GameObject obj = new GameObject("targeTest");
        transform.SetParent(obj.transform);
        move = Tween.AddPosition(transform.parent, new Vector3(0, 0, -3), 2f).ChangeLoop(TypeLoop.PingPong);
        if (reverse)
            move.ReverseProgress();
    }
}

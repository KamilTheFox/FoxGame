using System.Collections;
using System.Collections.Generic;
using Tweener;
using UnityEngine;

public class Platforms : MonoBehaviour, IMoveablePlatform
{
    public Vector3 Velosity => Vector3.zero;

    [SerializeField] Vector3 torward;

    private GameObject obj2;

    private IExpansionTween move;

    [SerializeField] private bool reverse;

    [SerializeField] private float time;

    void Start()
    {
        GameObject obj = new GameObject("targeTest");
        obj2 = new GameObject("targeTest2");
        transform.SetParent(obj.transform);
        move = Tween.AddPosition(obj2.transform, torward, time).ChangeLoop(TypeLoop.PingPong);
        if (reverse)
            move.ReverseProgress();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, torward + transform.position);
    }
    private void FixedUpdate()
    {
        transform.parent.transform.position = obj2.transform.position;
    }
}

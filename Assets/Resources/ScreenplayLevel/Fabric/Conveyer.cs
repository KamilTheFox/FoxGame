using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweener;
using UnityEngine;

public class Conveyer : MonoBehaviour, IMoveablePlatform
{
    public Vector3 Velosity => Vector3.zero;

    [SerializeField] Vector3 torward, started;

    private GameObject obj2;

    private IExpansionTween move;

    [SerializeField] private bool reverse;

    [SerializeField] private float time;

    private void OnValidate()
    {
        started = transform.position;
    }

    void Start()
    {
        GameObject obj = new GameObject("targeTest");
        obj2 = new GameObject("targeTest2");
        transform.SetParent(obj.transform);
        started = transform.position;
        move = Tween.AddPosition(obj2.transform, torward, time).ChangeLoop(TypeLoop.Loop);
        move.ToCompletion(() =>
        {
            foreach(Transform t in transform.parent)
            {
                if(t != transform)
                    t.SetParent(null);
            }
        });
        if (reverse)
            move.ReverseProgress();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(started, torward + started);
    }
    private void FixedUpdate()
    {
        transform.parent.transform.position = obj2.transform.position;
    }
}

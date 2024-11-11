using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Tweener;

public class Test : MonoBehaviour, IMoveablePlatform
{
    [SerializeField] bool reverse;
    [SerializeField] GameObject obj2;
    private Thread thread;
    private void Method()
    {
        obj2.SetActive(false);
    }

    public Vector3 Velosity => Vector3.up;

    private void Start()
    {
        thread = new Thread(Method);
        thread.Start();
    }
}

using System;
using UnityEngine;
using Random = UnityEngine.Random;
public abstract class RandomGo : IBehavior
{
    public string Name => nameof(RandomGo);

    public AI AI { get; set; }

    abstract protected TypeAnimation Go { get; }

    abstract protected Action Stopped { get; }

    abstract protected Vector3 PositionFrom { get; }

    protected Vector3 RanngeVector3(float min, float max)
    {
        if (min >= max)
        {
            Debug.LogError(LText.ErrorMinMax.GetTextUI());
        }
        float x;
        float z;
        do
        {
            x = Random.Range(-max, max);
            z = Random.Range(-max, max);
        }
        while (x < min && x > -min && z < min && z > -min);
        Vector3 vector = new Vector3(x, 0F, z);
        vector += AI.engine.Transform.position;
        return vector;
    }
    protected Vector3 RanngeVector3Forward(float min, float max)
    {
        if (min >= max)
        {
            Debug.LogError(LText.ErrorMinMax.GetTextUI());
        }
        Vector3 position = AI.engine.Transform.position;
        Vector3 forward = AI.engine.Transform.forward;
        Vector3 right = AI.engine.Transform.right;
        Vector3 left = -AI.engine.Transform.right;
        float x;
        float z;
        float t;
        Vector3 vector = Vector3.zero;
        do
        {
            x = Random.Range(-max, max);
            z = Random.Range(-max, max);
            vector = new Vector3(x, 0f, z);
            t = Vector3.Angle(forward, vector);
        }
        while (t >= 90F && x < min && x > -min && z < min);

        vector += position;
        return vector;
    }
    protected abstract IBehavior AbsencePath { get; }

    protected virtual bool СonditionRandomPoint(Vector3 vector)
    {
        return true;
    }


    protected void StartRun()
    {
        Vector3 vector2;
        bool flag = false;
        int protectCatch = 0;
        do
        {
            protectCatch++;
            vector2 = PositionFrom;
            if (!СonditionRandomPoint(vector2))
                continue;
            flag = AI.SetDestination(vector2);
        }
        while (!flag && protectCatch <= 9);
        if (!flag)
        {
            AI.SetBehavior(AbsencePath);
            return;
        }
        AI.SetAnimation(Go);
        AI.ContinueMove();

    }
    public virtual void Activate(AI ai)
    {
        AI = ai;
        StartRun();
    }

    public void Deactivate()
    {
        AI.StopMove();
    }

    public void Update()
    {
        if (AI.IsStopped)
        {
            Stopped.Invoke();
        }
    }
}

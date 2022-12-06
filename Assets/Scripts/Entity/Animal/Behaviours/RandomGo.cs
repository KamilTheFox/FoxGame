using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
public abstract class  RandomGo : IBehavior
{
    public string Name => nameof(RandomGo);

    public AI AI { get; set; }

    abstract protected TypeAnimation Go { get; }

    abstract protected Action Stopped { get; }

    abstract protected Vector2 PositionFrom { get; }

    private Vector3 RunRandomPosition 
    {
        get
        {
            Vector2 vector2 = PositionFrom;
            Vector3 vector = new Vector3(vector2.x, 0F, vector2.y);
            Debug.Log(vector);
            vector += AI.engine.Transform.position; 
            return vector;
        }
    }
    protected Vector2 RanngeVector2(float min, float max)
    {
        if(min >= max)
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
        return new Vector2(x, z);
    }
    public virtual void Activate(AI ai)
    {
        AI = ai;
        AI.SetAnimation(Go);
        if (AI.CheckStatePath(NavMeshPathStatus.PathPartial))
        {
            AI.SetBehavior(new Idle());
            return;
        }
            AI.SetDestination(RunRandomPosition);
            AI.ContinueMove();
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

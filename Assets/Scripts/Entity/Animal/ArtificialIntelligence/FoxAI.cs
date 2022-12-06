using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class FoxAI : AI
{
    float hungry = 100F;
    float TimeRest;
    float Hungry { get
        {

            return hungry;
        }
        set
        {
            hungry = value;
            if(hungry < 30F)
            {
                SetBehavior(new FoodSearch());
            }
        }
    } 
    public override Action<Collision, GameObject> BehaviorFromCollision => (col, Attached) =>
    {
        if (Attached.name.ToLower().Contains("head") && col.gameObject.TryGetComponent(out Rigidbody rigidbody) && col.relativeVelocity.magnitude * rigidbody.mass > 70F)
        {
            engine.Dead();
        }
        if(Attached.name.ToLower().Contains("teil") && col.gameObject.TryGetComponent(out PlayerControll player))
        {
            SetBehavior(new RandomFright());
        }

    };
    private void ChangeState()
    {
    }
    protected override void OnUpdate()
    {
        if (NameBehavior == "Idle")
            TimeRest -= Time.deltaTime;
        if (TimeRest <= 0 && NameBehavior != nameof(RandomGo))
        {
            Debug.Log("SetRandomGo");
            SetBehavior(new RandomRun());
            TimeRest = Random.Range(30F, 180F);
        }
    }

    protected override void OnStart()
    {
        ChangeStateBehaviour += ChangeState;
        TimeRest = Random.Range(100F, 150F);
    }
}

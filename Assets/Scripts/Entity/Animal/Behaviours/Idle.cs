using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Idle : IBehavior
{
    public Idle(bool isSit = false)
    {
        startSits = isSit;
    }
    bool startSits;
    public string Name => "Idle";

    public AI AI { get; set; }

    const float timeRest = 45F;
    float time;
    float timeFatigue;

    TypeAnimation CurrentAnimation;
    public void Activate(AI _ai)
    {
        AI = _ai;
        CurrentAnimation = TypeAnimation.Idle;
        AI.SetAnimation(CurrentAnimation);
        time = 0;
        if(!startSits)
        RandomFatigueTime();
    }

    public void Deactivate()
    {
        if(CurrentAnimation == TypeAnimation.Sits)
            AI.SetAnimation(TypeAnimation.StendUp);
    }

    private void RandomFatigueTime()
    {
        timeFatigue = UnityEngine.Random.Range(15F, 100F);
    }
    private void TimerFatigue()
    {
        if(time >= timeFatigue && CurrentAnimation == TypeAnimation.Idle)
        {
            time = 0;
            CurrentAnimation = TypeAnimation.Sits;
            AI.SetAnimation(CurrentAnimation);
        }
        else if( time >= timeRest && CurrentAnimation == TypeAnimation.Sits)
        {
            time = 0;
            RandomFatigueTime();
            AI.SetAnimation(TypeAnimation.StendUp);
            CurrentAnimation = TypeAnimation.Idle;
        }
    }

    public void Update()
    {
        time += Time.deltaTime;
        TimerFatigue();
    }
}

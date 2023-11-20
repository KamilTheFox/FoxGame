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

    TypeAnimationAnimal CurrentAnimation;
    public void Activate(AI _ai)
    {
        AI = _ai;
        CurrentAnimation = TypeAnimationAnimal.Idle;
        AI.SetAnimation(CurrentAnimation);
        time = 0;
        if(!startSits)
        RandomFatigueTime();
    }

    public void Deactivate()
    {
        if(CurrentAnimation == TypeAnimationAnimal.Sits)
            AI.SetAnimation(TypeAnimationAnimal.StendUp);
    }

    private void RandomFatigueTime()
    {
        timeFatigue = UnityEngine.Random.Range(15F, 100F);
    }
    private void TimerFatigue()
    {
        if(time >= timeFatigue && CurrentAnimation == TypeAnimationAnimal.Idle)
        {
            time = 0;
            CurrentAnimation = TypeAnimationAnimal.Sits;
            AI.SetAnimation(CurrentAnimation);
        }
        else if( time >= timeRest && CurrentAnimation == TypeAnimationAnimal.Sits)
        {
            time = 0;
            RandomFatigueTime();
            AI.SetAnimation(TypeAnimationAnimal.StendUp);
            CurrentAnimation = TypeAnimationAnimal.Idle;
        }
    }

    public void Update()
    {
        time += Time.deltaTime;
        TimerFatigue();
    }
}

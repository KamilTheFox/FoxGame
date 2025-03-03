using UnityEngine;
using System.Linq;

public class Barrel_Detonator_Timer : Barrel_Detonator
{
    protected override void OnAwake()
    {
        Transform timer = Transform.FirstToLowerPrefix("timer");
        timerDetonator = timer.gameObject.AddComponent<TimerDetonator>();

        timerDetonator.Initialized(this);
        distanceOrder = 15f;
    }
    public override float TimeDetonate => timerDetonator.StartTime;

    private TimerDetonator timerDetonator;
}

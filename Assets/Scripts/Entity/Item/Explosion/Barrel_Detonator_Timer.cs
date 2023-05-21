using UnityEngine;


public class Barrel_Detonator_Timer : Barrel_Detonator
{
    protected override void OnStart()
    {
        Transform timer = Transform.Find("Timer");
        timerDetonator = timer.gameObject.AddComponent<TimerDetonator>();

        timerDetonator.Initialized(this);
    }
    public override float TimeDetonate => timerDetonator.StartTime;

    private TimerDetonator timerDetonator;
}

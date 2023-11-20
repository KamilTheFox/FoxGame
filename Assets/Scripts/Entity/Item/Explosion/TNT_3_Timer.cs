using System.Collections;
using UnityEngine;


public class TNT_3_Timer : TNT_3
{
    protected override void OnAwake()
    {
        Transform timer = Transform.Find("Timer");
        timerDetonator = timer.gameObject.AddComponent<TimerDetonator>();

        timerDetonator.Initialized(this);
    }
    public override float TimeDetonate => timerDetonator.StartTime;

    private TimerDetonator timerDetonator;
}

using UnityEngine;


public class Barrel_Detonator_Timer : Barrel_Detonator
{
    protected override void OnAwake()
    {
        Transform timer = Transform.Find("Timer");
        timerDetonator = timer.gameObject.AddComponent<TimerDetonator>();

        timerDetonator.Initialized(this);
        distanceOrder = 15f;
    }
    public override float TimeDetonate => timerDetonator.StartTime;

    private TimerDetonator timerDetonator;
    public override void OnBatchDistanceCalculated(bool enable)
    {
        base.OnBatchDistanceCalculated(enable);
        timerDetonator.ActivateCanvas = enable;
    }
}

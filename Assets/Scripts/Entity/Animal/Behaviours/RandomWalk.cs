using System;
using UnityEngine;

public class RandomWalk : RandomGo
{
    protected override IBehavior AbsencePath => new Idle();
    protected override TypeAnimation Go => TypeAnimation.Walk;

    protected override Action Stopped => StoppedRun;

    protected override Vector3 PositionFrom => RanngeVector3Forward(1F, 3F);
    private int i;
    private void StoppedRun()
    {
        if (i <= 2)
        {
            StartRun();
        }
        else
            AI.SetBehavior(AbsencePath);
        i++;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RandomFright : RandomGo
{ 
    protected override TypeAnimation Go => TypeAnimation.Run_Fast;

    protected override Action Stopped => StoppedRun;

    protected override Vector2 PositionFrom => RanngeVector2(5F, 10F);
    private int i;
    private void StoppedRun()
    {
        if (i <= 2)
        {
            AI.SetDestination(new Vector3(PositionFrom.x, 0F, PositionFrom.y));
        }
        else
            AI.SetBehavior(new Idle());
        i++;
    }
}

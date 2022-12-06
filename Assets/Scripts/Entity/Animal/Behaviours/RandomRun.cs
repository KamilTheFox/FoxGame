using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomRun : RandomGo
{
    protected override TypeAnimation Go => TypeAnimation.Run;

    protected override Action Stopped => StoppedRun;

    protected override Vector2 PositionFrom => RanngeVector2(3F, 7F);
    private int i;
    private void StoppedRun()
    {
        if (i <= Random.Range(1,3))
        {
            AI.SetDestination(new Vector3(PositionFrom.x, 0F, PositionFrom.y));
        }
        else
            AI.SetBehavior(new Idle());
        i++;
    }
}

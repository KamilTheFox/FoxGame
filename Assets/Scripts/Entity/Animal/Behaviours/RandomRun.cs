using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomRun : RandomGo
{
    protected override TypeAnimationAnimal Go => TypeAnimationAnimal.Run;

    protected override Action Stopped => StoppedRun;

    protected override Vector3 PositionFrom => RanngeVector3Forward(3F, 7F);

    protected override IBehavior AbsencePath => new Idle();

    private int i;
    private void StoppedRun()
    {
        if (i <= Random.Range(1,3))
        {
            StartRun();
        }
        else
        AI.SetBehavior(AbsencePath);
        i++;
    }
}

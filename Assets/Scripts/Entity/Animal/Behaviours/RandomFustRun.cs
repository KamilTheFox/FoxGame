using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RandomFustRun : RandomGo
{ 
    protected override TypeAnimationAnimal Go => TypeAnimationAnimal.Run_Fast;

    protected override Action Stopped => StoppedRun;

    protected override Vector3 PositionFrom => RanngeVector3Forward(5F, 10F);

    protected override IBehavior AbsencePath => new RandomWalk();

    private int i;
    private void StoppedRun()
    {
        if (i <= 2)
        {
            StartRun();
        }
        else
            AI.SetBehavior(new Idle(true));
        i++;
    }
}



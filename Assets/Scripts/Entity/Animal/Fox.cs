using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fox : AnimalEngine
{
    public override Action<Collision> BehaviorFromCollision => (col) => Dead();

    protected override Type Started()
    {
        return typeof(TypeAnimationFox);
    }

    private enum TypeAnimationFox
    {
        Run_Fast,
        Idle_Sits
    }
}


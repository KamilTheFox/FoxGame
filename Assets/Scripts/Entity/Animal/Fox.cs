using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fox : AnimalEngine
{
    
    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
        {
            base.GetTextUI(),
            " " + TypeAnimal.ToString() +
            (IsDie? "" : "\nPserss F to Debug animation")
        });
    }
}


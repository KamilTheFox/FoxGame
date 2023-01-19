using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Apple : ItemEngine
{
    public Renderer MeshRenderer;

    private bool isActivated;
    protected override void OnAwake()
    {
        base.OnAwake();
        MeshRenderer = GetComponent<Renderer>();
    }

    public override void Interaction()
    {
        if (isActivated)
            return;
        isActivated = true;
        SoundMeneger.Play(SoundMeneger.Sounds.EatApple);
        Delete(0.5F);
    }

    
}

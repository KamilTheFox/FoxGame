using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Apple : ItemEngine
{
    public Renderer MeshRenderer;
    protected override void OnAwake()
    {
        base.OnAwake();
        MeshRenderer = GetComponent<Renderer>();

    }
    // Start is called before the first frame update
    bool isActivated;
    public override void Interaction()
    {
        if (isActivated)
            return;
        isActivated = true;
        SoundMeneger.Play(SoundMeneger.Sounds.EatApple);
        Delete(0.5F);
    }
}

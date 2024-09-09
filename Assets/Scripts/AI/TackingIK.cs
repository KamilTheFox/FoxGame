using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TackingIK : ITrackIK
{
    public TackingIK(Animator animator)
    {
        Animator = animator;
        Transform head = Animator.GetBoneTransform(HumanBodyBones.Head);
        smoothLookPoint = head.position + head.forward * 20F;
    }
    [field: SerializeField] public Animator Animator { get; set; }

    [field: SerializeField] public float LookIKSpeed { get; set; } = 2f;

    [field: Range(0F, 1F)]
    [field: SerializeField] public float LookIKWeight { get; set; }


    [field: Range(0F, 1F)]
    [field: SerializeField] public float EyesWeight { get; set;}


    [field: Range(0F, 1F)]
    [field: SerializeField] public float HeadWeight { get; set; }



    [field: Range(0F, 1F)]
    [field: SerializeField] public float BodyWeight { get; set; }



    [field: Range(0F,1F)]
    [field: SerializeField] public float ClampWeight { get; set; }

    [field: SerializeField] public Transform Target { get; set; }

    private Vector3 smoothLookPoint, target;

    public void OnIK()
    {
        if (Animator == null)
            throw new NullReferenceException("No Animator has been assigned in the TackingIK component");
        
        if (Target == null)
        {
            Transform head = Animator.GetBoneTransform(HumanBodyBones.Head);
            target = head.position + head.forward * 20F;
        }
        else
        {
            target = Target.position;
        }
        smoothLookPoint = Vector3.Lerp(smoothLookPoint, target, Time.deltaTime * LookIKSpeed);
        Animator.SetLookAtWeight(LookIKWeight, BodyWeight, HeadWeight, EyesWeight, ClampWeight);
        Animator.SetLookAtPosition(smoothLookPoint);
    }
}

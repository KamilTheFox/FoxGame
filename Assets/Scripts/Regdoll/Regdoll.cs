using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class Regdoll : IRegdoll
{
    private Animator Animator;

    private List<Rigidbody> RegdoolBody = new();

    private Rigidbody BodyController;

    public bool isActive { get; private set; }

    public Regdoll(Animator animator, GameObject rootRegdollObject)
    {
        BodyController = rootRegdollObject.GetComponent<Rigidbody>();
        List<Rigidbody> ParentsForRegdoll = rootRegdollObject.GetComponentsInChildren<Rigidbody>().ToList();
        Animator = animator;
        foreach (Rigidbody rigidbody in ParentsForRegdoll)
        {
            RegdoolBody.Add(rigidbody);
            RegdollDetect detect = rigidbody.gameObject.AddComponent<RegdollDetect>();
            if (BodyController == rigidbody)
                detect.isController = true;
            detect.Entity = rootRegdollObject.GetComponentInChildren<ICollideableDoll>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        ActivateKinematic();
    }
    private void ActivateKinematic(bool isActivate = true)
    {
        if (Animator) Animator.enabled = isActivate;
        foreach (Rigidbody body in RegdoolBody)
        {
            body.isKinematic = isActivate;
        }
        if (BodyController)
        {
            BodyController.isKinematic = !isActivate;
        }
    }
    public void Activate()
    {
        isActive = true;
        ActivateKinematic(false);
    }

    public void Deactivate()
    {
        isActive = false;
        ActivateKinematic();
    }
}

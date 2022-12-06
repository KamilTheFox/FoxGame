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
    public Regdoll(Animator animator, IAlive alive)
    {
        BodyController = alive.gameObject.GetComponent<Rigidbody>();
        List<Rigidbody> ParentsForRegdoll = alive.gameObject.GetComponentsInChildren<Rigidbody>().ToList();
        Animator = animator;
        foreach (Rigidbody rigidbody in ParentsForRegdoll)
        {
            RegdoolBody.Add(rigidbody);
            RegdollDetect detect = rigidbody.gameObject.AddComponent<RegdollDetect>();
            if (BodyController == rigidbody)
                detect.isController = true;
            detect.Entity = alive;
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
        ActivateKinematic(false);
    }

    public void Deactivate()
    {
        ActivateKinematic(true);
    }
}
public class RegdollDetect : MonoBehaviour
{
    public bool isController;
    public IAlive Entity;
    public void OnCollisionEnter(Collision collision)
    {
        Entity?.BehaviorFromCollision?.Invoke(collision, gameObject);
    }
}

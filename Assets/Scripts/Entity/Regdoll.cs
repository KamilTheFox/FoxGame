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
    public Regdoll(Animator animator, IAlive alive)
    {
        List<Transform> ParentsForRegdoll = alive.gameObject.GetComponentsInChildren<Transform>().ToList();
        Animator = animator;
        do
        {
            Transform[] ObjectPar = ParentsForRegdoll.ToArray();
            foreach (Transform parent in ObjectPar)
                foreach (Transform chield in parent)
                {
                    if (chield.childCount > 0)
                        ParentsForRegdoll.Add(chield);
                    Rigidbody rigidbody = chield.GetComponent<Rigidbody>();
                    if (rigidbody)
                    {
                        RegdoolBody.Add(rigidbody);
                        rigidbody.gameObject.AddComponent<RegdollDetect>().Entity = alive;
                        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    }
                }
        }
        while (ParentsForRegdoll.Count == 0);
        ActivateKinematic();
    }
    private void ActivateKinematic(bool isActivate = true)
    {
        if (!isActivate) Animator.enabled = false;
        foreach (Rigidbody body in RegdoolBody)
        {
            body.isKinematic = isActivate;
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
    public IAlive Entity;
    public void OnCollisionEnter(Collision collision)
    {
        Entity?.BehaviorFromCollision?.Invoke(collision);
    }
}

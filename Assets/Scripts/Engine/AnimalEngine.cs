using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

    public class AnimalEngine : EntityEngine, IAlive
{
    private List<Rigidbody> Regdoll = new();
    private List<Transform> ParentsForRegdoll = new(); 
    private Animator Animator { get; set; }
    public void Start()
    {
        Animator = GetComponent<Animator>();
        ParentsForRegdoll = GetComponentsInChildren<Transform>().ToList();
        do
        {
            Transform[] ObjectReg = ParentsForRegdoll.ToArray();
            foreach (Transform parent in ObjectReg)
            foreach (Transform chield in parent)
            {
                if(chield.childCount > 0)
                {
                    ParentsForRegdoll.Add(chield);
                }
                    Rigidbody rigidbody = chield.GetComponent<Rigidbody>();
                    if (rigidbody)
                    {
                        Regdoll.Add(rigidbody);
                        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    }

            }
        } while (ParentsForRegdoll.Count == 0);

        ActivateKinematic();
    }
    private void ActivateKinematic(bool isActivate = true)
    {
        if(!isActivate) Animator.enabled = false;
        foreach (Rigidbody body in Regdoll)
        {
            body.isKinematic = isActivate;
        }
    }
    public void Dead()
    {
        ActivateKinematic(false);
        Delete(60F);
    }

}

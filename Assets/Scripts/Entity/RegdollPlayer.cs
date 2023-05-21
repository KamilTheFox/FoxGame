using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RegdollPlayer : IRegdoll
{
   
        private Animator Animator;

        private List<Rigidbody> RegdoolBody = new();

        private Rigidbody BodyController;
        public RegdollPlayer(Animator animator, PlayerControll player)
        {
            BodyController = player.gameObject.GetComponent<Rigidbody>();
            BodyController.interpolation = RigidbodyInterpolation.Interpolate;
            List<Rigidbody> ParentsForRegdoll = player.gameObject.GetComponentsInChildren<Rigidbody>().ToList();
            Animator = animator;
            foreach (Rigidbody rigidbody in ParentsForRegdoll)
            {
                RegdoolBody.Add(rigidbody);
                RegdollDetect detect = rigidbody.gameObject.AddComponent<RegdollDetect>();
                if (BodyController == rigidbody)
                    detect.isController = true;
                detect.Entity = player;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
            ActivateKinematic();
        }
        private void ActivateKinematic(bool isActivate = true)
        {
            if (Animator) Animator.enabled = isActivate;
        if (BodyController)
        {
            BodyController.isKinematic = !isActivate;
            BodyController.detectCollisions = isActivate;
        }
        foreach (Rigidbody body in RegdoolBody)
            {
            if (BodyController == body) continue;
            body.interpolation = isActivate ? RigidbodyInterpolation.None : RigidbodyInterpolation.Interpolate;
            body.isKinematic = isActivate;
            }
        }
        public void Activate()
        {
            ActivateKinematic(false);
        }

        public void Deactivate()
        {
            ActivateKinematic();
        }
}
    


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PlayerDescription;

public class RegdollPlayer : IRegdoll
{
   
    private Animator Animator;

    private List<Rigidbody> RegdoolBody = new();

    private Rigidbody BodyController;

    public bool isActive { get; private set; }

    public RegdollPlayer(Animator animator, CharacterBody player)
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
            else
                rigidbody.gameObject.layer = MasksProject.SkinPlayer;
            detect.Entity = player.gameObject.GetComponent<ICollideableDoll>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        if (BodyController)
            BodyController.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        ActivateKinematic();
    }
    private void ActivateKinematic(bool isActivateKinematic = true)
    {
        if (Animator) Animator.enabled = isActivateKinematic;
    if (BodyController)
    {
        BodyController.isKinematic = !isActivateKinematic;
        BodyController.detectCollisions = isActivateKinematic;
    }
    foreach (Rigidbody body in RegdoolBody)
        {
        if (BodyController == body) continue;
        body.interpolation = isActivateKinematic ? RigidbodyInterpolation.None : RigidbodyInterpolation.Interpolate;
        body.isKinematic = isActivateKinematic;
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
        Transform Skin = BodyController.transform.Find("Skin");
        Vector3 ewPosition = Skin.GetComponentsInChildren<Transform>().ToList().Find(find => find.name == "Hips").position;
        if(Physics.Raycast(ewPosition,Vector3.down, out RaycastHit hit , MasksProject.Terrain))
        {
            ewPosition = hit.point;
        }
        BodyController.transform.position = ewPosition;
        ActivateKinematic();
    }
}
    


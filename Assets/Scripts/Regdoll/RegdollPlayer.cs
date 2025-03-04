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

    private CharacterBody characterBody;

    public bool isActive { get; private set; }

    public RegdollPlayer(Animator animator, CharacterBody player)
    {
        characterBody = player;
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
            if (body == null || body.gameObject == null)
                {
                    Debug.LogWarning($"{characterBody.name} - RegdoolBody is Null");
                    continue;
                }
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
        Vector3 ewPosition = characterBody.Hips.position;
        Quaternion quaternion = characterBody.Hips.rotation;

        Vector3 direction = characterBody.Hips.up;

        direction.y = 0f;

        if (Physics.Raycast(ewPosition,Vector3.down, out RaycastHit hit , MasksProject.Terrain))
        {
            ewPosition = hit.point;
        }
        characterBody.transform.position = ewPosition;
        characterBody.transform.rotation = Quaternion.FromToRotation(characterBody.transform.forward, direction.normalized);
        ActivateKinematic();
    }
}
    


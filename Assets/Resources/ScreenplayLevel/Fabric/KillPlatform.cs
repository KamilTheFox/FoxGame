using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerDescription;
using CameraScripts;

public class KillPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 positionRespawn;
    private void OnCollisionEnter(Collision collision)
    {
        CharacterBody body = collision.gameObject.GetComponentInParent<CharacterBody>();
        if (body.IsDie) return;
        body.Death();
        if (body is IExplosionDamage explosionDamage)
            explosionDamage.Explosion();
        CameraControll.instance.Transform.position = positionRespawn;
        CameraControll.instance.GiveBody(UnityEngine.Random.Range(1,5));

    }
}

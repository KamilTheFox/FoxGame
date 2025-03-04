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
        if (body == null)
            return;
        if (body.IsDie) return;

        body.Death();

        if (body is IExplosionDamaged explosionDamage)
            explosionDamage.Explosion(UnityEngine.Random.Range(0.2F, 5F));

        CameraControll.Instance.Transform.position = positionRespawn;
        CameraControll.Instance.GiveBody(UnityEngine.Random.Range(1,5));

    }
}

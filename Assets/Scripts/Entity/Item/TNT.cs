using UnityEngine;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using Random = UnityEngine.Random;

public class TNT : ItemEngine
{
    public float Radius = 10F;
    public float Force = 3000F;
    public const float TimeDetonate = 7F;
    bool isActivate;
    public override void Interaction()
    {
        InteractionTimeActivation(TimeDetonate);
    }
    public void InteractionTimeActivation(float time)
    {
        if (isActivate)
            return;
        isActivate = true;
        GameObject Detonate = EntityCreate.GetEntity(EffectItem.TNT_Detonate, Transform).GetPrefab;
         SoundMeneger.Play(SoundMeneger.Sounds.TNT_Detonate,
             Detonate.GetComponent<AudioSource>());
        Detonate.transform.SetParent(transform);
        Delete(time);
    }
    protected override void onDestroy()
    {
        if (isActivate && gameObject.scene.isLoaded)
        {
            Explosion();
        }
    }
    void ExplosionForce (Rigidbody rb , float ForseDelta = 1F)
    {
        if (!rb) return;
        for (int i = 6; i > 1; i -= 2)
        {
            rb.AddExplosionForce(Force / i / ForseDelta, Transform.position, Radius * (float)(6F / i * 1.2F));
        }
    }
    private Collider[] OverlapSphere()
    {
        return Physics.OverlapSphere(Transform.position, Radius * 5F, LayerMask.GetMask(new string[] { "Entity", "Player" }));
    }
    void Explosion()
    {
        Destroy(GetEffect(EffectItem.TNT_Explosion, Transform.position), 8F);
        SoundMeneger.PlayPoint(SoundMeneger.Sounds.Explosion, Transform.position, volume: 2F);

        Collider[] colliders = OverlapSphere();
        for (int i = 0; i < colliders.Length; i++)
        {
            EntityEngine item = colliders[i].GetComponentInParent<EntityEngine>();
            Rigidbody rb = colliders[i].GetComponent<Rigidbody>();
            if (item && item.Rigidbody)
                rb = item.Rigidbody;
            if(!rb)
                rb = colliders[i].GetComponentInParent<Rigidbody>();
            float Distance = Vector3.Distance(colliders[i].transform.position, Transform.position);
            IAlive Alive = colliders[i].GetComponentInParent<IAlive>();
            if (Alive != null && Distance < Radius)
            {
                    Alive.Dead();
            }
            if (Distance < Radius * 0.8F)
            {
                if (item is ItemEngine)
                {
                    if (item is TNT tnt)
                    {
                        new Random.State();
                        float Detonate = Random.Range(0.32F, TimeDetonate);
                        if (tnt.isActivate)
                            tnt.Delete(Detonate);
                        else
                            tnt.InteractionTimeActivation(Detonate);
                        ExplosionForce(rb, 5F);
                        continue;
                    }
                    else if(Distance < Radius * (Alive == null ? 0.5F : 0.1F))
                        item.Delete();
                }
            }
            ExplosionForce(rb);
        }
    }


}

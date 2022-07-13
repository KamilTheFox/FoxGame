using UnityEngine;
using System;
using FactoryLesson;

public class TNT : ItemEngine
{
    public float Radius = 10F;
    public float Force = 3000F;
    public float TimeDetonate = 7F;
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
         SoundMeneger.Play(SoundMeneger.Sounds.TNT_Detonate,
             EntityFactory.GetEntity(EffectItem.TNT_Detonate, Transform).GetPrefab.GetComponent<AudioSource>());
        Delete(time);
    }
    private void OnDestroy()
    {
        if(isActivate)
        Explosion();
    }
    void ExplosionForce (Rigidbody rb , float ForseDelta = 1F)
    {
        if (!rb) return;
        for (int i = 6; i > 1; i -= 2)
        {
            rb.AddExplosionForce(Force / i / ForseDelta, Transform.position, Radius * (float)(6F / i * 1.2F));
        }
    }
    void Explosion()
    {
        Destroy(EntityFactory.GetEntity(EffectItem.TNT_Explosion, Transform.position).GetPrefab, 8F);
        SoundMeneger.PlayPoint(SoundMeneger.Sounds.Explosion, Transform.position,2F);
        Collider[] colliders = Physics.OverlapSphere(Transform.position, Radius * 5F, LayerMask.GetMask(new string[] { "Entity", "Player"}));
        for (int i = 0; i < colliders.Length; i++)
        {
            var item = colliders[i].GetComponentInParent<ItemEngine>();
            Rigidbody rb;
            if (item)
                rb = item.Rigidbody;
            else
            {
                rb = colliders[i].GetComponent<Rigidbody>();
            }
            IAlive Alive = colliders[i].GetComponentInParent<IAlive>();
            if (Alive != null && Vector3.Distance(Alive.Transform.position, Transform.position) < Radius)
            {
                    Alive.Dead();
            }
            if (Vector3.Distance(colliders[i].transform.position, Transform.position) < Radius * 0.8F)
            {
                if (item)
                {
                    if (item is TNT tnt)
                    {
                        if (tnt.isActivate)
                            tnt.Delete(0.32F);
                        else
                        tnt.InteractionTimeActivation(0.32F);
                        ExplosionForce(rb, 5F);
                    }
                    else
                        item.Delete();
                }
                else if(Alive == null)
                    Destroy(colliders[i].gameObject);
                else ExplosionForce(rb);
            }
            else
                ExplosionForce(rb);
        }
    }


}

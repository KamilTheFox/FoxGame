using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using Random = UnityEngine.Random;

public abstract class Detonator : ItemEngine, IInteractive
{
    public abstract float Radius { get; }

    public float Force => Radius * 120F;
    public abstract float TimeDetonate { get; }
    public abstract float Volume { get; }
    public abstract EffectItem ExplosionEffect { get; }

    public bool isActivate { get; protected set; }
    public void Interaction()
    {
        InteractionTimeActivation(TimeDetonate);
    }

    public event Action onInteractionDetonator;
    private void InteractionTimeActivation(float time)
    {
        if (isActivate)
            return;
        isActivate = true;
        onInteractionDetonator?.Invoke();
        Delete(time);
    }
    protected override void onDestroy()
    {
        if (isActivate && gameObject.scene.isLoaded)
        {
            Explosion();
        }
    }
    public void Diactivate() => isActivate = false;
    
    void ExplosionForce (Rigidbody rb , float ForceDelta = 1F)
    {
        if (!rb) return;
        rb.AddExplosionForce(Force / 3 / ForceDelta, Transform.position, Radius * (float)(6F / 3 * 1.2F));
        //for (int i = 6; i > 1; i -= 2)
        //{
        //    rb.AddExplosionForce(Force / i / ForceDelta, Transform.position, Radius * (float)(6F / i * 1.2F));
        //}
    }
    private Collider[] OverlapSphere(LayerMask layer, float Radius)
    {
        return Physics.OverlapSphere(Transform.position, Radius, layer);
    }
    void Explosion()
    {
        Destroy(GetEffect(ExplosionEffect, Transform.position), 8F);
        SoundMeneger.PlayPoint(SoundMeneger.Sounds.Explosion, Transform.position, true , Volume);

        Collider[] colliders = OverlapSphere(MasksProject.EntityPlayer, Radius * 5F);
        Collider[] collidersKill = OverlapSphere(MasksProject.EntityPlayer, Radius * 0.6F);
        for (int i = 0; i < colliders.Length; i++)
        {
            EntityEngine item = colliders[i].GetComponentInParent<EntityEngine>();
            Rigidbody rb;
                if (item && item.Rigidbody)
                    rb = item.Rigidbody;
                else
                    rb = colliders[i].GetComponentInParent<Rigidbody>();
            float Distance = Vector3.Distance(colliders[i].transform.position, Transform.position);
            IDiesing Alive = colliders[i].GetComponentInParent<IDiesing>();
            bool isDeath = collidersKill.Contains(colliders[i]);
            if (Alive != null && isDeath)
            {
                Alive.Death();
            }
            if (isDeath)
            {
                if (item is ItemEngine)
                {
                    if (item is Detonator Detonators)
                    {
                        new Random.State();
                        float Detonate = Random.Range(0.32F, TimeDetonate);
                        if (Detonators.isActivate)
                            Detonators.Delete(Detonate);
                        else
                            Detonators.InteractionTimeActivation(Detonate);
                        ExplosionForce(rb, 5F);
                        continue;
                    }
                    else if (item is not IDiesing && isDeath)
                        item.Delete();
                }
            }
            ExplosionForce(rb);
        }
    }

    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
            {
            base.GetTextUI(),
            new TextUI(() => new object[] {"\n[",LText.KeyCodeF ,"] -", LText.Detonate })
            });
    }
}

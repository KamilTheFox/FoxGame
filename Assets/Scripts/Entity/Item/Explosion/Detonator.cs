using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine.Events;

public abstract class Detonator : ItemEngine, IInteractive
{
    public abstract float Radius { get; }

    public float Force => Radius * 120F;
    public abstract float TimeDetonate { get; }
    public abstract float Volume { get; }

    public UnityEvent<Detonator, Collision> OnCollision { get; set; } = new UnityEvent<Detonator, Collision>();

    Rigidbody IInteractive.Rigidbody => base.Rigidbody;

    public event Action onInteractionDetonator;

    protected override void OnStart()
    {
       // IsDetectionModeContinuousDynamic = true;
    }
    public abstract EffectItem ExplosionEffect { get; }

    public bool isActivate { get; protected set; }
    public void Interaction()
    {
        InteractionTimeActivation(TimeDetonate);
    }
    private void InteractionTimeActivation(float time)
    {
        if (isActivate)
            return;
        isActivate = true;
        onInteractionDetonator?.Invoke();
        Delete(time);
    }
    public void OnCollisionEnter(Collision collision)
    {
        OnCollision.Invoke(this,collision);
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
    }
    private Collider[] OverlapSphere(LayerMask layer, float Radius)
    {
        return Physics.OverlapSphere(Transform.position, Radius, layer);
    }
    void Explosion()
    {
        Destroy(GetEffect(ExplosionEffect, Transform.position), 8F);
        SoundMeneger.PlayPoint(SoundMeneger.Sounds.Explosion, Transform.position, true , Volume).pitch = UnityEngine.Random.Range(0.8F,1.2F);

        Collider[] colliders = OverlapSphere(MasksProject.EntityPlayer, Radius * 5F);
        Collider[] collidersKill = OverlapSphere(MasksProject.EntityPlayer, Radius * 0.6F);
        List<IDiesing> diesings = new();
        List<IExplosionDamage> explosionDamage = new();
        List<EntityEngine> Items = new();
        foreach (Collider collider in collidersKill)
        {
            IDiesing loc;
            EntityEngine entity;
            if (!diesings.Contains(loc = collider.GetComponentInParent<IDiesing>()))
            {
                diesings.Add(loc);
            }
            if (!Items.Contains(entity = collider.GetComponentInParent<EntityEngine>()))
            {
                Items.Add(entity);
            }
        }
        for (int i = 0; i < colliders.Length; i++)
        {
            EntityEngine item = colliders[i].GetComponentInParent<EntityEngine>();
            Rigidbody rb;
                if (item && item.Rigidbody)
                    rb = item.Rigidbody;
                else
                    rb = colliders[i].GetComponentInParent<Rigidbody>();

            IDiesing Alive = colliders[i].GetComponentInParent<IDiesing>();
            bool isDeath = diesings.Contains(Alive);
            diesings.Remove(Alive);
            if (Alive != null && isDeath)
            {
                float ThicknessWall = 0;
                if (Physics.Linecast(transform.position, colliders[i].ClosestPoint(transform.position), out RaycastHit raycast1, MasksProject.Terrain) &&
                    Physics.Linecast(colliders[i].ClosestPoint(transform.position), transform.position, out RaycastHit raycast2, MasksProject.Terrain))
                    ThicknessWall = Vector3.Distance(raycast1.point, raycast2.point);
                if (ThicknessWall < 1.5)
                {
                    Alive.Death();
                    if(Alive is IExplosionDamage damage)
                        damage.Explosion();
                }
            }
            
            if (Items.Contains(item) && item is ItemEngine)
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
                else if (item is not IDiesing)
                    item.Delete();
                if(item is IExplosionDamage damageExp)
                    damageExp.Explosion();
            }
            Items.Remove(item);
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

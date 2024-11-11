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
    public override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
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
    void ExplosionForce (Rigidbody rb, float ForceDelta = 1F, IAppliedExplosionForce explosionForce = null)
    {
        if (!rb) return;
        float force = Force / 3 / ForceDelta;
        Vector3 center = Transform.position;
        float radius = Radius * (float)(6F / 3 * 1.2F);
        rb.AddExplosionForce(force, center, radius);
        if (explosionForce != null)
            explosionForce.SetExplosionForce(force, center, radius);
    }
    private Collider[] OverlapSphere(LayerMask layer, float Radius)
    {
        return Physics.OverlapSphere(Transform.position, Radius, layer);
    }

    static List<IDiesing> diesings = new();
    static List<IExplosionDamaged> explosionDamage = new();
    static List<EntityEngine> Items = new();

    void Explosion()
    {
        if (gameObject.activeSelf) return;
        Destroy(GetEffect(ExplosionEffect, Transform.position), 8F);
        SoundMeneger.PlayPoint(SoundMeneger.Sounds.Explosion, Transform.position, true , Volume).pitch = UnityEngine.Random.Range(0.8F,1.2F);

        Collider[] colliders = OverlapSphere(MasksProject.EntityPlayer, Radius * 5F);
        Collider[] collidersKill = OverlapSphere(MasksProject.EntityPlayer, Radius * 0.6F);

        Items.Clear();
        explosionDamage.Clear();
        diesings.Clear();

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

            Vector3 pointHit = colliders[i].transform.position;

            MeshCollider mesh = colliders[i] as MeshCollider;

            if (mesh && !mesh.convex || mesh != null)
            {
                pointHit = colliders[i].ClosestPoint(transform.position);
            }

            float distance = Vector3.Distance(transform.position, pointHit);

            IDiesing Alive = colliders[i].GetComponentInParent<IDiesing>();

            IAppliedExplosionForce appForce = null;

            if(Alive is IAppliedExplosionForce force)
            {
                appForce = force;
            }
            else if (item is IAppliedExplosionForce force2)
            {
                appForce = force2;
            }

            ExplosionForce(rb, explosionForce: appForce);

            bool isDeath = diesings.Contains(Alive);
            diesings.Remove(Alive);
            if (Alive != null && isDeath)
            {
                float ThicknessWall = 0;
                
                if (Physics.Linecast(transform.position, pointHit, out RaycastHit raycast1, MasksProject.Terrain) &&
                    Physics.Linecast(pointHit, transform.position, out RaycastHit raycast2, MasksProject.Terrain))
                    ThicknessWall = Vector3.Distance(raycast1.point, raycast2.point);
                if (ThicknessWall < 1.5)
                {
                    Alive.Death();
                    if (Alive is IExplosionDamaged damage)
                        damage.Explosion(distance);
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
                    continue;
                }
                else if (item is not IDiesing)
                    item.Delete();
                if (item is IExplosionDamaged damageExp)
                    damageExp.Explosion(distance);
            }
            Items.Remove(item);
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

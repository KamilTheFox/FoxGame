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
        float massCompensation = Mathf.Min(1f / rb.mass, 1f);
        float force = Force * 1.5F / ForceDelta * massCompensation;
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

        CreateExplosionEffects();
        var (colliders, collidersKill) = GatherColliders();
        InitializeExplosionLists(collidersKill);
        ProcessExplosionEffects(colliders);
    }

    private void CreateExplosionEffects()
    {
        Destroy(GetEffect(ExplosionEffect, Transform.position), 8F);
        SoundMeneger.PlayPoint(SoundMeneger.Sounds.Explosion, Transform.position, true, Volume)
            .pitch = UnityEngine.Random.Range(0.8F, 1.2F);
    }

    private (Collider[] colliders, Collider[] collidersKill) GatherColliders()
    {
        return (
            OverlapSphere(MasksProject.EntityPlayer, Radius * 5F),
            OverlapSphere(MasksProject.EntityPlayer, Radius * 0.6F)
        );
    }

    private void InitializeExplosionLists(Collider[] collidersKill)
    {
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
    }

    private void ProcessExplosionEffects(Collider[] colliders)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            var collider = colliders[i];
            var explosionData = GatherExplosionData(collider);
            ApplyExplosionForce(explosionData);
            ProcessDiesingEntity(explosionData);
            ProcessItemEntity(explosionData);
        }
    }

    private struct ExplosionData
    {
        public EntityEngine Item;
        public Rigidbody Rigidbody;
        public Vector3 PointHit;
        public float Distance;
        public IDiesing Alive;
        public IAppliedExplosionForce AppliedForce;
    }

    private ExplosionData GatherExplosionData(Collider collider)
    {
        var data = new ExplosionData
        {
            Item = collider.GetComponentInParent<EntityEngine>(),
            Rigidbody = collider.attachedRigidbody,
            PointHit = collider.transform.position,
            Alive = collider.GetComponentInParent<IDiesing>()
        };

        if (collider is MeshCollider mesh && mesh.convex || collider is not MeshCollider)
        {
            data.PointHit = collider.ClosestPoint(transform.position);
        }

        data.Distance = Vector3.Distance(transform.position, data.PointHit);

        if (data.Alive is IAppliedExplosionForce force)
        {
            data.AppliedForce = force;
        }
        else if (data.Item is IAppliedExplosionForce force2)
        {
            data.AppliedForce = force2;
        }

        return data;
    }

    private void ApplyExplosionForce(ExplosionData data)
    {
        ExplosionForce(data.Rigidbody, explosionForce: data.AppliedForce);
    }

    private void ProcessDiesingEntity(ExplosionData data)
    {
        bool isDeath = diesings.Contains(data.Alive);
        diesings.Remove(data.Alive);
        if (data.Alive != null && isDeath)
        {
            float ThicknessWall = CheckWallThickness(data.PointHit);
            if (ThicknessWall < 1.5)
            {
                data.Alive.Death();
                if (data.Alive is IExplosionDamaged damage)
                    damage.Explosion(data.Distance);
            }
        }
    }

    private float CheckWallThickness(Vector3 pointHit)
    {
        if (Physics.Linecast(transform.position, pointHit, out RaycastHit raycast1, MasksProject.Terrain) &&
            Physics.Linecast(pointHit, transform.position, out RaycastHit raycast2, MasksProject.Terrain))
            return Vector3.Distance(raycast1.point, raycast2.point);
        return 0;
    }

    private void ProcessItemEntity(ExplosionData data)
    {
        if (Items.Contains(data.Item) && data.Item is ItemEngine)
        {
            if (data.Item is Detonator Detonators)
            {
                new Random.State();
                float Detonate = Random.Range(0.32F, TimeDetonate);
                if (Detonators.isActivate)
                    Detonators.Delete(Detonate);
                else
                    Detonators.InteractionTimeActivation(Detonate);
            }
            else if (data.Item is not IDiesing)
            {
                data.Item.Delete();
                if (data.Item is IExplosionDamaged damageExp)
                    damageExp.Explosion(data.Distance);
            }

            Items.Remove(data.Item);
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

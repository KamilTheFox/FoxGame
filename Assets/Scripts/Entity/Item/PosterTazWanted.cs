using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tweener;
public class PosterTazWanted : ItemEngine, IAlive
{
    public bool IsDead { get; set; }

    public Action<Collision, GameObject> BehaviorFromCollision => BehaviorFromCollisionEnter;

    private Animator Animator;

    IRegdoll regdoll;

    public const float WithstandingForce = 140F;

    protected override void OnStart()
    {
        regdoll = new Regdoll(Animator, this);
    }

    private IEnumerator Crumble()
    {
        yield return new WaitForSeconds(0.1F);
        Vector3 velosity = Rigidbody.velocity;
        regdoll.Activate();

        List<MeshRenderer> Render = Transform.GetComponentsInChildren<MeshRenderer>().ToList();
        Render.ForEach(chield =>
        {
            if (!chield.name.Contains("Collider"))
            {
                chield.transform.parent = null;
                Rigidbody rigidbodyChield = chield.gameObject.AddComponent<Rigidbody>();
                rigidbodyChield.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rigidbodyChield.interpolation = RigidbodyInterpolation.Interpolate;
                rigidbodyChield.velocity = velosity;
            }
        });
        Delete();
        yield return new WaitForSeconds(10F);
        Render.ForEach(renderer => renderer.material.ToFadeMode());

        foreach (MeshRenderer renderer in Render)
        {
            Tween.SetColor(renderer.transform, new Color(0, 0, 0, 0), 3F).IgnoreAdd(IgnoreARGB.RGB).ToCompletion(() =>
            {
                Destroy(renderer.gameObject);
            });
        }
        yield break;
    }
    private void BehaviorFromCollisionEnter(Collision collision, GameObject Attacked)
    {
        Rigidbody AttackingRig = collision.gameObject.GetComponent<Rigidbody>();
        float Force = Rigidbody.mass * Rigidbody.velocity.magnitude;
        float velosityAttacking = collision.relativeVelocity.magnitude;
        if (AttackingRig && velosityAttacking > 0.05F)
        {
            Force += AttackingRig.mass * velosityAttacking;
        }
        if (Force > WithstandingForce)
        {
            Dead();
        }
    }

    public void Dead()
    {
        if (IsDead) return;
        IsDead = true;
        StartCoroutine(Crumble());
    }

    public override TextUI GetTextUI()
    {
        return new TextUI(() => new object[]
        {
            new TextUI(() => new object[] {itemType.ToString() }),
            new TextUI(() => new object[] { "\n[", LText.KeyCodeE ,"] - ",LText.Take ,"/",LText.Leave }),
            new TextUI(() => new object[] {"\n[", LText.KeyCodeMouse0 ,"] - ",LText.Drop }),
        });
    }

}

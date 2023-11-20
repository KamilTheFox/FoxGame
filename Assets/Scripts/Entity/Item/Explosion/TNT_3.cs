using UnityEngine;

public class TNT_3 : Detonator, ITakenEntity, IDropEntity
{
    public override EffectItem ExplosionEffect => EffectItem.Medium_Explosion;
    public override float Radius => 8F;

    public override float TimeDetonate => 7F;

    public override float Volume => 0.7F;

    Rigidbody IDropEntity.Rigidbody => Rigidbody;

    protected override void OnAwake()
    {
        base.OnAwake();
        base.onInteractionDetonator += OnInteractionDetonator;
    }

    private void OnInteractionDetonator()
    {
        GameObject Detonate = EntityCreate.GetEntity(EffectItem.TNT_3_Detonate, Transform).GetPrefab;
        Transform transformDetonate = Detonate.transform;
        SoundMeneger.Play(SoundMeneger.Sounds.TNT_Detonate,
            Detonate.GetComponent<AudioSource>());
        transformDetonate.SetParent(transform);
    }
}

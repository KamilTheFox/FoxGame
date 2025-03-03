using UnityEngine;
using VulpesTool;

public class TNT : Detonator, ITakenEntity, IDropEntity
{
    public override EffectItem ExplosionEffect => EffectItem.Low_Explosion;
    public override float Radius => 4F;

    public override float TimeDetonate => 7F;

    public override float Volume => 0.5F;

    Rigidbody IDropEntity.Rigidbody => Rigidbody;

    protected override void OnAwake()
    {
        base.OnAwake();
        base.onInteractionDetonator += OnInteractionDetonator;
    }

    private void OnInteractionDetonator()
    {
        GameObject Detonate = EntityCreate.GetEntity(EffectItem.TNT_Detonate, transform).GetPrefab;
        Transform transformDetonate = Detonate.transform;
        SoundMeneger.Play(SoundMeneger.Sounds.TNT_Detonate,
            Detonate.GetComponent<AudioSource>());
        transformDetonate.SetParent(transform);
    }
}

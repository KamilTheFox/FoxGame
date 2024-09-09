using UnityEngine;

public class Barrel_Detonator : Detonator
{
    public override EffectItem ExplosionEffect => EffectItem.Big_Explosion;
    public override float Radius => 25F;

    public override float TimeDetonate => 30F;

    public override float Volume => 2F;

    protected override void OnAwake()
    {
        Stationary = GameState.IsAdventure;
        VolumeObject = 0.1F;
        base.onInteractionDetonator += OnInteractionDetonator;
    }
    private void OnInteractionDetonator()
    {
        GameObject Detonate = EntityCreate.GetEntity(EffectItem.Barrel_Detonator_Detonate, Transform).GetPrefab;
        Transform transformDetonate = Detonate.transform;
        SoundMeneger.Play(SoundMeneger.Sounds.TNT_Detonate,
            Detonate.GetComponent<AudioSource>());
        transformDetonate.SetParent(transform);
    }
    
}

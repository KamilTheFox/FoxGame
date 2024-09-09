using System.Collections;
using UnityEngine.Audio;
using UnityEngine;

public class SoundZone : MonoBehaviour
{
    [field: SerializeField] public AudioClip AudioZone { get; private set; }

    [field: SerializeField] public Collider[] GetColliders { get; private set; }

    [field: Range(0F,2F)]
    [field: SerializeField] public float BlendDistanse { get; private set; }

    [field: SerializeField] public AudioMixerSnapshot Snapshot { get; private set; }

    private void OnEnable()
    {
        GetColliders = GetComponents<Collider>();
        SoundMeneger.SoundBackgroundZone.Add(this);
    }
    
    private void OnDisable()
    {
        SoundMeneger.SoundBackgroundZone.Remove(this);
        GetColliders = new Collider[0];
    }
}

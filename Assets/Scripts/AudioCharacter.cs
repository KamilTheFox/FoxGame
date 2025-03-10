﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PlayerDescription;

[CreateAssetMenu(menuName = "AudioCharacter", fileName = "AudioCharacter")]
internal class AudioCharacter : ScriptableObject
{
    [field: SerializeField] public AudioClip[] MovementRun { get; private set; }

    [field: SerializeField] public AudioClip[] MovementWalk { get; private set; }

    [field: SerializeField] public AudioClip[] Swimming { get; private set; }

    [field: SerializeField] public AudioClip[] SwimEnter { get; private set; }

    [field: SerializeField] public AudioClip[] SwimExit { get; private set; }

    [field: SerializeField] public AudioClip[] Landing { get; private set; }

    [field: SerializeField] public AudioClip[] Jump { get; private set; }

    private AudioClip GetRandomAudio(AudioClip[] clips)
    {
        if(clips.Length < 1) return null;

        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }


    public void AddListnerEventInput(CharacterMotor input)
    {
        AnimatorCharacterInput characterInputAnim = input.GetComponent<AnimatorCharacterInput>();
        input.EventsMotor.EventMovement += (Velosity, isMove) =>
        {
            if (input.isCrouch) return;
            if (Velosity == Vector3.zero) return;
            if (isMove && !input.IsInAir)
                SoundMeneger.Play(input.isSwim ? GetRandomAudio(Swimming) :
                    input.isRun ? GetRandomAudio(MovementRun) :
                    GetRandomAudio(MovementWalk),
                    input.AudioSource, oneShoot: false);
        };
        input.EventsMotor[TypeAnimation.Landing].AddListener(() =>
        {
            if (characterInputAnim.IsPlayStateAnimator(TypeAnimation.Landing))
            {
                if (!Landing.Contains(input.AudioSource.clip))
                {
                    input.AudioSource.Stop();
                    SoundMeneger.Play(GetRandomAudio(Landing), input.AudioSource, oneShoot: false);
                }
            }
        });
        input.EventsMotor[TypeAnimation.Jump].AddListener(() =>
        {
            input.AudioSource.Stop();
            SoundMeneger.Play(GetRandomAudio(Jump), input.AudioSource, oneShoot: false);
        });
        input.EventsMotor[TypeAnimation.Swimming].AddListener(() =>
        {
            input.AudioSource.Stop();
            SoundMeneger.Play(GetRandomAudio(SwimEnter), input.AudioSource, oneShoot: false);
        });

        input.EventsMotor[TypeAnimation.DontSwimming].AddListener(() =>
        {
            input.AudioSource.Stop();
            SoundMeneger.Play(GetRandomAudio(SwimExit), input.AudioSource, oneShoot: false);
        });
    }



}

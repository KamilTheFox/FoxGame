using System;
using UnityEngine;

namespace PlayerDescription
{
    public abstract class BaseAnimate
    {
        [SerializeField] protected readonly AnimatorCharacterInput animatorCharacter;

        public CharacterBody PBody => animatorCharacter.PBody;

        public Animator Animator => animatorCharacter.AnimatorHuman;

        public Transform Transform => PBody.Transform;

        public BaseAnimate(AnimatorCharacterInput animatorCharacterInput)
        {
            animatorCharacter = animatorCharacterInput;
        }

        protected AnimationClip[] AnimationClip => animatorCharacter.AnimationClips;

        public abstract void StartAnimation();
    }
}

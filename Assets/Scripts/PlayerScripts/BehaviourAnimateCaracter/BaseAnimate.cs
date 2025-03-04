using System;
using UnityEngine;

namespace PlayerDescription
{
    public abstract class BaseAnimate
    {
        [SerializeField] protected readonly CharacterMediator mediator;

        public CharacterBody PBody => mediator.Body;

        public Animator Animator => mediator.AnimatorInput.AnimatorHuman;

        public Transform Transform => mediator.Transform;

        public BaseAnimate(CharacterMediator _mediator)
        {
            mediator = _mediator;
        }

        protected AnimationClip[] AnimationClip => mediator.AnimatorInput.AnimationClips;

        public abstract void StartAnimation();
    }
}

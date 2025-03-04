using System;
using System.Collections;
using UnityEngine;

namespace PlayerDescription
{
    public class AnimationTook : BaseAnimate
    {
        private const int ANIMATE_LAYER_ARM = 1;
        private const int ANIMATE_LAYER_HAND_R = 2;
        private const int ANIMATE_LAYER_HAND_L = 3;
        private Animator animator;

        public AnimationTook(CharacterMediator mediator) : base(mediator)
        {
            animator = mediator.AnimatorInput.AnimatorHuman;
            animator.SetLayerWeight(ANIMATE_LAYER_HAND_R, 0f);
            animator.SetLayerWeight(ANIMATE_LAYER_HAND_L, 0f);
        }

        public override void StartAnimation()
        {
            animator.Play("TookStick", ANIMATE_LAYER_HAND_R);
            mediator.AnimatorInput.SmoothWeightChange(ANIMATE_LAYER_HAND_R, 1F, 0.3F);
            mediator.AnimatorInput.WeightArm = 0.3F;
            mediator.AnimatorInput.SmoothWeightChange(ANIMATE_LAYER_ARM, mediator.AnimatorInput.WeightArm, 0.3F);
        }

        /// <summary>
        /// Не работает хуйня
        /// </summary>
        private void FreezeAnimate()
        {
            AnimatorStateInfo animatorState = animator.GetCurrentAnimatorStateInfo(ANIMATE_LAYER_HAND_R);
            float clipLength = animatorState.length;
            animator.PlayInFixedTime(animatorState.fullPathHash, ANIMATE_LAYER_HAND_R, clipLength / 4);
            animator.speed = 0;
        }
    }
}

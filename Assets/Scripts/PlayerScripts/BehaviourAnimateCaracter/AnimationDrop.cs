using System;
using System.Collections;
using UnityEngine;

namespace PlayerDescription
{
    public class AnimationDrop : BaseAnimate
    {
        private const int ANIMATE_LAYER_ARM = 1;
        private const int ANIMATE_LAYER_HAND_R = 2;
        private const int ANIMATE_LAYER_HAND_L = 3;
        private Animator animator;

        public AnimationDrop(CharacterMediator mediator) : base(mediator)
        {
            animator = mediator.AnimatorInput.AnimatorHuman;
        }

        public override void StartAnimation()
        {
            animator.Play("Idle", ANIMATE_LAYER_HAND_R);
            mediator.AnimatorInput.WeightArm = 0F;
            mediator.AnimatorInput.SmoothWeightChange(ANIMATE_LAYER_HAND_R, 0F, 0.3F);
            mediator.AnimatorInput.SmoothWeightChange(ANIMATE_LAYER_ARM, mediator.AnimatorInput.WeightArm, 0.3F);
        }
    }
}

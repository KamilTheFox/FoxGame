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

        public AnimationDrop(AnimatorCharacterInput animatorCharacterInput) : base(animatorCharacterInput)
        {
            animator = animatorCharacterInput.AnimatorHuman;
        }

        public override void StartAnimation()
        {
            animator.Play("Idle", ANIMATE_LAYER_HAND_R);
            animatorCharacter.WeightArm = 0F;
            animatorCharacter.SmoothWeightChange(ANIMATE_LAYER_HAND_R, 0F, 0.3F);
            animatorCharacter.SmoothWeightChange(ANIMATE_LAYER_ARM, animatorCharacter.WeightArm, 0.3F);
        }
    }
}

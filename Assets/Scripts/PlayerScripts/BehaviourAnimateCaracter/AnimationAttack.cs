using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PlayerDescription
{
    public class AnimationAttack : BaseAnimate
    {
        private const int ANIMATE_LAYER_ARM = 1;
        private Animator animator;
        private Action onAnimationFinished;

        public event Action OnAnimationFinished
        {
            add
            {
                onAnimationFinished += value;
            }
            remove
            {
                onAnimationFinished -= value;
            }
        }
        private Action onAnimationStarted;

        public event Action OnAnimationStarted
        {
            add
            {
                onAnimationStarted += value;
            }
            remove
            {
                onAnimationStarted -= value;
            }
        }

        public AnimationAttack(AnimatorCharacterInput animatorCharacterInput) : base(animatorCharacterInput)
        {
            animator = animatorCharacterInput.AnimatorHuman;
            animator.SetLayerWeight(ANIMATE_LAYER_ARM, 0F);
        }

        public override void StartAnimation()
        {
            OnAnimationFinished += OnAttackFinished;
            animator.Play("Attack " + Random.Range(1,4), ANIMATE_LAYER_ARM);
            animatorCharacter.SmoothWeightChange(ANIMATE_LAYER_ARM, 1f, 0.3f);
            animatorCharacter.StartCoroutine(WatchAnimation());
        }

        private void OnAttackFinished()
        {
            animatorCharacter.SmoothWeightChange(ANIMATE_LAYER_ARM, animatorCharacter.WeightArm, 0.3f);
            OnAnimationFinished -= OnAttackFinished;
        }

        private IEnumerator WatchAnimation()
        {
            yield return null;

            AnimatorStateInfo stateInfo;
            float normalizedTime;

            do
            {
                stateInfo = animator.GetCurrentAnimatorStateInfo(ANIMATE_LAYER_ARM);
                normalizedTime = stateInfo.normalizedTime;
                if (normalizedTime > 0.15f)
                {
                    onAnimationStarted?.Invoke();
                }
                yield return null;
            }
            while ((normalizedTime < 0.4f && (stateInfo.IsName("Attack 1") || stateInfo.IsName("Attack 2"))) || (normalizedTime < 0.8f && stateInfo.IsName("Attack 3")));

            onAnimationFinished?.Invoke();
        }
    }
}

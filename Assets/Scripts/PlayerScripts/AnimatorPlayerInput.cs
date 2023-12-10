using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

namespace PlayerDescription
{
    [RequireComponent(typeof(PlayerInput))]
    public class AnimatorPlayerInput : MonoBehaviour
    {
        private const float SmoothDelay = 3F;
        public Animator Animator { get; private set; }
        private PlayerBody PBody { get; set; }

        private PlayerInput InputC => PBody.PlayerInput;

        private InterceptionOnIK interceptionOnIK;

        private class InterceptionOnIK : MonoBehaviour
        {
            public UnityEvent onAnimatorIK = new UnityEvent();
            private void OnAnimatorIK()
            {
                onAnimatorIK.Invoke();
            }
        }
        private bool IsPlayStateAnimator(TypeAnimation animation)
        {
            return Animator.GetCurrentAnimatorStateInfo(0).IsName(animation.ToString());
        }
        private void SetTrigger(TypeAnimation animation)
        {
            Animator.SetTrigger(animation.ToString());
        }
        private void SetBool(TypeAnimation animation, bool target)
        {
            Animator.SetBool(animation.ToString(), target);
        }
        private void PlayForced(TypeAnimation animation)
        {
            Animator.Play(animation.ToString());
        }
        private void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
            //interceptionOnIK = Animator.gameObject.AddComponent<InterceptionOnIK>();
            PBody = GetComponent<PlayerBody>();
            InputC.eventInput.EventMovement += MoveAnimate;
            InputC.eventInput[TypeAnimation.Jump].AddListener(() => SetTrigger(TypeAnimation.Jump));
            InputC.eventInput[TypeAnimation.Fall].AddListener(() =>
            {
                if (IsPlayStateAnimator(TypeAnimation.Fall)) return;
                SetTrigger(TypeAnimation.Fall);
            });
            InputC.eventInput[TypeAnimation.Landing].AddListener(() =>
            {
                if (IsPlayStateAnimator(TypeAnimation.Landing)) return;
                SetBool(TypeAnimation.Landing, InputC._isGrounded);
            });
            InputC.AddFuncStopMovement(() =>
            {
                return IsPlayStateAnimator(TypeAnimation.Landing);
            });
            //interceptionOnIK.onAnimatorIK.AddListener(OnAnimatorIK);
        }
        [SerializeField] Vector3Int directionSmooth;
        [SerializeField] Vector3 farvardLo, farvardNorm;
        [SerializeField] bool isIKHead;

        [SerializeField] Vector3 vector;

        private Vector2 velositySmoothAnimation = Vector2.zero;
        private void MoveAnimate(Vector3 direction, bool move)
        {
            if (!move || direction == Vector3.zero)
            {
                Animator.SetInteger("Speed", 0);
                return;
            }
            int Speed = 2;
            if (InputC.isRun)
            {
                Speed = 5;
            }
            directionSmooth = Vector3Int.RoundToInt(transform.InverseTransformDirection(direction));

            Animator.SetInteger("Speed", Speed);

            Animator.SetFloat("RunForward", velositySmoothAnimation.x);
            Animator.SetFloat("RunRight", velositySmoothAnimation.y);
            velositySmoothAnimation = Vector2.Lerp(velositySmoothAnimation, new Vector2(directionSmooth.z, directionSmooth.x), Time.fixedDeltaTime * SmoothDelay);
        }

        public void StendUp()
        {
            PlayForced(TypeAnimation.StendUp);
        }
        /*
        private void OnAnimatorIK()
        {
            if (!isIKHead) return;
            if (!CameraControll.instance.IsPlayerControll(PBody)) return;
            Animator.SetLookAtWeight(1F);
            Transform head = Animator.GetBoneTransform(HumanBodyBones.Head);
            Animator.SetBoneLocalRotation(HumanBodyBones.Head, head.localRotation);
        }
        */
    }
}

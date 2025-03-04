using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerDescription
{
    public class AnimationStendUp : BaseAnimate
    {
        private IEnumerator corrutainStendUp;
        private bool IsStendUp { get; set; }
        private AnimationClip clipStendUpFace, clipStendUpBack;
        private Vector3 lastUpVector; // Для отладки

        public AnimationStendUp(CharacterMediator mediator) : base(mediator)
        {
            mediator.Input.AddFuncStopMovement(() => IsStendUp || IsPlayStateStendUp());

            if (AnimationClip == null) return;
            clipStendUpFace = AnimationClip[0];
            clipStendUpBack = AnimationClip[1];
        }

        private AnimationClip ClipStendUp
        {
            get
            {
                if (clipStendUpFace == null)
                {
                    clipStendUpFace = Animator.runtimeAnimatorController.animationClips
                        .First(clip => clip.name.ToLower().Contains("standup"));
                }
                return clipStendUpFace;
            }
        }


        public bool IsPlayStateStendUp()
        {
            AnimatorStateInfo state = Animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName("StendUp") || state.IsName("StendUpFace");
        }

        public override void StartAnimation()
        {
            if (corrutainStendUp != null) return;
            corrutainStendUp = StendUpCoroutin();
            mediator.StartCoroutine(corrutainStendUp);
        }

        private (Vector3 targetForward, bool isForwardDown) CalculateTargetOrientation(Transform hips)
        {
            Vector3 hipsDown = -hips.up;
            Vector3 hipsForward = hips.forward;

            Vector3 hipsDownFlat = Vector3.ProjectOnPlane(hipsDown, Vector3.up).normalized;

            bool isForwardDown = Vector3.Dot(hipsForward, Vector3.down) > 0.5f;

            if (isForwardDown)
            {
                return (hipsDownFlat, true);
            }
            else
            {
                return (-hipsDownFlat, false);
            }
        }

        private IEnumerator StendUpCoroutin()
        {
            CharacterBody PBody = base.PBody;

            var (targetForward, isForwardDown) = CalculateTargetOrientation(PBody.Hips);

            List<Transform> originBone = PBody.Hips.GetComponentsInChildren<Transform>().ToList();
            List<Bone> bonesStart = originBone.Select(x => (Bone)x).ToList();

            PBody.Regdool.Deactivate();

            Quaternion intitRotation = mediator.Transform.rotation;

            Quaternion targetRotation = Quaternion.LookRotation(targetForward, Vector3.up);
            mediator.Transform.rotation = targetRotation;

            mediator.AnimatorInput.AnimatorHuman.SetFloat("StandUpIndex", isForwardDown ? 1f : 0f);
            mediator.AnimatorInput.AnimatorHuman.Play(TypeAnimation.StendUp.ToString(), 0, 0);

            AnimationClip stendUpClip = isForwardDown && clipStendUpBack != null
                ? clipStendUpBack
                : ClipStendUp;

            stendUpClip.SampleAnimation(mediator.AnimatorInput.AnimatorHuman.gameObject, 0F);

            List<Bone> bonesEnd = originBone.Select(x => (Bone)x).ToList();

            mediator.AnimatorInput.AnimatorHuman.enabled = false;
            IsStendUp = true;
            float lerp = 0F;

            while (lerp < 1F)
            {
                lerp += Time.deltaTime;
                for (int i = 0; i < originBone.Count(); i++)
                {
                    originBone[i].localRotation = Quaternion.Lerp(bonesStart[i].rotation, bonesEnd[i].rotation, lerp);
                    originBone[i].localPosition = Vector3.Lerp(bonesStart[i].position, bonesEnd[i].position, lerp);
                }
                yield return null;
            }

            mediator.AnimatorInput.AnimatorHuman.enabled = true;
            IsStendUp = false;
            corrutainStendUp = null;
        }

    }
}

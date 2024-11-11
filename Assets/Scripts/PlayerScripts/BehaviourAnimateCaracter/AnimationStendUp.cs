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

        public AnimationStendUp(AnimatorCharacterInput animatorCharacterInput) : base(animatorCharacterInput)
        {
            animatorCharacterInput.InputC.AddFuncStopMovement(() => IsStendUp || IsPlayStateStendUp());

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
            animatorCharacter.StartCoroutine(corrutainStendUp);
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
            CharacterBody PBody = animatorCharacter.PBody;

            var (targetForward, isForwardDown) = CalculateTargetOrientation(PBody.Hips);

            List<Transform> originBone = PBody.Hips.GetComponentsInChildren<Transform>().ToList();
            List<Bone> bonesStart = originBone.Select(x => (Bone)x).ToList();

            PBody.Regdool.Deactivate();

            Quaternion intitRotation = PBody.Transform.rotation;

            Quaternion targetRotation = Quaternion.LookRotation(targetForward, Vector3.up);
            PBody.Transform.rotation = targetRotation;

            animatorCharacter.AnimatorHuman.SetFloat("StandUpIndex", isForwardDown ? 1f : 0f);
            animatorCharacter.AnimatorHuman.Play(TypeAnimation.StendUp.ToString(), 0, 0);

            AnimationClip stendUpClip = isForwardDown && clipStendUpBack != null
                ? clipStendUpBack
                : ClipStendUp;

            stendUpClip.SampleAnimation(animatorCharacter.AnimatorHuman.gameObject, 0F);

            List<Bone> bonesEnd = originBone.Select(x => (Bone)x).ToList();

            animatorCharacter.AnimatorHuman.enabled = false;
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

            animatorCharacter.AnimatorHuman.enabled = true;
            IsStendUp = false;
            corrutainStendUp = null;
        }

    }
}

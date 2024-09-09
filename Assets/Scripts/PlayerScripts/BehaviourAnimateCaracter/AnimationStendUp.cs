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
        public AnimationStendUp(AnimatorCharacterInput animatorCharacterInput) : base(animatorCharacterInput)
        {
            animatorCharacterInput.InputC.AddFuncStopMovement(() => IsStendUp || IsPlayStateStendUp());

            if (AnimationClip == null) return;
            clipStendUpFace = AnimationClip[0];
            clipStendUpBack = AnimationClip[1];
        }
        private IEnumerator corrutainStendUp;

        private bool IsStendUp { get; set; }

        private AnimationClip clipStendUpFace, clipStendUpBack;

        private AnimationClip ClipStendUp
        {
            get
            {
                if (clipStendUpFace == null)
                {
                    clipStendUpFace = Animator.runtimeAnimatorController.animationClips.First(clip => clip.name.ToLower().Contains("standup"));
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

        private IEnumerator StendUpCoroutin()
        {
            CharacterBody PBody = animatorCharacter.PBody;

            bool isHipsForwardDown = animatorCharacter.IsHipsForwardDown;

            List<Transform> originBone = PBody.Hips.GetComponentsInChildren<Transform>().ToList();

            List<Bone> bonesStart = originBone.Select(x => (Bone)x).ToList();

            Vector3 vectorUpHips = PBody.Hips.up;

            PBody.Regdool.Deactivate();

            vectorUpHips.Normalize();

            PBody.Transform.rotation = Quaternion.Euler(0, Vector3.Angle(Vector3.right, vectorUpHips), 0);

            animatorCharacter.Animator.SetFloat("StandUpIndex", isHipsForwardDown ? 1f : 0f);

            animatorCharacter.Animator.Play(TypeAnimation.StendUp.ToString(), 0, 0);

            AnimationClip stendUpClip;
            if (isHipsForwardDown && clipStendUpBack != null)
            {
                stendUpClip = clipStendUpBack;
            }
            else
            {
                stendUpClip = ClipStendUp;
                //PBody.Transform.rotation = Quaternion.Euler(PBody.Transform.eulerAngles + Vector3.up * 180);
            }
            stendUpClip.SampleAnimation(animatorCharacter.Animator.gameObject, 0);

            List<Bone> bonesEnd = originBone.Select(x => (Bone)x).ToList();

            animatorCharacter.Animator.enabled = false;
            IsStendUp = true;
            float lerp = 0F;

            while (lerp < 1F)
            {
                lerp += Time.deltaTime;
                for (int i = 0; i < originBone.Count(); i++)
                {
                    originBone[i].rotation = Quaternion.Lerp(bonesStart[i].rotation, bonesEnd[i].rotation, lerp);
                    originBone[i].position = Vector3.Lerp(bonesStart[i].position, bonesEnd[i].position, lerp);
                }
                yield return null;
            }
            animatorCharacter.Animator.enabled = true;

            IsStendUp = false;

            corrutainStendUp = null;

            yield break;
        }
    }
}

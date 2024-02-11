using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

namespace PlayerDescription
{
    [RequireComponent(typeof(CharacterInput))]
    public class AnimatorCharacterInput : MonoBehaviour
    {
        private const float SmoothDelay = 2F;
        public Animator Animator { get; private set; }
        private CharacterBody PBody { get; set; }

        private CharacterInput InputC => PBody.CharacterInput;

        private SkinnedMeshRenderer skinnedMeshRenderer;
        public SkinnedMeshRenderer SkinnedMeshRenderer
        {
            get
            {
                if(skinnedMeshRenderer == null)
                {
                    skinnedMeshRenderer = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>()
                    .Where(find => find.name.ToLower().Contains("mainskin")).ToArray()[0];
                }
                return skinnedMeshRenderer;
            }
        }

        private InterceptionOnIK interceptionOnIK;

        [SerializeField] private string[] BlandShapes;

        private class InterceptionOnIK : MonoBehaviour
        {
            public UnityEvent onAnimatorIK = new UnityEvent();
            private void OnAnimatorIK()
            {
                onAnimatorIK.Invoke();
            }
        }
        private void Reset()
        {
            if( SkinnedMeshRenderer.sharedMesh.blendShapeCount <= 0)
            {
                return;
            }
            BlandShapes = new string[SkinnedMeshRenderer.sharedMesh.blendShapeCount];
            for(int i = 0; i < BlandShapes.Length; i++)
            {
                BlandShapes[i] = SkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
            }
        }
        public bool IsPlayStateAnimator(TypeAnimation animation)
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
            Reset();
            //interceptionOnIK = Animator.gameObject.AddComponent<InterceptionOnIK>();
            PBody = GetComponent<CharacterBody>();
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
            InputC.eventInput[TypeAnimation.Fly].AddListener(() =>
            {
                if (IsPlayStateAnimator(TypeAnimation.Fall)) return;
                Animator.Play("Fall");
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
            if (!move || direction.magnitude < 0.01)
            {
                Animator.SetInteger("Speed", 0);
                velositySmoothAnimation = Vector3.zero;
                return;
            }
            int Speed = 2;
            if (InputC.isRun)
            {
                Speed = 5;
            }
            directionSmooth = Vector3Int.RoundToInt(transform.InverseTransformDirection(direction).normalized);

            Animator.SetInteger("Speed", Speed);

            Animator.SetFloat("RunForward", velositySmoothAnimation.x);
            Animator.SetFloat("RunRight", velositySmoothAnimation.y);
            velositySmoothAnimation = Vector2.Lerp(velositySmoothAnimation, new Vector2(directionSmooth.z, directionSmooth.x), Time.fixedDeltaTime * SmoothDelay);
        }
        /// <summary>
        /// value Range 0 - 100 
        /// </summary>
        /// <param name="value">Range 0 - 100 </param>
        public void BlinkingEyes(float value)
        {
            base.StartCoroutine(BlinkingEyesSmooth(value));
        }
        private IEnumerator BlinkingEyesSmooth(float value)
        {
            int indexBlinkingEyes = BlandShapes.ToList().IndexOf("BlinkingEyes");
            if(indexBlinkingEyes < 0) yield break;
            float lerp = 0F;
            while (lerp < 1F)
            {
                lerp += Time.deltaTime / 10;
                SkinnedMeshRenderer.SetBlendShapeWeight(indexBlinkingEyes, Vector2.Lerp(Vector2.right * SkinnedMeshRenderer.GetBlendShapeWeight(indexBlinkingEyes), Vector2.right * value, lerp).x);
                yield return null;
            }
            yield break;
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

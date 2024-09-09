using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayerDescription
{
    [RequireComponent(typeof(CharacterInput))]
    public class AnimatorCharacterInput : MonoBehaviour
    {
        private const float SmoothDelay = 2F;

        private Animator animator;
        public Animator Animator
        {
            get 
            {   if(animator == null)
                    animator = GetComponentInChildren<Animator>();
                return animator; 
            }
        }
        private CharacterBody pBody;

        public CharacterBody PBody 
        {
            get
            {
                if(!pBody)
                    pBody = GetComponent<CharacterBody>();
                return pBody;
            }
        }
        [field: SerializeField] public AnimationClip[] AnimationClips { get; private set; }

        private BaseAnimate[] baseAnimates;

        public CharacterInput InputC => PBody.CharacterInput;

        private SkinnedMeshRenderer skinnedMeshRenderer;

        public Vector3 AvatarPositionDefault { get; private set; }

        public bool IsHipsForwardDown => Vector3.Dot(PBody.Hips.forward,Vector3.down) > 0;

        public bool applyRootMotion
        {
            get
            {
                return Animator.applyRootMotion;
            }
            set
            {
                Animator.applyRootMotion = value;
                if (!Animator.applyRootMotion)
                {
                    Animator.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    Animator.transform.localPosition = Vector3.zero;
                }
            }
        }

        public SkinnedMeshRenderer SkinnedMeshRenderer
        {
            get
            {
                if(skinnedMeshRenderer == null)
                {
                    skinnedMeshRenderer = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().ToList()
                    .Find(find => find.name.ToLower().Contains("mainskin"));
                    if (skinnedMeshRenderer == null)
                        Debug.LogError("Don't Find Object Reference MainSkin, You may have forgotten to rename the main skin");
                }
                return skinnedMeshRenderer;
            }
        }

        private InterceptionOnIK interceptionOnIK;

        [SerializeField] private string[] BlandShapes;

        private class InterceptionOnIK : MonoBehaviour
        {
            public UnityEvent onAnimatorIK = new UnityEvent();
            public CharacterBody PBody { get; set; }
            private void OnAnimatorIK()
            {
                if(!PBody.CharacterInput.IsStopMovement)
                    onAnimatorIK.Invoke();
            }
        }
        [ContextMenu("ResetBlandShapes")]
        private void ResetBlandShapes()
        {
            if (SkinnedMeshRenderer == null)
                return;
            if (SkinnedMeshRenderer.sharedMesh.blendShapeCount <= 0)
            {
                return;
            }
            BlandShapes = new string[SkinnedMeshRenderer.sharedMesh.blendShapeCount];
            for (int i = 0; i < BlandShapes.Length; i++)
            {
                BlandShapes[i] = SkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
            }
#if UNITY_EDITOR
            if (!Application.isPlaying && PrefabUtility.IsPartOfAnyPrefab(gameObject))
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }


        public void AddListenerIK(ITrackIK iK)
        {
            if (iK == null) return;
            interceptionOnIK.onAnimatorIK.AddListener(iK.OnIK);
        }
        public void RemoveListenerIK(ITrackIK iK)
        {
            if (iK == null) return;
            interceptionOnIK.onAnimatorIK.RemoveListener(iK.OnIK);
        }
        public bool IsPlayStateAnimator(TypeAnimation animation)
        {
            return Animator.GetCurrentAnimatorStateInfo(0).IsName(animation.ToString());
        }
        public void SetTrigger(TypeAnimation animation)
        {
            Animator.SetTrigger(animation.ToString());
        }
        public void SetBool(TypeAnimation animation, bool target)
        {
            Animator.SetBool(animation.ToString(), target);
        }
        public void PlayForced(TypeAnimation animation)
        {
            Animator.Play(animation.ToString());
        }
        private void ApplayRootFalseClimbing()
        {
            InputC.Transform.position = Animator.transform.position;
            InputC.Transform.rotation = Animator.transform.rotation;
            applyRootMotion = false;
            if (CameraControll.instance.IsTypeViewPerson(typeof(CameraScripts.FirstPerson)))
            {
                CameraControll.instance.OnFirstPerson();
            }
        }
        public void OnAwake()
        {

            AvatarPositionDefault = Animator.transform.localPosition;

            ResetBlandShapes();
            interceptionOnIK = Animator.gameObject.AddComponent<InterceptionOnIK>();
            interceptionOnIK.PBody = PBody;

            baseAnimates = new BaseAnimate[]
            {
                new AnimationStendUp(this)
            };

            InputC.eventInput.EventMovement += MoveAnimate;

            InputC.eventInput[TypeAnimation.Jump].AddListener(() => SetTrigger(TypeAnimation.Jump));

            InputC.eventInput[TypeAnimation.Climbing].AddListener(() =>
            {
                if (IsPlayStateAnimator(TypeAnimation.Climbing)) return;
                if (CameraControll.instance.IsTypeViewPerson(typeof(CameraScripts.FirstPerson)))
                {
                    CameraControll.instance.transform.SetParent(PBody.Head);
                }
                applyRootMotion = true;
                Animator.transform.position = Animator.transform.position + Animator.transform.forward * 0.05f + Vector3.up * 0.15f;
                PBody.Rigidbody.velocity = Vector3.zero;
                SetTrigger(TypeAnimation.Climbing);
                Invoke(nameof(ApplayRootFalseClimbing), 3F);
            });

            InputC.eventInput[TypeAnimation.Fall].AddListener(() =>
            {
                if (IsPlayStateAnimator(TypeAnimation.Fall)) return;
                    SetTrigger(TypeAnimation.Fall);
            });

            InputC.eventInput[TypeAnimation.Swimming].AddListener(() =>
            {
                Animator.SetTrigger("Swimming");
                Animator.ResetTrigger("DontSwimming");
            });

            InputC.eventInput[TypeAnimation.DontSwimming].AddListener(() =>
            {
                Animator.SetTrigger("DontSwimming");
            });


            InputC.eventInput[TypeAnimation.Landing].AddListener(() =>
            {
                if (IsPlayStateAnimator(TypeAnimation.Landing)) return;
                    SetBool(TypeAnimation.Landing, InputC._isGrounded);
            });
            InputC.eventInput[TypeAnimation.Fly].AddListener(() =>
            {
                if (IsPlayStateAnimator(TypeAnimation.Fall)) return;
                    SetTrigger(TypeAnimation.Fall);
            });
            InputC.AddFuncStopMovement(() =>
            {
                return IsPlayStateAnimator(TypeAnimation.Landing);
            });
            InputC.AddFuncStopMovement(() =>
            {
                return  IsPlayStateAnimator(TypeAnimation.Climbing) || applyRootMotion;
            });
            //interceptionOnIK.onAnimatorIK.AddListener(OnAnimatorIK);
        }

        public void OnDisable()
        {
            interceptionOnIK.onAnimatorIK.RemoveAllListeners();
        }

        [SerializeField] Vector3Int directionSmooth;
        [SerializeField] Vector3 farvardLo, farvardNorm;

        [SerializeField] Vector3 vector;

        private Vector3 velositySmoothAnimation = Vector3.zero;
        private void MoveAnimate(Vector3 direction, bool move)
        {
            if (!move || direction.magnitude < 0.01)
            {
                Animator.SetInteger("Speed", 0);
                velositySmoothAnimation = Vector3.zero;
                Animator.SetFloat("RunForward", 0);
                Animator.SetFloat("RunRight", 0);
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
            Animator.SetFloat("RunRight", velositySmoothAnimation.z);

            Animator.SetFloat("UpDown", velositySmoothAnimation.y);

            velositySmoothAnimation = Vector3.Lerp(velositySmoothAnimation, new Vector3(directionSmooth.z, directionSmooth.y, directionSmooth.x), Time.fixedDeltaTime * SmoothDelay);
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

        public void StartUniqueAnimation<T>() where T : BaseAnimate
        {
            BaseAnimate animate = baseAnimates.FirstOrDefault(x => x is T);
            if(animate != null)
                animate.StartAnimation();
        }
        
        
        public void Happy()
        {
            base.StartCoroutine(HappyEyesSmooth());
        }
        private IEnumerator HappyEyesSmooth()
        {
            int indexBlinkingEyes = BlandShapes.ToList().IndexOf("HappyEyes");
            if (indexBlinkingEyes < 0) yield break;
            float lerp = 0F;
            while (lerp < 1F)
            {
                lerp += Time.deltaTime / 10;
                SkinnedMeshRenderer.SetBlendShapeWeight(indexBlinkingEyes, Vector2.Lerp(Vector2.right * SkinnedMeshRenderer.GetBlendShapeWeight(indexBlinkingEyes), Vector2.right * 100F, lerp).x);
                yield return null;
            }
            lerp = 0f;
            while (lerp < 60f)
            {
                lerp += Time.deltaTime;
            }
            lerp = 0f;
            while (lerp < 1F)
            {
                lerp += Time.deltaTime / 10;
                SkinnedMeshRenderer.SetBlendShapeWeight(indexBlinkingEyes, Vector2.Lerp(Vector2.right * SkinnedMeshRenderer.GetBlendShapeWeight(indexBlinkingEyes), Vector2.right * 0, lerp).x);
                yield return null;
            }
            yield break;
        }
        public void StendUp()
        {
            StartUniqueAnimation<AnimationStendUp>();
        }
    }
}

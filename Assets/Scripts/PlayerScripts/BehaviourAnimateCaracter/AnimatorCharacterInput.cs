using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using VulpesTool;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayerDescription
{
    [RequireComponent(typeof(CharacterInput))]
    public class AnimatorCharacterInput : VulpesMonoBehaviour
    {
        private const float SmoothDelay = 2F;

        private Animator animator;
        public Animator AnimatorHuman
        {
            get 
            {   if(animator == null)
                    animator = GetComponentInChildren<Animator>();
                return animator; 
            }
        }
        public Animator AnimatorCustom { get; private set; }

        private CharacterBody pBody;

        private UnityEvent onCompletedClimbing = new();

        public event Action OnCompletedClimbing
        {
            add
            {
                onCompletedClimbing.AddListener(value.Invoke);
            }
            remove
            {
                onCompletedClimbing.RemoveListener(value.Invoke);
            }
        }

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

        private List<IEnumerator> smoothChangeCorrutine;

        public float WeightArm { get; set; }

        public CharacterInput InputC => PBody.CharacterInput;

        [SerializeField] private float сorrectClimbinding;

        private SkinnedMeshRenderer skinnedMeshRenderer;

        public Vector3 AvatarPositionDefault { get; private set; }

        public bool IsHipsForwardDown => Vector3.Dot(PBody.Hips.forward,Vector3.down) > 0;

        public bool applyRootMotion
        {
            get
            {
                return AnimatorHuman.applyRootMotion;
            }
            set
            {
                AnimatorHuman.applyRootMotion = value;
                if (!AnimatorHuman.applyRootMotion)
                {
                    AnimatorHuman.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    AnimatorHuman.transform.localPosition = Vector3.zero;
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
                    .Find(find => find.name.ToLower().Contains("mainskin") && find.gameObject.activeSelf);
                    if (skinnedMeshRenderer == null)
                        Debug.LogError("Don't Find Object Reference MainSkin, You may have forgotten to rename the main skin");
                }
                return skinnedMeshRenderer;
            }
        }

        private InterceptionOnIK interceptionOnIK;

        [SerializeField] private string[] BlandShapes;

        [VulpesTool.SelectImplementation]
        [SerializeReference]
        private List<IAdditionalAnimate> additionalAnimates = new List<IAdditionalAnimate>();

        [SerializeField] private AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        Vector3Int directionSmooth;

        private Vector3 velositySmoothAnimation = Vector3.zero;

        private bool isBlinking;

        IEnumerator blinkingRoutineProcess;


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
        [Button("ResetBlandShapes", color: ColorsGUI.PastelRed)]
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
            if (iK == null || interceptionOnIK == null || interceptionOnIK.onAnimatorIK == null) return;
            interceptionOnIK.onAnimatorIK.AddListener(iK.OnIK);
        }
        public void RemoveListenerIK(ITrackIK iK)
        {
            if (iK == null) return;
            interceptionOnIK.onAnimatorIK.RemoveListener(iK.OnIK);
        }
        public bool IsPlayStateAnimator(TypeAnimation animation)
        {
            return AnimatorHuman.GetCurrentAnimatorStateInfo(0).IsName(animation.ToString());
        }
        public bool IsPlayStateAnimator(string animation)
        {
            return AnimatorHuman.GetCurrentAnimatorStateInfo(0).IsName(animation);
        }
        public void SetTrigger(TypeAnimation animation)
        {
            AnimatorHuman.SetTrigger(animation.ToString());
        }
        public void SetBool(TypeAnimation animation, bool target)
        {
            AnimatorHuman.SetBool(animation.ToString(), target);
        }
        public void PlayForced(TypeAnimation animation)
        {
            AnimatorHuman.Play(animation.ToString());
        }
        private void ApplayRootFalseClimbing()
        {
            InputC.Transform.position = AnimatorHuman.transform.position;
            InputC.Transform.rotation = AnimatorHuman.transform.rotation;
            applyRootMotion = false;
            PBody.Rigidbody.isKinematic = false;
            if (CameraControll.Instance.IsPlayerControll(PBody))
            {
                if (CameraControll.Instance.IsTypeViewPerson(typeof(CameraScripts.FirstPerson)))
                {
                    CameraControll.Instance.OnFirstPerson();
                }
            }
            onCompletedClimbing.Invoke();
        }
        public void OnAwake()
        {
            AvatarPositionDefault = AnimatorHuman.transform.localPosition;

            AnimatorHuman.Play("Idle01",0, UnityEngine.Random.Range(0f,1f));

            ResetBlandShapes();
            interceptionOnIK = AnimatorHuman.gameObject.AddComponent<InterceptionOnIK>();
            interceptionOnIK.PBody = PBody;

            baseAnimates = new BaseAnimate[]
            {
                new AnimationStendUp(this),
                new AnimationAttack(this),
                new AnimationTook(this),
                new AnimationDrop(this),
            };

            smoothChangeCorrutine = new();

            smoothChangeCorrutine.AddRange(new IEnumerator[AnimatorHuman.layerCount]);

            InputC.eventInput.EventMovement += MoveAnimate;

            InputC.eventInput[TypeAnimation.Jump].AddListener(() =>
            {
                SetTrigger(TypeAnimation.Jump);
            });

            InputC.eventInput[TypeAnimation.Climbing].AddListener(() =>
            {
                if (IsPlayStateAnimator(TypeAnimation.Climbing)) return;
                if (CameraControll.Instance.IsPlayerControll(PBody))
                {
                    if (CameraControll.Instance.IsTypeViewPerson(typeof(CameraScripts.FirstPerson)))
                    {
                        CameraControll.Instance.transform.SetParent(PBody.Head);
                    }
                }

                applyRootMotion = true;
                PBody.Rigidbody.isKinematic = true;
                AnimatorHuman.transform.position = AnimatorHuman.transform.position + AnimatorHuman.transform.forward * 0.05f + Vector3.up * 0.15f * сorrectClimbinding;
                Vector3 position = PBody.transform.position;
                PBody.transform.position = new Vector3(position.x, InputC.PointEdgePlaneClimbing.point.y - 1.38F, position.z);
                PBody.Rigidbody.velocity = Vector3.zero;

                SetTrigger(TypeAnimation.Climbing);
                Invoke(nameof(ApplayRootFalseClimbing), 2F);
            });

            InputC.eventInput[TypeAnimation.Fall].AddListener(() =>
            {
                if (IsPlayStateAnimator(TypeAnimation.Fall)) return;
                    SetTrigger(TypeAnimation.Fall);
            });

            InputC.eventInput[TypeAnimation.Swimming].AddListener(() =>
            {
                AnimatorHuman.SetTrigger("Swimming");
                AnimatorHuman.ResetTrigger("DontSwimming");
            });

            InputC.eventInput[TypeAnimation.DontSwimming].AddListener(() =>
            {
                AnimatorHuman.SetTrigger("DontSwimming");
            });



            InputC.eventInput[TypeAnimation.Crouch].AddListener(Crouch);

            InputC.AddFuncStopMovement(() =>
            {
                AnimatorStateInfo stateInfo = AnimatorHuman.GetCurrentAnimatorStateInfo(0);
                return stateInfo.IsName("StartCrouch") || stateInfo.IsName("EndCrouch");
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
            CreateAdditionalAnimate();
            pBody.OnDied += OnDisableAdditionAnimate;
            StartBlinking();
        }
        
        private void CreateAdditionalAnimate()
        {
            foreach (var additionalAnimate in additionalAnimates)
            {
                additionalAnimate.Initialize(this);
            }
        }
        private void ClearAdditionalAnimate()
        {
            pBody.OnDied -= ClearAdditionalAnimate;
            foreach (var additionalAnimate in additionalAnimates)
            {
                additionalAnimate.OnDestroy();
            }
        }
        private void Crouch()
        {
            if (InputC.isSwim) return;
            if (InputC.isPressCrouch)
            {
                if (!AnimatorHuman.GetBool("IsCrouch"))
                {
                    CancelInvoke(nameof(TimeOutResetCrouch));
                    AnimatorHuman.SetTrigger("Crouch");
                    AnimatorHuman.SetBool("IsCrouch", true);
                    InputC.isCrouch = true;
                    float height = InputC.CharacterCollider.height;
                    InputC.CharacterCollider.height = height * (2f / 3f);
                    InputC.CharacterCollider.center -= new Vector3(0, height * (1f / 6f), 0);
                    return;
                }
                
            }
            else if(AnimatorHuman.GetBool("IsCrouch"))
            {
                if(!InputC.CanStandUp())
                {
                    return;
                }
                AnimatorHuman.ResetTrigger("Crouch");
                Invoke(nameof(TimeOutResetCrouch),1f);
                float height = InputC.CharacterCollider.height;
                InputC.CharacterCollider.height = height * 1.5f;
                InputC.CharacterCollider.center += new Vector3(0, height * 0.25f, 0);
                AnimatorHuman.SetBool("IsCrouch", false);
            }
            
        }
        private void TimeOutResetCrouch()
        {
            InputC.isCrouch = false;
        }
        private void OnEnable()
        {
            foreach (var additionalAnimate in additionalAnimates)
            {
                additionalAnimate.Enable = true;
            }
        }

        private void OnDisableAdditionAnimate()
        {
            foreach (var additionalAnimate in additionalAnimates)
            {
                additionalAnimate.Enable = false;
            }
        }

        public void OnDisable()
        {
            OnDisableAdditionAnimate();
            interceptionOnIK.onAnimatorIK.RemoveAllListeners();
        }

        private void OnDestroy()
        {
            ClearAdditionalAnimate();
        }

        private void MoveAnimate(Vector3 direction, bool move)
        {
            if (!move || direction.magnitude < 0.01)
            {
                AnimatorHuman.SetInteger("Speed", 0);
                velositySmoothAnimation = Vector3.zero;
                AnimatorHuman.SetFloat("RunForward", 0);
                AnimatorHuman.SetFloat("RunRight", 0);
                return;
            }
            int Speed = 2;
            if (InputC.isRun)
            {
                Speed = 5;
            }
            directionSmooth = Vector3Int.RoundToInt(transform.InverseTransformDirection(direction).normalized);

            AnimatorHuman.SetInteger("Speed", Speed);

            AnimatorHuman.SetFloat("RunForward", velositySmoothAnimation.x);
            AnimatorHuman.SetFloat("RunRight", velositySmoothAnimation.z);

            AnimatorHuman.SetFloat("UpDown", velositySmoothAnimation.y);

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
            if (indexBlinkingEyes < 0) yield break;
            float lerp = 0F;
            float startValue = SkinnedMeshRenderer.GetBlendShapeWeight(indexBlinkingEyes);

            while (lerp < 1F)
            {
                lerp += Time.deltaTime * 5f;
                float curveValue = blinkCurve.Evaluate(lerp);
                SkinnedMeshRenderer.SetBlendShapeWeight(
                    indexBlinkingEyes,
                    Mathf.Lerp(startValue, value, curveValue)
                );
                yield return null;
            }
        }
        
        public void StartBlinking()
        {
            if (!isBlinking)
            {
                blinkingRoutineProcess = BlinkingRoutine();
                StartCoroutine(blinkingRoutineProcess);
            }
        }
        public void StopBlinking()
        {
            if (blinkingRoutineProcess == null) return;
            StopCoroutine(blinkingRoutineProcess);
            isBlinking = false;
            BlinkingEyes(0f);
        }
        private IEnumerator BlinkingRoutine()
        {
            isBlinking = true;

            while (!pBody.IsDie) 
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));

                if (pBody.IsDie) break; 

                yield return BlinkingEyesSmooth(100f);

                if (pBody.IsDie) break;

                yield return BlinkingEyesSmooth(0f);
            }
            if (pBody.IsDie)
            {
                yield return BlinkingEyesSmooth(100f); 
            }
            isBlinking = false;
        }
        public void SmoothWeightChange(int Layer, float targetWeight, float duration = 1F)
        {
            if (smoothChangeCorrutine[Layer] != null)
            {
                StopCoroutine(smoothChangeCorrutine[Layer]);
            }
            IEnumerator enumerator = smoothWeightChange(Layer, targetWeight, duration);
            StartCoroutine(enumerator);
            smoothChangeCorrutine[Layer] = enumerator;
        }
        private IEnumerator smoothWeightChange(int Layer ,float targetWeight, float duration = 1F)
        {
            float currentWeight = animator.GetLayerWeight(Layer);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float newWeight = Mathf.Lerp(currentWeight, targetWeight, elapsed / duration);
                animator.SetLayerWeight(Layer, newWeight);
                yield return null;
            }
            animator.SetLayerWeight(Layer, targetWeight);
            smoothChangeCorrutine[Layer] = null;
        }

        public T StartUniqueAnimation<T>() where T : BaseAnimate
        {
            BaseAnimate animate = baseAnimates.FirstOrDefault(x => x is T);
            if (animate != null)
                animate.StartAnimation();
            return (T)animate;
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
#if UNITY_EDITOR
        private Vector2 scrollPosition;
        [CreateGUI(title: "AdditionalAnimate",color: ColorsGUI.SuccessGreen,group: "Additional Animate Controls")]
        private void ConfigurableAdditionalAnimate()
        {
            GUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < additionalAnimates.Count; i++)
            {
                if (additionalAnimates[i] == null) continue;
                GUILayout.BeginHorizontal();
                additionalAnimates[i].Enable = GUILayout.Toggle(additionalAnimates[i].Enable ,$"Elem {i}: {additionalAnimates[i].Name}");
                if(GUILayout.Button("Reset"))
                {
                    additionalAnimates[i].Reset();
                }
                GUILayout.EndHorizontal();
                additionalAnimates[i].OnGUI();
            }
            GUILayout.EndScrollView();
        }
#endif
    }
}

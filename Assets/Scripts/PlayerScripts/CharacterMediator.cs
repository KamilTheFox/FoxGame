using System.Collections;
using System.Collections.Generic;
using PlayerDescription;
using UnityEngine;
using UnityEngine.Events;
using VulpesTool;
using UnityEngine.Networking.Types;
using UnityEditor;

namespace PlayerDescription
{
    public class CharacterMediator : VulpesMonoBehaviour
    {
        [SerializeField] private CharacterBody body;

        public CharacterBody Body => body;

        [SerializeField] private CharacterMotor motor;

        public CharacterMotor Motor => motor;

        [SerializeField] private AnimatorCharacterInput animatorInput;

        public AnimatorCharacterInput AnimatorInput => animatorInput;

        [SerializeField] private Rigidbody mainRigidbody;

        public Rigidbody MainRigidbody => mainRigidbody;

        [SerializeField] private CapsuleCollider mainCollider;

        public CapsuleCollider MainCollider => mainCollider;

        public Bounds BoundsCollider => mainCollider.bounds;

        [SerializeField] private AudioSource mainAudioSource;

        public AudioSource MainAudioSource => mainAudioSource;

        [SerializeField] private Transform mainTransform;

        public Transform Transform => mainTransform;

        private CameraControll cameraControll;

        public bool IsPlayerControll => cameraControll != null;

        void Awake()
        {
            AnimatorInput.SetMediator(this);
            Motor.SetMediator(this);
            Body.SetMediator(this);

            AnimatorInput.OnAwake();
            Motor.OnAwake();
            Body.OnAwake();
        }

        public void SetCameraController(CameraControll controll)
        {
            cameraControll = controll;
        }



        #region InspectorCustomized

        [CreateGUI(title: "", group: "Hiden Component", color: ColorsGUI.Yellow)]
        public void Customized()
        {
            bool bodyFlag = body.hideFlags.HasFlag(HideFlags.HideInInspector);
            if (GUILayout.Button($"Hide CharacterBody: {bodyFlag}"))
            {
                body.hideFlags = bodyFlag ? HideFlags.None : HideFlags.HideInInspector;
            }
            bool inputFlag = motor.hideFlags.HasFlag(HideFlags.HideInInspector);
            if (GUILayout.Button($"Hide Input: {inputFlag}"))
            {
                motor.hideFlags = inputFlag ? HideFlags.None : HideFlags.HideInInspector;
            }
            bool animatorInputFlag = animatorInput.hideFlags.HasFlag(HideFlags.HideInInspector);
            if (GUILayout.Button($"Hide AnimatorInput: {animatorInputFlag}"))
            {
                animatorInput.hideFlags = animatorInputFlag ? HideFlags.None : HideFlags.HideInInspector;
            }
            bool rigidBodyFlag = mainRigidbody.hideFlags.HasFlag(HideFlags.HideInInspector);
            if (GUILayout.Button($"Hide Rigidbody: {rigidBodyFlag}"))
            {
                mainRigidbody.hideFlags = rigidBodyFlag ? HideFlags.None : HideFlags.HideInInspector;
            }
            bool mainColliderFlag = mainCollider.hideFlags.HasFlag(HideFlags.HideInInspector);
            if (GUILayout.Button($"Hide Collider: {mainColliderFlag}"))
            {
                mainCollider.hideFlags = mainColliderFlag ? HideFlags.None : HideFlags.HideInInspector;
            }
            bool mainAudioSourceFlag = mainAudioSource.hideFlags.HasFlag(HideFlags.HideInInspector);
            if (GUILayout.Button($"Hide Collider: {mainAudioSourceFlag}"))
            {
                mainAudioSource.hideFlags = mainAudioSourceFlag ? HideFlags.None : HideFlags.HideInInspector;
            }
        }


        #endregion


    }
}

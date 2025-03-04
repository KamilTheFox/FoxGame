using System.Collections;
using System.Collections.Generic;
using PlayerDescription;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking.Types;

namespace PlayerDescription
{
    public class CharacterAdapter : MonoBehaviour
    {
        [SerializeField] private CharacterBody body;

        #region BodyAPI

        public event UnityAction OnDied
        {
            add
            {
                body.OnDied += value;
            }
            remove
            {
                body.OnDied -= value;
            }
        }

        public event UnityAction OnFell
        {
            add
            {
                body.OnFell += value;
            }
            remove
            {
                body.OnFell -= value;
            }
        }

        public Dictionary<CharacterBone, Transform> BoneBody => new()
        {
            [CharacterBone.Head] = body.Head,
            [CharacterBone.Chest] = body.Chest,
            [CharacterBone.RightHand] = body.RightHand,
            [CharacterBone.LeftHand] = body.LeftHand,
            [CharacterBone.Hips] = body.Hips,
        };

        public enum CharacterBone : byte
        {
            Chest = 1 << 0,
            RightHand = 1 << 1,
            LeftHand = 1 << 2,
            Head = 1 << 3,
            Hips = 1 << 4,
        }

        #endregion


        [SerializeField] private CharacterInput input;


        #region InputAPI



        #endregion

        [SerializeField] private AnimatorCharacterInput animatorInput;

        private CameraControll cameraControll;

        public bool IsPlayerControll => cameraControll != null;

        public void SetCameraController(CameraControll controll)
        {
            cameraControll = controll;
        }


        #region InspectorCustomized

        [VulpesTool.CreateGUI(title: "Hiden Component", color: VulpesTool.ColorsGUI.Yellow)]
        public void Customized()
        {
            bool bodyFlag = body.hideFlags.HasFlag(HideFlags.HideInInspector);
            if (GUILayout.Button($"Hide CharacterBody: {bodyFlag}"))
            {
                body.hideFlags = bodyFlag ? HideFlags.None : HideFlags.HideInInspector;
            }
            bool inputFlag = input.hideFlags.HasFlag(HideFlags.HideInInspector);
            if (GUILayout.Button($"Hide Input: {inputFlag}"))
            {
                input.hideFlags = inputFlag ? HideFlags.None : HideFlags.HideInInspector;
            }
            bool animatorInputFlag = animatorInput.hideFlags.HasFlag(HideFlags.HideInInspector);
            if (GUILayout.Button($"Hide AnimatorInput: {animatorInputFlag}"))
            {
                animatorInput.hideFlags = animatorInputFlag ? HideFlags.None : HideFlags.HideInInspector;
            }

        }

        #endregion


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PlayerDescription;

namespace CameraScripts
{
    public class ThirdUnlookPerson : IViewedCamera
    {
        IInputCaracter oldInputCaracter;
        private CharacterBody Player;
        private CameraControll _camera;

        private GameObject ThirdObject;

        private float DistanceViewCamera = 6F;

        private Vector3 ForvardRotate;

        private Quaternion SmoothRotate = Quaternion.identity;
        public ThirdUnlookPerson(CameraControll camera, CharacterBody _Player, int angleStartPerson)
        {
            Player = _Player;
            _camera = camera;
            ForvardRotate = Vector3.up * angleStartPerson;
        }
        public Vector3 RotateBody()
        {
            Vector3 vector = Player.CharacterInput.Velosity;
            if (vector != Vector3.zero)
            {
                ForvardRotate = SmoothRotate.eulerAngles;
                SmoothRotate = Quaternion.Lerp(SmoothRotate, Quaternion.LookRotation(new Vector3(vector.x, 0, vector.z)), Time.fixedDeltaTime * 7);
            }
            return ForvardRotate;
        }
        public Vector2 ViewAxisMaxVertical => new Vector2(-60, 60);

        public void ViewAxis(Transform camera, Vector3 euler)
        {
            camera.localEulerAngles = euler;
            float dir = Input.GetAxis("Mouse ScrollWheel");
            if (!ViewInteractEntity.isMoveItem && DistanceViewCamera + dir < 7F && DistanceViewCamera + dir > 1.5F)
            {
                DistanceViewCamera += dir;
            }
            Ray ray = new Ray(Player.transform.position + Vector3.up * 1.85F, -camera.forward);
            Vector3 position = ray.GetPoint(DistanceViewCamera);
            if (Physics.Raycast(ray, out RaycastHit hit, DistanceViewCamera, MasksProject.RigidObject))
            {
                position = hit.point + hit.normal * 0.01F;
            }
            _camera.Transform.position = position;
            ThirdObject.transform.rotation = Quaternion.LookRotation(new Vector3(camera.forward.x, 0, camera.forward.z));
        }

        public float DistanceView => 3F + DistanceViewCamera;
        public void Dispose()
        {
            Player.CharacterInput.IntroducingCharacter = oldInputCaracter;
            _camera.transform.SetParent(Player.Transform);
            Player.talkingTargetInteractEntity = true;
            Player.CharacterInput.ForwardTransform = null;
            GameObject.Destroy(ThirdObject);
        }

        public void Construct()
        {
            oldInputCaracter = Player.CharacterInput.IntroducingCharacter;
            Player.CharacterInput.IntroducingCharacter = new InputThirdUnlook(Player);
            ThirdObject = new GameObject("ThirdObject");
            Player.CharacterInput.ForwardTransform = ThirdObject.transform;
            _camera.transform.SetParent(null);
            Player.ResetTargetLook();
        }
        public class InputThirdUnlook : IInputCaracter
        {
            public InputThirdUnlook(CharacterBody _input)
            {
                input = _input;
                tacking = new TackingIK(_input.AnimatorInput.AnimatorHuman);
                tacking.LookIKSpeed = 10F;
                tacking.LookIKWeight = 1f;
                tacking.BodyWeight = 0.1f;
                tacking.ClampWeight = 0.35f;
                tacking.EyesWeight = 0.75f;
                tacking.HeadWeight = 0.35f;
            }
            private CharacterBody input;

            private TackingIK tacking;
            public bool IsRun => Input.GetKey(KeyCode.LeftShift);

            public bool IsCrouch => Input.GetKey(KeyCode.C);

            public void Enable()
            {
                tacking.Target = new GameObject("TargetInputDefault").transform;
                tacking.Target.SetParent(input.gameObject.transform);
                input.AnimatorInput.AddListenerIK(tacking);
            }

            public void Disable()
            {
                input.AnimatorInput.RemoveListenerIK(tacking);
                GameObject.Destroy(tacking.Target.gameObject);
            }

            bool IInputCaracter.Space()
            {
                return input.CharacterInput.isSwim ? Input.GetKey(KeyCode.Space) : Input.GetKeyDown(KeyCode.Space);
            }

            bool IInputCaracter.Shift()
            {
                return IsRun;
            }

            Vector3 IInputCaracter.Move(Transform source, out bool isMove)
            {
                if (input.talkingTargetInteractEntity)
                    if (tacking != null && input.InteractEntity != null)
                        tacking.Target.position = input.InteractEntity.pointTarget;
                return MovementMode.WASD(source, 1F, out isMove, true);
            }

        }
    }
}

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
        IInputCharacter oldInputCaracter;
        private CharacterMediator Player;
        private CameraControll _camera;

        private GameObject ThirdObject;

        private float DistanceViewCamera = 6F;

        private Vector3 ForvardRotate;

        private Quaternion SmoothRotate = Quaternion.identity;
        public ThirdUnlookPerson(CameraControll camera, CharacterMediator _Player, float angleStartPerson)
        {
            Player = _Player;
            _camera = camera;
            ForvardRotate = Vector3.up * angleStartPerson;
        }
        public Vector3 RotateBody()
        {
            Vector3 vector = Player.Input.Velosity;
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
            if (Player.IsPlayerControll)
                Player.Input.IntroducingCharacter = oldInputCaracter;
            _camera.transform.SetParent(Player.Transform);
            Player.Body.talkingTargetInteractEntity = true;
            Player.Input.ForwardTransform = null;
            Player.Body.ResetTargetLook();
            GameObject.Destroy(ThirdObject);
        }

        public void Construct()
        {
            oldInputCaracter = Player.Input.IntroducingCharacter;
            Player.Input.IntroducingCharacter = new _InputThirdUnlook(Player);
            ThirdObject = new GameObject("ThirdObject");
            Player.Input.ForwardTransform = ThirdObject.transform;
            _camera.transform.SetParent(null);
            Player.Body.ResetTargetLook();
        }
        private class _InputThirdUnlook : IInputCharacter
        {
            public _InputThirdUnlook(CharacterMediator _input)
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
            private CharacterMediator input;

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

            bool IInputCharacter.Space()
            {
                return input.Input.isSwim ? Input.GetKey(KeyCode.Space) : Input.GetKeyDown(KeyCode.Space);
            }

            bool IInputCharacter.Shift()
            {
                return IsRun;
            }

            Vector3 IInputCharacter.Move(Transform source, out bool isMove)
            {
                if (input.Body.talkingTargetInteractEntity)
                    if (tacking != null && input.Body.InteractEntity != null)
                        tacking.Target.position = input.Body.InteractEntity.pointTarget;
                return MovementMode.WASD(source, 1F, out isMove, true);
            }

        }
    }
}

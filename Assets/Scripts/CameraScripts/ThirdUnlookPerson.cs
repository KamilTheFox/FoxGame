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
        private CharacterBody Player;
        private CameraControll _camera;

        private GameObject ThirdObject;

        private float DistanceViewCamera = 6F;

        private Vector3 ForvardRotate;

        private Quaternion SmoothRotate = Quaternion.identity;
        public ThirdUnlookPerson(CameraControll camera, CharacterBody _Player)
        {
            Player = _Player;
            _camera = camera;
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
            _camera.Transform.position = position;
            ThirdObject.transform.rotation = Quaternion.LookRotation(new Vector3(camera.forward.x, 0, camera.forward.z));
        }

        public float DistanceView => 3F + DistanceViewCamera;
        public void Dispose()
        {
            _camera.transform.SetParent(Player.Transform);
            Player.talkingTargetInteractEntity = true;
            Player.CharacterInput.ForwardTransform = null;
            GameObject.Destroy(ThirdObject);
        }

        public void Construct()
        {
            ThirdObject = new GameObject("ThirdObject");
            Player.ResetTargetLook();

            Player.talkingTargetInteractEntity = false;
            Player.CharacterInput.ForwardTransform = ThirdObject.transform;
            _camera.transform.SetParent(null);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CameraScripts
{
    public class ThirdPerson : IViewedCamera
    {
        private Transform _parent;
        private CameraControll _camera;

        private float DistanceViewCamera = 6F;
        public ThirdPerson(CameraControll camera ,Transform Parent)
        {
            _parent = Parent;
            _camera = camera;
        }
        public void Construct()
        {
            _camera.Transform.parent = _parent;
            _camera.Transform.localPosition = Vector3.zero;
            _camera.Transform.localEulerAngles = Vector3.zero;
        }
        public Vector3 RotateBody()
        {
            return _camera.EulerHorizontal;
        }
        public Vector2 ViewAxisMaxVertical => new Vector2(-60, 60);

        public void ViewAxis(Transform camera, Vector3 euler)
        {
            _parent.localEulerAngles = -Vector3.left * euler.x;
            float dir = Input.GetAxis("Mouse ScrollWheel");
            if( !ViewInteractEntity.isMoveItem && DistanceViewCamera + dir < 6F && DistanceViewCamera + dir > 1.5F)
            {
                DistanceViewCamera += dir;
            }
            Ray ray = new Ray(_parent.transform.position, -_parent.forward);
            Vector3 position = ray.GetPoint(DistanceViewCamera);
            if (Physics.Raycast(ray,out RaycastHit hit, DistanceViewCamera, MasksProject.RigidObject))
            {
                position = hit.point + hit.normal * 0.01F;
            }
            _camera.Transform.position = position;
        }

        public float DistanceView => 3F + DistanceViewCamera;
        public void Dispose()
        {
            _parent.localEulerAngles = Vector3.zero;
        }
    }
}

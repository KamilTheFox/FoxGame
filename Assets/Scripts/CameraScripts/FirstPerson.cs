using UnityEngine;

namespace CameraScripts
{
    public class FirstPerson : IViewedCamera
    {
        CameraControll cameraControll;
        Transform _parent;
        public FirstPerson(CameraControll camera, Transform parent)
        {
            cameraControll = camera;
            _parent = parent;
        }
        public Vector2 ViewAxisMaxVertical => new Vector2(-90, 90);

        public void Construct()
        {
            cameraControll.Transform.parent = _parent;
            cameraControll.Transform.localPosition = Vector3.zero;
        }

        public void Dispose()
        {

        }

        public Vector3 RotateBody()
        {
            return cameraControll.EulerHorizontal;
        }

        public void ViewAxis(Transform camera, Vector3 euler)
        {
            camera.localEulerAngles = -Vector3.left * euler.x;
        }
    }
}

using UnityEngine;

namespace CameraScripts
{
    public class FirstPerson : MonoBehaviour , IViewedCamera
    {
        [SerializeField] Transform positionCamera;

        [SerializeField] private float correction = 0.22F;

        public Vector2 ViewAxisMaxVertical => new Vector2(-90, 90);

        public void Construct()
        {
            if (positionCamera == null)
                throw new MissingReferenceException("Transform Position Camera is Null");
            CameraControll.instance.Transform.parent = transform.parent;
            CameraControll.instance.Transform.position = positionCamera.position + Vector3.up * correction;
            ViewMeshOrShadow(true);
        }

        private void ViewMeshOrShadow(bool ShadowOnly)
        {
            foreach(var renderer in transform.parent.gameObject.GetComponentsInChildren<Renderer>())
            {
                renderer.shadowCastingMode = ShadowOnly ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly : UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }


        public void Dispose()
        {
            ViewMeshOrShadow(false);
        }

        public Vector3 RotateBody()
        {
            return CameraControll.instance.EulerHorizontal;
        }

        public void ViewAxis(Transform camera, Vector3 euler)
        {
            camera.localEulerAngles = -Vector3.left * euler.x;
        }
    }
}

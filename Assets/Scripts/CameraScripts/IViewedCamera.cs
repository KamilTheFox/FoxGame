using System;
using UnityEngine;

namespace CameraScripts
{
    public interface IViewedCamera : IDisposable
    {
        void Construct();
        Vector3 RotateBody();

        void OnGUI() { }

        void ViewAxis(Transform camera,Vector3 euler);

        Vector2 ViewAxisMaxVertical { get; }

        Vector2 ViewAxisMaxHorizontal => new Vector2(360,-360);

        float DistanceView => 3F;
    }
}

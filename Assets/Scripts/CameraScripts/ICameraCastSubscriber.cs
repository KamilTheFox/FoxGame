using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CameraScripts
{
    public interface ICameraCastSubscriber
    {
        void OnCameraCasting(RaycastHit hit);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraScripts
{
    interface IAddonsFunctionalCam
    {
        public void InitializeAddon(ICameraCastObserver cameraCaster);
    }
}

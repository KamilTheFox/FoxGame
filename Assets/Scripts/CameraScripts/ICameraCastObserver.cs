using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraScripts
{
    public interface ICameraCastObserver
    {
        public void AddSubscribeToCast(params ICameraCastSubscriber[] cameraCastSubscriber);
    }
}

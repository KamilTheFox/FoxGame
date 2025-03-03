using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerDescription
{
    public interface IAdditionalAnimate
    {
        public bool Enable { get; set; }

        public string Name { get; }
        public void Initialize(AnimatorCharacterInput charAnimInput);

        public void OnDestroy();

        public void Reset();

        public void OnGUI() { }

    }
}

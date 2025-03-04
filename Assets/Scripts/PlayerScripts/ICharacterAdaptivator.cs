using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerDescription
{
    interface ICharacterAdaptivator
    {
        public void SetMediator(CharacterMediator adapter);
        public void OnAwake();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerDescription
{
    public interface IListenedStateAnimation
    {
        void AddListnerEventInput(CharacterData data);
    }
}

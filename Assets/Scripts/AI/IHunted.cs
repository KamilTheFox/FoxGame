using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AIInput
{
    public interface IHunted
    {
        public Transform transform { get; }

        bool IsDie { get; }
    }
}

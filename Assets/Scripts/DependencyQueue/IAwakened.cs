using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyQueue
{
    public interface IDependent
    {
        public void Awake();
        public void Start();

    }
}

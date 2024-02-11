using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

    public interface IDiesing
    {
        Transform Transform { get; }
        bool IsDie { get; }
        void Death();
        GameObject gameObject { get; }
    }

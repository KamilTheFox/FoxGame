using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

    public interface IBehavior
    {
    string Name { get; }
    public AI AI { get; set; }
    void Activate(AI ai);
    void Deactivate();

    void Update();
    }

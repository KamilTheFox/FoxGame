using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface ITakenEntity
{
    Transform Transform { get; }
    Rigidbody Rigidbody => null;
    bool IsTake { get; }
    void Throw();
    ITakenEntity Take();
    }

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
    EntityEngine GetEngine { get; }

    ITakenEntity Take();

    void Throw();

}

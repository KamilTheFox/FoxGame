using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IAppliedExplosionForce
{
    public void SetExplosionForce(float explosionForce, Vector3 centerExplosion, float explosionRadius);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RegdollDetect : MonoBehaviour
{
    public bool isController;
    public ICollideableDoll Entity;
    public void OnCollisionEnter(Collision collision)
    {
        if (Entity == null)
            return;
        Entity.OnCollision(collision, gameObject);
    }
}

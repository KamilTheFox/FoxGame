using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class EntityParameters
{
    public Vector3 position;
    public Quaternion rotation;
    public TypeEntity typeEntity;
    public Transform parent;
    public string Direction, Name;
    public bool isStatic;
    public bool isIntangible;
    public LayerMask layerMask 
    {
        get
        {
                return isIntangible? MasksProject.IntangibleEntity : MasksProject.Entity;
        }
    }
}


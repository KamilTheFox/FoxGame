using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GPUInstansingEnable : MonoBehaviour
{
    private static MaterialPropertyBlock _block;
    private static MaterialPropertyBlock block
    {
        get
        {
            if (_block == null)
               _block = new MaterialPropertyBlock();
            return _block;
        }
    }
    public void Awake()
    {
        foreach(var renderer in GetComponentsInChildren<MeshRenderer>())
        {
            renderer.SetPropertyBlock(block);
        }
    }
}

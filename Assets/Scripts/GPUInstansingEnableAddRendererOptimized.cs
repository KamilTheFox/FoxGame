using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GPUInstansingEnableAddRendererOptimized : MonoBehaviour
{
    //private static MaterialPropertyBlock _block;
    private RendererBuffer _renderBuffer;
    //private static MaterialPropertyBlock block
    //{
    //    get
    //    {
    //        if (_block == null)
    //           _block = new MaterialPropertyBlock();
    //        return _block;
    //    }
    //}
    public void Awake()
    {
        _renderBuffer = new RendererBuffer(gameObject);

        _renderBuffer.IsDynamicSprite = true;

        OptimizedRenderer.AddRendererBuffer(_renderBuffer);

        //foreach (var renderer in GetComponentsInChildren<Renderer>())
        //{
        //    renderer.SetPropertyBlock(block);
        //}
    }
}

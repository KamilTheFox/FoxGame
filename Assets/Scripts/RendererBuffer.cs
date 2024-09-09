using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class RendererBuffer : IEnumerable
{
    public RendererBuffer(GameObject gameObject)
    {
        List<Renderer> list = new List<Renderer>();
        list.AddRange(gameObject.GetComponentsInChildren<Renderer>());
        if(gameObject.gameObject.TryGetComponent<Renderer>(out var r))
        {
            list.Add(r);
        }
        renderers = list.ToArray();
    }

    public void SetCastShadowMode(ShadowCastingMode mode)
    {
        foreach (Renderer renderer in renderers)
        {
            if(renderer)
                renderer.shadowCastingMode = mode;
        }
    }

    public void SetVisible(bool visible)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer)
                renderer.enabled = visible;
        }
    }

    public Renderer[] renderers;

    public IEnumerator GetEnumerator()
    {
         return renderers.GetEnumerator();
    }
}

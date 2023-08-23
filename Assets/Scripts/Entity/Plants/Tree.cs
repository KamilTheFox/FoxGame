using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweener;
using UnityEngine;

public class Tree : PlantEngine
{
    public override void Death()
    {
        if (IsDie) return;
        IsDie = true;
        GetComponentsInChildren<Transform>().Where(obj => obj.name.Contains("Crown")).ToList().ForEach(obj => LateСrown(obj.gameObject));
    }
    private void LateСrown(GameObject Crown)
    {
        Tween.SetColor(Crown.transform, new Color(1F, 0.5F, 0F, 0F), 2F);
        Tween.SetPosition(Crown.transform, Crown.transform.parent.position + Vector3.down * 5F, 2F);
        Tween.SetScale(Crown.transform, Crown.transform.localScale * 1.5F, 2F);
        GameObject.Destroy(Crown, 2.1F);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlantEngine : EntityEngine, IAlive
{

    public static PlantEngine[] GetPlants
    {
        get
        {
            List<PlantEngine> itemEngines = new List<PlantEngine>();
            Entities[TypeEntity.Plant].ForEach(engine => { if (engine is PlantEngine plant) { itemEngines.Add(plant); } });
            return itemEngines.ToArray();
        }
    }

    public override TypeEntity typeEntity => TypeEntity.Plant;
    [HideInInspector] public TypePlant typePlant;

    public bool IsDead { get; set; }

    public static PlantEngine AddPlant(TypePlant plant, Vector3 vector, Quaternion quaternion)
    {
        return AddEntity<PlantEngine>(plant, vector, quaternion);
    }
    public virtual void Dead()
    {
        if (IsDead) return;
        IsDead = true;

        GetComponentsInChildren<Transform>().Where(obj => obj.name.Contains("Crown")).ToList().ForEach(obj => LateСrown(obj.gameObject));
    }
    private void LateСrown(GameObject Crown)
    {
        Between.SetColorLerp(Crown.transform, new Color(1F,0.5F,0F,0F), 2F);
        Between.SetPosition(Crown.transform,  Crown.transform.parent.position + Vector3.down * 5F, 2F);
        Between.SetScaleLerp(Crown.transform, Crown.transform.localScale * 1.5F, 2F);
        GameObject.Destroy(Crown, 2.1F);
    }
    public override TextUI GetTextUI()
    {
        return (typeEntity.ToString() + " " + typePlant.ToString()).GetTextUI();
    }
}


using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Tweener;

public class PlantEngine : EntityEngine, IDiesing
{
    private IExpansionColor tween;
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
    public bool IsDie { get; set; }

    public static PlantEngine AddPlant(TypePlant plant, Vector3 vector, Quaternion quaternion)
    {
        return AddEntity<PlantEngine>(plant, vector, quaternion);
    }
    protected override void onDestroy()
    {
        if(tween!=null)
            Tween.Stop((IExpansionTween)tween);
        base.onDestroy();
    }
    public virtual void Death()
    {
        if (IsDie) return;
        IsDie = true;
        tween = Tween.SetColor(Transform, new Color(1, 1, 1, 0), 3F).IgnoreAdd(IgnoreARGB.RGB).ChangeEase(Ease.CubicRoot).TypeOfColorChange(TypeChangeColor.ObjectAndHierarchy).ToCompletion(() => Delete());
    }
   
    public override TextUI GetTextUI()
    {
        return (typeEntity.ToString() + " " + typePlant.ToString()).GetTextUI();
    }
}


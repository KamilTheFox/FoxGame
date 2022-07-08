using FactoryLesson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlantEngine : EntityEngine, IAlive
{
    public TypePlant typePlant;

    public static PlantEngine AddPlant(TypePlant plant, Vector3 vector, Quaternion quaternion)
    {
        IEntityFamily InfoEntity = EntityFactory.GetEntity(plant, vector, quaternion);
        PlantEngine plantEngine = InfoEntity.GetEngine as PlantEngine;
        plantEngine.InfoEntity = InfoEntity;
        return plantEngine;
    }
    public void Dead()
    {
        Delete();
    }
}
public enum TypePlant
{
    Tree_1
}

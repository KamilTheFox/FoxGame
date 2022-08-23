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

    public Action<Collision> BehaviorFromCollision => null;

    public bool IsDead => false;

    public static PlantEngine AddPlant(TypePlant plant, Vector3 vector, Quaternion quaternion)
    {
        return AddEntity<PlantEngine>(plant, vector, quaternion);
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

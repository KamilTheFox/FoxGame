 using System;
using System.Collections.Generic;
using UnityEngine;
using FactoryEntity;

    public static class EntityCreate
    {
        private static Dictionary<Type, Type> AcceptableEnum = new Dictionary<Type, Type>
        {
            [typeof(TypeAnimal)] = typeof(AnimalsCreating),
            [typeof(TypePlant)] = typeof(PlantsCreating),
            [typeof(EffectItem)] = typeof(ItemsCreating),
            [typeof(TypeItem)] = typeof(ItemsCreating),
        };
        public static IEntityCreated GetEntity(Enum _enum, Vector3 position, Quaternion quaternion, bool isStatic = true)
        {
            if (!AcceptableEnum.ContainsKey(_enum.GetType()))
                return null;
        IEntityCreated entity = (IEntityCreated)Activator.CreateInstance(AcceptableEnum[_enum.GetType()], new object[] { _enum, position, quaternion, isStatic });
        if (entity != null && entity.GetEngine != null)
        {
            entity.GetEngine.Stationary = entity.isStatic;
            entity.GetEngine.Layer = 1 << entity.GetPrefab.layer;
        }
        return entity;
        }
        public static IEntityCreated GetEntity(Enum _enum, Transform parent, bool isStatic = true)
        {
            if (!AcceptableEnum.ContainsKey(_enum.GetType()))
                return null;
        IEntityCreated entity = (IEntityCreated)Activator.CreateInstance(AcceptableEnum[_enum.GetType()], new object[] { _enum, parent, isStatic });
        if (entity != null && entity.GetEngine != null)
        {
            entity.GetEngine.Stationary = entity.isStatic;
            entity.GetEngine.Layer = 1 << entity.GetPrefab.layer;
        }
        return entity;
        }
    }





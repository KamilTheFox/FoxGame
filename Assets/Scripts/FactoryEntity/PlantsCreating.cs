using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FactoryEntity
{
    public class PlantsCreating : EntityStencilCreating, IEntityCreated
    {

        private static Dictionary<TypePlant, IParametersEntityes> keyValuePlants = new Dictionary<TypePlant, IParametersEntityes>
        {
            [TypePlant.Fir_Tree] = new PlantInfo() { EngineComponent = typeof(Tree), RandomSeze = new RandomSize(0.6F, 2F) },
            [TypePlant.Tree] = new PlantInfo() { EngineComponent = typeof(Tree), RandomSeze = new RandomSize(0.4F, 2F) },
            [TypePlant.White_Mushroom] = new PlantInfo() { EngineComponent = typeof(Mushroom), RandomSeze = new RandomSize(0.5F, 2F), isIntangible = true },
            [TypePlant.Nasty_Mushroom] = new PlantInfo() { EngineComponent = typeof(Mushroom), RandomSeze = new RandomSize(0.5F, 2F), isIntangible = true },
            [TypePlant.Agaric_Mushroom] = new PlantInfo() { EngineComponent = typeof(Mushroom), RandomSeze = new RandomSize(0.5F, 2F), isIntangible = true },
            [TypePlant.Apple_Tree] = new PlantInfo() { EngineComponent = typeof(Apple_Tree), RandomSeze = new RandomSize(2F) },
            [TypePlant.Bush] = new PlantInfo() { RandomSeze = new RandomSize(2F, 5F) },
        };
        private class PlantInfo : IParametersEntityes
        {
            public Type EngineComponent = typeof(PlantEngine);
            public Vector3 Size => RandomSeze.GetVector3();

            Type IParametersEntityes.EngineComponent => EngineComponent;

            public RandomSize RandomSeze = new RandomSize(1F);

            public bool isIntangible;
            public object SetParametrs(EntityEngine Prefab)
            {
                Prefab.transform.localScale = Size;
                if(isIntangible)
                {
                    Prefab.transform.GetComponentsInChildren<Transform>().ToList().ForEach(t =>
                    {
                        t.gameObject.layer = MasksProject.IntangibleEntity;
                    });
                }
                return Prefab;
            }
        }
        public PlantsCreating(TypePlant typePlant, Vector3 vector, Quaternion quaternion, bool isStatic = false) : base(vector, quaternion, true, typeEntity, typePlant.ToString())
        {
            IParametersEntityes parameters;
            if (!keyValuePlants.TryGetValue(typePlant, out parameters))
                parameters = new PlantInfo();
            if (GetPrefab.TryGetComponent(out PlantEngine obj))
                plantEngine = obj;
            else
                plantEngine = GetPrefab.AddComponent(parameters.EngineComponent) as PlantEngine;

            Transform[] Crowns = GetPrefab.GetComponentsInChildren<Transform>().Where(obj => obj.gameObject.name.Contains("Crown")).ToArray();
            foreach (Transform Crown in Crowns)
            {
#if UNITY_EDITOR
                GameObject.DestroyImmediate(Crown.GetComponent<Collider>());
#else
                GameObject.Destroy(Crown.GetComponent<Collider>());
#endif
            }
            parameters.SetParametrs(plantEngine);
            plantEngine.typePlant = typePlant;

        }
        private PlantEngine plantEngine;
        public static TypeEntity typeEntity => TypeEntity.Plant;
        public TypeEntity TypeEntity => typeEntity;

        public bool isStatic => _isStaticItem;

        public EntityEngine GetEngine => plantEngine;
    }
}

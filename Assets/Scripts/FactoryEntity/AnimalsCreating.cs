using System;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryEntity
{
    public class AnimalsCreating : EntityStencilCreating, IEntityCreated
    {
        private static Dictionary<TypeAnimal, IParametersEntityes> keyValueAnimals = new Dictionary<TypeAnimal, IParametersEntityes>
        {
            [TypeAnimal.Fox] = new AnimalInfo() { EngineComponent = typeof(Fox), AI = typeof(FoxAI) },
            [TypeAnimal.Fox_White] = new AnimalInfo() { EngineComponent = typeof(Fox), AI = typeof(FoxAI) },
            [TypeAnimal.Fox_Red] = new AnimalInfo() { EngineComponent = typeof(Fox), AI = typeof(FoxAI) },
        };
        public static AI GetAI(TypeAnimal type)
        {
            return (AI)Activator.CreateInstance(((AnimalInfo)keyValueAnimals[type]).AI);
        }
        private class AnimalInfo : IParametersEntityes
        {
            public Type AI = null;
            public Type EngineComponent = typeof(AnimalEngine);

            Type IParametersEntityes.EngineComponent => EngineComponent;

            public object SetParametrs(EntityEngine Prefab)
            {
                AnimalEngine animal = Prefab as AnimalEngine;
                return animal;
            }
        }
        public AnimalsCreating(TypeAnimal typeAnimal, Vector3 position, Quaternion quaternion, bool isStatic = false) : base(position, quaternion, true, type, typeAnimal.ToString())
        {
            IParametersEntityes parameters;
            if (!keyValueAnimals.TryGetValue(typeAnimal, out parameters))
                parameters = new AnimalInfo();
            if (GetPrefab.TryGetComponent(out AnimalEngine engine))
                animalEngine = engine;
            else
            animalEngine = GetPrefab.AddComponent(parameters.EngineComponent) as AnimalEngine;

            parameters.SetParametrs(animalEngine);
            animalEngine.TypeAnimal = typeAnimal;

        }
        private static TypeEntity type => TypeEntity.Animal;
        public TypeEntity TypeEntity => type;

        public bool isStatic => false;

        private AnimalEngine animalEngine;

        public EntityEngine GetEngine => animalEngine;
    }
}

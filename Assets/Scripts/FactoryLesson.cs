using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

//Пробую Паттерн Фабричного метода
namespace FactoryLesson
{
    public interface IEntityFamily
    {
        TypeEntity TypeEntity { get; }
        bool isStatic { get; }
        EntityEngine GetEngine { get; }
        GameObject GetPrefab { get; }
    }
    internal struct RandomSize
    {
        public bool IsRandom;
        /// <summary>
        /// Исключение рандома
        /// </summary>
        public RandomSize(float DefaultSize)
        {
            IsRandom = false;
            min = 0;
            max = DefaultSize;
        }
        /// <summary>
        /// Рандом в пределах чисел
        /// </summary>
        public RandomSize(float _min, float _max)
        {
            IsRandom = true;
            min = _min;
            max = _max;
        }
        public float GetValue()
        {
            return IsRandom ? UnityEngine.Random.Range(min, max) : max;
        }
        public Vector3 GetVector3()
        {
            return GetValue() * Vector3.one;
        }

        public float min, max;
    }
    public static class EntityFactory
    {
        private static Dictionary<Type, Type> AcceptableEnum = new Dictionary<Type, Type>
            {
            [typeof(TypeAnimal)] = typeof(EntityFamilyFactoryAnimals),
            [typeof(TypePlant)] = typeof(EntityFamilyFactoryPlants),
            [typeof(EffectItem)] = typeof(EntityFamilyFactoryItem),
            [typeof(TypeItem)] = typeof(EntityFamilyFactoryItem),
        };
        public static IEntityFamily GetEntity(Enum _enum, Vector3 position, Quaternion quaternion, bool isStatic = true)
        {
            if (!AcceptableEnum.ContainsKey(_enum.GetType()))
                return null;
            return (IEntityFamily)Activator.CreateInstance(AcceptableEnum[_enum.GetType()], new object[] { _enum, position, quaternion, isStatic });
        }
        public static IEntityFamily GetEntity(Enum _enum, Transform parent, bool isStatic = true)
        {
            if (!AcceptableEnum.ContainsKey(_enum.GetType()))
                return null;
            return (IEntityFamily)Activator.CreateInstance(AcceptableEnum[_enum.GetType()], new object[] { _enum, parent, isStatic });
        }
    }

    public class EntityFamilyFactoryAnimals : EntityFamilyFactory, IEntityFamily
    {
        private static Dictionary<TypeAnimal, AnimalInfo> keyValueAnimals = new Dictionary<TypeAnimal, AnimalInfo>
        {
            [TypeAnimal.Fox] = new AnimalInfo() { EngineComponent = null },
        };
        private class AnimalInfo
        {
            public IArtificialIntelligence AI = null;
            public Type EngineComponent = typeof(AnimalEngine);

        }
        public EntityFamilyFactoryAnimals(TypeAnimal typeAnimal, Vector3 position, Quaternion quaternion, bool isStatic = false) : base(position, quaternion, true, type, typeAnimal.ToString())
        {
            animalEngine = GetPrefab.GetComponent<AnimalEngine>();
            if (animalEngine != null) return;

            AnimalInfo info = new AnimalInfo();

            if (keyValueAnimals.TryGetValue(typeAnimal, out AnimalInfo NewInfo))
                info = NewInfo;

            animalEngine = GetPrefab.AddComponent(info.EngineComponent) as AnimalEngine;

        }
        private static TypeEntity type => TypeEntity.Animal;
        public TypeEntity TypeEntity => type;

        public bool isStatic => false;

        private AnimalEngine animalEngine;

        public EntityEngine GetEngine => animalEngine;
    }

    public class EntityFamilyFactoryPlants : EntityFamilyFactory, IEntityFamily
    {

        private static Dictionary<TypePlant, PlantInfo> keyValuePlants = new Dictionary<TypePlant, PlantInfo>
        {
            [TypePlant.Tree_1] = new PlantInfo() { RandomSeze = new RandomSize(0.6F, 2F) }
        };
        private class PlantInfo
        {
            public Type EngineComponent = typeof(PlantEngine);
            public Vector3 Size => RandomSeze.GetVector3();
            public RandomSize RandomSeze = new RandomSize(1F);
        }
        public EntityFamilyFactoryPlants(TypePlant typePlant, Vector3 vector, Quaternion quaternion, bool isStatic = false) : base(vector, quaternion, true, typeEntity, typePlant.ToString())
        {
            PlantInfo info = keyValuePlants[typePlant];
            PlantEngine plantEngine = GetPrefab.AddComponent(info.EngineComponent) as PlantEngine;
            plantEngine.transform.localScale = info.Size;
            plantEngine.typePlant = typePlant;
            this.plantEngine = plantEngine;
        }
        private PlantEngine plantEngine;
        public static TypeEntity typeEntity => TypeEntity.Plant;
        public TypeEntity TypeEntity => typeEntity;

        public bool isStatic => _isStaticItem;

        public EntityEngine GetEngine => plantEngine;
    }
    public abstract class EntityFamilyFactory
    {
        protected readonly bool _isStaticItem;
        public GameObject GetPrefab { get; private set; }
        private void DebugWarning(TypeEntity typeEntity, string Name)
        {
            Debug.LogWarning($"Resource not found; TypeEntity: {typeEntity} NameEntity: {Name}");
        }
        private void DebugWarning(string path)
        {
            Debug.LogWarning($"Resource not found; Path: {path}");
        }
        public void LoadPrefab(GameObject gameObject)
        {
            GetPrefab = gameObject;
        }
        private GameObject LoadInResource(string path)
        {
            GameObject game = Resources.Load<GameObject>(path);
            if (game)
                return game;
            DebugWarning(path);
            return null;
        }
        public EntityFamilyFactory(Transform parent, bool isStatic, TypeEntity typeEntity, string Direction, string Name)
        {
            _isStaticItem = isStatic;
            string path = typeEntity + "\\" + Direction + "\\";
            GameObject obj = LoadInResource($"{path}{Name}");
            if (obj == null)
                return;
            LoadPrefab(Object.Instantiate(obj, parent));
        }
        public EntityFamilyFactory(Vector3 vector, Quaternion quaternion, bool isStatic, TypeEntity typeEntity, string Direction, string Name)
        {
            _isStaticItem = isStatic;
            string path = typeEntity + "\\" + Direction + "\\";
            GameObject obj = LoadInResource($"{path}{Name}");
            if (obj == null)
                return;
            LoadPrefab(Object.Instantiate(obj, vector, quaternion));
        }
        public EntityFamilyFactory(Vector3 vector, Quaternion quaternion, bool isStatic, TypeEntity typeEntity, string Name)
        {
            _isStaticItem = isStatic;
            string path = typeEntity + "\\";
            #region LoadInResources
            GameObject obj = Resources.Load<GameObject>($"{path}Prefabs\\{Name}");
            if (obj)
            {
                GetPrefab = GameObject.Instantiate(obj, vector, quaternion);
                return;
            }
            obj = LoadInResource($"{path}Mesh\\{Name}");

            if (obj == null) return;

            obj = Object.Instantiate(obj, vector, quaternion);

            obj.layer = LayerMask.NameToLayer("Entity");

            Material material = Resources.Load<Material>($"{path}Material\\{Name}");

            if (obj.transform.childCount == 0)
            {
                if (material)
                    obj.GetComponent<Renderer>().material = material;
                obj.AddComponent<MeshCollider>().convex = !isStatic;
            }
            else
            {
                List<GameObject> Destroed = new List<GameObject>();
                bool IsColliders = false;
                foreach (Transform Chield in obj.transform)
                {
                    GameObject gameObject = Chield.gameObject;
                    gameObject.layer = obj.layer;
                    if (Chield.name.Contains("Collider"))
                    {
                        if (isStatic)
                        {
                            Debug.LogWarning(Chield.name + "Destroy");
                            Destroed.Add(gameObject);
                        }
                        else
                        {
                            gameObject.AddComponent<MeshCollider>().convex = true;
                            gameObject.GetComponent<MeshRenderer>().enabled = false;
                        }
                        IsColliders = true;
                    }
                    else
                    {
                        Material ChieldMaterial = (Material)Resources.Load($"{path}Material\\{Chield.name}");
                        if (ChieldMaterial != null)
                            gameObject.GetComponent<Renderer>().material = ChieldMaterial;
                        gameObject.AddComponent<MeshCollider>().enabled = isStatic;
                    }
                }
                foreach(GameObject Obj in Destroed)
#if UNITY_EDITOR
                    Object.DestroyImmediate(Obj);
#else
                    Object.DestroyImmediate(Obj);
#endif
                if (!IsColliders)
                    foreach (Transform Chield in obj.transform)
                    {
                        MeshCollider mesh = Chield.gameObject.GetComponent<MeshCollider>();
                        mesh.enabled = true;
                        if (!isStatic)
                            mesh.convex = true;
                    }
            }
#endregion
            LoadPrefab(obj);
        }
    }

    public class EntityFamilyFactoryItem : EntityFamilyFactory, IEntityFamily
    {
        private static Dictionary<TypeItem, InfoItem> keyOfTypeParameters = new()
        {
            [TypeItem.Apple] = new InfoItem()
            {
                EngineComponent = typeof(Apple),
                Mass = 0.1F,
                Size = Vector3.one * 0.7F
            },
            [TypeItem.Basket] = new InfoItem()
            {
                Mass = 2F
            },
            [TypeItem.Table] = new InfoItem()
            {
                Mass = 10F,
                Size = Vector3.one * 0.5F,
                ChangeCenterMass = true,
                vectorCenterMass = Vector3.up * 0.42F
            },
            [TypeItem.Chair] = new InfoItem()
            {
                Mass = 5F,
                Size = Vector3.one * 0.85F
            },
            [TypeItem.TNT] = new InfoItem()
            {
                EngineComponent = typeof(TNT),
                Mass = 2F,
                Size = Vector3.one * 1.5F
            }

        };

        class InfoItem
        {
            public Type EngineComponent = typeof(ItemEngine);
            public float Mass = 1F;
            public bool ChangeMovableOrStatic = true;
            public bool ChangeCenterMass = false;
            public Vector3 vectorCenterMass = Vector3.zero;
            public Vector3 Size = Vector3.one;
        }
        public EntityFamilyFactoryItem(EffectItem effect, Transform transform, bool isStatic = true)
              : base(transform, true, typeEntity, "Effects", effect.ToString())
        {
        }
        public EntityFamilyFactoryItem(EffectItem effect, Vector3 vector, Quaternion quaternion, bool isStatic = true)
              : base(vector, Quaternion.identity, true, typeEntity, "Effects", effect.ToString())
        {
        }
        public EntityFamilyFactoryItem(TypeItem itemType, Vector3 vector, Quaternion quaternion, bool isStatic = true)
            : base(vector, quaternion, isStatic && keyOfTypeParameters[itemType].ChangeMovableOrStatic, typeEntity, itemType.ToString())
        {
            InfoItem info = keyOfTypeParameters[itemType];
            ItemEngine itemEngine = GetPrefab.AddComponent(info.EngineComponent) as ItemEngine;
            itemEngine.transform.localScale = info.Size;
            itemEngine.itemType = itemType;
            if (!isStatic && info.ChangeMovableOrStatic)
            {
                Rigidbody body = GetPrefab.AddComponent<Rigidbody>();
                if (info.ChangeCenterMass)
                    body.centerOfMass = info.vectorCenterMass;
                body.mass = info.Mass;
                itemEngine.Rigidbody = body;
                itemEngine.gameObject.AddComponent<PlayerControll>();
            }
            this.itemEngine = itemEngine;
        }

        private ItemEngine itemEngine;
        public EntityEngine GetEngine => itemEngine;
        public bool isStatic => _isStaticItem;
        private static TypeEntity typeEntity => TypeEntity.Item;
        public TypeEntity TypeEntity => typeEntity;
    }

}
public enum TypeEntity
{
    Animal,
    Plant,
    Item
}



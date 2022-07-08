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

    public static class EntityFactory
    {
        public static IEntityFamily GetEntity(TypePlant plant, Vector3 vector, Quaternion quaternion) => new EntityFamilyFactoryPlants(plant, vector,quaternion);
        public static IEntityFamily GetEntity(EffectItem effect, Vector3 vector) => new EntityFamilyFactoryItem(effect, vector);
        public static IEntityFamily GetEntity(EffectItem effect, Transform transform) => new EntityFamilyFactoryItem(effect, transform);
        public static IEntityFamily GetEntity(TypeItem itemType, bool isStatic, Vector3 vector, Quaternion quaternion)
        {
            return new EntityFamilyFactoryItem(itemType, isStatic, vector, quaternion);
        }
    }
    public class EntityFamilyFactoryPlants : EntityFamilyFactory, IEntityFamily
    {

        private static Dictionary<TypePlant, PlantInfo> keyValuePlants = new Dictionary<TypePlant, PlantInfo>
        {
            [TypePlant.Tree_1] = new PlantInfo() { RandomSeze = new PlantInfo.RND(0.6F,2F) }
        };
            private class PlantInfo
            {
            public Type EngineComponent = typeof(PlantEngine);
            public Vector3 Size = Vector3.one;
            public RND RandomSeze = new RND();
            public struct RND
            {
                public bool IsRandom;
                public static readonly RND None = new RND();
                public RND(float _min, float _max)
                {
                    IsRandom = true;
                    min = _min;
                    max = _max;
                }
                public float min, max;
            }
            }
        public EntityFamilyFactoryPlants(TypePlant typePlant ,Vector3 vector, Quaternion quaternion) : base(vector, quaternion, true, typeEntity, typePlant.ToString())
        {
            PlantInfo info = keyValuePlants[typePlant];
            PlantEngine plantEngine = GetPrefab.AddComponent(info.EngineComponent) as PlantEngine;
            plantEngine.transform.localScale = info.RandomSeze.IsRandom? UnityEngine.Random.Range(info.RandomSeze.min, info.RandomSeze.max) * Vector3.one : info.Size;
            plantEngine.typePlant = typePlant;
            this.plantEngine = plantEngine;
        }
        private PlantEngine plantEngine;
        public static TypeEntity typeEntity => TypeEntity.Plant;
        public TypeEntity TypeEntity => typeEntity;

        public bool isStatic => _isStaticItem;

        public EntityEngine GetEngine => plantEngine;

        public GameObject GetPrefab => prefab;
    }
    public abstract class EntityFamilyFactory
    {
        protected readonly bool _isStaticItem;
        protected GameObject prefab;
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
            if (!gameObject)
            {
                Debug.LogWarning($"Resource not found;");
            }
            prefab = gameObject;
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
            if (!obj)
            {
                DebugWarning(typeEntity, Name);
            }
            else
                LoadPrefab(Object.Instantiate(obj, parent));
        }
        public EntityFamilyFactory(Vector3 vector, Quaternion quaternion, bool isStatic, TypeEntity typeEntity, string Direction, string Name)
        {
            _isStaticItem = isStatic;
            string path = typeEntity + "\\" + Direction + "\\";
            GameObject obj = LoadInResource($"{path}{Name}");
            if (!obj)
            {
                DebugWarning(typeEntity, Name);
            }
            else
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
                prefab = obj;
                return;
            }

            obj = LoadInResource($"{path}Mesh\\{Name}");
            if (!obj)
            {
                DebugWarning(typeEntity, Name);
            }
            else
            {
                obj.layer = LayerMask.NameToLayer("Entity");
                obj = Object.Instantiate(obj, vector, quaternion);
                Material material = (Material)Resources.Load($"{path}Material\\{Name}");
                if (obj.transform.childCount == 0)
                {
                    if(material)
                    obj.GetComponent<Renderer>().material = material;
                    obj.AddComponent<MeshCollider>().convex = !isStatic;
                }
                else
                {
                    bool IsColliders = false;
                    foreach (Transform Chield in obj.transform)
                    {
                        GameObject gameObject = Chield.gameObject;
                        gameObject.layer = obj.layer;
                        if (Chield.name.Contains("Collider"))
                        {
                            if (!isStatic)
                            {
                                gameObject.AddComponent<MeshCollider>().convex = true;
                                gameObject.GetComponent<MeshRenderer>().enabled = false;
                            }
                            else
                                Object.Destroy(gameObject);
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
                    if (!IsColliders)
                        foreach (Transform Chield in obj.transform)
                        {
                            MeshCollider mesh = Chield.gameObject.GetComponent<MeshCollider>();
                            mesh.enabled = true;
                            if (!isStatic)
                                mesh.convex = true;
                        }
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
        public EntityFamilyFactoryItem(EffectItem effect, Transform transform)
              : base(transform, true, typeEntity, "Effects", effect.ToString())
        {
        }
        public EntityFamilyFactoryItem(EffectItem effect, Vector3 vector)
              : base(vector, Quaternion.identity, true, typeEntity, "Effects", effect.ToString())
        {
        }
        public EntityFamilyFactoryItem(TypeItem itemType, bool isStatic, Vector3 vector, Quaternion quaternion)
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
        public GameObject GetPrefab => prefab; 
    }

}
public enum TypeEntity
{
    Animal,
    Plant,
    Item
}



using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace FactoryEntity
{
    public class ItemsCreating : EntityStencilCreating, IEntityCreated
    {
        private static Dictionary<TypeItem, IParametersEntityes> keyOfTypeParameters = new()
        {
            [TypeItem.Apple] = new InfoItem()
            {
                EngineComponent = typeof(Apple),
                RandomSeze = new RandomSize(0.3F, 0.8F),
                Mass = 0.1F
            },
            [TypeItem.Basket] = new InfoItem()
            {
                Controller = true,
                Mass = 2F
            },
            [TypeItem.Table] = new InfoItem()
            {
                Controller = true,
                Mass = 10F,
                RandomSeze = new RandomSize(0.5F),
                ChangeCenterMass = true,
                vectorCenterMass = Vector3.up * 0.42F
            },
            [TypeItem.Chair] = new InfoItem()
            {
                Controller = true,
                Mass = 5F,
                RandomSeze = new RandomSize(0.85F)
            },
            [TypeItem.TNT] = new InfoItem()
            {
                EngineComponent = typeof(TNT),
                Mass = 2F,
                RandomSeze = new RandomSize(1.5F)
            },
            [TypeItem.TazWanted] = new InfoItem()
            {
                EngineComponent = typeof(PosterTazWanted),
                Mass = 5F,
                ChangeCenterMass = true,
                vectorCenterMass = Vector3.up * 0.30F
            },
            [TypeItem.Door] = new InfoItem()
            {
                EngineComponent = typeof(Door),
                Mass = 10F,
                RandomSeze = new RandomSize(0.6F),
                ProtectStatic = true,
                rigidbodyInterpolation = RigidbodyInterpolation.None,
                AdditionalConstructor = (Item) =>
                {
                    Transform DoorObject = Item.transform.Find("Door");
                    NavMeshObstacle obst = DoorObject.gameObject.AddComponent<NavMeshObstacle>();
                    obst.carving = true;
                    DoorObject.localScale = Vector3.one * 0.99F;
                    DoorObject.localPosition += new Vector3(0.005F, 0F, 0F);
                    Transform Glass = DoorObject.transform.Find("Door_Glass");
                    Glass.localScale = new Vector3(0.62F, 1F, 0.88F);
                    Glass.localPosition = new Vector3(1F, 0.1F, 0.17F);
                    List<MeshCollider> meshes = new List<MeshCollider>();
                    meshes.Add(DoorObject.GetComponent<MeshCollider>());
                    meshes.AddRange(DoorObject.GetComponentsInChildren<MeshCollider>());
                    meshes.ForEach(c => c.convex = true);
                    Item.Rigidbody = DoorObject.gameObject.AddComponent<Rigidbody>();
                    Item.Rigidbody.isKinematic = true;
                },
            }

        };

        class InfoItem : IParametersEntityes
        {
            public Type EngineComponent { get; set; } = typeof(ItemEngine);

            public float Mass = 1F;
            public bool ProtectStatic;
            public bool Controller = false;
            public bool ChangeCenterMass = false;
            public RigidbodyInterpolation rigidbodyInterpolation = RigidbodyInterpolation.Interpolate;
            public Vector3 vectorCenterMass = Vector3.zero;
            public Vector3 Size => RandomSeze.GetVector3();
            public RandomSize RandomSeze = new RandomSize(1F);

            public Action<ItemEngine> AdditionalConstructor;

            public object SetParametrs(EntityEngine Prefab)
            {
                ItemEngine itemEngine = Prefab as ItemEngine;
                itemEngine.transform.localScale = Size;
                if (!itemEngine.Stationary && !ProtectStatic)
                {
                    Rigidbody body = Prefab.gameObject.AddComponent<Rigidbody>();
                    itemEngine.Rigidbody = body;
                    itemEngine.isController = Controller;
                    if (Controller)
                        Prefab.gameObject.AddComponent<PlayerControll>();
                }
                AdditionalConstructor?.Invoke(itemEngine);
                if(itemEngine.Rigidbody)
                {
                    Rigidbody body = itemEngine.Rigidbody;
                    if (ChangeCenterMass)
                        body.centerOfMass = vectorCenterMass;
                    body.interpolation = rigidbodyInterpolation;
                    body.mass = Mass;
                }
                return itemEngine;
            }
        }
        public ItemsCreating(EffectItem effect, Transform transform, bool isStatic = true)
              : base(transform, true, typeEntity, "Effects", effect.ToString())
        {
        }
        public ItemsCreating(EffectItem effect, Vector3 vector, Quaternion quaternion, bool isStatic = true)
              : base(vector, Quaternion.identity, true, typeEntity, "Effects", effect.ToString())
        {
        }
        private static bool PritectStaticObject(TypeItem itemType, bool IsStatic)
        {
            if ((keyOfTypeParameters[itemType] as InfoItem).ProtectStatic)
                return true;
                return IsStatic;

        }
        public ItemsCreating(TypeItem itemType, Vector3 vector, Quaternion quaternion, bool isStatic = true)
            : base(vector, quaternion, PritectStaticObject(itemType, isStatic), typeEntity, itemType.ToString())
        {
            IParametersEntityes parameters;
            if (!keyOfTypeParameters.TryGetValue(itemType, out parameters))
                parameters = new InfoItem();
            itemEngine = GetPrefab.AddComponent(parameters.EngineComponent) as ItemEngine;
            parameters.SetParametrs(itemEngine);
            itemEngine.itemType = itemType;
        }

        private ItemEngine itemEngine;
        public EntityEngine GetEngine => itemEngine;
        public bool isStatic => _isStaticItem;
        private static TypeEntity typeEntity => TypeEntity.Item;
        public TypeEntity TypeEntity => typeEntity;
    }
}

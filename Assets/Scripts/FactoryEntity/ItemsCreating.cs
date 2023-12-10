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
                Controller = false,
                Mass = 2F
            },
            [TypeItem.Barrel] = new InfoItem()
            {
                RandomSeze = new RandomSize(0.7F),
                Mass = 4000F
            },
            [TypeItem.Barrel_Detonator] = new InfoItem()
            {
                EngineComponent = typeof(Barrel_Detonator),
                RandomSeze = new RandomSize(0.7F),
                Mass = 4000F
            },
            [TypeItem.Barrel_Detonator_Timer] = new InfoItem()
            {
                EngineComponent = typeof(Barrel_Detonator_Timer),
                RandomSeze = new RandomSize(0.7F),
                Mass = 4000F,
            },
            [TypeItem.Table_Cardboard] = new InfoItem()
            {
                Controller = false,
                Mass = 30F,
                RandomSeze = new RandomSize(0.5F),
                ChangeCenterMass = true,
                vectorCenterMass = Vector3.up * 0.42F
            },
            [TypeItem.Chair_Cardboard] = new InfoItem()
            {
                Controller = false,
                Mass = 15F,
                RandomSeze = new RandomSize(0.85F)
            },
            [TypeItem.TNT_3] = new InfoItem()
            {
                EngineComponent = typeof(TNT_3),
                Mass = 2F,
                RandomSeze = new RandomSize(0.35F),
            },
            [TypeItem.TNT_3_Timer] = new InfoItem()
            {
                EngineComponent = typeof(TNT_3_Timer),
                Mass = 2F,
                RandomSeze = new RandomSize(0.35F),
            },
            [TypeItem.TNT] = new InfoItem()
            {
                EngineComponent = typeof(TNT),
                Mass = 0.7F,
                RandomSeze = new RandomSize(0.35F)
            },
            [TypeItem.TazWanted] = new InfoItem()
            {
                EngineComponent = typeof(PosterTazWanted),
                Mass = 30F,
                ChangeCenterMass = true,
                vectorCenterMass = Vector3.up * 0.30F
            },
            [TypeItem.DoorLegasy] = new InfoItem()
            {
                EngineComponent = typeof(FrameDoor),
                Mass = 30F,
                RandomSeze = new RandomSize(0.6F),
                ProtectStatic = true,
                rigidbodyInterpolation = RigidbodyInterpolation.None,
                AdditionalConstructor = (Item) =>
                {
                    Transform DoorObject = Item.transform.Find("Door");
                    DoorObject.gameObject.AddComponent<InteractiveBodies.Door>();
                    DoorObject.localScale = Vector3.one * 0.99F;
                    DoorObject.localPosition += new Vector3(0.005F, 0F, 0F);
                    Transform Glass = DoorObject.transform.Find("Door_Glass");
                    Glass.localScale = new Vector3(0.62F, 1F, 0.88F);
                    Glass.localPosition = new Vector3(1F, 0.1F, 0.17F);
                },
            },
            [TypeItem.Bench] = new InfoItem()
            {
                ProtectStatic = true,
            },
            [TypeItem.TrashCanMini] = new InfoItem()
            {
                EngineComponent = typeof(TrashCan),
                RandomSeze = new RandomSize(2F),
                ProtectStatic = true,
            },
            [TypeItem.Wardrobe] = new InfoItem()
            {
                ProtectStatic = true,
                EngineComponent = typeof(FrameDoor)
            },
            [TypeItem.Box] = new InfoItem()
            {
                Mass = 100F
            },
            [TypeItem.BoxMetal] = new InfoItem()
            {
                Mass = 150F
            },
            [TypeItem.Bed] = new InfoItem()
            {
                ProtectStatic = true
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
                if (!itemEngine.Stationary)
                {
                    if (!itemEngine.gameObject.TryGetComponent(out Rigidbody body))
                        body = Prefab.gameObject.AddComponent<Rigidbody>();

                    itemEngine.Rigidbody = body;
                    foreach (Transform child in body.transform.GetComponentsInChildren<Transform>())
                    {
                        if (!child.TryGetComponent(out NavMeshObstacle nav))
                        {
                            NavMeshObstacle obst = child.gameObject.AddComponent<NavMeshObstacle>();
                            obst.carving = true;
                        }
                    }
                    itemEngine.isController = Controller;
                    if (Controller)
                        Prefab.gameObject.AddComponent<PlayerBody>();
                }
                if (itemEngine.Rigidbody)
                {
                    Rigidbody body = itemEngine.Rigidbody;
                    if (ChangeCenterMass)
                        body.centerOfMass = vectorCenterMass;
                    body.interpolation = rigidbodyInterpolation;
                    body.mass = Mass;
                }
                AdditionalConstructor?.Invoke(itemEngine);
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
            if (keyOfTypeParameters.TryGetValue(itemType, out IParametersEntityes parameters))
            {
                if((parameters as InfoItem).ProtectStatic)
                        return true;
            }
            return IsStatic;

        }
        public ItemsCreating(TypeItem itemType, Vector3 vector, Quaternion quaternion, bool isStatic = true)
            : base(vector, quaternion, PritectStaticObject(itemType, isStatic), typeEntity, itemType.ToString())
        {
            if (!GetPrefab) return;
            IParametersEntityes parameters;
            if (!keyOfTypeParameters.TryGetValue(itemType, out parameters))
                parameters = new InfoItem();
            if (GetPrefab.TryGetComponent(out ItemEngine obj))
                itemEngine = obj;
            else
                itemEngine = GetPrefab.AddComponent(parameters.EngineComponent) as ItemEngine;
            itemEngine.Stationary = _isStaticItem;
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

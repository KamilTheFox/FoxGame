using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Collections;
using PlayerDescription;
using UnityEngine;

namespace FactoryEntity
{
    public class EntityEngineBase : MonoBehaviour, IGlobalUpdates
    {
        public List<EntityEngine> this[TypeEntity type]
        {
            get
            {
                return entities[type];
            }
        }
        static Dictionary<TypeEntity, List<EntityEngine>> entities = new()
        {
            [TypeEntity.Item] = new(),
            [TypeEntity.Animal] = new(),
            [TypeEntity.Plant] = new(),
        };

        static List<CharacterBody> characterBodies = new();

        Dictionary<TypeEntity, List<EntityEngine>> entitiesCopy;

        public static void AddCharacterInfoEntity(CharacterBody body)
        {
            if (!characterBodies.Contains(body))
                characterBodies.Add(body);
        }
        public static void RemoveCharacterInfoEntity(CharacterBody body)
        {
            if(characterBodies.Contains(body))
                characterBodies.Remove(body);
        }


        NativeArray<InputJobEntityData> positionOrder;

        NativeArray<ResultsJobEntity> result;

        private JobHandle WorkerDistanceCalculate;

        private static List<InputJobEntityData> vectors = new();
        
        private void Start()
        {
            this.AddListnerUpdate();
        }

        private void OnDestroy()
        {
            characterBodies.Clear();
            foreach (var list in entities)
                list.Value.Clear();
            WorkerDistanceCalculate.Complete();
            if(result.IsCreated)
                result.Dispose();
            if (positionOrder.IsCreated)
                positionOrder.Dispose();
        }
        void IGlobalUpdates.FixedUpdate()
        {
            if (!WorkerDistanceCalculate.IsCompleted || positionOrder.IsCreated || result.IsCreated) return;

            vectors.Clear();
            entitiesCopy = entities.ToDictionary(entry => entry.Key,
                                               entry => entry.Value.ToList());
            int y;
            for (int i = 0; i < 3; i++)
            {
                var list = entitiesCopy[(TypeEntity)i];
                y = 0;
                foreach (var entity in list)
                { 
                    if (entity.gameObject.activeSelf)
                    {
                        vectors.Add(new InputJobEntityData()
                        {
                            orderDistance = entity.distanceOrder,
                            entityId = y,
                            entityType = (TypeEntity)i,
                            shadowVisibilityDistance = Settings.DistanceDrawShadow
                        }.SetTransform(entity.Transform));
                        y++;
                    }
                }
            }
            y = 0;
            foreach (CharacterBody character in characterBodies)
            {
                if (character.gameObject.activeSelf)
                {
                    vectors.Add(new InputJobEntityData()
                    {
                        orderDistance = Settings.DistanceDrawCharacters,
                        entityId = y,
                        entityType = TypeEntity.BodyController,
                        shadowVisibilityDistance = Settings.DistanceDrawShadow
                    }.SetTransform(character.Transform));
                    y++;
                }
            }
                positionOrder = new NativeArray<InputJobEntityData>(vectors.ToArray(), Allocator.Persistent);
            result = new NativeArray<ResultsJobEntity>(vectors.Count, Allocator.Persistent);
            DistanceEntityJob job = new DistanceEntityJob(positionOrder, CameraControll.instance.Transform.position, result);
            WorkerDistanceCalculate = job.Schedule(vectors.Count, 256);
        }
        void IGlobalUpdates.EndFixedUpdate()
        {
            if (!WorkerDistanceCalculate.IsCompleted) return;
            WorkerDistanceCalculate.Complete();
            for(int i = 0; i< vectors.Count; i++)
            {
                TypeEntity type = vectors[i].entityType;
                if(type == TypeEntity.BodyController)
                {
                    characterBodies[vectors[i].entityId].rendererBuffer.SetCastShadowMode(result[i].castShadow);
                }
                else if (vectors[i].entityId < entitiesCopy[type].Count)
                {
                    EntityEngine entity = entitiesCopy[type][vectors[i].entityId];
                    entity.OnBatchDistanceCalculated(result[i].isInOrderDistance);
                    entity.rendererBuffer.SetCastShadowMode(result[i].castShadow);
                }
            }
            entitiesCopy.Clear();
            entitiesCopy = null;
            positionOrder.Dispose();
            result.Dispose();
        }
    }
}

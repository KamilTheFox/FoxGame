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

     
    }
}

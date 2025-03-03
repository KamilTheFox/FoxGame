using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Unity.Collections;
using VulpesTool;

namespace MoveablePlatform
{
    [Obsolete("Не работает на Игроке")] // Предполагаю, что это не сильно оптимизирует текущую реализацию
    [AddComponentMenu("Factory/ConveyorBeltPhysics")]
    [RequireComponent(typeof(BoxCollider))]
    public class ConveyorBeltPhysics : VulpesMonoBehaviour
    {
        [field: SerializeField] private Vector2 Velosity;
        [field: SerializeField] private Collider _collider;

        int instanceIDcollider;
        public void OnEnable()
        {
            _collider = GetComponent<Collider>();
            instanceIDcollider = _collider.GetInstanceID();
            _collider.hasModifiableContacts = true;
            Physics.ContactModifyEvent += PhysicsContactEvent;
        }

        public void OnDisable()
        {
            Physics.ContactModifyEvent -= PhysicsContactEvent;
        }
        //Это не всегда работает в мейн потоке. Иногда айди потока другой. Полагаю Физический поток
        private void PhysicsContactEvent(PhysicsScene scene, NativeArray<ModifiableContactPair> contactPairs)
        {
            foreach (var pair in contactPairs)
            {
                if (instanceIDcollider == pair.otherColliderInstanceID || instanceIDcollider == pair.colliderInstanceID)
                {
                    for(int i = 0; i < pair.contactCount; i++)
                        pair.SetTargetVelocity(i, new Vector3(Velosity.x ,0, Velosity.y) * pair.massProperties.otherInverseMassScale);
                }
            }

        }

    }
}

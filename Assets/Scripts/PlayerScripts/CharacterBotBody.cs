using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerDescription
{
    [DisallowMultipleComponent]
    public class CharacterBotBody : CharacterBody , IExplosionDamaged, IDamaged
    {
        [field: SerializeField] public float Health { get; private set; } = 5f;

        public void Explosion(float distanse)
        {
            if (enabled == false) return;
            gameObject.GetComponentsInChildren<CharacterJoint>().ToList().ForEach(joint =>
            {
                GameObject newJoint = joint.gameObject;
                if (!newJoint.name.ToLower().Contains("chest") && UnityEngine.Random.Range(0f, 0.7f * distanse) < 1F)
                {
                    newJoint.AddComponent<ConfigurableJoint>().connectedBody = joint.connectedBody;
                    GameObject.Destroy(joint);
                }
            });
        }

        public void TakeHit(IStriker striker)
        {
            Health -= striker.Damage;
            if(Health <= 0)
            {
                Death();
                Explosion(1);
            }
        }
    }
}

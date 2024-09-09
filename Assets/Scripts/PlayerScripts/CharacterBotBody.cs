using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerDescription
{
    public class CharacterBotBody : CharacterBody , IExplosionDamage
    {
        public void Explosion(float distanse)
        {
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
    }
}

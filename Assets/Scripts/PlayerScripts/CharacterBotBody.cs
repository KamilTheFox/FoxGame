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
        public void Explosion()
        {
            gameObject.GetComponentsInChildren<CharacterJoint>().ToList().ForEach(joint =>
            {
                GameObject newJoint = joint.gameObject;
                if (!newJoint.name.ToLower().Contains("chest") && UnityEngine.Random.Range(0, 3) == 2)
                {
                    newJoint.AddComponent<ConfigurableJoint>().connectedBody = joint.connectedBody;
                    GameObject.Destroy(joint);
                }
            });
        }
    }
}

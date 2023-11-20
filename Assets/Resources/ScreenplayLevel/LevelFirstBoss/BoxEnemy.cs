using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScreenplayLevel
{
    public class BoxEnemy : MonoBehaviour, IDiesing
    {

        public Transform Transform { get; set; }

        [field: SerializeField] public bool IsDie { get; set; }

        //public override TypeEntity typeEntity => TypeEntity.None;

        public void Death()
        {
            if (IsDie) return;
            EnemyBoss.Instance.Damage(this);
            IsDie = true;
            gameObject.SetActive(false);
        }
        public void Awake()
        {
            Transform = transform;
        }

    }
}

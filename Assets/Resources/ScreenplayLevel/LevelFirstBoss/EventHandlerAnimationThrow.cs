using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScreenplayLevel
{
    public class EventHandlerAnimationThrow : MonoBehaviour
    {
        public EnemyBoss enemyBoss;
        public void Shot()
        {
            enemyBoss.Shot();
        }
    }
}

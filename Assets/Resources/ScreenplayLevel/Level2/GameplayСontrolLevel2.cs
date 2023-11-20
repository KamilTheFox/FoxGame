using System;
using UnityEngine;
using Tweener;

namespace ScreenplayLevel
{
    public class GameplayСontrolLevel2 : MonoBehaviour
    {
        [SerializeField] private Transform BigDoor;
        public void OpenBigDoor()
        {
            if (BigDoor)
                Tween.AddPosition(BigDoor, Vector3.up * 3, 15F);
        }
    }
}

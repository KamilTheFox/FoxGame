using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IWieldable
{
    Transform RootParent { set; }
    bool IsActive { get; }  // Активно ли сейчас оружие (в замахе/ударе)
    void EnableWeapon();         // Включить во время анимации
    void DisableWeapon();        // Выключить после анимации

    void SetWieldHand(Transform parent);

    string UpAxis { get; }    // Ось куда смотрит макушка
}

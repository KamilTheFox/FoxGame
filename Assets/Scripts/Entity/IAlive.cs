﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

    public interface IAlive
    {
        Transform Transform { get; }
        bool IsDead { get; }
        void Dead();
        GameObject gameObject { get; }
        Action<Collision, GameObject> BehaviorFromCollision => null;
    }